using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 轻松版界面工具：纸张卡片、按钮、自动测量高度的文字和竖向排版。
namespace HarbourYear.Lite
{
    public enum Look2{Primary,Secondary,Text,Danger,Selected,Tab,Pill,Plain}
    public static class UI
    {
        public static TMP_FontAsset Font;public static float Scale=1;
        public static Color Cream=Hex("FBF4E6"),Cream2=Hex("F3E6CF"),Brown=Hex("4B3428"),Brown2=Hex("7A5A44"),Brick=Hex("B5473B"),Brick2=Hex("9C3A30"),Sage=Hex("7D9A6E"),Ink=Hex("3E2C22"),Muted=Hex("7D6858"),Line=Hex("D8CBB1"),
            Paper=Hex("F5EFDF"),ButtonBg=Hex("FCF8EE"),Gold=Hex("B79A63"),Warn=Hex("A7432F"),Good=Hex("4F7A43"),AlertBg=Hex("F8E3D6"),AlertLine=Hex("E2B7A2"),White=Hex("FFF6E8"),Shade=new Color(.16f,.10f,.07f,.72f);
        public static Color Hex(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c;}
        public static string Rich(Color c){return "#"+ColorUtility.ToHtmlStringRGB(c);}
        public static string Safe(string s){return s==null?"":s.Replace("<","＜").Replace(">","＞");}
        static readonly Dictionary<int,Sprite> rounds=new Dictionary<int,Sprite>();static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();static readonly Dictionary<string,Sprite> sliced=new Dictionary<string,Sprite>();
        static Dictionary<string,Vector2> sizes;
        // 导入时贴图被放大到 2 的幂（为了压缩与 mipmap），显示比例以原图尺寸为准。
        public static Vector2 Size(Texture t)
        {
            if(t==null)return Vector2.one;if(sizes==null){sizes=new Dictionary<string,Vector2>();var txt=Resources.Load<TextAsset>("Lite/sizes");if(txt!=null)foreach(var line in txt.text.Split('\n')){var p=line.Trim().Split(' ');if(p.Length==3&&!p[0].StartsWith("#"))sizes[p[0]]=new Vector2(float.Parse(p[1]),float.Parse(p[2]));}}
            Vector2 v;return sizes.TryGetValue(t.name,out v)?v:new Vector2(t.width,t.height);
        }
        public static float Aspect(Texture t){var s=Size(t);return s.x/s.y;}
        public static Texture2D Tex(string path){Texture2D t;if(!textures.TryGetValue(path,out t)){t=Resources.Load<Texture2D>("Lite/"+path);textures[path]=t;}return t;}
        public static Sprite Rounded(int r)
        {
            Sprite s;if(rounds.TryGetValue(r,out s))return s;int n=r*2+4;var t=new Texture2D(n,n,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};var px=new Color32[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float cx=Mathf.Clamp(x+.5f,r,n-r),cy=Mathf.Clamp(y+.5f,r,n-r);float d=Mathf.Sqrt((x+.5f-cx)*(x+.5f-cx)+(y+.5f-cy)*(y+.5f-cy));
                px[y*n+x]=new Color32(255,255,255,(byte)(255*Mathf.Clamp01(r-d+.5f)));
            }
            t.SetPixels32(px);t.Apply();s=Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(r+1,r+1,r+1,r+1));rounds[r]=s;return s;
        }
        public static Sprite Slice(string path,Vector4 border)
        {
            Sprite s;var key=path+border;if(sliced.TryGetValue(key,out s))return s;var t=Tex(path);if(t==null)return null;
            var o=Size(t);float sx=t.width/o.x,sy=t.height/o.y;s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(border.x*sx,border.y*sy,border.z*sx,border.w*sy));sliced[key]=s;return s;
        }
        public static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=(RectTransform)o.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static RectTransform Stretch(string name,Transform parent)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=(RectTransform)o.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        public static RectTransform Anchor(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=(RectTransform)o.transform;r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        public static Image Fill(RectTransform r,Color c,int radius=0){var im=r.gameObject.AddComponent<Image>();im.color=c;im.raycastTarget=false;if(radius>0){im.sprite=Rounded(radius);im.type=Image.Type.Sliced;}return im;}
        public static Image Backdrop(RectTransform r,Color c,int radius=0){var b=Stretch("Backdrop",r);b.SetAsFirstSibling();return Fill(b,c,radius);}
        public static Image Box(Transform p,float x,float y,float w,float h,Color c,int radius=0,string name="Box"){return Fill(Rect(name,p,x,y,w,h),c,radius);}
        public static Image Frame(Transform p,float x,float y,float w,float h,Color fill,Color border,int radius=10,float bw=2,string name="Card")
        {
            var outer=Box(p,x,y,w,h,border,radius,name);var inner=Stretch("Fill",outer.transform);inner.offsetMin=new Vector2(bw,bw);inner.offsetMax=new Vector2(-bw,-bw);Fill(inner,fill,Mathf.Max(1,radius-(int)bw));return outer;
        }
        public static TextMeshProUGUI Text(Transform p,string value,float x,float y,float w,float h,float size=19,Color? c=null,bool bold=false,TextAlignmentOptions align=TextAlignmentOptions.TopLeft,bool fit=false)
        {
            var r=Rect("Text",p,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=value;t.fontSize=size*Scale;t.color=c??Ink;t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;t.alignment=align;t.raycastTarget=false;
            t.enableWordWrapping=true;t.margin=Vector4.zero;t.lineSpacing=8;t.richText=true;
            if(fit){t.enableAutoSizing=true;t.fontSizeMax=size*Scale;t.fontSizeMin=size*Scale*.72f;t.overflowMode=TextOverflowModes.Ellipsis;}else t.overflowMode=TextOverflowModes.Overflow;return t;
        }
        public static float Measure(TextMeshProUGUI t,float w){return Mathf.Ceil(t.GetPreferredValues(t.text,w,0).y)+2;}
        public static float Width(string s,float size,bool bold=false)
        {
            var probe=Probe();probe.fontSize=size*Scale;probe.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;return Mathf.Ceil(probe.GetPreferredValues(s,9999,0).x)+2;
        }
        static TextMeshProUGUI probeText;
        static TextMeshProUGUI Probe()
        {
            if(probeText==null){var go=new GameObject("Text probe",typeof(RectTransform));UnityEngine.Object.DontDestroyOnLoad(go);probeText=go.AddComponent<TextMeshProUGUI>();probeText.font=Font;probeText.enableWordWrapping=false;go.SetActive(false);}
            return probeText;
        }
        public static RawImage Picture(Transform p,Texture tex,float x,float y,float w,float h,bool cover=true,float focusY=.5f)
        {
            var r=Rect("Picture",p,x,y,w,h);var im=r.gameObject.AddComponent<RawImage>();im.texture=tex;im.raycastTarget=false;if(tex==null){im.color=Line;return im;}
            float ir=Aspect(tex),fr=w/h;
            if(cover){if(fr>ir){float v=ir/fr;im.uvRect=new Rect(0,(1-v)*(1-focusY),1,v);}else{float u=fr/ir;im.uvRect=new Rect((1-u)*.5f,0,u,1);}}
            else{if(fr>ir){float nw=h*ir;r.anchoredPosition=new Vector2(x+(w-nw)/2,-y);r.sizeDelta=new Vector2(nw,h);}else{float nh=w/ir;r.anchoredPosition=new Vector2(x,-(y+(h-nh)/2));r.sizeDelta=new Vector2(w,nh);}}
            return im;
        }
        public static void Cover(RawImage im,float w,float h,float focusY=.5f)
        {
            var tex=im.texture;if(tex==null)return;float ir=Aspect(tex),fr=w/h;
            if(fr>ir){float v=ir/fr;im.uvRect=new Rect(0,(1-v)*(1-focusY),1,v);}else{float u=fr/ir;im.uvRect=new Rect((1-u)*.5f,0,u,1);}
        }
        public static Action<string> Sound=s=>LiteAudio.Sfx(s);
        public static Button Btn(Transform p,string label,float x,float y,float w,float h,Action click,Look2 style=Look2.Secondary,bool enabled=true,string id=null,string sfx="click",float size=18)
        {
            var r=Rect(id??label,p,x,y,w,h);Color fill,border,text;int radius=style==Look2.Pill||style==Look2.Tab?(int)Mathf.Min(h/2,22):7;
            switch(style)
            {
                case Look2.Primary:fill=Brick;border=Brick2;text=White;break;case Look2.Danger:fill=Brick2;border=Hex("7E2C24");text=White;break;
                case Look2.Selected:fill=Brown2;border=Brown;text=White;break;case Look2.Tab:fill=Hex("FBEFE0");border=Hex("D8B39A");text=Hex("5A2A20");break;
                case Look2.Pill:fill=Hex("FFF6EA");border=Hex("D8B39A");text=Hex("5A2A20");break;case Look2.Text:fill=new Color(0,0,0,0);border=new Color(0,0,0,0);text=Brick;break;
                case Look2.Plain:fill=new Color(1,1,1,.001f);border=new Color(0,0,0,0);text=Ink;break;default:fill=ButtonBg;border=Hex("C9AD84");text=Hex("5A3A2A");break;
            }
            var bg=Fill(r,border,radius);bg.raycastTarget=true;if(style!=Look2.Text&&style!=Look2.Plain){var inner=Stretch("Fill",r);inner.offsetMin=new Vector2(1.5f,1.5f);inner.offsetMax=new Vector2(-1.5f,-1.5f);Fill(inner,fill,Mathf.Max(1,radius-1));}
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=bg;b.interactable=enabled;var cb=b.colors;cb.normalColor=Color.white;cb.highlightedColor=new Color(1.08f,1.04f,.98f);cb.pressedColor=new Color(.86f,.84f,.82f);cb.selectedColor=Color.white;cb.disabledColor=new Color(.78f,.76f,.74f,.55f);b.colors=cb;
            if(!enabled)foreach(var g in r.GetComponentsInChildren<Image>())g.color=new Color(g.color.r,g.color.g,g.color.b,g.color.a*.55f);
            if(label!=null){float pad=Mathf.Min(10,w*.12f);var t=Text(r,label,pad,0,w-pad*2,h,size,enabled?text:new Color(text.r,text.g,text.b,.5f),style==Look2.Primary||style==Look2.Selected||style==Look2.Danger,TextAlignmentOptions.Center,true);t.rectTransform.anchoredPosition=new Vector2(pad,0);if(style==Look2.Text)t.fontStyle|=FontStyles.Underline;}
            if(click!=null)b.onClick.AddListener(()=>{if(sfx!="none")Sound(sfx=="click"?"sfx_click":"sfx_"+sfx);click();});return b;
        }
        public static Button ImageButton(Transform p,Texture tex,float x,float y,float w,float h,Action click,bool enabled=true,string id="ImageButton",string sfx="open",bool hover=true)
        {
            var r=Rect(id,p,x,y,w,h);var im=r.gameObject.AddComponent<RawImage>();im.texture=tex;im.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;b.interactable=enabled;
            var cb=b.colors;cb.normalColor=Color.white;cb.highlightedColor=new Color(1.05f,1.05f,1.05f);cb.pressedColor=new Color(.92f,.92f,.92f);cb.selectedColor=Color.white;cb.disabledColor=new Color(.62f,.6f,.6f,.62f);b.colors=cb;
            if(hover&&enabled)r.gameObject.AddComponent<HoverScale>();if(click!=null)b.onClick.AddListener(()=>{if(sfx!="none")Sound("sfx_"+sfx);click();});return b;
        }
        public static void Bar(Transform p,float x,float y,float w,float h,float value,Color c,Color? bg=null)
        {
            var back=Box(p,x,y,w,h,bg??Hex("E3D5BD"),(int)Mathf.Max(1,h/2));if(value>0)Box(back.transform,0,0,Mathf.Max(h,w*Mathf.Clamp01(value/100f)),h,c,(int)Mathf.Max(1,h/2));
        }
        public static RectTransform Scroll(Transform p,float x,float y,float w,float h,out ScrollRect sr)
        {
            var root=Rect("Scroll",p,x,y,w,h);sr=root.gameObject.AddComponent<ScrollRect>();var view=Rect("Viewport",root,0,0,w,h);view.gameObject.AddComponent<RectMask2D>();var hit=view.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.001f);
            var content=Rect("Content",view,0,0,w-14,h);sr.viewport=view;sr.content=content;sr.horizontal=false;sr.vertical=true;sr.scrollSensitivity=40;sr.movementType=ScrollRect.MovementType.Clamped;sr.inertia=true;return content;
        }
        public static void FinishScroll(ScrollRect sr,float contentHeight,float keep=-1)
        {
            var content=sr.content;var view=sr.viewport;float h=view.rect.height>0?view.rect.height:view.sizeDelta.y;content.sizeDelta=new Vector2(content.sizeDelta.x,Mathf.Max(h,contentHeight));
            if(contentHeight>h)
            {
                var rail=Box(sr.transform,view.sizeDelta.x-7,0,5,h,Hex("E8DCC6"),2,"Rail");var handle=Box(rail.transform,0,0,5,60,Hex("B89A74"),2,"Handle");var sb=rail.gameObject.AddComponent<Scrollbar>();
                sb.handleRect=(RectTransform)handle.transform;handle.rectTransform.anchorMin=Vector2.zero;handle.rectTransform.anchorMax=Vector2.one;handle.rectTransform.offsetMin=handle.rectTransform.offsetMax=Vector2.zero;
                sb.direction=Scrollbar.Direction.BottomToTop;sb.targetGraphic=handle;rail.raycastTarget=true;sr.verticalScrollbar=sb;sr.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;
                if(keep>=0)sr.verticalNormalizedPosition=keep;
            }
        }
        public static Slider Slider(Transform p,float x,float y,float w,float value,Action<float> change)
        {
            var r=Rect("Slider",p,x,y,w,28);var s=r.gameObject.AddComponent<Slider>();var bg=Box(r,0,10,w,8,Hex("E3D5BD"),4,"Track");var area=Rect("Fill Area",r,0,10,w,8);var fill=Box(area,0,0,w,8,Brick,4,"Fill");
            fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=new Vector2(0,1);fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
            var hArea=Rect("Handle Area",r,10,0,w-20,28);var handle=Box(hArea,0,0,24,24,Hex("FFF6EA"),12,"Handle");handle.rectTransform.anchorMin=handle.rectTransform.anchorMax=new Vector2(0,.5f);handle.rectTransform.pivot=new Vector2(.5f,.5f);handle.rectTransform.anchoredPosition=Vector2.zero;
            var ring=Stretch("Ring",handle.transform);Fill(ring,Brick2,12).rectTransform.offsetMin=Vector2.zero;var dot=Stretch("Dot",handle.transform);dot.offsetMin=new Vector2(3,3);dot.offsetMax=new Vector2(-3,-3);Fill(dot,Hex("FFF6EA"),9);
            handle.raycastTarget=true;bg.raycastTarget=true;s.fillRect=fill.rectTransform;s.handleRect=handle.rectTransform;s.targetGraphic=handle;s.minValue=0;s.maxValue=100;s.wholeNumbers=true;s.value=value;
            s.onValueChanged.AddListener(v=>change(v));return s;
        }
        public static TMP_InputField Input(Transform p,float x,float y,float w,float h,string value,int limit,Action<string> change,string placeholder="")
        {
            var r=Rect("Input",p,x,y,w,h);var bg=Fill(r,Hex("C9AD84"),7);bg.raycastTarget=true;var inner=Stretch("Fill",r);inner.offsetMin=new Vector2(1.5f,1.5f);inner.offsetMax=new Vector2(-1.5f,-1.5f);Fill(inner,Hex("FFFCF5"),6);
            var area=Stretch("Text Area",r);area.offsetMin=new Vector2(14,4);area.offsetMax=new Vector2(-14,-4);area.gameObject.AddComponent<RectMask2D>();
            var ph=Stretch("Placeholder",area).gameObject.AddComponent<TextMeshProUGUI>();ph.font=Font;ph.fontSize=19*Scale;ph.color=Muted;ph.text=placeholder;ph.alignment=TextAlignmentOptions.MidlineLeft;ph.enableWordWrapping=false;
            var tx=Stretch("Text",area).gameObject.AddComponent<TextMeshProUGUI>();tx.font=Font;tx.fontSize=19*Scale;tx.color=Ink;tx.alignment=TextAlignmentOptions.MidlineLeft;tx.enableWordWrapping=false;tx.richText=false;
            var f=r.gameObject.AddComponent<TMP_InputField>();f.textViewport=area;f.textComponent=tx;f.placeholder=ph;f.characterLimit=limit;f.fontAsset=Font;f.pointSize=19*Scale;f.text=value;f.richText=false;f.caretColor=Brick;f.selectionColor=new Color(.71f,.28f,.23f,.3f);
            f.onValueChanged.AddListener(v=>change(v));return f;
        }
        public static Texture2D Gradient()
        {
            Texture2D t;if(textures.TryGetValue("__gradient",out t))return t;t=new Texture2D(1,64,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<64;y++){float v=y/63f;float a=v>.82f?Mathf.Lerp(0,.12f,(v-.82f)/.18f):v<.38f?Mathf.Lerp(.32f,0,v/.38f):0;t.SetPixel(0,y,new Color(.165f,.09f,.063f,a));}
            t.Apply();textures["__gradient"]=t;return t;
        }
    }
    public class HoverScale:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler
    {
        public static bool Reduced;float target=1;bool over,down;
        void Set(){target=down?.98f:over?(Reduced?1:1.035f):1;}
        public void OnPointerEnter(PointerEventData e){over=true;Set();}public void OnPointerExit(PointerEventData e){over=false;down=false;Set();}
        public void OnPointerDown(PointerEventData e){down=true;Set();}public void OnPointerUp(PointerEventData e){down=false;Set();}
        public void OnSelect(BaseEventData e){over=true;Set();}public void OnDeselect(BaseEventData e){over=false;Set();}
        void Update(){float s=Mathf.MoveTowards(transform.localScale.x,target,Time.unscaledDeltaTime*0.6f);transform.localScale=new Vector3(s,s,1);}
    }
    // 竖向排版：逐段放置并累计高度，最后把高度交给滚动区域。
    public class Flow
    {
        public RectTransform root;public float x,w,y,gap=12;Flow parent;RectTransform box;float pad;
        public Flow(RectTransform root,float x,float w,float y=0){this.root=root;this.x=x;this.w=w;this.y=y;}
        public TextMeshProUGUI P(string s,float size=19,Color? c=null,bool bold=false,float after=-1,TextAlignmentOptions align=TextAlignmentOptions.TopLeft,float indent=0)
        {
            var t=UI.Text(root,s,x+indent,y,w-indent,10,size,c,bold,align);float h=UI.Measure(t,w-indent);t.rectTransform.sizeDelta=new Vector2(w-indent,h);y+=h+(after<0?gap:after);return t;
        }
        public void Space(float h){y+=h;}
        public RectTransform Block(float h,float after=-1){var r=UI.Rect("Block",root,x,y,w,h);y+=h+(after<0?gap:after);return r;}
        public Flow Card(Color fill,Color border,float padding=18,int radius=10,float bw=1.5f)
        {
            var b=UI.Frame(root,x,y,w,10,fill,border,radius,bw);var f=new Flow(b.rectTransform,padding,w-padding*2,padding){parent=this,box=b.rectTransform,pad=padding,gap=gap};return f;
        }
        public void End(float after=-1){float h=y-gap+pad;box.sizeDelta=new Vector2(box.sizeDelta.x,h);parent.y+=h+(after<0?parent.gap:after);}
        public class B{public string label,id,sfx="click";public Action click;public Look2 style=Look2.Secondary;public bool enabled=true;public float min=0;}
        public static B Bt(string label,Action click,Look2 style=Look2.Secondary,bool enabled=true,string id=null,string sfx="click"){return new B{label=label,click=click,style=style,enabled=enabled,id=id,sfx=sfx};}
        public float Row(params B[] buttons)
        {
            float cx=x,cy=y,h=46,used=0;
            foreach(var b in buttons)
            {
                if(b==null)continue;float bw=Mathf.Max(b.min,UI.Width(b.label,17,b.style==Look2.Primary)+34);if(cx>x&&cx+bw>x+w){cx=x;cy+=h+10;}bw=Mathf.Min(bw,w);
                UI.Btn(root,b.label,cx,cy,bw,h,b.click,b.style,b.enabled,b.id,b.sfx,17);cx+=bw+10;used=cy+h-y;
            }
            y+=used+gap;return used;
        }
        public Button Wide(string label,Action click,Look2 style=Look2.Primary,bool enabled=true,string id=null,float h=50,string sfx="click"){var b=UI.Btn(root,label,x,y,w,h,click,style,enabled,id,sfx,19);y+=h+gap;return b;}
        public RawImage Image(Texture tex,float maxH=380,Action click=null)
        {
            if(tex==null)return null;float ar=UI.Aspect(tex),h=Mathf.Min(maxH,w/ar);float iw=h*ar;var r=UI.Rect("Photo",root,x+(w-iw)/2,y,iw,h);
            var im=r.gameObject.AddComponent<RawImage>();im.texture=tex;im.raycastTarget=click!=null;
            if(click!=null){var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>{UI.Sound("sfx_open");click();});}y+=h+gap;return im;
        }
        public void Rule(Color c,float h=1){UI.Box(root,x,y,w,h,c);y+=h+gap;}
    }
}
