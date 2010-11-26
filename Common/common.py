import shootblues
import types
import traceback
import pprint
import threading
import json

__channels = {}

isInitialized = False

def _initChannel(name, handle):
    __channels[name] = shootblues.createChannel(handle)

def getChannel(name):
    return __channels.get(name, None)

def log(format, *args):
    logger = getChannel("log")
    if logger:
        if args:
            logger.send(str(format) % args)
        elif isinstance(format, types.StringType):
            logger.send(format)
        else:
            logger.send(repr(format))

def remoteCall(script, methodName, *args):
    channel = getChannel("remotecall")
    channel.send(json.dumps([script, methodName, args]))

def explorerResolveKey(obj, key):
    if key.startswith("["):
        indexer = json.loads(key)[0]
        return obj[indexer]
    else:
        return getattr(obj, key)

def explorerResolveContext(context, index):
    import sys
    obj = sys.modules.get(context[0])
    currentKey = []
    for key in context[1:index+1]:
        nextObj = explorerResolveKey(obj, key)       
        currentKey.append(key)
        obj = nextObj
    
    return obj

def explorerGetKeys(context, index):
    obj = explorerResolveContext(context, index)
    result = dir(obj)
    
    if isinstance(obj, types.ListType):
        for i in xrange(len(obj)):
            result.append(json.dumps([i]))
    elif isinstance(obj, types.DictType):
        for key in obj.iterkeys():
            result.append(json.dumps([key]))
    elif hasattr(obj, "__keys__"):
        for key in getattr(obj, "__keys__"):
            result.append(json.dumps([key]))            
    
    return result
    
def explorerRepr(value, maxLength=512):
    result = repr(value)
    if len(result) > maxLength:
        result = result[0:maxLength - 3] + "..."
    return result

def explorerGetValues(context, index, keys):
    obj = explorerResolveContext(context, index)
    result = []
    
    for key in keys:
        value = None
        try:
            value = explorerResolveKey(obj, key)
        except Exception, e:
            value = e
        
        if value is not None:
            value = explorerRepr(value)
        else:
            value = ""
        
        result.append(value)
    
    return result

def logException(*args, **kwargs):
    global oldLogException
    try:
        if len(args) > 3:
            args[3] = False
        if len(args) > 4:
            args[4] = False
        if ('toLogServer' in kwargs) or len(args) <= 3:
            kwargs['toLogServer'] = False
        if ('toAlertSvc' in kwargs) or len(args) <= 4:
            kwargs['toAlertSvc'] = False
        oldLogException(*args, **kwargs)
    except:
        pass

def logTraceback(*args, **kwargs):
    global oldLogTraceback
    try:
        if len(args) > 3:
            args[3] = False
        if len(args) > 4:
            args[4] = False
        if ('toAlertSvc' in kwargs) or len(args) <= 3:
            kwargs['toAlertSvc'] = False
        if ('toLogServer' in kwargs) or len(args) <= 4:
            kwargs['toLogServer'] = False
        oldLogTraceback(*args, **kwargs)
    except:
        pass
    
def getLockedTargets():
    ballpark = eve.LocalSvc("michelle").GetBallpark()
    targetSvc = sm.services["target"]
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]

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

def forceStopService(serviceName):
    import stackless
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        serviceInstance = sm.services.get(serviceName, None)
        if serviceInstance:
            del sm.services[serviceName]
            
            for event in serviceInstance.__notifyevents__:
                notifies = sm.notify.get(event, None)
                if notifies is None:
                    continue
                
                if serviceInstance in notifies:
                    notifies.remove(serviceInstance)
    finally:
        stackless.getcurrent().block_trap = old_block_trap

def forceStartService(serviceName, serviceType):
    import stackless
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        import service
        oldInstance = sm.services.get(serviceName, None)
        if oldInstance:
            forceStopService(serviceName)
        
        result = serviceType()
        sm.services[serviceName] = result
        result.state = service.SERVICE_RUNNING
        
        for event in result.__notifyevents__:
            empty_list = []
            notifies = sm.notify.setdefault(event, empty_list)
            notifies.append(result)
            if (not hasattr(result, event)):
                log("Missing event handler for %r on %r", event, result)
        
        return result
    finally:
        stackless.getcurrent().block_trap = old_block_trap
                
def getFlagName(slimItem):
    if (slimItem.categoryID != const.categoryEntity):
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

def replaceEveLogger():
    global oldLogException, oldLogTraceback
    import log
    oldLogException = log.LogException
    oldLogTraceback = log.LogTraceback
    log.LogException = logException
    log.LogTraceback = logTraceback

def onMainThread():
    try:
        thread = threading.currentThread()
        return isinstance(thread, threading._MainThread)
    except:
        return None

def installCharacterMonitor():
    import blue
    import service

    class CharacterMonitorSvc(service.Service):
        __guid__ = "svc.charmonitor"
        __update_on_reload__ = 0
        __exportedcalls__ = {}
        __notifyevents__ = [
            "OnSessionChanged"
        ]

        def __init__(self):
            service.Service.__init__(self)
            self.loggedInCharacter = None

        def OnSessionChanged(self, isRemote, session, change):
            if self.loggedInCharacter != session.charid:
                self.loggedInCharacter = session.charid
                characterName = getCharacterName(session.charid)
                remoteCall("common.script.dll", "LoggedInCharacterChanged", characterName)
    
    svcInstance = forceStartService("charmonitor", CharacterMonitorSvc)
    svcInstance.OnSessionChanged(False, eve.session, None)

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

def initialize():
    global isInitialized
    if not isInitialized:
        replaceEveLogger()
        installCharacterMonitor()
        isInitialized = True

def __unload__():
    global isInitialized, __channels
    for key in __channels:
        __channels[key] = None
    
    __channels = {}

    if isInitialized:
        forceStopService("charmonitor")
        global oldLogException, oldLogTraceback
        import log
        log.LogException = oldLogException
        log.LogTraceback = oldLogTraceback
        isInitialized = False