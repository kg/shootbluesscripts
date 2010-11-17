import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json
import state
import base
import moniker
import trinity
import blue

ActionThreshold = ((10000000L) * 175) / 100

serviceInstance = None

def getLockedTargets():
    ballpark = eve.LocalSvc('michelle').GetBallpark()
    targetSvc = sm.services['target']
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]

def notifyPrefsChanged(newPrefsJson):
    global serviceInstance
    if serviceInstance:
        serviceInstance.prefs = json.loads(newPrefsJson)        

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
        self.disabled = False
        self.prefs = {}
    
    def checkUpdateTimer(self, droneID = None):
        drones = self.getDronesInLocalSpace()
        if (droneID != None) and (droneID not in drones):
            drones.append(droneID)
        droneCount = len(drones)
        if (droneCount > 0) and (self.__updateTimer == None):
            self.__updateTimer = base.AutoTimer(500, self.updateDrones)
        elif (droneCount <= 0) and (self.__updateTimer != None):
            self.__updateTimer = None
    
    def getPref(self, key, default):
        return self.prefs.get(key, default)
    
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
                
        sortedTargets = sorted(
            [(count, targetID) for targetID, count in 
             targetCounts.items() if count > threshold],
            cmp=lambda lhs, rhs: -cmp(lhs, rhs)
        )
        
        if len(sortedTargets):
            return sortedTargets[0][1]
        else:
            return None
    
    def getSignatureRadius(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        targetitem = eve.LocalSvc("godma").GetType(ballpark.GetInvItem(targetID).typeID)
        if targetitem.AttributeExists("signatureRadius"):
            result = float(targetitem.signatureRadius)
        else:
            result = float(ballpark.GetBall(targetID).radius)
        return result
    
    def makeDistanceSorter(self, reversed):
        def droneSorter(lhs, rhs):
            sigLhs = self.getSignatureRadius(lhs)
            sigRhs = self.getSignatureRadius(rhs)
            result = cmp(sigLhs, sigRhs)
            
            if result == 0:
                distLhs = self.getDistance(lhs)
                distRhs = self.getDistance(rhs)
                result = cmp(distLhs, distRhs)
            elif reversed:
                result = -result
            
            return result
        
        return droneSorter
    
    def filterExisting(self, ids):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return [id for id in ids if ballpark.GetInvItem(id)]
    
    def selectTarget(self):
        targets = self.filterExisting(sm.services["target"].targets)
        if len(targets):
            if self.getPref("Largest", False):
                targets.sort(
                    self.makeDistanceSorter(True)
                )
            if self.getPref("Smallest", True):
                targets.sort(
                    self.makeDistanceSorter(False)
                )
            elif self.getPref("ClosestToDrones", False):
                targets.sort(
                    lambda lhs, rhs: cmp(
                        self.getDistance(lhs), 
                        self.getDistance(rhs)
                    )
                )
            
            return targets[0]
        else:
            return None
    
    def doAttack(self, idleOnly, *dronesToAttack):
        if self.disabled:
            return
            
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        timestamp = blue.os.GetTime()
        isCommonTarget = False
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
        
        timestamp = blue.os.GetTime()
        
        for droneID in list(dronesToRecall):
            droneObj = self.getDroneObject(droneID)
            if ((droneObj.state == const.entityDeparting) or
               abs(droneObj.actionTimestamp - timestamp) <= ActionThreshold):
                dronesToRecall.remove(droneID)
        
        if len(dronesToRecall):
            entity = moniker.GetEntityAccess()
            if entity:
                log("Drone(s) %r returning", dronesToRecall)
                entity.CmdReturnBay(dronesToRecall)
                for droneID in dronesToRecall:  
                    droneObj = self.getDroneObject(id)
                    droneObj.setState(const.entityDeparting, timestamp)
                    droneObj.actionTimestamp = timestamp
    
    def updateDrones(self):
        if self.disabled:
            return
        
        timestamp = blue.os.GetTime()
        droneIDs = self.getDronesInLocalSpace()
        dronesToRecall = []
        dronesToAttack = []
        
        for pendingID in self.__pendingStateChanges.keys():
            if pendingID not in droneIDs:
                continue
                
            (ts, oldState, newState) = self.__pendingStateChanges[pendingID]
            del self.__pendingStateChanges[pendingID]
            
            self.OnDroneStateChange2(pendingID, oldState, newState, timestamp=ts)
        
        for droneID in droneIDs:
            drone = self.getDroneObject(droneID)
            drone.update(timestamp)
            if self.checkDroneHealth(drone):
                dronesToRecall.append(droneID)
            elif ((drone.state == const.entityIdle) and
                  self.getPref("AutoAttackWhenIdle", False)):
                dronesToAttack.append(droneID)
        
        if len(dronesToRecall):
            for id in dronesToRecall:
                if id in dronesToAttack:
                    dronesToAttack.remove(id)
            self.doRecall(*dronesToRecall)
        
        if len(dronesToAttack):
            self.doAttack(True, *dronesToAttack)
    
    def checkDroneHealth(self, drone):
        if not drone:
            return False
        if drone.state == const.entityDeparting:
            return False
        
        result = False
        if self.getPref("RecallIfShieldsBelow", False):
            threshold = float(self.getPref("RecallShieldThreshold", 50)) / 100.0
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
           self.getPref("AutoAttackWhenTargetLost", False)):
            shouldAutoAttack = True
        
        if ((oldActivityState != const.entityIdle) and 
           (drone.state == const.entityIdle) and
           self.getPref("AutoAttackWhenIdle", False)):
            shouldAutoAttack = True
        
        if shouldRecall:
            self.doRecall(droneID)
        elif shouldAutoAttack:
            self.doAttack(False, droneID)
            
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
    if serviceRunning:
        forceStopService("dronehelper")
