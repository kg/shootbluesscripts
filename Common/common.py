import shootblues
import types
import threading
import json

_channels = {}
_pendingMessages = {}

isInitialized = False
pid = None

class FakeChannel(object):
    def __init__(self, name):
        self.name = name
    
    def send(self, data):        
        global _pendingMessages
        pending = _pendingMessages.setdefault(self.name, [])
        pending.append(data)

def _initChannel(name, handle):
    global _channels, _pendingMessages
    pms = _pendingMessages.get(name, None)
    _channels[name] = channel = shootblues.createChannel(handle)
    
    if pms:
        del _pendingMessages[name]
        for pm in pms:
            channel.send(pm) 

def getChannel(name):
    global _channels
    return _channels.get(name, FakeChannel(name))

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

def getMyModule():
    return getFrameModule(2)

def getCallingModule():
    return getFrameModule(3)

def getFrameModule(i):
    import inspect
    callingModule = inspect.getmodule(inspect.stack()[i][0])
    if callingModule:
        return callingModule.__name__
    
    return None

def remoteCall(script, methodName, *args):
    channel = getChannel("remotecall")
    channel.send(json.dumps([script, methodName, args]))

def playSound(filename):        
    moduleName = getCallingModule()
    if moduleName:
        moduleName = moduleName.replace("shootblues.", "")
    
    remoteCall("Common.Script.dll", "PlaySound", moduleName, filename)

def showBalloonTip(title, text, timeout=None):
    if timeout:
        args = [timeout, title, text]
    else:
        args = [title, text]
    
    remoteCall("Common.Script.dll", "ShowBalloonTip", *args)

def showMessageBox(title, text):
    remoteCall("Common.Script.dll", "ShowMessageBox", title, text)

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
    global isInitialized, _channels
    
    if isInitialized:        
        isInitialized = False
    
    for key in _channels.keys():
        del _channels[key]
    
    _channels = {}
