import sys
import json
import types
import weakref
import inspect

def tupleize(i):
    if isinstance(i, types.ListType):
        return tuple(tupleize(x) for x in i)
    else:
        return i

def resolveKey(obj, key):
    if key == "<Arguments>":
        result = inspect.getargspec(obj)
    elif key.startswith("["):
        indexer = tupleize(json.loads(key)[0])
        result = obj[indexer]
    else:
        result = getattr(obj, key)
    
    if isinstance(result, weakref.ref):
        result = result()
    
    return result

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
    
    if callable(obj):
        result.append("<Arguments>")
    
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