import shootblues
from shootblues.common import log
from shootblues.common.eve import SafeTimer, MainThreadInvoker, getFlagName, getNamesOfIDs
from shootblues.common.service import forceStart, forceStop
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
    
    def isPlayerJumping(self):
        return ("jumping" in eve.session.sessionChangeReason) and (eve.session.changing)
    
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
        elif self.isPlayerJumping():
            return []
        
        result = []
        
        maxTargetRange = self.getMaxTargetRange()
        
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
            elif ((flag == "StandingGood") or
                  (flag == "StandingHigh") or
                  (flag == "SameGang") or
                  (flag == "SameFleet") or
                  (flag == "Alliance") or
                  (flag == "SameCorp")):
                if not getPref("TargetFriendlyPlayers", False):
                    continue
                
            distance = ballpark.DistanceBetween(eve.session.shipid, id)
            if distance > maxTargetRange:
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
                        targetSvc.TryLockTarget,
                        targetID
                    )
    
    def populateTargets(self):
        self.__populateTargets = None
        ballpark = eve.LocalSvc("michelle").GetBallpark()
        if not ballpark:
            return
        
        targetSvc = sm.services.get('target', None)
        if targetSvc:
            self.__lockedTargets = list(targetSvc.targets)
            for id in self.__lockedTargets:
                setItemColor(id, "Automatic Target")
        
        ids = list(ballpark.balls.keys())
        uthread.pool(
            "PopulateTargets",
            self._populateTargets,
            ballpark, ids
        )
    
    def _populateTargets(self, ballpark, ids):
        lst = []
        for ballID in ids:
            slimItem = ballpark.GetInvItem(ballID)
            if slimItem:
               lst.append((ballpark.GetBall(ballID), slimItem))
        
        self._DoBallsAdded(lst)
                
    def DoBallsAdded(self, lst, **kwargs):
        uthread.pool(
            "DoBallsAdded",
            self._DoBallsAdded,
            lst
        )
    
    def _DoBallsAdded(self, lst):
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
            elif ((flag == "StandingGood") or
                  (flag == "StandingHigh") or
                  (flag == "SameGang") or
                  (flag == "SameFleet") or
                  (flag == "Alliance") or
                  (flag == "SameCorp")):
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
