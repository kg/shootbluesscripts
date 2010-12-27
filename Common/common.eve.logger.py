oldLogException = None
oldLogTraceback = None
replacedMethods = []

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

def replaceMethod(obj, name):
    def stub(*args, **kwargs):
        from shootblues.common import log
        log("stubbed: %r (%r, %r)", name, args, kwargs)
        return
    
    replacedMethods.append((obj, name, getattr(obj, name, None)))
    setattr(obj, name, stub)

def replaceLogger():
    global oldLogException, oldLogTraceback, replacedMethods
    
    replacedMethods = []
    alertSvc = sm.services.get("alert", None)
    if alertSvc:
        replaceMethod(alertSvc, "SendClientStackTraceAlert")
        replaceMethod(alertSvc, "SendProxyStackTraceAlert")
        replaceMethod(alertSvc, "SendStackTraceAlert")
        replaceMethod(alertSvc, "SendMail")
    
    debugSvc = sm.services.get("debug", None)
    if debugSvc:
        replaceMethod(debugSvc, "Eval")
        replaceMethod(debugSvc, "Exec")
        replaceMethod(debugSvc, "OnRemoteExecute")
    
    try:
        import log
    except ImportError:
        return
    
    oldLogException = log.LogException
    oldLogTraceback = log.LogTraceback
    log.LogException = logException
    log.LogTraceback = logTraceback

def __unload__():
    global oldLogException, oldLogTraceback, replacedMethods
    
    for obj, name, oldValue in replacedMethods:
        try:
            setattr(obj, name, oldValue)
        except:
            pass
        
    replacedMethods = []
    
    try:
        import log
    except ImportError:
        return
    
    if oldLogException:
        log.LogException = oldLogException
    if oldLogTraceback:
        log.LogTraceback = oldLogTraceback