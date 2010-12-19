from shootblues.common import log
from shootblues.common.eve.state import getCachedItem
import uix
import json

priorities = {}
priorityBoosts = {}

def notifyPrioritiesChanged(newPrioritiesJson):
    global priorities
    priorities = json.loads(newPrioritiesJson)

def adjustPriority(targetID, delta=1):
    global priorityBoosts
    if delta != 0:
        priorityBoosts[targetID] = delta
    elif targetID in priorityBoosts:
        del priorityBoosts[targetID]

def getPriority(id):
    global priorities, priorityBoosts
    
    if not id:
        return -1
    
    ci = getCachedItem(id)
    
    if not ci.slimItem:
        return -1
    
    charID = getattr(ci.slimItem, "charID", None)
    if charID:
        priority = priorities.get("char:%d" % (charID,), 0)
    else:
        priority = 0
    
    if priority == 0:
        priority = priorities.get("type:%d" % (ci.slimItem.typeID,), 0)
        if priority == 0:
            priority = priorities.get("group:%d" % (ci.slimItem.groupID,), 0)
    
    if priority == 0:
        flag = ci.flag
        
        if ((flag == "StandingGood") or
            (flag == "StandingHigh") or
            (flag == "SameGang") or
            (flag == "SameFleet") or
            (flag == "SameAlliance") or
            (flag == "SameCorp")):
            priority = -1
    
    priority += priorityBoosts.get(id, 0)
    
    return priority