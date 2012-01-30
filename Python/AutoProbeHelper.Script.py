from shootblues import Dependency
Dependency("Common.Script.dll")

import uix
import uiconst
import blue
import math
import form.Scanner
from foo import Vector3
from shootblues.common import log, showException
from shootblues.common.eve import SafeTimer, canActivateModule, activateModule, findModule, runOnMainThread, getModuleAttributes, getTypeAttributes
from shootblues.common.service import forceStart, forceStop
from shootblues.common.eve.state import isItemInsideForceField
from math import pow, sqrt, exp

serviceInstance = None
serviceRunning = False

if (getattr(form.Scanner.ApplyAttributes, "shootblues", None) is not 420):
	old_apply_attributes = form.Scanner.ApplyAttributes
else:
	log("probehelper already in a partially initialized state! :(")

def GetSensorStrength():
	module = findModule("Scan Probe Launcher")
	godma = eve.LocalSvc("godma")
	moduleAttrs = getModuleAttributes(module)
	if getattr(module, "charge", None):
	    chargeObj = godma.GetItem(module.charge.itemID)
	    chargeAttrs = getTypeAttributes(module.charge.typeID, obj=chargeObj)
	else:        
	    chargeAttrs = {}
	if chargeAttrs['baseSensorStrength']:
		return chargeAttrs['baseSensorStrength']
	else:
		return 0
	
def GuessClass():
	# doesn't function well with mixed-range probes
	# probably breaks horribly with >4 probes as well!
	sr = sm.services.get('scanSvc').GetScanResults()
	pd = sm.services.get('scanSvc').GetProbeData()
	probepos = {}
	probestep = {}
	sensor = GetSensorStrength()/100
	
	def dist(a,b):
		return sqrt(pow((a[0] - b[0]),2) + pow((a[1] - b[1]),2) + pow((a[2] - b[2]),2))
		
	for key in pd:
		probepos[key] = pd[key].pos
		probestep[key] = pd[key].rangeStep

	#selected = sm.services.get('window', None).GetWindow('scanner').sr.resultscroll.GetSelected()
	try:
		selected = sm.services.get('window', None).GetWindow('scanner').sr.resultscroll.GetNodes()[1]
		if selected.result:
			data = selected.result.data
		else:
			return
	except:
		showException()
		return
	
	if isinstance(data, float):
		data = selected.result.pos
	
	if not isinstance(data, Vector3):
		data = data.point
	
	probehits = selected.result.probeID
	if not isinstance(probehits, long):
		numprobes = len(probehits)
	else:
		numprobes  = 1
		
	sumstr = 0
	if numprobes > 3:
		for probeval in probehits:
			range = 0.125*pow(2,probestep[key])
			sumstr += exp(-0.915*pow(dist(probepos[probeval],data)/(range*149598000000),2))
			#log("sum of probe strengths: %s", sumstr)
		intrinsic = selected.result.certainty*2*100*2/(sensor*sumstr)
		log("intrinsic core: %s", intrinsic)
	elif numprobes > 2:
		for probeval in probehits:
			range = 0.125*pow(2,probestep[key])
			sumstr += exp(-0.915*pow(dist(probepos[probeval],data)/(range*149598000000),2))
			#log("sum of probe strengths: %s", sumstr)
		intrinsic = selected.result.certainty*numprobes*100*2/(sensor*sumstr)
		log("intrinsic core: %s", intrinsic)
	elif numprobes > 1:
		for probeval in probehits:
			range = 0.125*pow(2,probestep[key])
			sumstr += exp(-0.915*pow(dist(probepos[probeval],data)/(range*149598000000),2))
			#log("sum of probe strengths: %s", sumstr)
		intrinsic = selected.result.certainty*100*2/(sensor*sumstr)
		log("intrinsic core: %s", intrinsic)
	else:
		range = 0.125*pow(2,probestep[key])
		sumstr = exp(-0.915*pow(selected.result.data/(range*149598000000),2))
		intrinsic = selected.result.certainty*100*2/(sensor*sumstr)
		#log("sum of probe strengths: %s", sumstr)
		log("intrinsic core: %s", intrinsic)

	siteclass = 0
	if (intrinsic > 1.8) or (intrinsic < 1.7):
		siteclass += round(math.log(intrinsic,2)+1,1)
	else:
		siteclass += 1.5
	log("Site Class: %s", siteclass)
	
	
