from shootblues import Dependency
Dependency("Common.Script.dll")
Dependency("EventNotifications.Script.dll")

from shootblues.common import log
from shootblues.common.eve import runOnMainThread
from shootblues.common.service import forceStart, forceStop
import util
from HTMLParser import HTMLParser

from shootblues.eventnotifications import DefineEvent, fireEvent
DefineEvent("UnreadNotification")

serviceInstance = None

class MLStripper(HTMLParser):
    def __init__(self):
        self.reset()
        self.fed = []
    def handle_data(self, d):
        self.fed.append(d)
    def get_data(self):
        return ''.join(self.fed)

def strip_tags(html):
    s = MLStripper()
    s.feed(html)
    return s.get_data()

def getName(o):
    if o:
        return o.name
    else:
        return "None"

class NotificationListenerSvc:
    __notifyevents__ = [
        "OnNotificationReceived",
        "OnSessionChanged"
    ]
    
    def __init__(self):
        runOnMainThread(self.checkUnreadNotifications)
    
    def checkUnreadNotifications(self):
        notifyService = sm.services.get("notificationSvc", None)
        if not notifyService:
            return
        
        if not eve.session.charid:
            return
        
        unreadNotifications = notifyService.GetFormattedUnreadNotifications() 
        
        if len(unreadNotifications):
            for n in unreadNotifications:
                fireEvent("UnreadNotification", text=strip_tags(str(n.body)))
        
        notifyIDs = [n.notificationID for n in unreadNotifications]
        notifyService.MarkAsRead(notifyIDs)
        notifyService.UpdateCacheAfterMarkingRead(notifyIDs)

    def OnSessionChanged(self, isRemote, session, change):
        self.checkUnreadNotifications()

    def OnNotificationReceived(self, notificationID, typeID, senderID, created, data={}):
        notifyService = sm.services.get("notificationSvc", None)
        if not notifyService:
            return
        
        self.checkUnreadNotifications()

def __load__():
    global serviceInstance
    serviceInstance = forceStart("notificationlistener", NotificationListenerSvc)

def __unload__():
    global serviceInstance
    if serviceInstance:
        forceStop("notificationlistener")
        serviceInstance = None