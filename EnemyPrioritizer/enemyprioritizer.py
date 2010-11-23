import uix
import json

priorities = {}

def notifyPrioritiesChanged(newPrioritiesJson):
    global priorities
    priorities = json.loads(newPrioritiesJson)

def overridePriority(id, priority):
    priorities["id:%d" % (id,)] = priority

def clearPriority(id):
    key = "id:%d" % (id,)
    if priorities.has_key(key):
        del priorities[key]

def getPriority(targetID=None, slimItem=None):
    global priorities
    
    if targetID and not slimItem:    
        ballpark = eve.LocalSvc('michelle').GetBallpark()
        if not ballpark:
            return 0
        
        slimItem = ballpark.GetInvItem(targetID)
    
    if not slimItem:
        return -1
    
    priority = priorities.get("id:%d" % (slimItem.itemID,), 0)
    if priority == 0:
        priority = priorities.get("type:%d" % (slimItem.typeID,), 0)
        if priority == 0:
            priority = priorities.get("group:%d" % (slimItem.groupID,), 0)
    
    return priority