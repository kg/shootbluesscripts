serviceInstance = None

def installCharacterMonitor():
    global serviceInstance
    
    import blue
    from shootblues.common.eve import getCharacterName
    from shootblues.common import remoteCall
    from shootblues.common.service import forceStart

    class CharacterMonitorSvc:
        __notifyevents__ = [
            "OnSessionChanged"
        ]

        def __init__(self):
            self.loggedInCharacter = None

        def OnSessionChanged(self, isRemote, session, change):
            if self.loggedInCharacter != session.charid:
                self.loggedInCharacter = session.charid
                characterName = getCharacterName(session.charid)
                remoteCall("common.script.dll", "LoggedInCharacterChanged", characterName)
    
    serviceInstance = forceStart("charmonitor", CharacterMonitorSvc)
    serviceInstance.OnSessionChanged(False, eve.session, None)

def __unload__():
    global serviceInstance
    if serviceInstance:
        from shootblues.common.service import forceStop
        forceStop("charmonitor")