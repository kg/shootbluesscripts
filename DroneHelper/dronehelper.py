import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, getFlagName, getNamesOfIDs, ChanceToHitCalculator, getTypeAttributes, runOnMainThread
from shootblues.common.eve.state import getCachedItem
from shootblues.common.service import forceStart, forceStop
import service
import uix
import json
import state
import base
import moniker
import trinity
import blue
from util import Memoized

ActionThreshold = ((10000000L) * 190) / 100

prefs = {}
serviceInstance = None
serviceRunning = False

try:
    from shootblues.enemyprioritizer import getPriority
except:
    def getPriority(*args, **kwargs):
        return 0

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)        

class DroneInfo(object):
    def __init__(self, droneID):
        self.id = droneID
        ci = getCachedItem(droneID)
        
        self.ci = ci
        self.slimItem = ci.slimItem
        self.target = None
        self.actionTimestamp = self.timestamp = 0
        self.shield = self.armor = self.structure = 1.0
        self.state = None
        
        if ci.slimItem:        
            attributes = getTypeAttributes(ci.slimItem.typeID, obj=ci.ball)
            self.isSentry = float(attributes.get("entityCruiseSpeed", 0.0)) <= 0.0
        else:
            self.isSentry = False
    
    @property
    def ball(self):
        return self.ci.ball
    
    def setState(self, newState, timestamp):
        if timestamp > self.timestamp:
            self.state = newState
            self.timestamp = timestamp
            self.update(timestamp)
    
    def setTarget(self, targetID, timestamp):
        if timestamp > self.timestamp:
            oldTarget = self.target
            self.target = targetID
            self.timestamp = timestamp

    def update(self, timestamp):
        if timestamp > self.timestamp:
            ds = eve.LocalSvc("michelle").GetDroneState(self.id)
            if ds:
                self.target = ds.targetID
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            ds = ballpark.GetDamageState(self.id)
            if ds:
                (self.shield, self.armor, self.structure) = ds
            self.timestamp = timestamp

