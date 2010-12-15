from shootblues.common import log, onMainThread

ActionThreshold = (10000000L * 200) / 100
MainThreadQueue = []
MainThreadQueueRunning = False
MainThreadQueueInvoker = None
MainThreadQueueItemDelay = 1
MainThreadQueueInterval = 50
TimerErrorDelay = 2000

def getNamesOfIDs(ids):
    import uix

    godma = eve.LocalSvc("godma")
    ballpark = eve.LocalSvc("michelle").GetBallpark()
    if (not ballpark) and (not godma):
        return [str(id) for id in ids]
    
    def getName(id):
        if ballpark:
            slimItem = ballpark.GetInvItem(id)        
            if slimItem:
                return uix.GetSlimItemName(slimItem)
            
        if godma:
            godmaItem = godma.GetItem(id)
            if godmaItem:
                invtype = cfg.invtypes.Get(godmaItem.typeID)                
                return invtype.name

        return repr(id)
    
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

def isPlayerJumping():
    return ("jumping" in eve.session.sessionChangeReason) and (eve.session.changing)

def isBallWarping(ball):
    import destiny
    return ball.mode == destiny.DSTBALL_WARP

def isBallCloaked(ball):
    return bool(getattr(ball, "isCloaked", 0))

def isBallTargetable(ball):        
    from common.eve.state import isItemInsideForceField
    if ball is None:
        return False
    elif isBallWarping(ball):
        return False
    elif isItemInsideForceField(ball.id):
        return False
    else:
        return True

def findModule(groupNames=None, groupIDs=None, typeNames=None, typeIDs=None):
    modules = findModules(
        count=1, 
        groupNames=groupNames, groupIDs=groupIDs, 
        typeNames=typeNames, typeIDs=typeIDs
    )
    
    if modules and len(modules):
        return modules.values()[0]
    else:
        return None

def findModules(count=9999, groupNames=None, groupIDs=None, typeNames=None, typeIDs=None):
    shipui = uicore.GetLayer("l_shipui")
    godma = eve.LocalSvc("godma")
    
    if not shipui:
        return {}
    if not getattr(shipui, "sr", None):
        return {}
    if not getattr(shipui.sr, "modules", None):
        return {}
    
    results = {}
    for moduleID, module in shipui.sr.modules.items():
        item = godma.GetItem(moduleID)
        if not item:
            continue
        
        if groupIDs and (item.groupID not in groupIDs):
            continue
        if typeIDs and (item.typeID not in typeIDs):
            continue
        if groupNames and (cfg.invgroups.Get(item.groupID).name not in groupNames):
            continue
        if typeNames and (cfg.invtypes.Get(item.typeID).name not in typeNames):
            continue
        
        if not hasattr(module, "sr"):
            continue
        
        results[moduleID] = module
        if len(results) >= count:
            break
    
    return results

def getModuleAttributes(module):
    moduleInfo = module.sr.moduleInfo
    moduleObj = eve.LocalSvc("godma").GetItem(moduleInfo.itemID)
    return getTypeAttributes(moduleInfo.typeID, obj=moduleObj)

def getCharAttributes(charID):
    char = eve.LocalSvc("godma").GetItem(charID)
    return getTypeAttributes(char.typeID, obj=char)

def getShipAttributes(shipID):
    ship = eve.LocalSvc("godma").GetItem(shipID)
    return getTypeAttributes(ship.typeID, obj=ship)
    
def getTypeAttributes(typeID, obj=None):
    result = {}
    
    attribs = cfg.dgmtypeattribs.get(typeID, None)
    if not attribs:
        return result
            
    for attrib in attribs:
        attribName = cfg.dgmattribs.get(attrib.attributeID, None).attributeName
        value = attrib.value
        if obj:
            value = getattr(obj, attribName, value)
        result[attribName] = value
    
    return result

def canActivateModule(module, targetID=None):
    import uix
    from common.eve.state import isItemInsideForceField
    
    moduleInfo = module.sr.moduleInfo
    
    ballpark = eve.LocalSvc("michelle").GetBallpark()
    if not ballpark:
        return (False, "No ballpark")
        
    playerBall = ballpark.GetBall(eve.session.shipid)
    
    if not playerBall:
        return (False, "Player ball nonpresent")
    elif isBallCloaked(playerBall):
        return (False, "Player is cloaked")
    elif isPlayerJumping():
        return (False, "Player is jumping")
    elif isItemInsideForceField(eve.session.shipid):
        return (False, "Player is inside a force field")
    
    if targetID:
        targetBall = ballpark.GetBall(targetID)
        if not targetBall:
            return (False, "Target ball not in ballpark")            
        elif isBallWarping(targetBall):
            return (False, "Target is warping")            
        elif isItemInsideForceField(targetBall.id):
            return (False, "Target is inside a force field")
    else:
        targetBall = None
    
    def_effect = getattr(module, "def_effect", None)
    if not def_effect:
        return (False, "passive module")
    
    if def_effect.isActive:
        return (False, "already active")
    
    if bool(getattr(module, "goingOnline", False)):
        return (False, "module going online")
    
    if bool(getattr(module, "effect_activating", False)):
        return (False, "module activating")
    
    onlineEffect = moduleInfo.effects.get("online", None)
    if onlineEffect and not onlineEffect.isActive:
        return (False, "module offline")
        
    if bool(getattr(module, "changingAmmo", False)):
        return (False, "module changing ammo")
        
    if bool(getattr(module, "reloadingAmmo", False)):
        return (False, "module reloading ammo")
        
    if getattr(module, "blockClick", 0) != 0:
        return (False, "module clicks blocked")
    
    if module.state == uix.UI_DISABLED:
        return (False, "button disabled")
    
    return (True, "")

