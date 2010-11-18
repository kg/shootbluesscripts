import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json

prefs = {}
serviceInstance = None

def notifyPrefsChanged(newPrefsJson):
    prefs = json.loads(newPrefsJson)   
     
class EnemyPrioritizerSvc(service.Service):
    __guid__ = "svc.enemyprioritizer"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnTargets",
        "OnTarget"
    ]

    def __init__(self):
        service.Service.__init__(self)
    
    def OnTarget(self, what, tid = None, reason = None):
        ballpark = eve.LocalSvc('michelle').GetBallpark()
        targetName = None
        targetType = None
        targetGroup = None
        if tid:
            slimItem = ballpark.GetInvItem(tid)
            if slimItem:
                targetName = uix.GetSlimItemName(slimItem)
                targetType = slimItem.typeID
                targetGroup = getattr(slimItem, "groupID")
        
        log("OnTarget %r (type %r group %r) what=%r reason=%r", targetName, targetType, targetGroup, what, reason)
    
    def OnTargets(self, targets):
        for target in targets:
            self.OnTarget(*(target[1:]))

def initialize():
    global serviceInstance
    # serviceInstance = forceStartService("enemyprioritizer", EnemyPrioritizerSvc)

def __unload__():
    global serviceInstance
    if serviceInstance:
        forceStopService("enemyprioritizer")
        serviceInstance = None