import shootblues
from shootblues.common import log
import json

prefs = {}

def notifyPrefsChanged(newPrefsJson):
    prefs = json.loads(newPrefsJson)        

def initialize():
    pass

def __unload__():
    pass