def activateModule(module, pulse=False, targetID=None, actionThreshold=ActionThreshold):
    import blue
    import base
    import uix
        
    moduleInfo = module.sr.moduleInfo
    moduleName = cfg.invtypes.Get(moduleInfo.typeID).name
    
    canActivate = canActivateModule(module, targetID=targetID)
    if not canActivate[0]:
        return canActivate
    
    def_effect = getattr(module, "def_effect", None)
    
    timestamp = blue.os.GetTime()
    lastAction = int(getattr(module, "__last_action__", 0))
    if (ActionThreshold is not None and 
        abs(lastAction - timestamp) <= ActionThreshold):
        return (False, "too soon to activate again (lag protection)")
    
    setattr(module, "__last_action__", timestamp)
    
    oldautorepeat = getattr(module, "autorepeat", False)
    if oldautorepeat:
        # Temporarily disable auto-repeat for this module so that we can just pulse it once
        if pulse:
            repeatCount = 0
        else:
            repeatCount = 1000 # Not sure why it's this instead of 1 or true
        module.SetRepeat(repeatCount)    
            
    try:
        module.activationTimer = base.AutoTimer(500, module.ActivateEffectTimer)
        module.effect_activating = 1
        
        module.ActivateEffect(
            effect=def_effect, 
            targetID=targetID
        )
        return (True, None)
    except Exception, e:
        log("Activating module %s failed: %r", moduleName, e)
        return (False, "error")
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
        "OnSessionChanged",
        "OnClientReady",
        "OnJukeboxChange"
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
    
    def OnClientReady(self, *args, **kwargs):
        self.doInvoke()
    
    def OnJukeboxChange(self, *args, **kwargs):
        self.doInvoke()

def _mainThreadQueueFunc():
    import blue
    import uthread
    
    global MainThreadQueueRunning, MainThreadQueue, MainThreadQueueInvoker, MainThreadQueueInterval, MainThreadQueueItemDelay
    
    MainThreadQueueInvoker = None
    MainThreadQueueRunning = True
    while MainThreadQueueRunning:
        item = None
        if len(MainThreadQueue):
            item = MainThreadQueue.pop(0)
        
        if item:
            fn, args, kwargs = item
            
            uthread.pool("MTQ", fn, *args, **kwargs)
            blue.pyos.synchro.Sleep(MainThreadQueueItemDelay)
        else:        
            blue.pyos.synchro.Sleep(MainThreadQueueInterval)

def runOnMainThread(fn, *args, **kwargs):
    import uthread
    
    global MainThreadQueueRunning, MainThreadQueue, MainThreadQueueInvoker
    item = (fn, args, kwargs)
    
    if item not in MainThreadQueue:
        MainThreadQueue.append(item)
    
    if (not MainThreadQueueRunning) and (not MainThreadQueueInvoker):  
        def startMainThreadQueue():
            uthread.pool("MTQ", _mainThreadQueueFunc)
        
        MainThreadQueueInvoker = MainThreadInvoker(startMainThreadQueue)
            
class SafeTimer(object):
    def __init__(self, interval, handler):
        self.__interval = interval
        self.__handler = handler
        self.__timer = None
        self.__started = True
        
        self.syncState()
    
    def start(self):
        if self.__started:
            return
        
        self.__started = True
        self.resetState()
           
    def stop(self):
        self.__started = False
        self.__timer = None
    
    def getName(self):
        func = self.__handler
        if hasattr(func, "im_func"):
            func = func.im_func
        
        return getattr(func, "func_name", "<unknown>")
        
    def syncState(self):
        if onMainThread():
            if self.__started and (not self.__timer):
                import base
                self.__timer = base.AutoTimer(self.__interval, self.tick)
            elif (not self.__started) and self.__timer:
                self.__timer = None
        else:
            runOnMainThread(self.syncState)
    
    def delayedSyncState(self):
        import blue
        global TimerErrorDelay
        blue.pyos.synchro.Sleep(TimerErrorDelay)
        self.syncState()
    
    def tick(self):
        try:
            self.__handler()
        except:
            log("SafeTimer %r temporarily disabled due to error", self.getName())
            self.__timer = None
            runOnMainThread(self.delayedSyncState)
            
            raise

def __unload__():
    global MainThreadQueue, MainThreadQueueRunning, MainThreadQueueInvoker
    MainThreadQueueRunning = False
    MainThreadQueueInvoker = None
    MainThreadQueue = []