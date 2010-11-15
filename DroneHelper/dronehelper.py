import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json
import state
import base
import moniker
import trinity

serviceInstance = None

def getLockedTargets():
    ballpark = eve.LocalSvc('michelle').GetBallpark()
    targetSvc = sm.services['target']
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]

def notifyPrefsChanged(newPrefsJson):
    global serviceInstance
    if serviceInstance:
        serviceInstance.prefs = json.loads(newPrefsJson)        

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
        self.__droneTargets = {}
        self.__droneStates = {}
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
    
    def getDronePosition(self, droneID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if self.getPref("AttackAsGroup", False):
            drones = self.getDronesInLocalSpace()
            if droneID not in drones:
                drones.append(droneID)
            
            divisor = float(len(drones))
            if divisor <= 0:
                divisor = 1.0
                
            avg = (0, 0, 0)
            for id in drones:
                ball = ballpark.GetBall(id)
                avg = (
                    avg[0] + (ball.x / divisor), 
                    avg[1] + (ball.y / divisor), 
                    avg[2] + (ball.z / divisor)
                )
                        
            return trinity.TriVector(avg[0] / divisor, avg[1] / divisor, avg[2] / divisor)
        else:
            ball = ballpark.GetBall(droneID)
            return trinity.TriVector(ball.x, ball.y, ball.z)
        
    def getDronesInLocalSpace(self):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if ballpark is None:
            return []
        
        drones = eve.LocalSvc("michelle").GetDrones()
        return [droneID for droneID in drones if (
            (droneID in ballpark.slimItems) and
            ((drones[droneID].ownerID == eve.session.charid) or (drones[droneID].controllerID == eve.session.shipid))
        )]
        
    def getDroneState(self, droneID):
        return eve.LocalSvc("michelle").GetDroneState(droneID)
    
    def getDistance(self, dronePosition, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        targetBall = ballpark.GetBall(targetID)
        targetPosition = trinity.TriVector(targetBall.x, targetBall.y, targetBall.z)
        distance = (dronePosition - targetPosition).Length()
        return distance
    
    def getSignatureRadius(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        targetitem = eve.LocalSvc("godma").GetType(ballpark.GetInvItem(targetID).typeID)
        if targetitem.AttributeExists("signatureRadius"):
            result = float(targetitem.signatureRadius)
        else:
            result = float(ballpark.GetBall(targetID).radius)
        return result
    
    def makeDistanceSorter(self, dronePosition, reversed):
        def droneSorter(lhs, rhs):
            result = cmp(
                self.getSignatureRadius(lhs),
                self.getSignatureRadius(rhs)
            )
            
            if result == 0:
                result = cmp(
                    self.getDistance(dronePosition, lhs),
                    self.getDistance(dronePosition, rhs)
                )
            elif reversed:
                result = -result
            
            return result
        
        return droneSorter
    
    def selectTarget(self, droneID):
        minSigRadius = float(self.getPref("MinSigRadius", 0))
        maxSigRadius = float(self.getPref("MaxSigRadius", 99999))
        targets = [
            targetID for targetID in sm.services["target"].targets if
            minSigRadius <= self.getSignatureRadius(targetID) <= maxSigRadius
        ]
        dronePosition = self.getDronePosition(droneID)
        if len(targets):
            if self.getPref("Largest", False):
                targets.sort(
                    self.makeDistanceSorter(dronePosition, True)
                )
            if self.getPref("Smallest", True):
                targets.sort(
                    self.makeDistanceSorter(dronePosition, False)
                )
            elif self.getPref("ClosestToDrones", False):
                targets.sort(
                    lambda lhs, rhs: cmp(
                        self.getDistance(dronePosition, lhs), 
                        self.getDistance(dronePosition, rhs)
                    )
                )
            
            return targets[0]
        else:
            return None
    
    def doAttack(self, droneID):
        if self.disabled:
            return         
        targetID = self.selectTarget(droneID)
        if targetID:
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            if targetID not in ballpark.slimItems:
                log("Target %r rejected due to not being in ballpark", targetID)
                return
            
            targetName = uix.GetSlimItemName(ballpark.GetInvItem(targetID))
            
            if self.getPref("AttackAsGroup", False):
                drones = self.getDronesInLocalSpace()
                if droneID not in drones:
                    drones.append(droneID)
            else:
                drones = [droneID]
                
            for id in list(drones):
              if self.__droneTargets.get(id, None) == targetID:
                drones.remove(id)
            
            if len(drones):
                entity = moniker.GetEntityAccess()
                if entity:
                    log("%r drone(s) attacking %s", len(drones), targetName)
                    ret = entity.CmdEngage(drones, targetID)
                    for id in list(drones):
                        self.__droneTargets[id] = targetID
    
    def doRecall(self, droneID):
        if self.disabled:
            return
        log("Drone %r returning", droneID)
        entity = moniker.GetEntityAccess()
        if entity:
            entity.CmdReturnBay([droneID])            
            if self.__droneStates.get(droneID, const.entityIdle) == const.entityIdle:
                self.__droneStates[droneID] = const.entityDeparting
            if self.__droneTargets.has_key(droneID):
                del self.__droneTargets[droneID]
    
    def updateDrones(self):
        if self.disabled:
            return
        droneIDs = self.getDronesInLocalSpace()
        dronesToRecall = []
        
        for pendingID in self.__pendingStateChanges.keys():
            if pendingID not in droneIDs:
                continue
                
            args = self.__pendingStateChanges[pendingID]
            del self.__pendingStateChanges[pendingID]
            self.OnDroneStateChange2(pendingID, args[0], args[1])
        
        for droneID in droneIDs:
            if self.checkDroneHealth(droneID):
                dronesToRecall.append(droneID)
        
        if len(dronesToRecall):
            log("Recalling drones: %r", dronesToRecall)
            entity = moniker.GetEntityAccess()
            if entity:
                ret = entity.CmdReturnBay(dronesToRecall)
    
    def checkDroneHealth(self, droneID):
        currentState = self.__droneStates.get(droneID, const.entityIdle)
        if currentState == const.entityDeparting:
            return False
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        (shield, armor, structure) = ballpark.GetDamageState(droneID)
        result = False
        if self.getPref("RecallIfShieldsBelow", False):
            threshold = float(self.getPref("RecallShieldThreshold", 50)) / 100.0
            if shield < threshold:
                result = True

        return result
    
    def OnDroneStateChange2(self, droneID, oldActivityState, newActivityState, deferred=False):
        dronesInLocal = self.getDronesInLocalSpace()
        if droneID not in dronesInLocal:
            self.__pendingStateChanges[droneID] = (oldActivityState, newActivityState)
            self.checkUpdateTimer(droneID=droneID)
            return
        
        droneState = self.getDroneState(droneID)
        
        oldTarget = self.__droneTargets.get(droneID, None)
        if droneState:
            if (not self.__droneTargets.has_key(droneID)) or (not deferred):
                self.__droneTargets[droneID] = droneState.targetID
        else:
            self.__droneTargets[droneID] = None
        
        self.__droneStates[droneID] = newActivityState
        
        shouldAutoAttack = False        
        shouldRecall = self.checkDroneHealth(droneID)
        
        if oldTarget != self.__droneTargets[droneID]:
            if (self.__droneTargets[droneID] == None) and (newActivityState == const.entityIdle) and self.getPref("AutoAttackWhenTargetLost", False):
                shouldAutoAttack = True
        
        if (oldActivityState != const.entityIdle) and (newActivityState == const.entityIdle):
            if self.getPref("AutoAttackWhenIdle", False):
                shouldAutoAttack = True
        
        if shouldRecall:
            self.doRecall(droneID)
        elif shouldAutoAttack:
            self.doAttack(droneID)
            
        self.checkUpdateTimer(droneID=droneID)

    def OnDroneControlLost(self, droneID):
        if self.__droneTargets.has_key(droneID):
            del self.__droneTargets[droneID]
        if self.__droneStates.has_key(droneID):
            del self.__droneStates[droneID]
        
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
