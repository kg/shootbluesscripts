import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json
import state
import base
import moniker 

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
            log("Creating health timer")
            self.__healthTimer = base.AutoTimer(1000, self.checkAllDronesHealth)
        elif (droneCount <= 0) and (self.__healthTimer != None):
            log("Destroying health timer")
            self.__healthTimer = None
    
    def getPref(self, key, default):
        return self.prefs.get(key, default)
        
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
    
    def selectTarget(self, droneID):
        targetSvc = sm.services["target"]
        if len(targetSvc.targets):
            return targetSvc.targets[0]
    
    def doAttack(self, droneID):
        targetID = self.selectTarget(droneID)
        if targetID:
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            targetName = uix.GetSlimItemName(ballpark.GetInvItem(targetID))
            log("Performing attack for drone %r against selected target %r", droneID, targetName)
            entity = moniker.GetEntityAccess()
            if entity:
                ret = entity.CmdEngage([droneID], targetID)
        else:
            log("Unable to attack for drone %r because no target was found", droneID)
    
    def doRecall(self, droneID):
        log("Recalling drone %r", droneID)
        entity = moniker.GetEntityAccess()
        if entity:
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