def initialScan():
	# enumerate planets!
	main = __import__("__main__")
	ballpark = sm.GetService("michelle").GetBallpark(doWait=True)
	if ballpark is None:
		log("Can't play ball without a ballpark!")
		return
	else:
		ball = ballpark.GetBall(eve.session.shipid)
		mypos = [ball.x, ball.y, ball.z]
		plocs = []
		planetlocs = []
		itpos = [0,0,0,0]
		for itemID in ballpark.balls.keys():
			if ballpark is None:
				log("Ballpark disappeared while playing with balls!")
				break
			if itemID == eve.session.shipid:
				pass
			else:
				slimItem = ballpark.GetInvItem(itemID)
				ball = ballpark.GetBall(itemID)
				if not (ball and slimItem):
					pass
				else:
					blue.pyos.BeNice(100)
					name = uix.GetSlimItemName(slimItem) or "Some object"

					if slimItem.groupID == 7:
						dst = sqrt(pow(ball.x,2) + pow(ball.y,2) + pow(ball.z,2))
						itpos = [ball.x, ball.y, ball.z, dst]
						plocs.append(itpos)
						#log("item: %s - %s - %s", name, itemID, dst)

	maxprobes = getMaxProbes()
	offset = [50000000000.0, 0.0, 0.0, 0.0]
	planetlocs = sorted(plocs, key=lambda plocs: plocs[3], reverse=True)
	planetlocs.insert(0, offset)
	main.plocs = planetlocs
	main.plen = len(planetlocs)

	probeData = sm.GetService("scanSvc").GetProbeData()
	if probeData == {}:
		return
		
	i = 0
	
	for key in probeData:
		sm.GetService("scanSvc").SetProbeDestination(key, [planetlocs[i][0] + offset[0], planetlocs[i][1] + offset[1], planetlocs[i][2] + offset[2]])
		sm.GetService("scanSvc").SetProbeRangeStep(key, 8)
		i = i + 1
		if( i >= len(planetlocs) ):
			break
	
	sm.services.get('window', None).GetWindow('scanner').UpdateProbeSpheres()

def getMaxProbes():
	astrosp = sm.services.get('godma',None).GetItem(eve.session.charid).skills[3412].skillPoints
	if astrosp == 768000:
		maxprobes = 8
	elif astrosp > 135764:
		maxprobes = 7
	elif astrosp > 23999:
		maxprobes = 6
	elif astrosp > 4241:
		maxprobes = 5
	else:
		maxprobes = 4
	return maxprobes
	
def constructBest(center, deviation):
	return
	
	
	
