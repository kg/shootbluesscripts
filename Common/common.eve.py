from . import log, onMainThread

ActionThreshold = 20000000L

def getNamesOfIDs(ids):
    import uix

    ballpark = eve.LocalSvc("michelle").GetBallpark()
    if not ballpark:
        return [str(id) for id in ids]
    
    def getName(id):
        slimItem = ballpark.GetInvItem(id)
        if not slimItem:
            return str(id)
        return uix.GetSlimItemName(slimItem)
    
    names = [getName(id) for id in ids]
    result = []
    
    for name in set(names):
        count = names.count(name)
        if count > 1:
            result.append("%s x%d" % (name, count))
        else:
            result.append(name)
    
    return result
                
def getFlagName(slimItem):
    validCategories = [const.categoryShip, const.categoryDrone, const.categoryEntity]
    if slimItem.categoryID not in validCategories:
        return None

    stateSvc = eve.LocalSvc("state")
    props = stateSvc.GetProps()
    
    flag = stateSvc.CheckStates(slimItem, "flag")
    if flag:
        flagProps = props.get(flag, None)
        if flagProps:
            return flagProps[1]
    
    colorFlag = 0
    if slimItem.typeID:
        itemType = eve.LocalSvc("godma").GetType(slimItem.typeID)
        for attr in itemType.displayAttributes:
            if attr.attributeID == const.attributeEntityBracketColour:
                if attr.value == 1:
                    return "HostileNPC"
                elif attr.value == 0:
                    return "NeutralNPC"
    
    return None

def getCharacterName(charID):
    if not charID:
        return None
    
    char = cfg.eveowners.Get(charID)
    if char:
        return char.name

def findModule(groupName=None, groupID=None, typeName=None, typeID=None):   
    shipui = uicore.GetLayer("l_shipui")
    godma = eve.LocalSvc("godma")
    
    if not shipui:
        return None
    if not getattr(shipui, "sr", None):
        return
    if not getattr(shipui.sr, "modules", None):
        return 
                
    for moduleId, module in shipui.sr.modules.items():
        item = godma.GetItem(moduleId)
        if not item:
            continue
        
        if groupID and (item.groupID != groupID):
            continue
        if typeID and (item.typeID != typeID):
            continue
        if groupName and (cfg.invgroups.Get(item.groupID).name != groupName):
            continue
        if typeName and (cfg.invtypes.Get(item.typeID).name != typeName):
            continue
        
        if not hasattr(module, "sr"):
            continue
        
        return module
    
    return None

def getModuleAttributes(module):
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

def activateModule(module, pulse=False, actionThreshold=ActionThreshold):
    import blue
    import uix
    
    moduleInfo = module.sr.moduleInfo
    moduleName = cfg.invtypes.Get(moduleInfo.typeID).name

    def_effect = getattr(module, "def_effect", None)
    if not def_effect:
        log("Module %s cannot be activated", moduleName)
        return
    
    if def_effect.isActive:
        return
    
    if module.state == uix.UI_DISABLED:
        log("Module %s is disabled", moduleName)
        return
    
    onlineEffect = moduleInfo.effects.get("online", None)
    if onlineEffect and not onlineEffect.isActive:
        log("Module %s is not online", moduleName)
        return
    
    timestamp = blue.os.GetTime()
    lastAction = int(getattr(module, "__last_action__", 0))
    if (ActionThreshold is not None and 
        abs(lastAction - timestamp) <= ActionThreshold):
        return
    
    setattr(module, "__last_action__", timestamp)
    log("Activating %s", moduleName)    
    
    oldautorepeat = getattr(module, "autorepeat", False)
    if oldautorepeat:
        # Temporarily disable auto-repeat for this module so that we can just pulse it once
        if pulse:
            repeatCount = 0
        else:
            repeatCount = 9999
        module.SetRepeat(repeatCount)
            
    try:
        module.Click()
    except Exception, e:
        log("Activating module %s failed: %r", moduleName, e)
    finally:
        if oldautorepeat:
            module.SetRepeat(oldautorepeat)

class MainThreadInvoker(object):
    __notifyevents__ = [
        "OnStateSetupChance",
        "OnDamageMessage",
        "OnDamageMessages",
        "DoBallsAdded",
        "DoBallRemove",
        "OnTarget",
        "OnTargets",
        "OnStateChange",
        "ProcessShipEffect",
        "OnLSC",
        "OnSessionChanged"
    ]
    
    def __init__(self, handler):
        self.__handler = handler
        sm.RegisterNotify(self)
    
    def doInvoke(self):
        handler = self.__handler
        if handler:
            self.dispose()
            handler()
    
    def dispose(self):
        if self.__handler:
            sm.UnregisterNotify(self)    
        self.__handler = None
    
    def DoBallsAdded(self, lst, **kwargs):
        self.doInvoke()
    
    def DoBallRemove(self, ball, slimItem, *args, **kwargs):
        self.doInvoke()
    
    def DoBallClear(self, solItem, **kwargs):
        self.doInvoke()
        
    def ProcessShipEffect(self, godmaStm, effectState):
        self.doInvoke()
        
    def OnDamageMessage(self, key, args):
        self.doInvoke()

    def OnDamageMessages(self, msgs):
        self.doInvoke()
        
    def OnStateSetupChance(self, what):
        self.doInvoke()
        
    def OnLSC(self, channelID, estimatedMemberCount, method, who, args):
        self.doInvoke()
        
    def OnSessionChanged(self, isRemote, session, change):
        self.doInvoke()
    
    def OnTargets(self, targets):
        self.doInvoke()
    
    def OnTarget(self, what, tid=None, reason=None):
        self.doInvoke()
            
class SafeTimer(object):
    __notifyevents__ = [
        "OnSessionChanged"
    ]
        
    def __init__(self, interval, handler):
        self.__interval = interval
        self.__handler = handler
        self.__timer = None
        self.__started = True
        self.__invoker = None
        
        self.syncState()
        sm.RegisterNotify(self)
    
    def start(self):
        if self.__started:
            return
        
        self.__started = True
        self.resetState()
           
    def stop(self):
        self.__started = False
        self.__timer = None
        self.__invoker = None
    
    def getName(self):
        func = self.__handler
        if hasattr(func, "im_func"):
            func = func.im_func
        
        return getattr(func, "func_name", "<unknown>")
        
    def syncState(self):
        self.__invoker = None
        
        if onMainThread():
            if self.__started and (not self.__timer):
                import base
                self.__timer = base.AutoTimer(self.__interval, self.tick)
            elif (not self.__started) and self.__timer:
                self.__timer = None
        else:
            self.__invoker = MainThreadInvoker(self.syncState)
    
    def tick(self):
        try:
            self.__handler()
        except:
            log("SafeTimer %r temporarily disabled due to error", self.getName())
            self.__timer = None                
            self.__invoker = MainThreadInvoker(self.syncState)
            
            raise