import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json
import state
import base

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

class AutoTargeterSvc(service.Service):
    __guid__ = "svc.autotargeter"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "DoBallsAdded",
        "DoBallRemove",
        "DoBallClear"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.disabled = False
    
    def checkUpdateTimer(self, droneID = None):
        if self.disabled:
            self.__updateTimer = None
        elif self.__updateTimer == None:
            self.__updateTimer = base.AutoTimer(1000, self.updateTargets)
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return ballpark.DistanceBetween(eve.session.shipid, targetID)
    
    def targetSorter(self, lhs, rhs):
        # Highest priority first
        priLhs = getPriority(targetID=lhs)
        priRhs = getPriority(targetID=rhs)
        result = cmp(priRhs, priLhs)
        
        if result == 0:
            distLhs = self.getDistance(lhs)
            distRhs = self.getDistance(rhs)
            result = cmp(distLhs, distRhs)
        
        return result
    
    def filterTargets(self, ids):
        targetSvc = sm.services['target']
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return [id for id in ids if ballpark.GetInvItem(id) and id in targetSvc.targets]
    
    def selectTarget(self):
        targets = self.filterTargets(sm.services["target"].targets)
        if len(targets):
            targets.sort(self.targetSorter)
            
            return targets[0]
        else:
            return None
    
    def DoBallsAdded(self, lst, **kwargs):
        pass
    
    def DoBallRemove(self, ball, slimItem, *args, **kwargs):
        pass
    
    def DoBallClear(self, solItem, **kwargs):
        pass
    
    def updateTargets(self):
        pass

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("autotargeter", AutoTargeterSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("autotargeter")
        serviceRunning = False
