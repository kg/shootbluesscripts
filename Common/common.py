import shootblues
import types
import traceback
import pprint

__channels = {}

isInitialized = False

def _initChannel(name, handle):
    __channels[name] = shootblues.createChannel(handle)

def getChannel(name):
    return __channels[name]

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
    import json
    channel = getChannel("remotecall")
    channel.send(json.dumps([script, methodName, args]))

def logException(*args, **kwargs):
    global oldLogException
    try:
        log("An unhandled exception was thrown and I stopped it from being reported.")
    except:
        pass
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
        log("I stopped a traceback from being reported.")
    except:
        pass
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

def forceStopService(serviceName):
    import stackless
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        serviceInstance = sm.services[serviceName]
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

def replaceEveLogger():
    global oldLogException, oldLogTraceback
    import log
    oldLogException = log.LogException
    oldLogTraceback = log.LogTraceback
    log.LogException = logException
    log.LogTraceback = logTraceback

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
                characterName = None
                try:
                    characterName = cfg.eveowners.Get(session.charid).name
                except:
                    pass
                remoteCall("common.script.dll", "LoggedInCharacterChanged", characterName)
    
    svcInstance = forceStartService("charmonitor", CharacterMonitorSvc)
    svcInstance.OnSessionChanged(False, eve.session, None)

def initialize():
    global isInitialized
    if not isInitialized:
        replaceEveLogger()
        installCharacterMonitor()
        isInitialized = True

def __unload__():
    global isInitialized
    if isInitialized:
        forceStopService("charmonitor")
        global oldLogException, oldLogTraceback
        import log
        log.LogException = oldLogException
        log.LogTraceback = oldLogTraceback
        isInitialized = False