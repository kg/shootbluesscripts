from shootblues.common import log
from shootblues.common.eve import MainThreadInvoker

oldAppend = None
oldGetParams = None
CMGlyphString = None
LabelCore = None

fontScale = 1.0
invoker = None

def doFontChange():
    wnds = [ w for w in (eve.triapp.uilib.desktop.Find("triui.UISprite") + eve.triapp.uilib.desktop.Find("triui.UIContainer")) if hasattr(w, "DoFontChange") ]
    for s in wnds:
        s.DoFontChange()

def setFontScale(newScale):
    global invoker, fontScale
    fontScale = float(newScale or 1.0)    
    invoker = MainThreadInvoker(doFontChange)

def initialize():
    global oldAppend, oldGetParams, CMGlyphString, LabelCore
    log("Installing hook")
    
    def scaleParams(params):
        global fontScale
        newParams = params.Copy()
        size = max(
            int(params.Get("fontSize", None) or 9),
            int(params.Get("fontsize", None) or 9)
        ) * fontScale
        newParams.Set("fontSize", size)
        newParams.Set("fontsize", size)
        return newParams
    
    from svc import font as fontclass
    fontmoduledict = fontclass.AlterParams.im_func.__globals__
    CMGlyphString = fontmoduledict["CMGlyphString"]
    oldAppend = CMGlyphString.Append
    
    def _Append(self, params, *args, **kwargs):
        newParams = scaleParams(params)        
        
        result = oldAppend(self, newParams, *args, **kwargs)
        
        return result
    
    CMGlyphString.Append = _Append
    
    from uicls import LabelCore as labelclass
    LabelCore = labelclass
    oldGetParams = LabelCore.GetParams

    def _GetParams(self, new=0):
        result = oldGetParams(self, new)
        if new:
            self.params = result = scaleParams(result)
        return result
    
    LabelCore.GetParams = _GetParams

def __unload__():
    global oldAppend, oldGetParams, CMGlyphString, LabelCore, invoker
    invoker = None
    if oldAppend and CMGlyphString:
        CMGlyphString.Append = oldAppend
        oldAppend = None
        CMGlyphString = None
    if oldGetParams and LabelCore:
        LabelCore.GetParams = oldGetParams
        oldGetParams = None
        LabelCore = None