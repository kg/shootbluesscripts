import shootblues
import shootblues.common

def getLockedTargets():
    ballpark = eve.LocalSvc('michelle').GetBallpark()
    targetSvc = sm.services['target']
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]