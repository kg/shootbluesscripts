from shootblues import Dependency
Dependency("Common.Script.dll")
Dependency("EventNotifications.Script.dll")

from shootblues.common import log
from shootblues.common.eve import runOnMainThread
from shootblues.common.service import forceStart, forceStop
import util
from HTMLParser import HTMLParser

from shootblues.eventnotifications import DefineEvent, fireEvent
DefineEvent("NewTowermail")

TowermailFormat = (
    "{subject} // {timestamp}",
    "Moon: {moonName} // Object: {typeName} // Current Shield Level: {shieldPercentage}%",
    "Pilot: {aggressorName} // Corp: {aggressorCorpName} // Alliance: {aggressorAllianceName}"
)

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

def getName(collection, index):
    try:
      o = collection.Get(index)
    except:
      return "None"
    
    if o:
        return o.name
    else:
        return "None"

class TowermailSvc:
    __notifyevents__ = [
        "OnNotificationReceived",
        "OnSessionChanged"
    ]
    
    def __init__(self):
        runOnMainThread(self.checkUnreadNotifications)
    
    def formatTowermail(self, n):
        data = n.data.copy()
        data["subject"] = strip_tags(str(n.subject))
        data["body"] = strip_tags(str(n.body))
        data["timestamp"] = util.FmtDate(n.created)
        data["moonName"] = getName(cfg.evelocations, n.data["moonID"])
        data["typeName"] = getName(cfg.invtypes, n.data["typeID"])
        data["aggressorName"] = getName(cfg.eveowners, n.data["aggressorID"])
        data["aggressorCorpName"] = getName(cfg.eveowners, n.data["aggressorCorpID"])
        data["aggressorAllianceName"] = getName(cfg.eveowners, n.data["aggressorAllianceID"])
        data["shieldPercentage"] = int(float(n.data["shieldValue"]) * 100)
        return [l.format(**data) for l in TowermailFormat]
    
    def checkUnreadNotifications(self):
        notifyService = sm.services.get("notificationSvc", None)
        if not notifyService:
            return
        
        if not eve.session.charid:
            return
        
        towermails = [n for n in notifyService.GetFormattedUnreadNotifications() 
                      if n.typeID == const.notificationTypeTowerAlertMsg]
        
        if len(towermails):
            for n in towermails:
                formatted = self.formatTowermail(n)
                fireEvent("NewTowermail", text=formatted)
        
        notifyIDs = [n.notificationID for n in towermails]
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
    serviceInstance = forceStart("towermail", TowermailSvc)

def __unload__():
    global serviceInstance
    if serviceInstance:
        forceStop("towermail")
        serviceInstance = None