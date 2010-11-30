from shootblues.common import log
from shootblues.common.eve import runOnMainThread

oldAppend = None
oldGetParams = None
CMGlyphString = None
LabelCore = None

fontScale = 1.0
fontWidth = 1.0

def doFontChange():
    import form
    channel = form.LSCChannel
    wnds = [w for w in (
        eve.triapp.uilib.desktop.Find("triui.UISprite") + 
        eve.triapp.uilib.desktop.Find("triui.UIContainer")
    ) if (
        hasattr(w, "DoFontChange") or
        getattr(w, "__class__", None) is channel
    )]
    for s in wnds:
        if getattr(s, "__class__", None) is channel:
            s.ChangeFont()
        else:
            s.DoFontChange()

def setFontSize(newScale, newWidth):
    global fontScale, fontWidth
    fontScale = float(newScale or 1.0)
    fontWidth = float(newWidth or 1.0)
    runOnMainThread(doFontChange)

def initialize():
    global oldAppend, oldGetParams, CMGlyphString, LabelCore
    
    def scaleParams(params):
        global fontScale, fontWidth
        newParams = params.Copy()
        
        size = max(
            float(params.Get("fontSize", None) or 9),
            float(params.Get("fontsize", None) or 9)
        ) * fontScale
        newParams.Set("fontSize", size)
        newParams.Set("fontsize", size)
        
        if getattr(settings, "user", None) and getattr(settings.user, "ui", None):
            defaultWidth = {
                "condensed": 0.14999999999999999,
                "normal": 0.5,
                "expanded": 0.90000000000000002
            }.get(
                settings.user.ui.get("fontWFactor", "normal"), 
                0.5
            )
        else:
            defaultWidth = 0.5
        
        width = max(
            float(params.Get("width", None) or defaultWidth),
            float(params.Get("mmwidth", None) or defaultWidth)
        ) * fontWidth
        newParams.Set("width", width)
        newParams.Set("mmwidth", width)
        
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
    global oldAppend, oldGetParams, CMGlyphString, LabelCore
    if oldAppend and CMGlyphString:
        CMGlyphString.Append = oldAppend
        oldAppend = None
        CMGlyphString = None
    if oldGetParams and LabelCore:
        LabelCore.GetParams = oldGetParams
        oldGetParams = None
        LabelCore = None