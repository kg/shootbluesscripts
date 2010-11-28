import shootblues
import types
import threading
import json

__channels = {}

isInitialized = False

def _initChannel(name, handle):
    __channels[name] = shootblues.createChannel(handle)

def getChannel(name):
    return __channels.get(name, None)

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
    channel = getChannel("remotecall")
    channel.send(json.dumps([script, methodName, args]))

def onMainThread():
    try:
        thread = threading.currentThread()
        return isinstance(thread, threading._MainThread)
    except:
        return None

def initialize():
    global isInitialized
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
        
        isInitialized = True

def __unload__():
    global isInitialized, __channels
    
    if isInitialized:        
        isInitialized = False
    
    for key in __channels:
        __channels[key] = None
    
    __channels = {}
