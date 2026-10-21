using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// 轻松版主页：夕阳下的出租屋、立绘、顶部状态、两张卡片、主按钮与六个入口。
namespace HarbourYear.Lite
{
    public partial class LiteGame
    {
        static readonly string[][] NavItems={new[]{"schedule","日程","lite"},new[]{"bag","背包","items"},new[]{"studies","作业","courses"},new[]{"classmates","同学录","team"},new[]{"journal","手账","journal"},new[]{"goals","目标","goals"}};
        public void Nav(string id)
        {
            autoRun=false;if(id!=win)LiteAudio.Sfx(id=="room"?"sfx_close":win=="room"?"sfx_open":"sfx_page");win=id;if(id=="lite"&&S.phase=="planning")focusSeen=Pace.PhaseIndex(S.round);Render();
        }
        Image Sliced(Transform p,string tex,Vector4 border,float mult,float x,float y,float w,float h)
        {
            var r=UI.Rect("Sliced",p,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=UI.Slice(tex,border);im.type=Image.Type.Sliced;im.pixelsPerUnitMultiplier=mult;im.raycastTarget=false;return im;
        }
        void DrawHome()
        {
            var scene=UI.Stretch("Home",root);
            var room=UI.Picture(scene,S.housing=="standard"||S.housing==null?UI.Tex("ui/room_standard"):UI.Tex("proto/room_v2"),0,0,W,H,true,.55f);
            var shade=UI.Rect("Shade",scene,0,0,W,H).gameObject.AddComponent<RawImage>();shade.texture=UI.Gradient();shade.raycastTarget=false;
            Figure(scene);Hud(scene);PeriodCard(scene);GrowthCard(scene);Center(scene);NavBar(scene);
        }
        void Figure(RectTransform p)
        {
            string body=S.appearance.bodyId;var tex=Portrait(body);if(tex==null)return;bool young=body.EndsWith("young");
            var sz=UI.Size(tex);float height=young?sz.y:1536,top=young?0:(body.StartsWith("female")?16:12)/1536f,bottom=young?1:1518/1536f,cx=body=="female_young"?.499f:body=="male_young"?.501f:body.StartsWith("female")?.52f:.504f;
            float k=80f/1442f,heightPct=height*k,bottomPct=4-(1-bottom)*height*k;float h=H*heightPct/100f,w=h*sz.x/sz.y;
            var r=UI.Anchor("Figure",p,new Vector2(0,0),new Vector2(0,0),new Vector2(W*.5f-cx*w,H*bottomPct/100f),new Vector2(w,h));var im=r.gameObject.AddComponent<RawImage>();im.texture=tex;im.raycastTarget=false;
        }
        static readonly Dictionary<string,float[]> Tracks=new Dictionary<string,float[]>{{"hud_energy",new[]{24.4f,67.4f,58.4f,18.5f}},{"hud_stress",new[]{22.8f,69.8f,57.5f,20.5f}}};
        void Hud(RectTransform p)
        {
            int day=Core.Today(S);string half=S.phase=="running"?Cal.SlotOf(S.round,Math.Min(S.cursor,S.plan.Count-1)).half:null;int sem=Cal.Rounds[S.round].semester;int? wk=sem>0?(day-Cal.SemStart(sem))/7+1:(int?)null;
            float x=16,dw=W*.165f,mw=W*.15f,iw=W*.038f;float rowH=dw*184/520;Action status=()=>Nav("status");
            var date=HudImage(p,"hud_date",x,12,dw,rowH,status,"HudDate");var dr=(RectTransform)date.transform;
            UI.Text(dr,Cal.Date(day),dw*.05f,rowH*.32f,dw*.47f,rowH*.36f,24,UI.Hex("47291F"),true,TextAlignmentOptions.Center,true);
            UI.Text(dr,Cal.Iso(day).Substring(0,4)+" 年",dw*.05f,rowH*.70f,dw*.47f,rowH*.17f,13,UI.Hex("8A5A44"),true,TextAlignmentOptions.Center,true);
            UI.Text(dr,(wk!=null?"第 "+wk+" 周":"寒假")+"\n<size=80%><color=#7A5A44>"+Cal.Weekdays[Cal.Weekday(day)]+(half!=null?" · "+Cal.Half(half):"")+"</color></size>",dw*.56f,rowH*.22f,dw*.25f,rowH*.56f,15,UI.Hex("5A3A2A"),true,TextAlignmentOptions.Center,true);
            x+=dw+10;float mh=mw*117/520;var money=HudImage(p,"hud_money",x,12+(rowH-mh)/2,mw,mh,status,"HudMoney");var mr=(RectTransform)money.transform;
            UI.Text(mr,"港币",mw*.31f,mh*.2f,mw*.2f,mh*.6f,13,UI.Hex("7A5A44"),true,TextAlignmentOptions.MidlineLeft);UI.Text(mr,Fmt.Money(S.money),mw*.45f,mh*.2f,mw*.47f,mh*.6f,21,UI.Hex("3F2A1F"),true,TextAlignmentOptions.MidlineRight,true);
            float right=W-16-3*iw-16;x=right-dw*2-10;var last=Pace.LastLog(S);
            Meter(p,"hud_energy","精力",Pace.EnergyOf(S.fatigue),Pace.EnergyLevel(S.fatigue),last!=null?Pace.Neg(last.fatigue):(int?)null,x,12,dw,rowH,status);
            Meter(p,"hud_stress","压力",S.stress,Pace.StressLevel(S.stress),last!=null?last.stress:(int?)null,x+dw+10,12,dw,rowH,status);
            int todo=Pace.Todo(S,M).Count;float ix=right+16;
            var msg=UI.ImageButton(p,UI.Tex("ui/icon_messages"),ix,12+(rowH-iw)/2,iw,iw,()=>Nav("messages"),true,"IconMessages","none");
            if(todo>0){var b=UI.Box(msg.transform,iw-20,-6,24,24,UI.Hex("C43D32"),12,"Badge");UI.Text(b.transform,todo.ToString(),0,1,24,22,13,Color.white,true,TextAlignmentOptions.Center);}
            UI.ImageButton(p,UI.Tex("ui/icon_review"),ix+iw+8,12+(rowH-iw)/2,iw,iw,()=>Nav("review"),true,"IconReview","none");
            UI.ImageButton(p,UI.Tex("ui/icon_settings"),ix+2*(iw+8),12+(rowH-iw)/2,iw,iw,()=>Nav("settings"),true,"IconSettings","none");
        }
        RawImage HudImage(RectTransform p,string tex,float x,float y,float w,float h,Action click,string id)
        {
            var b=UI.ImageButton(p,UI.Tex("ui/"+tex),x,y,w,h,click,true,id,"none",false);return (RawImage)b.targetGraphic;
        }
        void Meter(RectTransform p,string id,string name,int value,string[] level,int? change,float x,float y,float w,float rowH,Action click)
        {
            var tex=UI.Tex("ui/"+id);float h=w/UI.Aspect(tex);var r=(RectTransform)HudImage(p,id,x,y+(rowH-h)/2,w,h,click,id).transform;var s=Tracks[id];
            string ch=change!=null&&change.Value!=0?" <color="+((change>0)==(id=="hud_energy")?"#4F7A43":"#A7432F")+">"+(change>0?"▲":"▼")+Math.Abs(change.Value)+"</color>":"";
            UI.Text(r,"<b><color=#4A2C22>"+name+"</color></b>  <size=85%>"+level[0]+ch+"</size>",w*s[0]/100,h*.12f,w*s[1]/100*.72f,h*.44f,15,UI.Hex("7A5A44"),false,TextAlignmentOptions.MidlineLeft,true);
            UI.Text(r,value+"<size=70%><color=#8A705C>/100</color></size>",w*(s[0]+s[1]*.62f)/100,h*.12f,w*s[1]/100*.38f,h*.44f,17,UI.Hex("3F2A1F"),true,TextAlignmentOptions.MidlineRight,true);
            Color c=level[1]=="risk"?UI.Hex("B9473A"):level[1]=="low"?UI.Hex("D99A3C"):id=="hud_energy"?UI.Hex("D9606F"):UI.Hex("6F93A8");
            float tw=w*s[1]/100,th=h*s[3]/100;var back=UI.Rect("Track",r,w*s[0]/100,h*s[2]/100,tw,th);if(value>0)UI.Box(back,0,0,Mathf.Max(th,tw*value/100f),th,c,(int)Mathf.Max(1,th/2));
        }
        RectTransform Card(RectTransform p,string title,string small,bool left,float width,out Flow body,Action fold=null,bool folded=false)
        {
            var c=UI.Anchor(title,p,new Vector2(left?0:1,0),new Vector2(left?0:1,0),new Vector2(left?16:-16,16),new Vector2(width,10));
            var head=UI.Rect("Head",c,2,2,width-4,40);UI.Fill(head,UI.Brick,8);UI.Text(head,title,14,0,width-90,40,18,UI.White,true,TextAlignmentOptions.MidlineLeft);
            if(small!=null)UI.Text(head,small,width-150,0,130,40,13,UI.Hex("FFE2CF"),false,TextAlignmentOptions.MidlineRight);
            if(fold!=null){var b=UI.Btn(head,folded?"▶":"▼",width-44,6,30,28,fold,Look2.Danger,true,"Fold","page",16);}
            body=new Flow(c,16,width-32,52){gap=6};return c;
        }
        void FinishCard(RectTransform c,Flow f,float minH=0)
        {
            float h=Mathf.Max(minH,f.y+8);c.sizeDelta=new Vector2(c.sizeDelta.x,h);var bg=UI.Stretch("Paper",c);bg.SetAsFirstSibling();UI.Fill(bg,UI.Brown,11);var inner=UI.Stretch("Cream",bg);inner.offsetMin=new Vector2(2,2);inner.offsetMax=new Vector2(-2,-2);UI.Fill(inner,UI.Cream,9);
        }
        void PeriodCard(RectTransform p)
        {
            var h=Pace.Home(S,M);Flow f;var c=Card(p,"这段时间",h.round,true,Mathf.Clamp(W*.205f,270,360),out f);
            f.P(h.phase,16,UI.Hex("5B4231"),true,4);var bar=f.Block(10,2);UI.Bar(bar,0,0,f.w,9,h.total>0?100f*h.done/h.total:0,UI.Sage);f.P(h.progressText,13,UI.Muted,false,8);
            UI.Box(c,f.x,f.y,f.w,1,UI.Hex("CDB795"));f.y+=8;
            f.P((h.goalDone?"✓ ":"")+h.goalTitle+(h.goalStatus!="done"&&h.goalStatus!="current"?"  <size=80%><color=#A7432F>"+Pace.GoalLabels[h.goalStatus]+"</color></size>":""),16,h.goalDone?UI.Good:UI.Hex("47291F"),true,4);
            var next=f.P(h.goalNext,14,UI.Hex("5E4838"));if(next.rectTransform.sizeDelta.y>78){next.rectTransform.sizeDelta=new Vector2(f.w,76);next.overflowMode=TextOverflowModes.Ellipsis;f.y-=next.preferredHeight-74;}
            if(h.attention!=null){var a=f.Card(UI.AlertBg,UI.AlertLine,8,6);a.P("⚠ "+h.attention,13,UI.Hex("8C3A2A"),false,0);a.End(6);}
            else if(h.monologue!=null)f.P("「"+h.monologue+"」",14,UI.Hex("6A5040"),false,6);
            f.Wide("详情与安排",()=>Nav("lite"),Look2.Secondary,true,"PeriodMore",36);FinishCard(c,f);
        }
        void GrowthCard(RectTransform p)
        {
            Flow f;float w=growthOpen?Mathf.Clamp(W*.205f,270,360):190;var c=Card(p,"我的成长",null,false,w,out f,()=>{growthOpen=!growthOpen;Render();},!growthOpen);
            if(!growthOpen){c.sizeDelta=new Vector2(w,44);var bg=UI.Stretch("Paper",c);bg.SetAsFirstSibling();UI.Fill(bg,UI.Brown,10);return;}
            Color[] tones={UI.Hex("7FA3C4"),UI.Hex("C99A5C"),UI.Hex("D98A8F"),UI.Hex("8FAE79"),UI.Hex("A98AC0")};
            for(int i=0;i<5;i++)
            {
                var row=f.Block(26,4);string name=i<4?Cat.AbilityNames[i]:"声望";double v=i<4?S.abilities[i]:S.reputation;
                UI.Text(row,name,0,2,80,24,14,UI.Hex("5B4231"));UI.Bar(row,84,8,f.w-84-52,10,(float)Math.Min(100,v),tones[i]);UI.Text(row,i<4?Fmt.F1(v):((int)v).ToString(),f.w-48,0,48,24,16,UI.Hex("3F2A1F"),true,TextAlignmentOptions.TopRight);
                if(i==3){UI.Box(c,f.x,f.y,f.w,1,UI.Hex("CDB795"));f.y+=6;}
            }
            f.P("能力与声望 0—100；课程成绩在“作业”里单独查看。",12,UI.Muted,false,4);FinishCard(c,f);
        }
        void Center(RectTransform p)
        {
            var a=Pace.MainAction(S,M,stop,focusSeen==Pace.PhaseIndex(S.round));float navH=Mathf.Clamp(W*.054f,66,86);float bottom=navH+14+18;
            float hintW=Mathf.Min(560,UI.Width(a.hint,14)+30),cy=bottom;
            if(saveFailed){var warn=UI.Anchor("SaveWarning",p,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,cy),new Vector2(460,26));UI.Fill(warn,UI.Brick2,12);UI.Text(warn,"这一步没能写入存档：请在设置里导出存档。",0,2,460,22,13,Color.white,false,TextAlignmentOptions.Center);cy+=32;}
            var hint=UI.Anchor("Hint",p,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,cy),new Vector2(hintW,10));var ht=UI.Text(hint,a.hint,14,4,hintW-28,10,14,UI.White,false,TextAlignmentOptions.Center);
            float hh=UI.Measure(ht,hintW-28)+8;hint.sizeDelta=new Vector2(hintW,hh);UI.Backdrop(hint,new Color(.17f,.1f,.07f,.7f),12);cy+=hh+8;
            float bw=Mathf.Max(270,UI.Width(a.label,22,true)+110),bh=64;var r=UI.Anchor("MainButton",p,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,cy),new Vector2(bw,bh));
            var img=r.gameObject.AddComponent<Image>();img.sprite=UI.Slice("ui/button_primary",new Vector4(72,59,72,59));img.type=Image.Type.Sliced;img.pixelsPerUnitMultiplier=3.6f;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=img;b.onClick.AddListener(()=>{LiteAudio.Sfx("sfx_click");MainButton();});r.gameObject.AddComponent<HoverScale>();
            UI.Text(r,a.label,10,0,bw-20,bh,22,UI.Hex("4A2016"),true,TextAlignmentOptions.Center,true);
        }
        void NavBar(RectTransform p)
        {
            float bw=Mathf.Clamp(W*.069f,86,124),bh=Mathf.Clamp(W*.054f,66,86),gap=10,total=NavItems.Length*bw+(NavItems.Length-1)*gap;
            var bar=UI.Anchor("Nav",p,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,14),new Vector2(total,bh));
            var dots=new Dictionary<string,bool>{
                {"studies",Cat.Courses.Any(c=>new[]{"complete","overdue"}.Contains(S.Task(c.id).status)||new[]{"complete","overdue"}.Contains(S.Proj(c.id).status))},
                {"classmates",!Ended&&Cat.Courses.Any(c=>S.Proj(c.id).status!="unreleased"&&S.Proj(c.id).status!="submitted"&&S.Proj(c.id).status!="missing"&&!Core.TeamReady(S.Proj(c.id)))},{"schedule",M.conflict!=null}};
            for(int i=0;i<NavItems.Length;i++)
            {
                var n=NavItems[i];var r=UI.Rect("Nav:"+n[0],bar,i*(bw+gap),0,bw,bh);var img=r.gameObject.AddComponent<Image>();img.sprite=UI.Slice("ui/button_secondary",new Vector4(50,36,50,36));img.type=Image.Type.Sliced;img.pixelsPerUnitMultiplier=3.8f;
                var b=r.gameObject.AddComponent<Button>();b.targetGraphic=img;var target=n[2];b.onClick.AddListener(()=>Nav(target));r.gameObject.AddComponent<HoverScale>();
                UI.Text(r,n[1],4,0,bw-8,bh,19,UI.Hex("4A2C22"),true,TextAlignmentOptions.Center);bool dot;if(dots.TryGetValue(n[0],out dot)&&dot)UI.Box(r,bw-10,-6,15,15,UI.Hex("D0453A"),7,"Dot");
            }
        }
        // ---------- 主按钮 ----------
        float mainLock;
        void MainButton()
        {
            if(Time.unscaledTime<mainLock)return;mainLock=Time.unscaledTime+.3f;var a=Pace.MainAction(S,M,stop,focusSeen==Pace.PhaseIndex(S.round));
            switch(a.kind)
            {
                case "notice":case "event":case "result":case "trip":laterKey=null;Render();return;
                case "round_end":case "sample_end":case "focus":Nav("lite");return;
                case "conflict":case "handle":stop=null;Nav("lite");return;
            }
            var e=S;var t=M;
            if(e.phase=="planning"){var n=Pace.StartRound(e,t);if(n.error!=null){Toast(n.error);Render();return;}e=n.state;t=n.meta;}
            var x=Pace.Run(e,t);laterKey=null;stop=x.stop.kind=="team"||x.stop.kind=="adjust"?x.stop:null;
            if(new[]{"blocked","adjust","team","limit"}.Contains(x.stop.kind))Toast(x.stop.message);if(x.stop.kind=="round_end"||x.stop.kind=="sample_end")win="lite";Commit(x.state,x.meta);
        }
    }
}
