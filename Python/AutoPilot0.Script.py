from shootblues.common import log
import util.Moniker

oldMonikeredCall = util.Moniker.MonikeredCall

def _MonikeredCall(self, call, sess):
    methodName, args, kwargs = call
    
    if methodName == "WarpToStuffAutopilot":
        methodName = "WarpToStuff"
        args = ('item', args[0])
        kwargs = {'throttleCalls': True, 'minRange': 0}
        log("Intercepting autopilot warp attempt")
    
    return oldMonikeredCall(self, (methodName, args, kwargs), sess)
    
util.Moniker.MonikeredCall = _MonikeredCall

def __unload__():
    global oldMonikeredCall
    util.Moniker.MonikeredCall = oldMonikeredCall