class AutoScanSvc:
	def __init__(self):
		self.disabled = False
		self.probesset = False
		self.firstscan = False
		self.scanstep = 8
		self.lastcert = 0
		self.__updateTimer = SafeTimer(2334, self.AutoScanTimer)

	def AutoScanTimer(self):
		ss = sm.services.get('scanSvc', None)
		if self.disabled:
			self.__updateTimer = None
			#sm.services.get('window', None).GetWindow('scanner').StopAuto()
			return
		if ss and not ss.IsScanning():
			self.disabled = self.AutoScan()
		if self.disabled:
			self.__updateTimer = None
			#sm.services.get('window', None).GetWindow('scanner').StopAuto()
			return
				

	def sendToResult(self):
		selected =  sm.services.get('window', None).GetWindow('scanner').sr.resultscroll.GetNodes()
		try:
			data = selected[1]["result"].data
		except:
			return
		
		if isinstance(data, float):
			data = selected[1]["result"].pos
		
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
		sm.services.get('window', None).GetWindow('scanner').UpdateProbeSpheres()

	
	def AutoScan(self):
		#try:
		from shootblues.common import log
		probeData = sm.GetService("scanSvc").GetProbeData()
		numprobes = len(probeData)
		maxprobes = getMaxProbes()

		main = __import__("__main__")
		if numprobes < maxprobes:
			if isItemInsideForceField(eve.session.shipid):
				self.disabled = True
				return
			launchProbe()
			self.probesset = False
		elif not self.probesset:
			self.probesset = True
			self.firstscan = True
			self.scanstep = 8
			initialScan()
			scanProbes()
		elif self.firstscan:
			if sm.services.get('window',None).GetWindow('scanner').sr:
				log("Total sites found: %s", (len(sm.services.get('window',None).GetWindow('scanner').sr.resultscroll.GetNodes()) - 2))
				#GuessClass()
				if len(sm.services.get('window',None).GetWindow('scanner').sr.resultscroll.GetNodes()) < 3:
					main.errorthrow = sm.services.get('window',None).GetWindow('scanner').sr.resultscroll.GetNodes()
					log("No results!")
					self.disabled = True
					return True
				setupProbes()
				self.sendToResult()
				scanProbes()
				self.firstscan = False
			else:
				log("No results!")
				self.disabled = True
				return True
				
		elif self.probesset and not self.firstscan:
			curcert = sm.services.get('window', None).GetWindow('scanner').sr.resultscroll.GetNodes()[1].result.certainty
			if self.lastcert == 1 or curcert == 1:
				self.probesset == False
				#recoverProbes()
				log("Got it!")
				self.disabled = True
				return True
			elif self.scanstep == -1 and not self.lastcert == 1:
				self.probesset == False
				#recoverProbes()
				log("Can't get it! :(")
				self.disabled = True
				return True
			else:	
				if self.lastcert < curcert:
					self.lastcert = curcert
					log("Scanstep %s, Certainty at %s, stepping down", self.scanstep, curcert)
					sm.services.get('window', None).GetWindow('scanner').ContractProbes()
					self.scanstep -= 1
					self.sendToResult()
					scanProbes()
				else:
					self.lastcert = curcert
					log("Scanstep %s, Certainty at %s, stepping up", self.scanstep, curcert)
					sm.services.get('window', None).GetWindow('scanner').ExpandProbes()
					self.scanstep += 1
					if self.scanstep > 8:
						self.scanstep = 8
					self.sendToResult()
					scanProbes()
		return(False)
		#~ except Exception, e:
			#~ showException()
			
def Automate(self, *args):
	global serviceRunning, serviceInstance
	serviceRunning = True
	serviceInstance = forceStart("autoscan", AutoScanSvc)
	log("Starting up Autoscanner")
form.Scanner.Automate = Automate

def launchProbe():
	modtype = "Scan Probe Launcher"
	launcher = findModule(modtype)
	if canActivateModule(launcher):
		activateModule(launcher, pulse=True)

def setupProbes():
	probeData = sm.GetService("scanSvc").GetProbeData()
	if probeData == {}:
		return

	maxprobes = getMaxProbes()

	ideallocs = {}
	scanstep = {}
	au = 149598000*1000

	if maxprobes == 8:
		ideallocs[0] = Vector3(-13*au, 0, 0)
		ideallocs[1] = Vector3(13*au, 0, 0)
		ideallocs[2] = Vector3(0,0,-13*au)
		ideallocs[3] = Vector3(0, 0, 13*au)
		ideallocs[4] = Vector3(-1.625*au, 0,0)
		ideallocs[5] = Vector3(1.625*au, 0 ,0)
		ideallocs[6] = Vector3(0, 0, -1.625*au)
		ideallocs[7] = Vector3(0, 0, 1.625*au)
		scanstep[0] = 8
		scanstep[1] = 8
		scanstep[2] = 8
		scanstep[3] = 8
		scanstep[4] = 5
		scanstep[5] = 5
		scanstep[6] = 5
		scanstep[7] = 5

	if maxprobes == 7:
		ideallocs[0] = Vector3(-13*au, 0, 0)
		ideallocs[1] = Vector3(13*au, 0, 0)
		ideallocs[2] = Vector3(0,-13*au, 0)
		ideallocs[3] = Vector3(0, 13*au, 0)
		ideallocs[4] = Vector3(0, 0, -13*au)
		ideallocs[5] = Vector3(0, 0, 13*au)
		ideallocs[6] = Vector3(0, 0, .1*au)
		scanstep[0] = 8
		scanstep[1] = 8
		scanstep[2] = 8
		scanstep[3] = 8
		scanstep[4] = 8
		scanstep[5] = 8
		scanstep[6] = 6

	if maxprobes == 6:
		ideallocs[0] = Vector3(-13*au, 0, 0)
		ideallocs[1] = Vector3(13*au, 0, 0)
		ideallocs[2] = Vector3(0,-13*au, 0)
		ideallocs[3] = Vector3(0, 13*au, 0)
		ideallocs[4] = Vector3(0, 0, -13*au)
		ideallocs[5] = Vector3(0, 0, 13*au)
		scanstep[0] = 8
		scanstep[1] = 8
		scanstep[2] = 8
		scanstep[3] = 8
		scanstep[4] = 8
		scanstep[5] = 8

	if maxprobes == 5:
		ideallocs[0] = Vector3(-13*au, 0, 0)
		ideallocs[1] = Vector3(13*au, 0, 0)
		ideallocs[2] = Vector3(0,-13*au, 0)
		ideallocs[3] = Vector3(0, 13*au, 0)
		ideallocs[4] = Vector3(0, 0, .1*au)
		scanstep[0] = 8
		scanstep[1] = 8
		scanstep[2] = 8
		scanstep[3] = 8
		scanstep[4] = 6

	if maxprobes == 4:
		ideallocs[0] = Vector3(-13*au, 0, 0)
		ideallocs[1] = Vector3(13*au, 0, 0)
		ideallocs[2] = Vector3(0,-13*au, 0)
		ideallocs[3] = Vector3(0, 13*au, 0)
		scanstep[0] = 8
		scanstep[1] = 8
		scanstep[2] = 8
		scanstep[3] = 8

	i = 0
	for key in probeData:
		sm.GetService("scanSvc").SetProbeDestination(key, ideallocs[i])
		sm.GetService("scanSvc").SetProbeRangeStep(key, scanstep[i])
		i = i + 1
		if( i >= len(ideallocs) ):
			break

