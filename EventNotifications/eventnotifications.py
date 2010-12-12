from shootblues.common import log, remoteCall, playSound, showBalloonTip, showMessageBox
from shootblues.common.service import forceStart, forceStop
from shootblues.common.messaging import subscribe, unsubscribe, send
import json

notificationSettings = {}

serviceRunning = False

def notifySettingsChanged(newSettingsJson):
    global notificationSettings
    
    notificationSettings = json.loads(newSettingsJson)

def DefineEvent(eventName):
    remoteCall("EventNotifications.Script.dll", "DefineEvent", eventName)

def handleEvent(source, name, data):
    global notificationSettings
    
    settings = notificationSettings.get(name, None)
    if settings:
        if settings.get("sound", None):
            playSound(settings["sound"])
        if settings.get("balloonTip", False):
            showBalloonTip(name, str(data.get("text", source)))
        if settings.get("messageBox", False):
            showMessageBox(name, str(data.get("text", source)))

def fireEvent(name, **extraData):
    send(name, **extraData)

def initialize():
    global serviceRunning
    if not serviceRunning:
        serviceRunning = True
        subscribe(handleEvent)

def __unload__():
    global serviceRunning
    if serviceRunning:
        serviceRunning = False
        unsubscribe(handleEvent)