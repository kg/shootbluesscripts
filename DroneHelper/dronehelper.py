import shootblues
from shootblues.common import forceStartService, forceStopService, log, SafeTimer, getFlagName, getNamesOfIDs
import service
import uix
import json
import state
import base
import moniker
import trinity
import blue

ActionThreshold = ((10000000L) * 150) / 100

prefs = {}
serviceInstance = None

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
        self.target = None
        self.actionTimestamp = self.timestamp = 0
        self.shield = self.armor = self.structure = 1.0
        self.state = None
    
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
            self.target = ds.targetID
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            (self.shield, self.armor, self.structure) = ballpark.GetDamageState(self.id)
            self.timestamp = timestamp

class DroneHelperSvc(service.Service):
    __guid__ = "svc.dronehelper"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnDroneStateChange2",
        "OnDroneControlLost"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.__drones = {}
        self.__pendingStateChanges = {}
        self.__updateTimer = None
        self.__lastAttackOrder = None
        self.disabled = False
        self.checkUpdateTimer()
    
    def checkUpdateTimer(self, droneID = None):
        drones = self.getDronesInLocalSpace()
        if (droneID != None) and (droneID not in drones):
            drones.append(droneID)
        droneCount = len(drones)
        
        if (droneCount > 0) and (self.__updateTimer == None):
            self.__updateTimer = SafeTimer(500, self.updateDrones)
        elif (droneCount <= 0) and (self.__updateTimer != None):
            self.__updateTimer = None
            self.__lastAttackOrder = None
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        
        # Attempts to use the average location of all active drones
        drones = self.getDronesInLocalSpace()
        
        divisor = float(len(drones))
        if divisor <= 0:
            divisor = 1.0
            
        avg = 0
        for id in drones:
            avg += ballpark.DistanceBetween(id, targetID)
                    
        return avg / divisor
        
    def getDronesInLocalSpace(self):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if ballpark is None:
            return []
        
        drones = eve.LocalSvc("michelle").GetDrones()
        return [droneID for droneID in drones if (
            (droneID in ballpark.slimItems) and
            ((drones[droneID].ownerID == eve.session.charid) or (drones[droneID].controllerID == eve.session.shipid))
        )]
    
    def getCommonTarget(self, threshold):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        
        targetCounts = {}
        for drone in self.__drones.values():
            if drone.target and ballpark.GetInvItem(drone.target):
                targetCounts[drone.target] = targetCounts.setdefault(drone.target, 0) + 1
                
        sortedTargets = [st[1] for st in sorted(
            [(count, targetID) for targetID, count in 
             targetCounts.items() if count > threshold],
            cmp=lambda lhs, rhs: -cmp(lhs, rhs)
        )]
        
        sortedTargets = self.filterTargets(sortedTargets)
        
        if len(sortedTargets):
            return sortedTargets[0]
        else:
            return None
    
    def targetSorter(self, lhs, rhs):
        # Highest priority first
        priLhs = getPriority(targetID=lhs)
        priRhs = getPriority(targetID=rhs)
        result = cmp(priRhs, priLhs)
        
        if result == 0:
            # targets of equal priority that were already attacked by dronehelper come first
            result = cmp(
                rhs is self.__lastAttackOrder,
                lhs is self.__lastAttackOrder
            )
            
            if result == 0:        
                distLhs = self.getDistance(lhs)
                distRhs = self.getDistance(rhs)
                result = cmp(distLhs, distRhs)
        
        return result
    
    def filterTargets(self, ids):
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return []
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return [id for id in ids if 
                ballpark.GetInvItem(id) and 
                (id in targetSvc.targets) and
                getFlagName(ballpark.GetInvItem(id)) == "HostileNPC" and
                (getPriority(targetID=id) >= 0)]
    
    def selectTarget(self):
        targets = self.filterTargets(sm.services["target"].targets)
        if len(targets):
            targets.sort(self.targetSorter)
            
            return targets[0]
        else:
            return None
    
    def doAttack(self, idleOnly, targetID=None, dronesToAttack=[]):
        if self.disabled:
            return
            
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        timestamp = blue.os.GetTime()
        isCommonTarget = False
        if not targetID:
            targetID = self.getCommonTarget(len(dronesToAttack))
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
            
            drones = self.getDronesInLocalSpace()
            for id in dronesToAttack:
                if id not in drones:
                    drones.append(id)
                
            for id in list(drones):            
                droneObj = self.getDroneObject(id)
                if (droneObj.state == const.entityDeparting):
                    drones.remove(id)
                elif ((droneObj.target == targetID) or
                    abs(droneObj.actionTimestamp - timestamp) <= ActionThreshold):
                    drones.remove(id)
                elif (idleOnly and (droneObj.state != const.entityIdle)):
                    drones.remove(id)
            
            if len(drones):
                if ((len(drones) > 1) or 
                    (not self.__lastAttackOrder) or 
                    isCommonTarget):
                    self.__lastAttackOrder = targetID
                
                entity = moniker.GetEntityAccess()
                if entity:
                    if isCommonTarget:
                        targetName += " (existing target)"
                    log("%r drone(s) attacking %s", len(drones), targetName)
                    for id in drones:
                        droneObj = self.getDroneObject(id)
                        droneObj.setTarget(targetID, timestamp)
                        droneObj.actionTimestamp = timestamp
                    ret = entity.CmdEngage(drones, targetID)    
    
    def doRecall(self, *dronesToRecall):
        if self.disabled:
            return
        
        dronesToRecall = list(dronesToRecall)
        timestamp = blue.os.GetTime()
        
        for droneID in list(dronesToRecall):
            droneObj = self.getDroneObject(droneID)
            if ((droneObj.state == const.entityDeparting) or
               (droneObj.state == const.entityDeparting2) or
               (droneObj.state == const.entityPursuit) or
               abs(droneObj.actionTimestamp - timestamp) <= ActionThreshold):
                dronesToRecall.remove(droneID)
        
        if len(dronesToRecall):
            entity = moniker.GetEntityAccess()
            if entity:
                log("Drone(s) returning: %s", ", ".join(getNamesOfIDs(dronesToRecall)))
                entity.CmdReturnBay(dronesToRecall)
                for droneID in dronesToRecall:                    
                    droneObj = self.getDroneObject(id)
                    droneObj.setState(const.entityDeparting, timestamp)
                    droneObj.actionTimestamp = timestamp
    
    def updateDrones(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        if self.__lastAttackOrder:
            if not ballpark.GetBall(self.__lastAttackOrder):
                self.__lastAttackOrder = None
        
        timestamp = blue.os.GetTime()
        droneIDs = self.getDronesInLocalSpace()
        dronesToRecall = []
        dronesToAttack = []
        
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
        
        if len(dronesToRecall):
            for id in dronesToRecall:
                if id in dronesToAttack:
                    dronesToAttack.remove(id)
            self.doRecall(*dronesToRecall)
        
        if len(dronesToAttack):
            self.doAttack(idleOnly=True, targetID=None, dronesToAttack=dronesToAttack)
        elif self.__lastAttackOrder and getPref("WhenTargetLost", False):
            oldPriority = getPriority(targetID=self.__lastAttackOrder)
            newTarget = self.selectTarget()
            newPriority = getPriority(targetID=newTarget)
            if newPriority > oldPriority:
                commonTarget = self.getCommonTarget(2)
                if commonTarget == self.__lastAttackOrder:
                    slimItem = ballpark.GetInvItem(commonTarget)
                    if slimItem:
                        log("Abandoning low-priority drone target %s", uix.GetSlimItemName(slimItem))
                    
                    self.doAttack(idleOnly=False, targetID=newTarget)
    
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
            timestamp = blue.os.GetTime()
    
        dronesInLocal = self.getDronesInLocalSpace()
        if droneID not in dronesInLocal:
            self.__pendingStateChanges[droneID] = (timestamp, oldActivityState, newActivityState)
            self.checkUpdateTimer(droneID)
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
            
        self.checkUpdateTimer(droneID)

    def OnDroneControlLost(self, droneID):
        if self.__drones.has_key(droneID):
            del self.__drones[droneID]
        
        self.checkUpdateTimer()

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("dronehelper", DroneHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("dronehelper")
        serviceRunning = False
