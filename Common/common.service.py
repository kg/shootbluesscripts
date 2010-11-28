from . import log

def forceStop(serviceName):
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

def forceStart(serviceName, serviceType):
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