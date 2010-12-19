import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, getFlagName, getNamesOfIDs, findModules, getModuleAttributes, getTypeAttributes, activateModule, canActivateModule, ChanceToHitCalculator
from shootblues.common.service import forceStart, forceStop
import service
import json
import base
import uix
import blue
import foo
import math
import uthread
from util import Memoized

prefs = {}
serviceInstance = None
serviceRunning = False

ChanceToHitBias = 0.5

WeaponGroupNames = [
    "Energy Weapon", "Hybrid Weapon", 
    "Missile Launcher", "Missile Launcher Assault", 
    "Missile Launcher Bomb", "Missile Launcher Citadel", 
    "Missile Launcher Cruise", "Missile Launcher Defender", 
    "Missile Launcher Heavy", "Missile Launcher Heavy Assault", 
    "Missile Launcher Rocket", "Missile Launcher Siege", 
    "Missile Launcher Standard", "Projectile Weapon"
]

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)

try:
    from shootblues.enemyprioritizer import getPriority
except:
    def getPriority(*args, **kwargs):
        return 0

class WeaponHelperSvc:
    def __init__(self):
        self.disabled = False
        self.__updateTimer = SafeTimer(250, self.updateWeapons)
        self.__ammoChanges = {}
        self.__hookedMethods = []
        self.__lastAttackOrder = None
    
    def getTargetSorter(self, module):
        godma = eve.LocalSvc("godma")
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        
        gp = Memoized(getPriority)
        
        cthc = ChanceToHitCalculator(eve.session.shipid)
        cthc.setModule(module)
        chanceToHitGetter = Memoized(cthc.calculate)
        
        def targetSorter(lhs, rhs):        
            # Highest priority first
            priLhs = gp(lhs)
            priRhs = gp(rhs)
            result = cmp(priRhs, priLhs)
            
            if result == 0:
                # Highest chance to hit first, but bias the chance to hit up for things we previously attacked
                cthLhs = chanceToHitGetter(lhs)
                if lhs is self.__lastAttackOrder:
                    cthLhs += ChanceToHitBias
                cthRhs = chanceToHitGetter(rhs)
                if rhs is self.__lastAttackOrder:
                    cthRhs += ChanceToHitBias
                result = cmp(cthRhs, cthLhs)
        
            return result
                
        return targetSorter
    
    def filterTargets(self, ids):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        result = []
        
        for id in ids:
            invItem = ballpark.GetInvItem(id)
            if not invItem:
                continue
                
            if getFlagName(id) != "HostileNPC":
                continue
            
            if getPriority(id) < 0:
                continue
                
            result.append(id)
        
        return result
    
    def selectTarget(self, module):
        targets = self.filterTargets(sm.services["target"].targets)
        if len(targets):
            targetSorter = self.getTargetSorter(module)
            targets.sort(targetSorter)
            
            return targets[0]
        else:
            return None
    
    def ensureModuleHooked(self, module):
        if hasattr(module, "ChangeAmmo") and getattr(module.ChangeAmmo, "shootblues", 0) != 420:
            _oldChangeAmmo = module.ChangeAmmo            
            def myChangeAmmo(itemID, quantity):
                if not canActivateModule(module)[0]:
                    self.__ammoChanges[module] = (_oldChangeAmmo, itemID, quantity)
                else:
                    return _oldChangeAmmo(itemID, quantity)
            
            setattr(myChangeAmmo, "shootblues", 420)
            module.ChangeAmmo = myChangeAmmo
            
            self.__hookedMethods.append((module, "ChangeAmmo", _oldChangeAmmo))
        
        if hasattr(module, "ChangeAmmoType") and getattr(module.ChangeAmmoType, "shootblues", 0) != 420:
            _oldChangeAmmoType = module.ChangeAmmoType
            def myChangeAmmoType(typeID, singleton):
                if not canActivateModule(module)[0]:
                    self.__ammoChanges[module] = (_oldChangeAmmoType, typeID, singleton)
                else:
                    return _oldChangeAmmoType(typeID, singleton)
            
            setattr(myChangeAmmoType, "shootblues", 420)
            module.ChangeAmmoType = myChangeAmmoType
            
            self.__hookedMethods.append((module, "ChangeAmmoType", _oldChangeAmmoType))
    
    def unhookModules(self):
        m = self.__hookedMethods
        self.__hookedMethods = []
        
        for (obj, name, old) in m:
            try:
                setattr(obj, name, old)
            except:
                pass
        
    def updateWeapons(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        weaponModules = findModules(groupNames=WeaponGroupNames)
        
        for moduleID, module in weaponModules.items():
            if hasattr(module, "ChangeAmmo"):
                self.ensureModuleHooked(module)
            
            if canActivateModule(module)[0]:
                uthread.pool("DoUpdateModule", self.doUpdateModule, moduleID, module)
    
    def doUpdateModule(self, moduleID, module):
        ammoChange = self.__ammoChanges.get(module, None)
        if ammoChange:
            del self.__ammoChanges[module]
            fn = ammoChange[0]
            args = tuple(ammoChange[1:])
            log("Changing ammo")
            fn(*args)
        else:        
            targetID = self.selectTarget(module)
            if targetID:
                self.__lastAttackOrder = targetID
                activated, reason = activateModule(module, pulse=True, targetID=targetID)

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("weaponhelper", WeaponHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.unhookModules()
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("weaponhelper")
        serviceRunning = False
