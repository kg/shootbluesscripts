from shootblues.common import forceStartService, forceStopService
import blue
import service

class SessionTimerSvc(service.Service):
    __guid__ = "svc.sessiontimer"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "OnSessionChanged"
    ]

    def __init__(self):
        service.Service.__init__(self)

    def Run(self, memStream=None):
        service.Service.Run(self, memStream)

    def OnSessionChanged(self, isRemote, session, change):
        session.nextSessionChange = blue.os.GetTime(1) + 1

forceStartService("sessiontimer", SessionTimerSvc)

def __unload__():
    forceStopService("sessiontimer")