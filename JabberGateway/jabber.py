from shootblues.common import log, remoteCall
import json

def initialize():
    pass

def send(endpoint, text):
    remoteCall("JabberGateway.Script.dll", "Send", str(endpoint), str(text))

def __unload__():
    pass