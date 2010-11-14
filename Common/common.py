import shootblues
import types

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