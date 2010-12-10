from shootblues import Dependency
Dependency("Common.Script.dll")

from shootblues.common import log
from shootblues.common.service import forceStart, forceStop
import blue
import service

class SessionTimerSvc:
    __notifyevents__ = [
        "OnSessionChanged"
    ]

    def __init__(self):
        pass

    def OnSessionChanged(self, isRemote, session, change):
        log("Session changed, timer suppressed.")
        session.nextSessionChange = blue.os.GetTime(1) + 1

forceStart("sessiontimer", SessionTimerSvc)

def __unload__():
    forceStop("sessiontimer")