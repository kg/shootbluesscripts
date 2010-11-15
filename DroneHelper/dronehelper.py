import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix

serviceInstance = None

def getLockedTargets():
    ballpark = eve.LocalSvc('michelle').GetBallpark()
    targetSvc = sm.services['target']
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]

class DroneHelperSvc(service.Service):
    __guid__ = "svc.dronehelper"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnTarget",
        "OnTargets",
        "OnDroneStateChange2",
        "OnDroneActivityChange"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.__targets = {}
        self.__droneTargets = {}

    def Run(self, memStream=None):
        service.Service.Run(self, memStream)

    def OnTargets(self, targets):
        for each in targets:
            self.OnTarget(*each[1:])

    def OnTarget(self, what, tid = None, reason = None):
        log("DroneHelperSvc.OnTarget what=%r tid=%r reason=%r", what, tid, reason)
        if (what == "add"):
            ballpark = eve.LocalSvc('michelle').GetBallpark()
            slimItem = ballpark.GetInvItem(tid)
            self.__targets[tid] = slimItem
            log("Target Added: %r", uix.GetSlimItemName(slimItem))
        elif (what == "clear"):
            self.__targets.clear()
        elif (what == "lost"):
            slimItem = self.__targets[tid]
            log("Target Lost: %r (reason=%r)", uix.GetSlimItemName(slimItem), reason)
            del self.__targets[tid]
    
    def getDroneState(self, droneID):
        return eve.LocalSvc("michelle").GetDroneState(droneID)
    
    def OnDroneStateChange2(self, droneID, oldActivityState, newActivityState):
        log("DroneHelperSvc.OnDroneStateChange2 droneId=%r oldState=%r newState=%r", droneID, oldActivityState, newActivityState)
        droneState = self.getDroneState(droneID)
        
        oldTarget = self.__droneTargets.get(droneID, None)
        if droneState:
            self.__droneTargets[droneID] = droneState.targetID
        else:
            self.__droneTargets[droneID] = None
        if oldTarget != self.__droneTargets[droneID]:
            log("Drone %r changed targets from %r to %r.", droneID, oldTarget, self.__droneTargets[droneID])
        
        if (oldActivityState != const.entityIdle) and (newActivityState == const.entityIdle):
            log("Drone %r became idle.", droneID)

    def OnDroneActivityChange(self, droneID, activityID, activity):
        log("DroneHelperSvc.OnDroneActivityChange droneId=%r activityId=%r activity=%r", droneID, activityID, activity)

def initialize():
    global serviceRunning
    log("Initializing DroneHelper")
    serviceRunning = True
    forceStartService("dronehelper", DroneHelperSvc)

def __unload__():
    global serviceRunning
    if serviceRunning:
        log("Tearing down DroneHelper")
        forceStopService("dronehelper")
