import sys
import json
import types

def resolveKey(obj, key):
    if key.startswith("["):
        indexer = json.loads(key)[0]
        return obj[indexer]
    else:
        return getattr(obj, key)

def getModules():
    return sorted(sys.modules.keys(), key=str.lower)

def resolveContext(context, index):
    obj = __import__(context[0])
    currentKey = []
    for key in context[1:index+1]:
        nextObj = resolveKey(obj, key)       
        currentKey.append(key)
        obj = nextObj
    
    return obj

def getKeys(context, index):
    obj = resolveContext(context, index)
    result = dir(obj)
    
    if isinstance(obj, (types.ListType, types.TupleType)):
        for i in xrange(len(obj)):
            result.append(json.dumps([i]))
    elif isinstance(obj, types.DictType):
        for key in obj.iterkeys():
            result.append(json.dumps([key]))
    elif hasattr(obj, "__keys__"):
        for key in getattr(obj, "__keys__"):
            result.append(json.dumps([key]))
    
    result.sort(key=str.lower)
    
    return result
    
def shortRepr(value, maxLength=512):
    result = repr(value)
    if len(result) > maxLength:
        result = result[0:maxLength - 3] + "..."
    return result

def getValues(context, index, keys):
    obj = resolveContext(context, index)
    result = []
    
    for key in keys:
        value = None
        try:
            value = resolveKey(obj, key)
        except Exception, e:
            value = e
        
        if value is not None:
            value = shortRepr(value)
        else:
            value = "None"
        
        result.append(value)
    
    return result