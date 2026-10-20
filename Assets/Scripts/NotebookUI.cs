using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace HarbourYear
{
    public static class NotebookUI
    {
        public static Color Paper=Hex("F6F0E4"), White=Hex("FFFCF5"), Navy=Hex("20394B"), Ink=Hex("314A57"), Muted=Hex("78857F"), Orange=Hex("C97745"), Sage=Hex("638675"), Line=Hex("DDD7C9"), Pale=Hex("EAE6DA"), Red=Hex("AB5848");
        public static TMP_FontAsset Font;
        public static Color Hex(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c;}
        public static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=(RectTransform)o.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static Image Box(Transform p,float x,float y,float w,float h,Color c,string name="Paper")
        {
            var r=Rect(name,p,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.color=c;im.raycastTarget=false;return im;
        }
        public static TextMeshProUGUI Text(Transform p,string value,float x,float y,float w,float h,float size=20,Color? c=null,bool bold=false,TextAlignmentOptions align=TextAlignmentOptions.TopLeft)
        {
            var r=Rect("Text",p,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=value;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=size*.8f;t.fontSizeMax=size;t.color=c??Ink;t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;t.alignment=align;t.raycastTarget=false;t.enableWordWrapping=true;t.overflowMode=TextOverflowModes.Ellipsis;t.margin=Vector4.zero;t.lineSpacing=5;return t;
        }
        public static Button Button(Transform p,string title,float x,float y,float w,float h,Action click,Color? color=null,Color? textColor=null,float size=19,bool enabled=true,string id=null)
        {
            var im=Box(p,x,y,w,h,color??Navy,id??title);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.interactable=enabled;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.1f,1.07f,1.03f);colors.pressedColor=new Color(.84f,.84f,.84f);colors.selectedColor=new Color(1.07f,1.04f,1);colors.disabledColor=new Color(.7f,.7f,.7f,.6f);b.colors=colors;
            float pad=Mathf.Min(10,w*.1f);Text(im.transform,title,pad,1,w-pad*2,h-2,size,textColor??White,false,TextAlignmentOptions.Midline);if(click!=null)b.onClick.AddListener(()=>{YearAudio.Tap();click();});return b;
        }
        public static void Bar(Transform p,float x,float y,float w,float value,Color c,float height=5)
        {Box(p,x,y,w,height,Pale);Box(p,x,y,Mathf.Max(0,w*Mathf.Clamp01(value/100f)),height,c);}
        public static RawImage Picture(Transform p,Texture texture,float x,float y,float w,float h)
        {
            var r=Rect("Harbour illustration",p,x,y,w,h);var im=r.gameObject.AddComponent<RawImage>();im.texture=texture;im.raycastTarget=false;
            if(texture!=null){float imageRatio=(float)texture.width/texture.height,frame=w/h;if(frame>imageRatio){float v=imageRatio/frame;im.uvRect=new Rect(0,(1-v)*.48f,1,v);}else{float u=frame/imageRatio;im.uvRect=new Rect((1-u)*.5f,0,u,1);}}return im;
        }
        public static RectTransform Scroll(Transform p,float x,float y,float w,float h,float contentHeight)
        {
            var root=Rect("Scroll",p,x,y,w,h);var sr=root.gameObject.AddComponent<ScrollRect>();var view=Rect("Viewport",root,0,0,w,h);view.gameObject.AddComponent<RectMask2D>();var input=view.gameObject.AddComponent<Image>();input.color=new Color(1,1,1,.001f);input.raycastTarget=true;
            var content=Rect("Content",view,0,0,w-12,Mathf.Max(h,contentHeight));sr.viewport=view;sr.content=content;sr.horizontal=false;sr.vertical=true;sr.scrollSensitivity=32;sr.movementType=ScrollRect.MovementType.Clamped;sr.inertia=true;
            if(contentHeight>h){var rail=Box(root,w-6,0,4,h,Line);var handle=Box(rail.transform,0,0,4,50,Sage);var sb=rail.gameObject.AddComponent<Scrollbar>();sb.handleRect=(RectTransform)handle.transform;sb.direction=Scrollbar.Direction.BottomToTop;sb.targetGraphic=handle;sr.verticalScrollbar=sb;}return content;
        }
    }
}
