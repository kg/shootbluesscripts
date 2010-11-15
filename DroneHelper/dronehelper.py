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
        self.prefs = {}
        self.__healthTimer = None
    
    def checkHealthTimer(self):
        droneCount = len(self.getDronesInLocalSpace())
        if (droneCount > 0) and (self.__healthTimer == None):
            self.__healthTimer = base.AutoTimer(500, self.checkAllDronesHealth)
        elif (droneCount <= 0) and (self.__healthTimer != None):
            self.__healthTimer = None
    
    def getPref(self, key, default):
        return self.prefs.get(key, default)
    
    def getDronePosition(self, droneID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if self.getPref("AttackAsGroup", False):
            drones = self.getDronesInLocalSpace()
            
            avg = (0, 0, 0)
            for drone in drones:
                ball = ballpark.GetBall(droneID)
                avg = (avg[0] + ball.x, avg[1] + ball.y, avg[2] + ball.z)
            
            divisor = float(len(drones))
            if divisor <= 0:
                divisor = 1.0
            
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
        return (dronePosition - targetPosition).Length2()
    
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
                    self.getDistance(droneID, lhs),
                    self.getDistance(droneID, rhs)
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
        targetID = self.selectTarget(droneID)
        if targetID:
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            targetName = uix.GetSlimItemName(ballpark.GetInvItem(targetID))
            
            if self.getPref("AttackAsGroup", False):
                drones = self.getDronesInLocalSpace()
            else:
                drones = [droneID]
            
            entity = moniker.GetEntityAccess()
            if entity:
                log("Drones %r attacking %s", drones, targetName)
                ret = entity.CmdEngage(drones, targetID)
        else:
            log("Drone %r found no targets", droneID)
    
    def doRecall(self, droneID):
        log("Drone %r returning", droneID)
        entity = moniker.GetEntityAccess()
        if entity:
            if self.__droneStates.get(droneID, const.entityIdle) == const.entityIdle:
                self.__droneStates[droneID] = const.entityDeparting
            ret = entity.CmdReturnBay([droneID])
    
    def checkAllDronesHealth(self):
        droneIDs = self.getDronesInLocalSpace()
        dronesToRecall = []
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
    
    def OnDroneStateChange2(self, droneID, oldActivityState, newActivityState):
        droneState = self.getDroneState(droneID)
        
        oldTarget = self.__droneTargets.get(droneID, None)
        if droneState:
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
            
        self.checkHealthTimer()

    def OnDroneControlLost(self, droneID):
        log("Lost control of drone %r", droneID)
        if self.__droneTargets.has_key(droneID):
            del self.__droneTargets[droneID]
        if self.__droneStates.has_key(droneID):
            del self.__droneStates[droneID]
        
        self.checkHealthTimer()

def initialize():
    global serviceRunning, serviceInstance
    log("Initializing DroneHelper")
    serviceRunning = True
    serviceInstance = forceStartService("dronehelper", DroneHelperSvc)

def __unload__():
    global serviceRunning
    if serviceRunning:
        log("Tearing down DroneHelper")
        forceStopService("dronehelper")
