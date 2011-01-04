import shootblues
import types
import threading
import json
import traceback
import sys

_channels = {}
_pendingMessages = {}
_pendingRemoteCalls = {}
_nextRemoteCallId = 1L

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

def showException(etype=None, value=None, tb=None):    
    if (not etype) or (not value) or (not tb):
        exc_info = sys.exc_info()
        if not etype:
            etype = exc_info[0]
        if not value:
            value = exc_info[1]
        if not tb:
            tb = exc_info[2]
        
    remoteCall(
        "Common.Script.dll", "ShowError", 
        "".join(traceback.format_exception(
            etype, value, tb
        ))
    )

class RemoteCallResult(object):
    def __init__(self, stack):
        self.__stack = stack
        self.__result = []
        self.__callbacks = []

    def set(self, result, errorText):   
        if len(self.__result) == 0:
            self.__result.append((result, errorText))
            
            while len(self.__callbacks):
                callback = self.__callbacks.pop()
                callback(self, result, errorText)                
        else:
            raise Exception("Already have a result")
    
    def addCallback(self, callback):
        if len(self.__result) == 0:
            self.__callbacks.append(callback)
        else:
            r = self.__result[0]
            callback(r[0], r[1])
    
    @property
    def hasResult(self):
        return len(self.__result) == 1
    
    @property
    def result(self):
        if len(self.__result) == 0:
            raise Exception("No result yet")
            
        r = self.__result[0]
        if r[1] is not None:
            raise Exception(r[1])
        else:
            return r[0]

def _remoteCallComplete(resultId, result, errorText):
    global _pendingRemoteCalls
    
    prc = _pendingRemoteCalls.get(resultId, None)
    if prc is not None:
        del _pendingRemoteCalls[resultId]    
        prc.set(result, errorText)

def remoteCall(script, methodName, *args, **kwargs):
    global _nextRemoteCallId, _pendingRemoteCalls

    callData = [script, methodName, args]
    result = None

    if kwargs.get("async", False):
        id = _nextRemoteCallId
        _nextRemoteCallId += 1
    
        stack = traceback.format_stack()
        result = RemoteCallResult(stack)
        _pendingRemoteCalls[id] = result
        
        callData.append(id)        
    
    channel = getChannel("remotecall")
    channel.send(json.dumps(callData))
    
    return result

def playSound(filename):        
    moduleName = getCallingModule()
    if moduleName:
        moduleName = moduleName.replace("shootblues.", "")
    
    remoteCall("Common.Script.dll", "PlaySound", moduleName, filename)

def getPreference(name, moduleName=None):
    if moduleName is None:
        moduleName = getCallingModule()
        moduleName = moduleName.replace("shootblues.", "")
    
    return remoteCall(moduleName, "GetPreference", name, async=True)

def setPreference(name, value, moduleName=None):
    if moduleName is None:
        moduleName = getCallingModule()
        moduleName = moduleName.replace("shootblues.", "")
    
    return remoteCall(moduleName, "SetPreference", name, value, async=True)

def showBalloonTip(title, text, timeout=None):
    if timeout:
        args = [timeout, title, text]
    else:
        args = [title, text]
    
    remoteCall("Common.Script.dll", "ShowBalloonTip", *args)

def showMessageBox(title, text, buttons="OK"):
    return remoteCall("Common.Script.dll", "ShowMessageBox", title, text, buttons, async=True)

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
