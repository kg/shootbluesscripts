import shootblues
from shootblues.common import forceStartService, forceStopService, log, SafeTimer, MainThreadInvoker, getFlagName, getNamesOfIDs
import service
import uix
import json
import state
import base
import destiny
import uthread
import trinity

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
        self.__updateTimer = SafeTimer(500, self.updateTargets)
        self.__potentialTargets = []
        self.__populateTargets = MainThreadInvoker(self.populateTargets)
        self.__lockedTargets = []
        self.warping = False
        
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
        if not ballpark:
            return []
        
        myBall = ballpark.GetBall(eve.session.shipid)
        
        if not myBall:
            return []
        elif self.isBallWarping(myBall):
            return []
        
        result = []
        
        for id in ids:
            ball = ballpark.GetBall(id)
            if not ball:
                continue
            if not self.isBallTargetable(ball):
                continue
                
            slimItem = ballpark.GetInvItem(id)
            if not slimItem:
                continue
            
            if getPriority(slimItem=slimItem) < 0:
                continue
                
            flag = getFlagName(slimItem)
            if flag == "HostileNPC":
                if not getPref("TargetHostileNPCs", False):
                    continue
            elif ((flag == "StandingBad") or 
                  (flag == "StandingHorrible") or
                  (flag == "AtWarCanFight")):
                if not getPref("TargetHostilePlayers", False):
                    continue
            elif flag == "StandingNeutral":
                if not getPref("TargetNeutralPlayers", False):
                    continue                
            
            result.append(id)
        
        return result
    
    def updateTargets(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
    
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return
            
        maxTargets = self.getMaxTargets()
        if maxTargets <= 0:
            return
        
        currentTargets = self.filterTargets([
            id for id in self.__lockedTargets 
            if id in targetSvc.targets
        ])
        exclusionSet = set(targetSvc.targeting + targetSvc.autoTargeting + currentTargets)
        targetSorter = self.makeTargetSorter(exclusionSet)
        
        targets = self.filterTargets(self.__potentialTargets)
        targets.sort(targetSorter)
                
        if len(targets):
            currentlyTargeting = set([
                id for id in (targetSvc.targeting + targetSvc.autoTargeting) 
                if id in self.__lockedTargets
            ])
            
            maxNewTargets = max(
                maxTargets - len(currentlyTargeting),
                0
            )
            targets = set(targets[0:maxTargets])
            
            currentTargets.sort(targetSorter)
            currentTargets = set(currentTargets)
                        
            targetsToUnlock = (currentTargets - targets) - currentlyTargeting
            targetsToLock = (targets - set(targetSvc.targets)) - currentlyTargeting
            targetsToLock = list(targetsToLock)[0:maxNewTargets]
        
            if len(targetsToUnlock):
                log("Unlocking %s", ", ".join(getNamesOfIDs(targetsToUnlock)))
                for targetID in targetsToUnlock:
                    if targetID in self.__lockedTargets:
                        self.__lockedTargets.remove(targetID)
                        
                    uthread.pool(
                        "UnlockTarget",
                        targetSvc.UnlockTarget,
                        targetID
                    )
            
            if len(targetsToLock):
                log("Locking %s", ", ".join(getNamesOfIDs(targetsToLock)))
                for targetID in targetsToLock:
                    if targetID not in self.__lockedTargets:
                        self.__lockedTargets.append(targetID)
                    
                    uthread.pool(
                        "LockTarget",
                        targetSvc.TryLockTarget,
                        targetID
                    )
        
        color = (1.0, 1.0, 0.8, 1.0)
               
        for id in self.__lockedTargets:
            targetFrame = targetSvc.targetsByID.get(id, None)
            if not targetFrame:
                continue
            if not hasattr(targetFrame, "sr"):
                continue
            
            if hasattr(targetFrame.sr, "label"):            
                targetFrame.sr.label.color.SetRGB(*color)
            if hasattr(targetFrame.sr, "iconPar"):
                for obj in targetFrame.sr.iconPar.children:
                    if hasattr(obj, "color"):
                        obj.color.SetRGB(*color)
    
    def populateTargets(self):
        self.__populateTargets = None
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        targetSvc = sm.services.get('target', None)
        if targetSvc:
            self.__lockedTargets = list(targetSvc.targets)
        
        lst = []
        for ballID in ballpark.balls.keys():
            slimItem = ballpark.GetInvItem(ballID)
            if slimItem:
               lst.append((ballpark.GetBall(ballID), slimItem))
        self.DoBallsAdded(lst)
    
    def DoBallsAdded(self, lst, **kwargs):
        for (ball, slimItem) in lst:
            isValidTarget = False
            flag = getFlagName(slimItem)
            
            if flag == "HostileNPC":
                self.__potentialTargets.append(slimItem.itemID)
            elif ((flag == "StandingBad") or 
                  (flag == "StandingHorrible") or
                  (flag == "AtWarCanFight")):
                self.__potentialTargets.append(slimItem.itemID)
            elif flag == "StandingNeutral":
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
            if tid in self.__lockedTargets:
                self.__lockedTargets.remove(tid)        
        elif (what == "clear"):
            self.__lockedTargets = []

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
