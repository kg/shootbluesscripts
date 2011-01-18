import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, runOnMainThread, getFlagName, getNamesOfIDs, isBallTargetable, isBallWarping, isPlayerJumping
from shootblues.common.service import forceStart, forceStop
from shootblues.common.eve.state import TargetableCategories, getCachedItem
import uix
import json
import state
import base
import destiny
import uthread
import trinity
import blue
from util import Memoized

prefs = {}
serviceInstance = None
serviceRunning = False

try:
    from shootblues.enemyprioritizer import getPriority
except:
    def getPriority(*args, **kwargs):
        return 0

try:
    from shootblues.targetcolors import setItemColor
except:
    def setItemColor(*args, **kwargs):
        return

def getPref(key, default):
    global prefs
    return prefs.get(key, default)

def notifyPrefsChanged(newPrefsJson):
    global prefs
    prefs = json.loads(newPrefsJson)
    
    if serviceInstance:
        serviceInstance.populateEligibleBalls()

FlagHostileNPC = set(["HostileNPC"])
FlagHostilePlayer = set(["StandingBad", "StandingHorrible", "AtWarCanFight"])
FlagNeutralPlayer = set(["StandingNeutral"])
FlagFriendlyPlayer = set(["StandingGood", "StandingHigh", "SameFleet", "SameAlliance", "SameCorp"])

