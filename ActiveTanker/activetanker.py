import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import json
import base
import uix

prefs = {}
serviceInstance = None

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

class ActiveTankerSvc(service.Service):
    __guid__ = "svc.activetanker"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnDamageMessage",
        "OnDamageMessages",
        "OnSessionChanged",
        "OnWarpFinished",
        "ProcessShipEffect"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.__updateTimer = None
        self.disabled = False
    
    def checkUpdateTimer(self):
        if self.disabled:
            self.__updateTimer = None
        elif self.__updateTimer is None:
            self.__updateTimer = base.AutoTimer(1000, self.updateHealth)
    
    def updateHealth(self):
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
        module = self.findModule(repairType)
        if module:
            attributeName = RepairTypes[repairType]["attributeName"]
            repairAmount = self.getModuleAttributes(module)[attributeName]
            threshold = getattr(self, repairType + "Max") - repairAmount
            current = getattr(self, repairType)
            if current < threshold:
                log("%s needs repair: %r < %r", repairType, current, threshold)
                self.pulseModule(module)
    
    def findModule(self, repairType):
        groupName = RepairTypes.get(repairType, {}).get("groupName", None)
        if not groupName:
            log("Invalid repair module type: %r", repairType)
            return None
        
        shipui = uicore.GetLayer("l_shipui")
        godma = eve.LocalSvc("godma")
        for moduleId, module in shipui.sr.modules.items():
            item = godma.GetItem(moduleId)
            if not item:
                continue
            
            if cfg.invgroups.Get(item.groupID).name != groupName:
                continue
            
            return module
        
        return None
    
    def getModuleAttributes(self, module):
        moduleInfo = module.sr.moduleInfo
        moduleName = cfg.invtypes.Get(moduleInfo.typeID).name
   
        def_effect = getattr(module, "def_effect", None)
        if not def_effect:
            log("Module %r has no default effect", moduleName)
            return None
        
        result = {}
        
        attribs = cfg.dgmtypeattribs.get(moduleInfo.typeID, None)
        if not attribs:
            return result
                
        for attrib in attribs:
            attribName = cfg.dgmattribs.get(attrib.attributeID, None).attributeName
            result[attribName] = attrib.value
        
        return result
    
    def pulseModule(self, module):
        moduleInfo = module.sr.moduleInfo
        moduleName = cfg.invtypes.Get(moduleInfo.typeID).name
   
        def_effect = getattr(module, "def_effect", None)
        if not def_effect:
            log("Module %r has no default effect", moduleName)
            return
        
        if def_effect.isActive:
            return
        
        if module.state == uix.UI_DISABLED:
            log("Module %r is disabled", moduleName)
            return
        
        onlineEffect = moduleInfo.effects.get("online", None)
        if onlineEffect and not onlineEffect.isActive:
            log("Module %r is not online", moduleName)
            return
        
        log("Activating module %r", moduleName)
        
        oldautorepeat = getattr(module, "autorepeat", False)
        if oldautorepeat:
            # Temporarily disable auto-repeat for this module so that we can just pulse it once
            module.SetRepeat(0)
        
        try:
            module.Click()
        finally:
            if oldautorepeat:
                module.SetRepeat(oldautorepeat)
    
    def ProcessShipEffect(self, godmaStm, effectState):
        self.checkUpdateTimer()
        
    def OnDamageMessage(self, key, args):
        self.checkUpdateTimer()

    def OnDamageMessages(self, msgs):
        self.checkUpdateTimer()
    
    def OnWarpFinished(self):
        self.checkUpdateTimer()
    
    def OnSessionChanged(self, isRemote, session, change):
        self.checkUpdateTimer()

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("activetanker", ActiveTankerSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("activetanker")
        serviceRunning = False
