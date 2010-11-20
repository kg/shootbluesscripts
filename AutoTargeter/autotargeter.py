import shootblues
from shootblues.common import forceStartService, forceStopService, log, SafeTimer, MainThreadInvoker
import service
import uix
import json
import state
import base
import destiny
import uthread

prefs = {}
serviceInstance = None

try:
    from shootblues.enemyprioritizer import getPriority
except:
    def getPriority(*args, **kwargs):
        return 0

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)

class AutoTargeterSvc(service.Service):
    __guid__ = "svc.autotargeter"
    __update_on_reload__ = 0
    __exportedcalls__ = {}
    __notifyevents__ = [
        "DoBallsAdded",
        "DoBallRemove",
        "DoBallClear",
        "OnTargets",
        "OnTarget"
    ]

    def __init__(self):
        service.Service.__init__(self)
        self.disabled = False
        self.__updateTimer = SafeTimer(2000, self.updateTargets)
        self.__potentialTargets = []
        self.__populateTargets = MainThreadInvoker(self.populateTargets)
        self.warping = False
        self.targetsToLock = set([])
        self.targetsToUnlock = set([])
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return ballpark.DistanceBetween(eve.session.shipid, targetID)
    
    def makeTargetSorter(self, currentTargets):
        def targetSorter(lhs, rhs):
            # Highest priority first
            priLhs = getPriority(targetID=lhs)
            priRhs = getPriority(targetID=rhs)
            result = cmp(priRhs, priLhs)
            
            if result == 0:
                result = cmp(
                    rhs in currentTargets,
                    lhs in currentTargets
                )
                
                if result == 0:
                    distLhs = self.getDistance(lhs)
                    distRhs = self.getDistance(rhs)
                    result = cmp(distLhs, distRhs)
        
            return result
        
        return targetSorter
    
    def getMaxTargets(self):
        godma = eve.LocalSvc("godma")
        reservedSlots = int(getPref("ReservedTargetSlots", 1))
        maxTargets = int(min(
            godma.GetItem(eve.session.charid).maxLockedTargets, 
            godma.GetItem(eve.session.shipid).maxLockedTargets
        ))
        return max(maxTargets - reservedSlots, 0)
    
    def isBallWarping(self, ball):
        return ball.mode == destiny.DSTBALL_WARP
    
    def isBallTargetable(self, ball):
        if ball is None:
            return False
        elif self.isBallWarping(ball):
            return False
        else:
            return True
    
    def filterTargets(self, ids):
        targetSvc = sm.services['target']
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        myBall = ballpark.GetBall(eve.session.shipid)
        
        if not myBall:
            return []
        elif self.isBallWarping(myBall):
            return []
        else:
            return [id for id in ids if
                    self.isBallTargetable(ballpark.GetBall(id)) and 
                    (getPriority(targetID=id) >= 0)]
    
    def updateTargets(self):
        if self.disabled:
            return
        if not eve.LocalSvc("michelle").GetBallpark():
            return
    
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return
            
        maxTargets = self.getMaxTargets()
        if maxTargets <= 0:
            return
        
        currentTargets = self.filterTargets(targetSvc.targets)
        exclusionSet = set(targetSvc.targeting + targetSvc.autoTargeting + currentTargets)
        targetSorter = self.makeTargetSorter(exclusionSet)
        
        targets = self.filterTargets(self.__potentialTargets)
        targets.sort(targetSorter)
                
        if len(targets):
            currentlyTargeting = set(targetSvc.targeting + targetSvc.autoTargeting)
            
            maxNewTargets = max(
                maxTargets - len(currentlyTargeting),
                0
            )
            targets = set(targets[0:maxTargets])
            
            currentTargets.sort(targetSorter)
            currentTargets = set(currentTargets)
                        
            self.targetsToUnlock = (currentTargets - targets) - currentlyTargeting
            self.targetsToLock = (targets - currentTargets) - currentlyTargeting
            self.targetsToLock = list(self.targetsToLock)[0:maxNewTargets]
        
            if len(self.targetsToUnlock):
                log("Unlocking %r target(s)", len(self.targetsToUnlock))
                for targetID in self.targetsToUnlock:
                    uthread.pool(
                        "UnlockTarget",
                        targetSvc.UnlockTarget,
                        targetID
                    )
            
            if len(self.targetsToLock):
                log("Locking %r target(s)", len(self.targetsToLock))
                for targetID in self.targetsToLock:
                    uthread.pool(
                        "LockTarget",
                        targetSvc.TryLockTarget,
                        targetID
                    )
                
    def getFlagName(self, slimItem):
        if (slimItem.categoryID != const.categoryEntity):
            return None
    
        stateSvc = eve.LocalSvc("state")
        props = stateSvc.GetProps()
        
        flag = stateSvc.CheckStates(slimItem, "flag")
        if flag:
            flagProps = props.get(flag, None)
            if flagProps:
                return flagProps[1]
        
        colorFlag = 0
        if slimItem.typeID:
            itemType = eve.LocalSvc("godma").GetType(slimItem.typeID)
            for attr in itemType.displayAttributes:
                if attr.attributeID == const.attributeEntityBracketColour:
                    if attr.value == 1:
                        return "HostileNPC"
                    elif attr.value == 0:
                        return "NeutralNPC"
        
        return None
    
    def populateTargets(self):
        self.__populateTargets = None
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        lst = []
        for ballID in ballpark.balls.keys():
            slimItem = ballpark.GetInvItem(ballID)
            if slimItem:
               lst.append((ballpark.GetBall(ballID), slimItem))
        self.DoBallsAdded(lst)
    
    def DoBallsAdded(self, lst, **kwargs):
        for (ball, slimItem) in lst:
            isValidTarget = False
            flag = self.getFlagName(slimItem)
            
            if getPref("TargetHostileNPCs", True) and (flag == "HostileNPC"):
                self.__potentialTargets.append(slimItem.itemID)
            elif getPref("TargetHostilePlayers", True) and (
                    (flag == "StandingBad") or 
                    (flag == "StandingHorrible") or
                    (flag == "AtWarCanFight")
                ):
                self.__potentialTargets.append(slimItem.itemID)
            elif getPref("TargetNeutralPlayers", True) and (flag == "StandingNeutral"):
                self.__potentialTargets.append(slimItem.itemID)                
    
    def DoBallRemove(self, ball, slimItem, *args, **kwargs):
        if slimItem and (slimItem.itemID in self.__potentialTargets):
            self.__potentialTargets.remove(slimItem.itemID)
    
    def DoBallClear(self, solItem, **kwargs):
        self.__potentialTargets = []
    
    def OnTargets(self, targets):
        for each in targets:
            self.OnTarget(*each[1:])
    
    def OnTarget(self, what, tid=None, reason=None):
        if (what == "lost"):
            if (reason == None) and (tid in self.__potentialTargets):            
                self.__potentialTargets.remove(tid)

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStartService("autotargeter", AutoTargeterSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStopService("autotargeter")
        serviceRunning = False
