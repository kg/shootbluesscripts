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
        self.__drones = {}

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
    
    def OnDroneStateChange2(self, droneID, oldActivityState, newActivityState):
        log("DroneHelperSvc.OnDroneStateChange2 droneId=%r oldState=%r newState=%r", droneID, oldActivityState, newActivityState)
        if self.__drones.has_key(droneID):
            self.__drones[droneID][1] = newActivityState
        else:
            ballpark = eve.LocalSvc('michelle').GetBallpark()
            self.__drones[droneID] = [ballpark.GetInvItem(droneID), newActivityState]
        
        if (oldActivityState != const.entityIdle) and (newActivityState == const.entityIdle):
            log("Drone %r is becoming idle.", droneID)

    def OnDroneActivityChange(self, droneID, activityID, activity):
        log("DroneHelperSvc.OnDroneActivityChange droneId=%r activityId=%r activity=%r", droneID, activityID, activity)

def initialize():
    global serviceInstance
    log("Initializing DroneHelper")
    serviceInstance = DroneHelperSvc
    forceStartService("dronehelper", serviceInstance)

def __unload__():
    global serviceInstance
    log("Tearing down DroneHelper")
    if serviceInstance:
        forceStopService("dronehelper")
