import blue 
import service
from shootblues import rpcSend

def log(format, *args):
    if args:
        rpcSend(str(format) % args)
    else:
        rpcSend(repr(format))

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
    
    log("Service force stopped: %r", serviceName)

def forceStartService(serviceName, serviceType):
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
            
    log("Service force started: %r", serviceName)
    return result