class DroneHelperSvc:
    __notifyevents__ = [
        "OnDroneStateChange2",
        "OnDroneControlLost"
    ]

    def __init__(self):
        self.__drones = {}
        self.__pendingStateChanges = {}
        self.__lastAttackOrder = None
        self.__lastLaunchTimestamp = 0
        self.__recalling = set()
        self.__recalled = []
        self.__numFighters = 0
        self.__numSentries = 0
        self.__updateTimer = SafeTimer(1000, self.updateDrones)
        self.disabled = False
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        
        # Attempts to use the average location of all active drones
        drones = self.getDronesInLocalSpace()
        
        divisor = float(len(drones))
        if divisor <= 0:
            divisor = 1.0
            
        avg = 0
        for id in drones.iterkeys():
            avg += ballpark.DistanceBetween(id, targetID)
                    
        return avg / divisor
        
    def getDronesInLocalSpace(self):
        result = {}
    
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if ballpark is None:
            return result
        
        drones = eve.LocalSvc("michelle").GetDrones()
        for droneID in drones.iterkeys():
            drone = drones[droneID]
            
            if not ballpark.slimItems.has_key(droneID):
                continue
            if drone.ownerID != eve.session.charid:
                continue
            if drone.controllerID != eve.session.shipid:
                continue
            
            result[droneID] = drone
        
        return result
    
    def getDronesInLocalSpaceByGroup(self):
        dronesInSpace = self.getDronesInLocalSpace()
        groups = self.getDroneGroups()
        
        result = {}
        
        for k in groups.iterkeys():
            s = set([id for id in groups[k] if dronesInSpace.has_key(id)])
            if len(s):
                result[k] = s
        
        return result
    
    def getDroneControlRange(self):
        godma = eve.LocalSvc("godma")
        ship = godma.GetItem(eve.session.shipid)
        char = godma.GetItem(eve.session.charid)
        if self.__numFighters > 0:
            fighterRangeMultiplier = float(getattr(ship, "droneRangeBonus", 1.0))
        else:
            fighterRangeMultiplier = 1.0
        result = int(max(
            ship.droneControlDistance,
            char.droneControlDistance
        ) * fighterRangeMultiplier)
        return result
    
    def getDronesInBay(self):
        baylist = eve.GetInventoryFromId(eve.session.shipid).ListDroneBay()
        
        result = {}
        for i in baylist:
            result[i.itemID] = i
        
        return result
    
    def getDroneGroups(self):
        result = {}
        
        droneview = uicore.desktop.FindChild("droneview")
        if not droneview:
            return result
        
        groups = getattr(droneview, "groups", None)
        if not groups:
            return result
               
        dronesInBay = self.getDronesInBay()
        dronesInSpace = self.getDronesInLocalSpace()
        
        for k, v in groups.iteritems():
            ids = v.get("droneIDs", None)
            if ids:
                result[k] = tuple([
                    i for i in ids if (
                        dronesInBay.has_key(i) or dronesInSpace.has_key(i)
                    )
                ])
            else:
                result[k] = tuple()
        
        return result
    
    def launchNamedGroup(self, name):
        groups = self.getDroneGroups()
        ids = groups.get(name, None)
        if ids and len(ids):
            self.doLaunch(*ids)
    
    def recallNamedGroup(self, name):
        groups = self.getDronesInLocalSpaceByGroup()
        ids = groups.get(name, None)
        if ids and len(ids):
            self.doRecall(*ids)
    
    def getCommonTarget(self, filtered=True):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        
        targetCounts = {}
        for drone in self.__drones.values():
            if (drone.state == const.entityCombat and 
                drone.target and 
                ballpark.GetInvItem(drone.target)):
                targetCounts[drone.target] = targetCounts.setdefault(drone.target, 0) + 1
                
        sortedTargets = [st[1] for st in sorted(
            [(count, targetID) for targetID, count in 
             targetCounts.items() if count > 1],
            cmp=lambda lhs, rhs: -cmp(lhs, rhs)
        )]
        
        if filtered:
            sortedTargets = self.filterTargets(sortedTargets)
        
        if len(sortedTargets):
            return sortedTargets[0]
        else:
            return None
    
    def getTargetSorter(self):
        gp = Memoized(getPriority)
        gd = Memoized(self.getDistance)
        
        numDrones = len(self.__drones)
        useChanceToHit = (self.__numSentries >= (numDrones / 2)) and (self.__numSentries > 0)
        
        if useChanceToHit:
            def getCalcForDrone(id):
                cthc = ChanceToHitCalculator(id)
                cthc.setDrone(id)
                return Memoized(cthc.calculate)
        
            calcs = [
                getCalcForDrone(id) for id in self.__drones.iterkeys()
                if self.__drones[id].isSentry
            ]
            
            def toHitSorter(lhs, rhs):
                cthLhs = 0
                cthRhs = 0
                
                for calc in calcs:
                    cthLhs += calc(lhs)
                    cthRhs += calc(rhs)
                
                cthLhs /= len(calcs)
                cthRhs /= len(calcs)
                
                return cmp(cthRhs, cthLhs)
            
        else:
            def toHitSorter(lhs, rhs):
                distLhs = gd(lhs)
                distRhs = gd(rhs)
                return cmp(distLhs, distRhs)
        
        def targetSorter(lhs, rhs):
            # Highest priority first
            priLhs = gp(lhs)
            priRhs = gp(rhs)
            result = cmp(priRhs, priLhs)
            
            if result == 0:
                # targets of equal priority that were already attacked by dronehelper come first
                result = cmp(
                    rhs is self.__lastAttackOrder,
                    lhs is self.__lastAttackOrder
                )
                
                if result == 0:
                    result = toHitSorter(lhs, rhs)
            
            return result
        
        return targetSorter
    
    def filterTargets(self, ids):
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return []
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return []
        
        controlRange = self.getDroneControlRange()
        result = []
        
        for id in ids:
            invItem = ballpark.GetInvItem(id)
            if not invItem:
                continue
            
            if not (id in targetSvc.targets):
                continue
            
            flag = getFlagName(id)
            if (flag != "HostileNPC") and (flag is not None):
                continue
            
            if getPriority(id) < 0:
                continue
            
            if ballpark.DistanceBetween(eve.session.shipid, id) > controlRange:
                continue
                
            result.append(id)
        
        return result
    
    def selectTarget(self):
        targets = self.filterTargets(sm.services["target"].targets)
        if len(targets):
            targetSorter = self.getTargetSorter()
            targets.sort(targetSorter)
            
            return targets[0]
        else:
            return None
    
    def doAttack(self, idleOnly, targetID=None, dronesToAttack=[], oldTarget=None):
        if self.disabled:
            return
            
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        timestamp = blue.os.GetTime(1)
        isCommonTarget = False
        if not targetID:
            targetID = self.getCommonTarget()
            if targetID:
                slimItem = ballpark.GetInvItem(targetID)
                if slimItem:
                    targetName = uix.GetSlimItemName(slimItem)
                    isCommonTarget = True
        
        if not targetID:
            targetID = self.selectTarget()
        
        if targetID:        
            slimItem = ballpark.GetInvItem(targetID)
            if slimItem:
                targetName = uix.GetSlimItemName(slimItem)
            else:
                targetName = "Unknown"
            
            drones = list(self.getDronesInLocalSpace().keys())
            for id in dronesToAttack:
                if id not in drones:
                    drones.append(id)
                
            for id in list(drones):
                droneObj = self.getDroneObject(id)
                if ((droneObj.state == const.entityDeparting) or
                    (droneObj.state == const.entityDeparting2) or
                    (droneObj.state == const.entityFleeing) or
                    (droneObj.state == const.entityPursuit)):
                    drones.remove(id)
                elif ((droneObj.target == targetID) or
                    abs(droneObj.actionTimestamp - timestamp) <= ActionThreshold):
                    drones.remove(id)
                elif (idleOnly and (droneObj.state != const.entityIdle)):
                    drones.remove(id)
            
            if len(drones):
                if ((len(drones) > 1) or 
                    (not self.__lastAttackOrder)):
                    self.__lastAttackOrder = targetID
                
                entity = moniker.GetEntityAccess()
                if entity:
                    if isCommonTarget:
                        targetName += " (existing target)"
                    
                    oldTargetName = None
                    if oldTarget:
                        slimItem = ballpark.GetInvItem(oldTarget)
                        oldTargetName = uix.GetSlimItemName(slimItem)
                    
                    if oldTargetName:
                        log("%s changing target from %s to %s", ", ".join(getNamesOfIDs(drones)), oldTargetName, targetName)
                    else:
                        log("%s attacking %s", ", ".join(getNamesOfIDs(drones)), targetName)
                    
                    for id in drones:
                        droneObj = self.getDroneObject(id)
                        droneObj.setTarget(targetID, timestamp)
                        droneObj.actionTimestamp = timestamp
                    ret = entity.CmdEngage(drones, targetID)    
    
    def doRecall(self, *dronesToRecall):
        if self.disabled:
            return
        
        dronesToRecall = list(dronesToRecall)
        timestamp = blue.os.GetTime(1)
        
        for droneID in list(dronesToRecall):
            droneObj = self.getDroneObject(droneID)
            if ((droneObj.state == const.entityDeparting) or
               (droneObj.state == const.entityDeparting2) or
               (droneObj.state == const.entityPursuit) or
               (droneObj.state == const.entityFleeing) or
               abs(droneObj.actionTimestamp - timestamp) <= ActionThreshold):
                dronesToRecall.remove(droneID)
        
        if len(dronesToRecall):
            entity = moniker.GetEntityAccess()
            if entity:
                log("Drone(s) returning: %s", ", ".join(getNamesOfIDs(dronesToRecall)))
                entity.CmdReturnBay(dronesToRecall)
                for droneID in dronesToRecall:                    
                    droneObj = self.getDroneObject(droneID)
                    droneObj.setState(const.entityDeparting, timestamp)
                    droneObj.actionTimestamp = timestamp
                    self.__recalling.add(droneObj.id)
    
    def doLaunch(self, *dronesToLaunch):
        if self.disabled:
            return
        
        dronesToLaunch = list(dronesToLaunch)
        timestamp = blue.os.GetTime(1)
        
        #if (timestamp - self.__lastLaunchTimestamp) < ActionThreshold:
        #    return
    
        dronesInBay = self.getDronesInBay()        
        droneObjs = [dronesInBay[id] for id in dronesToLaunch if dronesInBay.has_key(id)]
        
        log("Launching %d drone(s)", len(droneObjs))
        self.__lastLaunchTimestamp = timestamp
        sm.services["menu"].LaunchDrones(droneObjs)
    
    def updateDrones(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        targetSvc = sm.services["target"]
        if not targetSvc:
            return
        
        if self.__lastAttackOrder:
            if not ballpark.GetBall(self.__lastAttackOrder):
                self.__lastAttackOrder = None
        
        timestamp = blue.os.GetTime(1)
        droneIDs = set(self.getDronesInLocalSpace().keys())
        dronesToRecall = []
        dronesToAttack = []
        
        self.__numFighters = 0
        for droneID in droneIDs:
            slimItem = ballpark.GetInvItem(droneID)
            if (slimItem and (
                (slimItem.groupID == const.groupFighterDrone) or
                (slimItem.groupID == const.groupFighterBomber)
               )):
                self.__numFighters += 1
        
        self.__numSentries = 0
        for droneObj in self.__drones.itervalues():
            if droneObj.isSentry:
                self.__numSentries += 1
        
        for (pendingID, psc) in list(self.__pendingStateChanges.items()):
            if self.__pendingStateChanges.has_key(pendingID):
                del self.__pendingStateChanges[pendingID]

            if pendingID not in droneIDs:
                continue
            
            (ts, oldState, newState) = psc
            self.OnDroneStateChange2(pendingID, oldState, newState, timestamp=ts)
        
        for droneID in droneIDs:
            drone = self.getDroneObject(droneID)
            drone.update(timestamp)
            if self.checkDroneHealth(drone):
                dronesToRecall.append(droneID)
            elif ((drone.state == const.entityIdle) and
                  getPref("WhenIdle", False)):
                dronesToAttack.append(droneID)
        
        if getPref("RedeployAfter", True) and len(self.__recalled):
            redeployAfter = int(float(getPref("RedeployAfterSeconds", 30.0)) * 10000000)
            dronesToRelaunch = []
            
            ts, id = self.__recalled[0]
            while ts + redeployAfter <= timestamp:                    
                dronesToRelaunch.append(id)
                self.__recalled.pop(0)
                
                if len(self.__recalled):
                    ts, id = self.__recalled[0]
                else:
                    break
            
            if len(dronesToRelaunch):
                self.doLaunch(*dronesToRelaunch)
                
        if len(dronesToRecall):
            for id in dronesToRecall:
                if id in dronesToAttack:
                    dronesToAttack.remove(id)
            self.doRecall(*dronesToRecall)
        
        if len(dronesToAttack):
            self.doAttack(idleOnly=True, targetID=None, dronesToAttack=dronesToAttack)
        elif getPref("WhenTargetLost", False):
            commonTarget = self.getCommonTarget(filtered=False)
            if commonTarget == eve.session.shipid:
                return
                        
            oldPriority = getPriority(commonTarget)
            newTarget = self.selectTarget()
            newPriority = getPriority(newTarget)
            
            if commonTarget and (newPriority > oldPriority):
                if ((commonTarget == self.__lastAttackOrder) or 
                    (commonTarget not in targetSvc.targets)):                    
                    self.doAttack(idleOnly=False, targetID=newTarget, oldTarget=commonTarget)        
    
    def checkDroneHealth(self, drone):
        result = False
        if getPref("RecallIfShieldsBelow", False):
            threshold = float(getPref("RecallShieldThreshold", 50)) / 100.0
            if drone.shield < threshold:
                result = True

        return result
    
    def getDroneObject(self, droneID):
        if self.__drones.has_key(droneID):
            drone = self.__drones[droneID]
        else:
            drone = DroneInfo(droneID)
            self.__drones[droneID] = drone
        
        return drone
    
    def OnDroneStateChange2(self, droneID, oldActivityState, newActivityState, timestamp=None):       
        if not timestamp:
            timestamp = blue.os.GetTime(1)
    
        dronesInLocal = set(self.getDronesInLocalSpace().keys())
        if droneID not in dronesInLocal:
            self.__pendingStateChanges[droneID] = (timestamp, oldActivityState, newActivityState)
            return
                
        drone = self.getDroneObject(droneID)
        oldTarget = drone.target
        drone.setState(newActivityState, timestamp)
        
        shouldAutoAttack = False
        shouldRecall = self.checkDroneHealth(drone)
        idleOnly = False
        
        if ((oldTarget != None) and (drone.target == None) and 
           (drone.state == const.entityIdle) and 
           (oldTarget not in sm.services["target"].targets) and
           getPref("WhenTargetLost", False)):
            shouldAutoAttack = True
        
        if ((oldActivityState != const.entityIdle) and 
           (drone.state == const.entityIdle) and
           getPref("WhenIdle", False)):
            shouldAutoAttack = True
        
        if shouldRecall:
            self.doRecall(droneID)
        elif shouldAutoAttack:
            self.doAttack(idleOnly=False, dronesToAttack=[droneID])

    def OnDroneControlLost(self, droneID):
        timestamp = blue.os.GetTime(1)
        drone = self.getDroneObject(droneID)
        drone.setState(None, timestamp)
        
        if drone.id in self.__recalling:
            self.__recalling.remove(drone.id)
            self.__recalled.append((timestamp, drone.id))
            self.__recalled.sort()
        
        if self.__drones.has_key(droneID):
            del self.__drones[droneID]
        
        if len(self.__drones) == 0:
            self.__lastAttackOrder = None        

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("dronehelper", DroneHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("dronehelper")
        serviceRunning = False
