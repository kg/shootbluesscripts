import shootblues
import types
import threading
import json

__channels = {}

isInitialized = False
pid = None

def _initChannel(name, handle):
    __channels[name] = shootblues.createChannel(handle)

def getChannel(name):
    return __channels.get(name, None)

def log(format, *args):
    logger = getChannel("log")
    if logger:
        try:
            if args:
                logger.send(str(format) % args)
            elif isinstance(format, types.StringType):
                logger.send(format)
            else:
                logger.send(repr(format))
        except:
            pass

def remoteCall(script, methodName, *args):
    channel = getChannel("remotecall")
    channel.send(json.dumps([script, methodName, args]))

def onMainThread():
    try:
        thread = threading.currentThread()
        return isinstance(thread, threading._MainThread)
    except:
        return None

def initialize(myPid):
    global isInitialized, pid
    
    pid = myPid
        
    if not isInitialized:
        try:
            from common.eve.logger import replaceLogger
            replaceLogger()
        except Exception, e:
            log("Failed to initialize logger: %r", e)
        
        try:
            from common.eve.charmonitor import installCharacterMonitor
            installCharacterMonitor()
        except Exception, e:
            log("Failed to initialize character monitor: %r", e)
        
        try:
            from common.eve.state import installStateMonitor
            installStateMonitor()
        except Exception, e:
            log("Failed to initialize state monitor: %r", e)
        
        isInitialized = True

def __unload__():
    global isInitialized, __channels
    
    if isInitialized:        
        isInitialized = False
    
    for key in __channels:
        __channels[key] = None
    
    __channels = {}
