from . import log

runningServices = {}

def get(serviceName):
    global runningServices
    
    result = runningServices.get(serviceName, None)
    if not result:
        result = sm.services.get(serviceName, None)
    
    return result

def forceStop(serviceName):
    global runningServices
    
    import stackless
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        serviceInstance = runningServices.get(serviceName, None)
        if serviceInstance:
            del runningServices[serviceName]
            ne = getattr(serviceInstance, "__notifyevents__", [])
            if len(ne):
                sm.UnregisterNotify(serviceInstance)
    finally:
        stackless.getcurrent().block_trap = old_block_trap

def forceStart(serviceName, serviceType):
    global runningServices
    
    import stackless
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        oldInstance = runningServices.get(serviceName, None)
        if oldInstance:
            forceStop(serviceName)
        
        result = serviceType()
        runningServices[serviceName] = result
        
        ne = getattr(result, "__notifyevents__", [])
        if len(ne):
            for event in ne:
                if (not hasattr(result, event)):
                    log("Missing event handler for %r on %r", event, result)
            
            sm.RegisterNotify(result)
        
        return result
    finally:
        stackless.getcurrent().block_trap = old_block_trap