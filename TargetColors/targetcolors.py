from shootblues.common import forceStartService, forceStopService, log, SafeTimer, MainThreadInvoker, getFlagName, getNamesOfIDs
import service
import json
import blue
import uthread
import math

PulseRate = 500.0
DefaultDuration = 12000.0
Alphas = {
    "icon": 1.0,
    "framesprite": 0.5,
    "fill": 0.13,
    "frame": 0.5,
    "corner": 0.25,
    "simplepic": 0.9
}

namedColors = {}
itemColors = {}
pendingFlashes = {}

def notifyColorsChanged(newColorsJson):
    global namedColors
    
    namedColors = json.loads(newColorsJson)

def getNamedColor(name):
    global namedColors
    
    if name:
        return namedColors.get(name, None)
        
def getItemColor(itemID):
    global itemColors
    
    colorName = itemColors.get(itemID, None)
    return getNamedColor(colorName)

def setItemColor(itemID, name):
    global itemColors
    
    if name:
        itemColors[itemID] = name
    elif itemColors.has_key(itemID):
        del itemColors[itemID]

def flashItemColor(itemID, name):
    global pendingFlashes
    
    pendingFlashes[itemID] = {
        "name": name,
        "started": blue.os.GetTime()
    }

class TargetColorsSvc(service.Service):
    __guid__ = "svc.targetcolors"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnTargets",
        "OnTarget"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.disabled = False
        self.__updateTimer = SafeTimer(1000, self.updateTargets)
        self.__activeBlinks = {}
    
    def setObjectColor(self, obj, color=None, alpha=1.0):
        global Alphas
    
        name = getattr(obj, "name", None)
        if hasattr(obj, "color"):
            baseAlpha = Alphas.get(name, 1.0)
            
            if color:            
                obj.color.SetRGB(
                    1.0 + (color[0] - 1.0) * alpha,
                    1.0 + (color[1] - 1.0) * alpha,
                    1.0 + (color[2] - 1.0) * alpha,
                    baseAlpha
                )
            else:
                obj.color.SetRGB(1.0, 1.0, 1.0, baseAlpha)
        
        if hasattr(obj, "children"):
            for child in obj.children:
                self.setObjectColor(child, color, alpha)
    
    def _blinkThread(self, obj):
        try:
            while True:
                if self.disabled:
                    break
            
                info = self.__activeBlinks.get(obj, None)
                if not info:
                    break
                
                startTime = info["started"]
                duration = info["duration"]
                color = info["color"]
                
                elapsed = blue.os.TimeDiffInMs(startTime)                
                alpha = math.sin(((elapsed % PulseRate) / PulseRate) * math.pi)
                self.setObjectColor(obj, color, alpha)
                
                if elapsed > duration:
                    break
            
                blue.pyos.synchro.Yield()
        finally:
            if obj in self.__activeBlinks:
                del self.__activeBlinks[obj]
    
    def updateTargets(self):
        global pendingFlashes
    
        if self.disabled:
            self.__updateTimer = None
            return
    
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return
               
        for id in targetSvc.targets:               
            targetFrame = targetSvc.targetsByID.get(id, None)
            if not targetFrame:
                continue
            if not hasattr(targetFrame, "sr"):
                continue
            
            color = getItemColor(id)
            
            if hasattr(targetFrame.sr, "iconPar"):
                obj = targetFrame.sr.iconPar
                
                blinkActive = self.__activeBlinks.has_key(obj)
                
                flashColor = None
                flash = pendingFlashes.get(id, None)
                if flash:
                    del pendingFlashes[id]
                    flashColor = getNamedColor(flash["name"])
                    needStart = not blinkActive
                    self.__activeBlinks[obj] = {
                        "started": flash["started"],
                        "duration": DefaultDuration,
                        "color": flashColor
                    }
                    
                    if needStart:
                        uthread.new(self._blinkThread, obj)
                    
                    continue
                
                if blinkActive:
                    continue
                
                if color:
                    self.setObjectColor(obj, color)
                else:
                    self.setObjectColor(obj)
    
    def OnTargets(self, targets):
        self.updateTargets()
    
    def OnTarget(self, what, tid=None, reason=None):
        self.updateTargets()

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("targetcolors", TargetColorsSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("targetcolors")
        serviceRunning = False