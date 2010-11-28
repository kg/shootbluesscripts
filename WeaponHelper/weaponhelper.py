import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, getFlagName, getNamesOfIDs
from shootblues.common.service import forceStart, forceStop
import service
import json
import base
import uix
import blue

prefs = {}
serviceInstance = None

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

class WeaponHelperSvc(service.Service):
    __guid__ = "svc.weaponhelper"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.disabled = False
        self.__updateTimer = SafeTimer(1000, self.updateWeapons)
        self.__lastAction = 0
    
    def updateWeapons(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        ship = eve.LocalSvc("godma").GetItem(eve.session.shipid)
        if not ship:
            return

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("weaponhelper", WeaponHelperSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("weaponhelper")
        serviceRunning = False
