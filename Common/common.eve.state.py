serviceInstance = None

try:
    TargetableCategories = set([
        const.categoryShip, const.categoryDrone, 
        const.categoryEntity, const.categoryStructure,
        const.categoryStation
    ])
except:
    TargetableCategories = set()

class ItemDataCache(object):
    __slots__ = ("id", "_ball", "_slimItem", "_flag", "_flagCached")

    def __init__(self, id, ball=None, slimItem=None):    
        self.id = id
        self._ball = ball
        self._slimItem = slimItem
        self._flag = None
        self._flagCached = False
    
    @property
    def ball(self):
        if not self._ball:
            ballpark = sm.services["michelle"].GetBallpark()
            if not ballpark:
                return None
            
            self._ball = ballpark.GetBall(self.id)
        
        return self._ball
    
    @property
    def slimItem(self):
        if not self._slimItem:
            ballpark = sm.services["michelle"].GetBallpark()
            if not ballpark:
                return None
            
            self._slimItem = ballpark.GetInvItem(self.id)
        
        return self._slimItem
    
    @property
    def radius(self):
        r = None
    
        if self.slimItem:
            r = getattr(self.slimItem, "signatureRadius", None)
            
        if ((r is None) or (r <= 0.0)) and (self.ball):
            r = self.ball.radius
        
        return r
        
    def removedFromBallpark(self):
        self._slimItem = None
        self._ball = None
    
    def clearFlag(self):
        self._flag = None
        self._flagCached = False
    
    @property
    def flag(self):
        if not self._flagCached:
            slimItem = self.slimItem
            self._flag = None
            
            if not slimItem:
                return None
            
            if slimItem.categoryID in TargetableCategories:
                stateSvc = sm.services["state"]
                props = stateSvc.GetProps()
                
                flag = stateSvc.CheckStates(slimItem, "flag")
                if flag:
                    flagProps = props.get(flag, None)
                    if flagProps:
                        self._flag = flagProps[1]
                
                colorFlag = 0
                if slimItem.typeID:
                    itemType = eve.LocalSvc("godma").GetType(slimItem.typeID)
                    for attr in itemType.displayAttributes:
                        if attr.attributeID == const.attributeEntityBracketColour:
                            if attr.value == 1:
                                self._flag = "HostileNPC"
                                break
                            elif attr.value == 0:
                                self._flag = "NeutralNPC"
                                break
            
            self._flagCached = True
        
        return self._flag

def installStateMonitor():
    global serviceInstance
    
    import blue
    import uthread
    from shootblues.common import remoteCall, log
    from shootblues.common.eve import getCharacterName, runOnMainThread
    from shootblues.common.service import forceStart

    class StateMonitorSvc:
        __notifyevents__ = [
            "DoBallsAdded",
            "DoBallRemove",
            "DoBallClear",
            "OnStandingSet",
            "OnStandingsModified",
            "OnSessionChanged"
        ]

        def __init__(self):
            self.forceFields = []
            self.objectCache = {}
            
            runOnMainThread(self.populateBalls)
        
        def dispose(self):
            for ci in self.objectCache.values():
                ci.removedFromBallpark()
                ci.clearFlag()
        
            self.forceFields = []
            self.objectCache = {}
        
        def flushCacheFor(self, id):
            if self.objectCache.has_key(id):
                del self.objectCache[id]
            
        def flushCache(self):
            self.objectCache = {}
        
        def get(self, id, ball=None, slimItem=None):
            result = self.objectCache.get(id, None)
            if not result:
                result = self.objectCache[id] = ItemDataCache(
                    id=id, ball=ball, slimItem=slimItem
                )
            
            return result
        
        def OnStandingSet(self, fromID, toID, standing):
            ce = self.objectCache.get(toID, None)
            if ce:
                ce.clearFlag()
        
        def OnStandingsModified(self, modifications):
            for obj in self.objectCache.values():
                obj.clearFlag()
        
        def OnSessionChanged(self, isRemote, session, change):
            self.flushCache()
                
        def populateBalls(self):    
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            if not ballpark:
                return
            
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
            ballpark = eve.LocalSvc("michelle").GetBallpark()
            if not ballpark:
                return
            
            for (ball, slimItem) in lst:
                if slimItem.groupID == const.groupForceField:
                    self.forceFields.append(slimItem.itemID)
        
        def DoBallRemove(self, ball, slimItem, *args, **kwargs):
            if slimItem and (slimItem.itemID in self.forceFields):
                self.forceFields.remove(slimItem.itemID)
            
            if slimItem:
                ce = self.objectCache.get(slimItem.itemID, None)
                if ce:
                    ce.removedFromBallpark()
        
        def DoBallClear(self, solItem, **kwargs):
            self.forceFields = []
            
            for ce in self.objectCache.values():
                ce.removedFromBallpark()
            
    serviceInstance = forceStart("statemonitor", StateMonitorSvc)

def getCachedItem(id=None, ball=None, slimItem=None):
    if (not id):
        if slimItem:
            id = slimItem.itemID
        else:
            raise Exception("Must specify an id or a slimItem")

    if not serviceInstance:
        result = ItemDataCache(id, ball, slimItem)
    else:
        result = serviceInstance.get(id, ball, slimItem)
    
    return result

def isItemInsideForceField(itemID):
    from shootblues.common import log
    
    if not serviceInstance:
        return False
    
    ballpark = eve.LocalSvc("michelle").GetBallpark()    
    for id in serviceInstance.forceFields:
        distance = ballpark.DistanceBetween(itemID, id)
        if distance <= 0:
            return True
    
    return False

def __unload__():
    global serviceInstance
    if serviceInstance:
        serviceInstance.dispose()
        from shootblues.common.service import forceStop
        forceStop("statemonitor")
        serviceInstance = None