import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import json
import base

prefs = {}
serviceInstance = None

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)        

class ActiveTankerSvc(service.Service):
    __guid__ = "svc.activetanker"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnDamageMessage",
        "OnDamageMessages",
        "OnSessionChanged",
        "OnWarpFinished"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.__updateTimer = None
        self.disabled = False
    
    def checkUpdateTimer(self):
        if self.disabled:
            self.__updateTimer = None
        else:
            self.__updateTimer = base.AutoTimer(2000, self.updateHealth)
    
    def updateHealth(self):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        damageState = ballpark.GetDamageState(eve.session.shipid)
        if not damageState:
            return
        (self.shield, self.armor, self.structure) = damageState
        
        needShieldRepair = False
        needArmorRepair = False
        
        if getPref("RepairShields", True):
            threshold = float(getPref("RepairShieldThreshold", 80)) / 100.0
            if self.shield < threshold:
                needShieldRepair = True
        
        if getPref("RepairArmor", True):
            threshold = float(getPref("RepairArmorThreshold", 96)) / 100.0
            if self.armor < threshold:
                needArmorRepair = True
        
        if needShieldRepair:
            module = self.findModule("shield")
            if module:
                self.pulseModule(module)
        
        if needArmorRepair:
            module = self.findModule("armor")
            if module:
                self.pulseModule(module)
    
    def findModule(self, repairType):
        if repairType == "shield":
            groupName = "Shield Booster"
        elif repairType == "armor":
            groupName = "Armor Repair Unit"
        elif repairType == "structure":
            groupName = "Hull Repair Unit"
        else:
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
    
    def pulseModule(self, module):
        moduleInfo = module.sr.moduleInfo
        moduleName = cfg.invtypes.Get(moduleInfo.typeID).name
   
        def_effect = getattr(module, "def_effect", None)
        if not def_effect:
            log("Module %r has no default effect", moduleName)
            return
        
        if def_effect.isActive:
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
