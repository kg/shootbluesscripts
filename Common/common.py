import shootblues
import types
import traceback
import pprint

__channels = {}

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

def getLoggedInCharacter():
    try:
        return eve.session.charid
    except:
        return None

def getLoggedInCharacterName():
    try:
        return cfg.eveowners.Get(eve.session.charid).name
    except:
        return None
    
def getLockedTargets():
    ballpark = eve.LocalSvc("michelle").GetBallpark()
    targetSvc = sm.services["target"]
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]

def forceStopService(serviceName):
    serviceInstance = sm.services[serviceName]
    del sm.services[serviceName]
    
    for event in serviceInstance.__notifyevents__:
        notifies = sm.notify.get(event, None)
        if notifies is None:
            continue
        
        if serviceInstance in notifies:
            notifies.remove(serviceInstance)

def forceStartService(serviceName, serviceType):
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

def replaceEveLogger():
    global oldLogException, oldLogTraceback
    import log
    oldLogException = log.LogException
    oldLogTraceback = log.LogTraceback
    log.LogException = logException
    log.LogTraceback = logTraceback
    
replaceEveLogger()

def __unload__():
    global oldLogException, oldLogTraceback
    import log
    log.LogException = oldLogException
    log.LogTraceback = oldLogTraceback