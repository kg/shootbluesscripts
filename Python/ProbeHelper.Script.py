import uix
import uiconst
import form.Scanner
from foo import Vector3
from shootblues.common import log

if (getattr(form.Scanner.ApplyAttributes, "shootblues", None) is not 420):
    old_apply_attributes = form.Scanner.ApplyAttributes
else:
    log("probehelper already in a partially initialized state! :(")

def ContractProbes(self, *args):
    probeData = sm.GetService("scanSvc").GetProbeData()
    if probeData == {}:
        return
    
    avg = Vector3( 0,0,0 )
    min_range = const.scanProbeNumberOfRangeSteps
    for key in probeData:
        min_range = min( min_range, probeData[key].rangeStep )
        avg = avg + probeData[key].destination
    if (min_range <= 1):
        return
    avg = avg / len(probeData)

    for key in probeData:
        destination = (probeData[key].destination + avg)/2
        sm.GetService("scanSvc").SetProbeDestination(key, destination)
        sm.GetService("scanSvc").SetProbeRangeStep(key, probeData[key].rangeStep - 1)
    self.UpdateProbeSpheres()

form.Scanner.ContractProbes = ContractProbes

def ExpandProbes(self, *args):
    probeData = sm.GetService("scanSvc").GetProbeData()
    if probeData == {}:
        return

    avg = Vector3( 0,0,0 )
    max_range = 1
    for key in probeData:
        max_range = max(max_range, probeData[key].rangeStep)
        avg = avg + probeData[key].destination
    if (max_range >= const.scanProbeNumberOfRangeSteps):
        return
    avg = avg / len(probeData)
    
    for key in probeData:
        destination = (2*probeData[key].destination) - avg
        sm.GetService("scanSvc").SetProbeDestination(key, destination)
        sm.GetService("scanSvc").SetProbeRangeStep(key, probeData[key].rangeStep + 1)
    self.UpdateProbeSpheres()

form.Scanner.ExpandProbes = ExpandProbes
    
def SendProbes(self, *args):
    selected = self.sr.resultscroll.GetSelected()
    try:
        data = selected[0].result.data
    except:
        return
        
    if isinstance(data, float):
        data = selected[0].result.pos
    
    if not isinstance(data, Vector3):
        data = data.point
 
    probeData = sm.GetService("scanSvc").GetProbeData()
    if probeData == {}:
        return

    avg = Vector3( 0,0,0 )
    for key in probeData:
       avg = avg + probeData[key].destination
    avg = avg / len(probeData)

    for key in probeData:
        destination = data + probeData[key].destination - avg
        sm.GetService("scanSvc").SetProbeDestination(key, destination)
    self.UpdateProbeSpheres()

form.Scanner.SendProbes = SendProbes

def SaveLoadProbePositions(self, *args):
    probeData = sm.GetService("scanSvc").GetProbeData()
    if probeData == {}:
        return

    avg = Vector3( 0,0,0 )
    for key in probeData:
       avg = avg + probeData[key].destination
    avg = avg / len(probeData)

    shift = uicore.uilib.Key(uiconst.VK_SHIFT)
    if( shift ):
        pos = []
        for key in probeData:
            pos.append( [probeData[key].destination - avg, probeData[key].rangeStep] )
        settings.public.ui.Set('ProbePositions', pos)
        return
    
    pos = settings.public.ui.Get('ProbePositions', [])
    if( pos == [] ):
        return
        
    i = 0
    for key in probeData:
        sm.GetService("scanSvc").SetProbeDestination(key, pos[i][0] + avg)
        sm.GetService("scanSvc").SetProbeRangeStep(key, pos[i][1])
        i = i + 1
        if( i >= len(pos) ):
            break
    self.UpdateProbeSpheres()
        
form.Scanner.SaveLoadProbePositions = SaveLoadProbePositions

def ApplyAttributes(self, attributes):
    old_apply_attributes(self, attributes)
    
    self.sr.destroyBtn.Close()
    btn = uix.GetBigButton(32, self.sr.systemTopParent, left=108)
    btn.OnClick = self.SaveLoadProbePositions
    btn.hint = "SHIFT-CLICK TO SAVE PROBES, CLICK TO LOAD PROBES"
    uix.MapSprite('44_03', btn.sr.icon)
    self.sr.saveloadBtn = btn

    btn = uix.GetBigButton(32, self.sr.systemTopParent, left=152)
    btn.OnClick = self.ContractProbes
    btn.hint = "CONTRACT PROBES"
    uix.MapSprite('44_44', btn.sr.icon)
    self.sr.contractBtn = btn
 
    btn = uix.GetBigButton(32, self.sr.systemTopParent, left=184)
    btn.OnClick = self.ExpandProbes
    btn.hint = "EXPAND PROBES"
    uix.MapSprite('44_43', btn.sr.icon)
    self.sr.expandBtn = btn

    btn = uix.GetBigButton(32, self.sr.systemTopParent, left=228)
    btn.OnClick = self.SendProbes
    btn.hint = "SEND PROBES TO SELECTED RESULT"
    uix.MapSprite('44_59', btn.sr.icon)
    self.sr.sendBtn = btn
    
setattr(ApplyAttributes, "shootblues", 420)
form.Scanner.ApplyAttributes = ApplyAttributes

def __unload__():    
    form.Scanner.ApplyAttributes = old_apply_attributes