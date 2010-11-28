from . import log

__runningServices = []

def forceStop(serviceName):
    global __runningServices
    
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
        
        if serviceName in __runningServices:
            __runningServices.remove(serviceName)
    finally:
        stackless.getcurrent().block_trap = old_block_trap

def forceStart(serviceName, serviceType):
    global __runningServices
    
    import stackless
    import service
    old_block_trap = stackless.getcurrent().block_trap
    stackless.getcurrent().block_trap = 1
    try:
        oldInstance = sm.services.get(serviceName, None)
        if oldInstance:
            forceStop(serviceName)
        
        result = serviceType()
        sm.services[serviceName] = result
        __runningServices.append(serviceName)
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

def __unload__():
    global __runningServices
    if len(__runningServices):
        log("Services left running: %r", __runningServices)