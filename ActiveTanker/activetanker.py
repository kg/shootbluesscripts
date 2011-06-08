import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, findModule, getModuleAttributes, activateModule, isLoggedIn
from shootblues.common.service import forceStart, forceStop
import service
import json
import base
import uix
import blue

prefs = {}
serviceInstance = None
serviceRunning = False

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)

RepairTypes = {
    "shield": {
        "groupName": "Shield Booster",
        "attributeName": "shieldBonus"
    },
    "armor": {
        "groupName": "Armor Repair Unit",
        "attributeName": "armorDamageAmount"
    },
    "structure": {
        "groupName": "Hull Repair Unit",
        "attributeName": "structureDamageAmount"
    }
}

class ActiveTankerSvc:
    def __init__(self):
        self.disabled = False
        self.__updateTimer = SafeTimer(250, self.updateHealth)
    
    def updateHealth(self):
        if self.disabled:
            self.__updateTimer = None
            return

        if not isLoggedIn():
            return
        
        ship = eve.LocalSvc("godma").GetItem(eve.session.shipid)
        if not ship:
            return
        
        self.shieldMax = ship.shieldCapacity
        self.shield = ship.shieldCharge
        self.armorMax = ship.armorHP
        self.armor = ship.armorHP - ship.armorDamage
        self.structureMax = ship.hp
        self.structure = ship.hp - ship.damage
        
        if getPref("KeepShieldsFull", False):
            self.repairIfNeeded("shield")
        
        if getPref("KeepArmorFull", False):
            self.repairIfNeeded("armor")
        
        if getPref("KeepStructureFull", False):
            self.repairIfNeeded("structure")
    
    def repairIfNeeded(self, repairType):
        module = self.findRepairModule(repairType)
        if module:
            attributeName = RepairTypes[repairType]["attributeName"]
            repairAmount = getModuleAttributes(module)[attributeName]
            threshold = getattr(self, repairType + "Max") - repairAmount
            current = getattr(self, repairType)
            if current < threshold:
                activated, reason = activateModule(module, pulse=True)
    
    def findRepairModule(self, repairType):
        groupName = RepairTypes.get(repairType, {}).get("groupName", None)
        if not groupName:
            log("Invalid repair module type: %r", repairType)
            return None
        
        return findModule(groupNames=[groupName])

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("activetanker", ActiveTankerSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("activetanker")
        serviceRunning = False
