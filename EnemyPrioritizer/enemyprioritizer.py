import shootblues
from shootblues.common import forceStartService, forceStopService, log
import service
import uix
import json

priorities = {}

def notifyPrioritiesChanged(newPrioritiesJson):
    global priorities
    priorities = json.loads(newPrioritiesJson)
    
def getPriority(targetID=None, slimItem=None):
    global priorities
    
    if targetID and not slimItem:    
        ballpark = eve.LocalSvc('michelle').GetBallpark()
        slimItem = ballpark.GetInvItem(targetID)
    
    if not slimItem:
        return 0
    
    priority = priorities.get("type:%d" % (slimItem.typeID,), 0)
    if priority == 0:
        priority = priorities.get("group:%d" % (slimItem.groupID,), 0)
    
    return priority