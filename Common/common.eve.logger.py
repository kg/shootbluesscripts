oldLogException = None
oldLogTraceback = None

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

def replaceLogger():
    global oldLogException, oldLogTraceback
    try:
        import log
    except ImportError:
        return
    oldLogException = log.LogException
    oldLogTraceback = log.LogTraceback
    log.LogException = logException
    log.LogTraceback = logTraceback

def __unload__():
    global oldLogException, oldLogTraceback
    try:
        import log
    except ImportError:
        return
    
    if oldLogException:
        log.LogException = oldLogException
    if oldLogTraceback:
        log.LogTraceback = oldLogTraceback