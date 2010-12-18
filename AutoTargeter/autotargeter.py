import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, runOnMainThread, getFlagName, getNamesOfIDs, isBallTargetable, isBallWarping, isPlayerJumping
from shootblues.common.service import forceStart, forceStop
import uix
import json
import state
import base
import destiny
import uthread
import trinity
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

TargetableCategories = set([const.categoryEntity, const.categoryDrone, const.categoryShip])

class BallInfo:
    def __init__(self, ball, slimItem):
        self.id = slimItem.itemID
        self.ball = ball
        self.slimItem = slimItem
        self.flag = getFlagName(slimItem)
    
    def updateFlag(self):
        self.flag = getFlagName(self.slimItem)
    
    @property
    def isEligible(self):            
        return (
            (getPref("TargetHostileNPCs", False) and self.flag in FlagHostileNPC) or
            (getPref("TargetHostilePlayers", False) and self.flag in FlagHostilePlayer) or
            (getPref("TargetNeutralPlayers", False) and self.flag in FlagNeutralPlayer) or
            (getPref("TargetFriendlyPlayers", False) and self.flag in FlagFriendlyPlayer)
        )

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
        self.__balls = {}
        self.__eligibleBalls = set()
        self.__lockedTargets = set()
        
        runOnMainThread(self.populateBalls)
        
    def getDistance(self, targetID):
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        return ballpark.DistanceBetween(eve.session.shipid, targetID)
    
    def makeTargetSorter(self, currentTargets):
        gd = Memoized(self.getDistance)
        gp = Memoized(getPriority)
    
        def targetSorter(lhs, rhs):
            # Highest priority first
            priLhs = gp(targetID=lhs)
            priRhs = gp(targetID=rhs)
            result = cmp(priRhs, priLhs)
            
            if result == 0:
                result = cmp(
                    rhs in currentTargets,
                    lhs in currentTargets
                )
                
                if result == 0:
                    distLhs = gd(lhs)
                    distRhs = gd(rhs)
                    result = cmp(distLhs, distRhs)
        
            return result
        
        return targetSorter
    
    def getMaxTargetRange(self):
        godma = eve.LocalSvc("godma")
        return float(godma.GetItem(eve.session.shipid).maxTargetRange)        
    
    def getMaxTargets(self):
        godma = eve.LocalSvc("godma")
        reservedSlots = int(getPref("ReservedTargetSlots", 1))
        maxTargets = int(min(
            godma.GetItem(eve.session.charid).maxLockedTargets, 
            godma.GetItem(eve.session.shipid).maxLockedTargets
        ))
        return max(maxTargets - reservedSlots, 0)
       
    def filterTargets(self, bis):
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
            if not isBallTargetable(bi.ball):
                continue
            
            if getPriority(slimItem=bi.slimItem) < 0:
                continue
                
            distance = ballpark.DistanceBetween(eve.session.shipid, bi.id)
            if distance > maxTargetRange:
                continue
            
            result.append(bi.id)
        
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
            self.__balls[id] for id in self.__lockedTargets 
            if id in targetSvc.targets
        ])
        exclusionSet = set(targetSvc.targeting + targetSvc.autoTargeting + currentTargets)
        targetSorter = self.makeTargetSorter(exclusionSet)
        
        targets = self.filterTargets(self.__eligibleBalls)
        targets.sort(targetSorter)
                
        currentlyTargeting = set([
            id for id in (targetSvc.targeting + targetSvc.autoTargeting) 
            if id in self.__lockedTargets
        ])
        
        allLockedTargets = set(targetSvc.targeting + targetSvc.autoTargeting + targetSvc.targets)
        maxNewTargets = max(maxTargets - len(allLockedTargets), 0)
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
                    setItemColor(targetID, None)
                
                targetSvc.UnlockTarget(targetID)
        
        if len(targetsToLock):
            log("Locking %s", ", ".join(getNamesOfIDs(targetsToLock)))
            for targetID in targetsToLock:
                if targetID not in self.__lockedTargets:
                    self.__lockedTargets.append(targetID)
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
            self.__lockedTargets = list(targetSvc.targets)
            for id in self.__lockedTargets:
                setItemColor(id, "Automatic Target")
        
        lst = []
        for ballID, ball in ballpark.balls.iteritems():
            slimItem = ballpark.GetInvItem(ballID)
            if slimItem:
               lst.append((ball, slimItem))
        
        self._DoBallsAdded(lst)
                
    def DoBallsAdded(self, lst, **kwargs):
        uthread.pool(
            "DoBallsAdded",
            self._DoBallsAdded,
            lst
        )
    
    def _DoBallsAdded(self, lst):
        for (ball, slimItem) in lst:
            if not slimItem.categoryID in TargetableCategories:
                continue
            if slimItem.itemID == eve.session.shipid:
                continue
            
            bi = BallInfo(ball, slimItem)
            self.__balls[slimItem.itemID] = bi
            
            if bi.isEligible:
                self.__eligibleBalls.add(bi)
    
    def populateEligibleBalls(self):
        self.__eligibleBalls = set()
        
        for ballInfo in self.__balls.itervalues():
            if ballInfo.isEligible:
                self.__eligibleBalls.add(ballInfo)
    
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
            if (reason == None) and (tid in self.__potentialTargets):
                self.__potentialTargets.remove(tid)
            if tid in self.__lockedTargets:
                self.__lockedTargets.remove(tid) 
                setItemColor(tid, None)
        
        elif (what == "clear"):
            for id in self.__lockedTargets:
                setItemColor(id, None)
            
            self.__lockedTargets = []

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
