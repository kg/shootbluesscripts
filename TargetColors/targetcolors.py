from shootblues.common import forceStartService, forceStopService, log, SafeTimer, MainThreadInvoker, getFlagName, getNamesOfIDs
import service
import json

DefaultColor = (1, 1, 1, 1)

namedColors = {}
itemColors = {}

def notifyColorsChanged(newColorsJson):
    global namedColors
    
    namedColors = json.loads(newColorsJson)

def getNamedColor(name):
    global namedColors
    
    if name:
        return namedColors.get(name, DefaultColor)
    else:
        return DefaultColor

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
    
    def updateTargets(self):
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
            
            if hasattr(targetFrame.sr, "label"):            
                targetFrame.sr.label.color.SetRGB(*color)
            if hasattr(targetFrame.sr, "iconPar"):
                for obj in targetFrame.sr.iconPar.children:
                    if hasattr(obj, "color"):
                        obj.color.SetRGB(*color)
    
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