def scanProbes():
	sm.services.get('window', None).GetWindow('scanner').Analyze()

def recoverProbes():
	sm.services.get('window', None).GetWindow('scanner').RecoverActiveProbes()

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

def SetupProbes(self, *args):
	setupProbes()
	#initialScan()
	self.UpdateProbeSpheres()

form.Scanner.SetupProbes = SetupProbes

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

def StopAuto(self, *args):
	global serviceRunning, serviceInstance
	if serviceInstance:
		serviceInstance.disabled = True
		serviceInstance = None
	if serviceRunning:
		
		forceStop("autoscan")
		serviceRunning = False
form.Scanner.StopAuto= StopAuto
	

def ApplyAttributes(self, attributes):
	old_apply_attributes(self, attributes)
	
	self.sr.destroyBtn.Close()
	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=108)
	btn.OnClick = self.SaveLoadProbePositions
	btn.hint = "SHIFT-CLICK TO SAVE PROBES, CLICK TO LOAD PROBES"
	btn.sr.icon.LoadIcon('44_03')
	self.sr.saveloadBtn = btn

	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=152)
	btn.OnClick = self.ContractProbes
	btn.hint = "CONTRACT PROBES"
	btn.sr.icon.LoadIcon('44_44')
	self.sr.contractBtn = btn
 
	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=184)
	btn.OnClick = self.ExpandProbes
	btn.hint = "EXPAND PROBES"
	btn.sr.icon.LoadIcon('44_43')
	self.sr.expandBtn = btn

	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=228)
	btn.OnClick = self.SendProbes
	btn.hint = "SEND PROBES TO SELECTED RESULT"
	btn.sr.icon.LoadIcon('44_59')
	self.sr.sendBtn = btn

	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=272)
	btn.OnClick = self.SetupProbes
	btn.hint = "SETUP PROBES"
	btn.sr.icon.LoadIcon('44_03')
	self.sr.setBtn = btn

	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=316)
	btn.OnClick = self.Automate
	btn.hint = "AUTOSCAN"
	btn.sr.icon.LoadIcon('44_04')
	self.sr.autoBtn = btn

	btn = uix.GetBigButton(32, self.sr.systemTopParent, left=348)
	btn.OnClick = self.StopAuto
	btn.hint = "STOP AUTOSCAN"
	btn.sr.icon.LoadIcon('44_07')
	self.sr.stopautoBtn = btn
	
setattr(ApplyAttributes, "shootblues", 420)
form.Scanner.ApplyAttributes = ApplyAttributes
		
def __unload__():    
	form.Scanner.ApplyAttributes = old_apply_attributes
	global serviceRunning, serviceInstance
	if serviceInstance:
		serviceInstance.disabled = True
		serviceInstance = None
	if serviceRunning:
		forceStop("autoscan")
		serviceRunning = False

