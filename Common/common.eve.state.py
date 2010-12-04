serviceInstance = None

def installStateMonitor():
    global serviceInstance
    
    import blue
    import uthread
    from shootblues.common import remoteCall, log
    from shootblues.common.eve import getCharacterName, runOnMainThread
    from shootblues.common.service import forceStart

    class StateMonitorSvc:
        __notifyevents__ = [
            "DoBallsAdded",
            "DoBallRemove",
            "DoBallClear"
        ]

        def __init__(self):
            self.forceFields = []
            
            runOnMainThread(self.populateBalls)            
        
        def populateBalls(self):    
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            if not ballpark:
                return
            
            ids = list(ballpark.balls.keys())
            
            lst = []
            for ballID in ids:
                slimItem = ballpark.GetInvItem(ballID)
                if slimItem:
                   lst.append((ballpark.GetBall(ballID), slimItem))
            
            self._DoBallsAdded(lst)
        
        def DoBallsAdded(self, lst, **kwargs):
            uthread.pool(
                "DoBallsAdded",
                self._DoBallsAdded,
                lst
            )
        
        def _DoBallsAdded(self, lst):
            for (ball, slimItem) in lst:
                if slimItem.groupID == const.groupForceField:
                    self.forceFields.append(slimItem.itemID)
        
        def DoBallRemove(self, ball, slimItem, *args, **kwargs):
            if slimItem and (slimItem.itemID in self.forceFields):
                self.forceFields.remove(slimItem.itemID)
        
        def DoBallClear(self, solItem, **kwargs):
            self.forceFields = []
            
    serviceInstance = forceStart("statemonitor", StateMonitorSvc)

def isItemInsideForceField(itemID):
    from shootblues.common import log
    
    if not serviceInstance:
        return False
    
    ballpark = eve.LocalSvc("michelle").GetBallpark()    
    for id in serviceInstance.forceFields:
        distance = ballpark.DistanceBetween(itemID, id)
        if distance <= 0:
            return True
    
    return False

def __unload__():
    global serviceInstance
    if serviceInstance:
        from shootblues.common.service import forceStop
        forceStop("statemonitor")
        serviceInstance = None