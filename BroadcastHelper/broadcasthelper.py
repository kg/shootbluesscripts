import shootblues
from shootblues.common import log
from shootblues.common.eve import getCharacterName
from shootblues.common.service import forceStart, forceStop
import service
import uix
import json

prefs = {}
serviceInstance = None
serviceRunning = False

MaxRepBoosts = 3

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

class BroadcastHelperSvc:
    __notifyevents__ = [
        "OnFleetBroadcast",
        "OnSessionChanged",
    ]

    def __init__(self):
        self.disabled = False
        self.__needReps = []
    
    def clear(self):
        while len(self.__needReps) > 0:
            item = self.__needReps[0]
            self.__needReps.remove(item)
            adjustPriority(item, 0)
            
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
            
            adjustPriority(targetID, int(getPref("TargetPriorityBoost", 1)))
        elif broadcastType == "HealArmor":
            flashItemColor(targetID, "Broadcast: Need Armor")
            self.needsReps(targetID)
        elif broadcastType == "HealShield":
            flashItemColor(targetID, "Broadcast: Need Shield")
            self.needsReps(targetID)
        elif broadcastType == "HealCapacitor":
            flashItemColor(targetID, "Broadcast: Need Capacitor")
    
    def needsReps(self, id):
        if id not in self.__needReps:
            self.__needReps.append(id)
            adjustPriority(id, int(getPref("RepPriorityBoost", 1)))
        
        while len(self.__needReps) > MaxRepBoosts:
            item = self.__needReps[0]
            self.__needReps.remove(item)
            adjustPriority(item, 0)
    
    def OnSessionChanged(self, isRemote, session, change):
        self.clear()

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("broadcasthelper", BroadcastHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("broadcasthelper")
        serviceRunning = False
