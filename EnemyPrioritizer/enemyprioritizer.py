from shootblues.common import log
from shootblues.common.eve import getFlagName
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

def getPriority(targetID=None, slimItem=None):
    global priorities, priorityBoosts
    
    if targetID and not slimItem:    
        ballpark = eve.LocalSvc('michelle').GetBallpark()
        if not ballpark:
            return 0
        
        slimItem = ballpark.GetInvItem(targetID)
    
    if not slimItem:
        return -1
    
    targetID = slimItem.itemID
    charID = getattr(slimItem, "charID", None)
    if charID:
        priority = priorities.get("char:%d" % (charID,), 0)
    else:
        priority = 0
    
    if priority == 0:
        priority = priorities.get("type:%d" % (slimItem.typeID,), 0)
        if priority == 0:
            priority = priorities.get("group:%d" % (slimItem.groupID,), 0)
    
    if priority == 0:
        flag = getFlagName(slimItem)
        if ((flag == "StandingGood") or
            (flag == "StandingHigh") or
            (flag == "SameGang") or
            (flag == "SameFleet") or
            (flag == "SameAlliance") or
            (flag == "SameCorp")):
            priority = -1
    
    priority += priorityBoosts.get(targetID, 0)
    
    return priority