class AutoTargeterSvc:
    __notifyevents__ = [
        "DoBallsAdded",
        "DoBallRemove",
        "DoBallClear",
        "OnTargets",
        "OnTarget"
    ]

    def __init__(self):
        self.disabled = False
        self.__updateTimer = SafeTimer(1000, self.updateTargets)
        self.__repopulateEligible = SafeTimer(60000, self.populateEligibleBalls)
        self.__balls = {}
        self.__eligibleBalls = set()
        self.__lockedTargets = set()
        
        runOnMainThread(self.populateBalls)
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return ballpark.DistanceBetween(eve.session.shipid, targetID)
    
    def makeTargetSorter(self, currentTargets, gp, gd):    
        def targetSorter(lhs, rhs):
            # Highest priority first
            priLhs = gp(lhs.id)
            priRhs = gp(rhs.id)
            result = cmp(priRhs, priLhs)
            
            if result == 0:
                result = cmp(
                    rhs.id in currentTargets,
                    lhs.id in currentTargets
                )
                
                if result == 0:
                    distLhs = gd(lhs.id)
                    distRhs = gd(rhs.id)
                    result = cmp(distLhs, distRhs)
        
            return result
        
        return targetSorter
    
    def getMaxTargetRange(self):
        godma = eve.LocalSvc("godma")
        return float(godma.GetItem(eve.session.shipid).maxTargetRange)        
    
    def getMaxTargets(self):
        godma = eve.LocalSvc("godma")
        maxTargets = int(min(
            godma.GetItem(eve.session.charid).maxLockedTargets, 
            godma.GetItem(eve.session.shipid).maxLockedTargets
        ))
        return max(maxTargets, 0)
       
    def filterTargets(self, bis, gp, gd):
        targetSvc = sm.services['target']
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return []
        
        myBall = ballpark.GetBall(eve.session.shipid)
        
        if not myBall:
            return []
        elif isBallWarping(myBall):
            return []
        elif isPlayerJumping():
            return []
        elif myBall.isCloaked:
            return []
        
        result = []
        
        maxTargetRange = self.getMaxTargetRange()
        
        for bi in bis:
            if not bi:
                continue
            
            if gp(bi.id) < 0:
                continue
            
            if not isBallTargetable(bi.ball):
                continue
                
            distance = gd(bi.id)
            if distance > maxTargetRange:
                continue
            
            result.append(bi)
        
        return result
    
    def updateTargets(self):
        if self.disabled:
            self.__updateTimer = None
            return
        
        ballpark = sm.services["michelle"].GetBallpark()
        if not ballpark:
            return
    
        targetSvc = sm.services.get('target', None)
        if not targetSvc:
            return
            
        maxTargets = self.getMaxTargets()
        if maxTargets <= 0:
            return
        
        reservedSlots = int(getPref("ReservedTargetSlots", 1))
        maxAutoTargets = max(0, maxTargets - reservedSlots)
            
        gd = Memoized(self.getDistance)
        gp = Memoized(getPriority)
        
        currentTargets = [self.__balls.get(id, None) for id in self.__lockedTargets 
            if id in targetSvc.targets]
        currentTargets = [bi.id for bi in self.filterTargets(currentTargets, gp, gd)]
        exclusionSet = set(targetSvc.targeting + targetSvc.autoTargeting + currentTargets)
        targetSorter = self.makeTargetSorter(exclusionSet, gp, gd)
        
        targets = self.filterTargets(self.__eligibleBalls, gp, gd)        
        targets.sort(targetSorter)
                
        currentlyTargeting = set([
            id for id in (targetSvc.targeting + targetSvc.autoTargeting) 
            if id in self.__lockedTargets
        ])
        
        allLockedTargets = set(targetSvc.targeting + targetSvc.autoTargeting + targetSvc.targets)
        maxNewTargets = max(maxTargets - len(allLockedTargets), 0)
        targets = set([bi.id for bi in targets[0:maxAutoTargets]])
        
        currentTargets = set(currentTargets)
                    
        targetsToUnlock = (currentTargets - targets) - currentlyTargeting
        targetsToLock = list((targets - set(targetSvc.targets)) - currentlyTargeting)
        targetsToLock = targetsToLock[0:maxNewTargets]
                
        if len(targetsToUnlock):
            log("Unlocking %s", ", ".join(getNamesOfIDs(targetsToUnlock)))
            for targetID in targetsToUnlock:
                if targetID in self.__lockedTargets:
                    self.__lockedTargets.remove(targetID)
                    setItemColor(targetID, None)
                
                targetSvc.UnlockTarget(targetID)
        
        if len(targetsToLock):
            log("Locking %s", ", ".join(getNamesOfIDs(targetsToLock)))
            for targetID in targetsToLock:
                if targetID not in self.__lockedTargets:
                    self.__lockedTargets.add(targetID)
                    setItemColor(targetID, "Automatic Target")
                
                uthread.pool(
                    "LockTarget",
                    self.tryLockTarget,
                    targetSvc, targetID
                )
            
    def tryLockTarget(self, targetSvc, targetID):
        try:
            targetSvc.TryLockTarget(targetID)
        except:
            log("Failed to lock %r", targetID)
    
    def populateBalls(self):        
        self.__balls = {}
        
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        targetSvc = sm.services.get('target', None)
        if targetSvc:
            self.__lockedTargets = set(targetSvc.targets)
            for id in self.__lockedTargets:
                setItemColor(id, "Automatic Target")
        
        lst = []
        for ballID, ball in ballpark.balls.iteritems():
           lst.append((ball, ballpark.GetInvItem(ballID)))
        
        self._DoBallsAdded(lst)
                
    def DoBallsAdded(self, lst, **kwargs):
        uthread.pool(
            "DoBallsAdded",
            self._DoBallsAdded,
            lst
        )
    
    def isEligible(self, cachedItem):
        return (
            (getPref("TargetHostileNPCs", False) and cachedItem.flag in FlagHostileNPC) or
            (getPref("TargetHostilePlayers", False) and cachedItem.flag in FlagHostilePlayer) or
            (getPref("TargetNeutralPlayers", False) and cachedItem.flag in FlagNeutralPlayer) or
            (getPref("TargetFriendlyPlayers", False) and cachedItem.flag in FlagFriendlyPlayer)
        )
    
    def _DoBallsAdded(self, lst):
        for (ball, slimItem) in lst:
            if not slimItem:
                continue
            if not slimItem.categoryID in TargetableCategories:
                continue
            if slimItem.itemID == eve.session.shipid:
                continue
            
            bi = getCachedItem(slimItem.itemID, ball, slimItem)
            self.__balls[slimItem.itemID] = bi
            
            if self.isEligible(bi):
                self.__eligibleBalls.add(bi)
    
    def populateEligibleBalls(self):
        newset = set()
        
        for ballInfo in self.__balls.itervalues():
            if self.isEligible(ballInfo):
                newset.add(ballInfo)
        
        self.__eligibleBalls = newset
    
    def DoBallRemove(self, ball, slimItem, *args, **kwargs):
        if not slimItem:
            return
        
        id = slimItem.itemID
        bi = self.__balls.get(id, None)
        if bi:
            del self.__balls[id]
            if bi in self.__eligibleBalls:
                self.__eligibleBalls.remove(bi)
    
    def DoBallClear(self, solItem, **kwargs):
        self.__balls = {}
        self.__eligibleBalls = set()
    
    def OnTargets(self, targets):
        for each in targets:
            self.OnTarget(*each[1:])
    
    def OnTarget(self, what, tid=None, reason=None):
        if (what == "lost"):
            if (reason == None) and (tid in self.__balls):
                ball = self.__balls[tid]
                del self.__balls[tid]
                if ball in self.__eligibleBalls:
                    self.__eligibleBalls.remove(ball)
            if tid in self.__lockedTargets:
                self.__lockedTargets.remove(tid) 
                setItemColor(tid, None)
        
        elif (what == "clear"):
            for id in self.__lockedTargets:
                setItemColor(id, None)
            
            self.__lockedTargets = set()

def initialize():
    global serviceRunning, serviceInstance
    serviceRunning = True
    serviceInstance = forceStart("autotargeter", AutoTargeterSvc)

def __unload__():
    global serviceRunning, serviceInstance
    if serviceInstance:
        serviceInstance.disabled = True
        serviceInstance = None
    if serviceRunning:
        forceStop("autotargeter")
        serviceRunning = False
