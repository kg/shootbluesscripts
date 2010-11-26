import shootblues
from shootblues.common import forceStartService, forceStopService, log, getCharacterName
import service
import uix
import json

prefs = {}
serviceInstance = None

try:
    from shootblues.enemyprioritizer import adjustPriority
except:
    def adjustPriority(*args, **kwargs):
        return

try:
    from shootblues.targetcolors import flashItemColor
except:
    def flashItemColor(*args, **kwargs):
        return

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)

class BroadcastHelperSvc(service.Service):
    __guid__ = "svc.broadcasthelper"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnFleetBroadcast",
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.disabled = False
    
    def OnFleetBroadcast(self, broadcastType, arg1, charID, locationID, targetID):
        targetName = None
        locationName = None
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if ballpark:
            slimItem = ballpark.GetInvItem(targetID)
            if slimItem:
                targetName = uix.GetSlimItemName(slimItem)
            else:
                location = cfg.evelocations.Get(targetID)
                if location:
                    targetName = location.name
            
        location = cfg.evelocations.Get(locationID)
        if location:
            locationName = location.name
                        
        log("Broadcast of type %s by %s with target %s in %s", broadcastType, getCharacterName(charID), targetName, locationName)
        if broadcastType == "Target":
            flashItemColor(targetID, "Broadcast: Target")
            
            adjustPriority(targetID, int(getPref("TargetPriorityBoost", 0)))
        elif broadcastType == "HealArmor":
            flashItemColor(targetID, "Broadcast: Need Armor")            
        elif broadcastType == "HealShield":
            flashItemColor(targetID, "Broadcast: Need Shield")
        elif broadcastType == "HealCapacitor":
            flashItemColor(targetID, "Broadcast: Need Capacitor")

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("broadcasthelper", BroadcastHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("broadcasthelper")
        serviceRunning = False
