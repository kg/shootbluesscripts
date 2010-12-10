from shootblues.common import log, getChannel
import json

messageSubscribers = {}

AllMessages = "*"

def send(name, **extraData):
    channel = getChannel("messages")
    if not channel:
        return
        
    data = dict(extraData)
    data["__name__"] = name
    channel.send(json.dumps(data))

def subscribe(handler, name=AllMessages):
    global messageSubscribers
    subscriberList = messageSubscribers.setdefault(name, [])
    if handler not in subscriberList:
        subscriberList.append(handler)    

def unsubscribe(handler, name=AllMessages):
    global messageSubscribers
    subscriberList = messageSubscribers.get(name, [])
    if handler in subscriberList:
        subscriberList.remove(handler)

def callHandler(fn, source, name, data):
    try:
        fn(source, name, data)
    except Exception, e:
        log("Error in message handler: %r", e)

def notifyNewMessage(source, data):
    global messageSubscribers
    messageName = data["__name__"]
    del data["__name__"]
    
    handlers = messageSubscribers.get(AllMessages, [])
    for handler in handlers:
        callHandler(handler, source, messageName, data)        
    
    handlers = messageSubscribers.get(messageName, [])
    for handler in handlers:
        callHandler(handler, source, messageName, data)        