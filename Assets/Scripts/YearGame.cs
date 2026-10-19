using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame:MonoBehaviour
    {
        public YearState State{get;private set;}
        RectTransform root,modal;Texture2D harbour,roomArt;string tab="日程",toast="";bool menu=true,paused,swapMode;int swapFirst=-1,speed=1,selectedCourse,teamCourse;float tick,toastUntil;
        public bool MenuVisible{get{return menu;}}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(FindObjectOfType<YearGame>()==null)new GameObject("Harbour Year / Game").AddComponent<YearGame>();}
        void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;harbour=Resources.Load<Texture2D>("Art/Harbour");roomArt=Resources.Load<Texture2D>("Art/RoomV2");
            NotebookUI.Font=Resources.Load<TMP_FontAsset>("Fonts/Notebook SDF");if(NotebookUI.Font==null){NotebookUI.Font=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("Fonts/NotebookSans"));NotebookUI.Font.atlasPopulationMode=AtlasPopulationMode.Dynamic;}
            var go=new GameObject("Notebook Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,1000);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            root=Rect("Notebook",go.transform,0,0,1600,1000);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            if(FindObjectOfType<EventSystem>()==null)new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            var cam=new GameObject("Paper Camera",typeof(Camera)).GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Paper;cam.orthographic=true;
            YearAudio.Start(gameObject);Render();
        }
        void Update()
        {
            if(!menu&&State!=null&&State.phase=="executing"&&!paused&&modal==null){tick+=Time.unscaledDeltaTime;if(tick>=.65f/speed){tick=0;Rules.Step(State);SaveAndRender();}}
            if(Input.GetKeyDown(KeyCode.Escape)){if(modal!=null&&State!=null&&(State.phase=="plan"||State.phase=="executing")){CloseModal();}else if(!menu&&(State.phase=="plan"||State.phase=="executing"))ConfirmMenu();}
            if(toast!=""&&Time.unscaledTime>toastUntil){toast="";Render();}
        }
        void OnApplicationPause(bool p){if(p&&State!=null)YearSave.Write(State);}void OnApplicationQuit(){if(State!=null)YearSave.Write(State);}
        void Clear(){for(int i=root.childCount-1;i>=0;i--){root.GetChild(i).gameObject.SetActive(false);Destroy(root.GetChild(i).gameObject);}modal=null;}
        public void NewGame(){State=new YearState{seed=UnityEngine.Random.Range(1,1000000)};menu=false;tab="日程";paused=false;SaveAndRender();}
        public void ContinueGame(){State=YearSave.Read();if(State==null){Notice("没有可读取的新版存档，请开始新游戏。");return;}menu=false;tab="日程";paused=false;Render();}
        public void SaveAndRender(){if(State!=null)YearSave.Write(State);Render();}
        void Notice(string message){toast=message;toastUntil=Time.unscaledTime+5;Render();}
        public void Render()
        {
            Clear();Box(root,0,0,1600,1000,Paper);
            if(menu){DrawMenu();return;}if(State.phase=="intro"){DrawIntro();return;}if(State.phase=="setup"){DrawSetup();return;}
            Header();Sidebar();
            if(State.phase=="ending")DrawEnding();else if(tab=="课程")DrawCourses();else if(tab=="小组")DrawTeams();else if(tab=="生活")DrawLife();else if(tab=="目标")DrawGoals();else if(tab=="回忆")DrawJournal();else DrawPlanner();
            Text(root,"HONG KONG, ONE YEAR  /  普通的日子，也在成为你。",40,957,1000,26,15,Muted);
            Text(root,YearSave.LastError!=""?YearSave.LastError:"v2 试玩版 · 自动保存",1100,957,460,25,15,YearSave.LastError!=""?Red:Muted,false,TextAlignmentOptions.TopRight);
            if(State.phase=="event")DrawEvent();else if(State.phase=="summary")DrawSummary();
            if(toast!=""){Box(root,320,916,1240,35,Navy);Text(root,toast,334,919,1208,29,17,White);}
        }
        void DrawMenu()
        {
            Picture(root,roomArt??harbour,550,0,1050,1000);Box(root,0,0,575,1000,Paper);Box(root,60,78,38,5,Orange);
            Text(root,"一本关于成长、选择与日常的手账",60,111,470,40,21,Muted);Text(root,"港硕\n这一年",53,205,510,240,94,Navy,true);Text(root,"HONG KONG, ONE YEAR",62,479,470,33,20,Orange,true);
            Text(root,"从一只行李箱开始。\n上课、租房、分工、求职，\n把未来写进每一个半天。",63,564,460,123,25,Ink);
            Button(root,"开始新的一年  →",62,737,453,60,()=>{if(YearSave.Exists)ConfirmRestart();else NewGame();},Navy,White,23,true,"NewGame");
            Button(root,"继续我的手账",62,811,453,57,ContinueGame,Pale,Navy,22,YearSave.Exists,"Continue");
            Text(root,"17 轮 · 8 门课程 · 可组合角色 · 多种去向",62,902,478,33,17,Muted);
            if(YearSave.HasLegacy)Text(root,"旧版存档已保留；新版规则需要新开一局。",62,942,475,28,15,Muted);
        }
        void DrawIntro()
        {
            Picture(root,harbour,0,0,1600,1000);Box(root,130,194,800,590,new Color(.98f,.96f,.91f,.97f));
            Text(root,"第一章 / 抵港",174,237,650,35,20,Orange,true);Text(root,"这座城，\n以后要叫它日常。",168,307,700,137,49,Navy,true);
            Text(root,"从内地出发，带着录取通知和一只行李箱。\n一年的授课型硕士，八门课，许多还没想好的问题。\n先安顿下来，再决定这一年怎么过。",174,493,697,139,25,Ink);
            Button(root,"写下我的名字  →",174,676,694,61,()=>{State.phase="setup";SaveAndRender();},Navy,White,24,true,"IntroNext");
        }
        void Header()
        {
            Text(root,"港硕这一年",40,32,440,52,34,Navy,true);string[] tabs={"日程","课程","小组","生活","目标","回忆"};
            for(int i=0;i<6;i++){string t=tabs[i];Button(root,t,670+i*112,34,99,47,()=>{tab=t;Render();},tab==t?Navy:Paper,tab==t?White:Ink,20,State.phase!="ending","Tab"+t);}
            Button(root,"帮助",1360,34,89,47,ShowGuide,Pale,Navy,18);Button(root,"菜单",1462,34,98,47,ConfirmMenu,Pale,Navy,18);Box(root,40,99,1520,1,Line);
        }
        void Sidebar()
        {
            var p=Box(root,40,122,250,810,Navy).transform;
            PixelPeople.Draw(p,State.look,11,15,80,156);Text(p,State.name,103,29,129,47,23,White,true);Text(p,"第 "+State.round+" / 17 轮",104,90,130,32,21,White);Text(p,Rules.Month(State.round),19,184,213,42,17,Hex("C6D2CC"));
            string[] ns={"学业","科研","职业","城市适应"};float[] vs={State.academic,State.research,State.career,State.adaptation};
            for(int i=0;i<4;i++){float y=249+i*56;Text(p,ns[i],21,y,150,26,18,White);Text(p,vs[i].ToString("0"),174,y,53,26,20,White,true,TextAlignmentOptions.TopRight);Bar(p,21,y+34,206,vs[i],i%2==0?Hex("DAA477"):Hex("93B3A0"),4);}
            Text(p,"疲劳 "+State.fatigue.ToString("0")+" / 100",21,488,207,29,18,White);Bar(p,21,528,206,State.fatigue,State.fatigue>=65?Orange:Sage,6);
            Text(p,"压力 "+State.stress.ToString("0")+" / 100",21,549,207,29,18,White);Bar(p,21,588,206,State.stress,State.stress>=70?Orange:Sage,6);
            Text(p,"生活费余额",21,625,200,27,16,Hex("C6D2CC"));Text(p,"$ "+State.money.ToString("N0"),21,661,209,40,28,White,true);
            Text(p,State.debt>0?"应急金待还 $"+State.debt:"本轮账单 $"+Life.Bill(State),21,718,211,31,16,Hex("DDBB93"));
            Text(p,State.tourBuffRound==State.round?"天地宽广 · 本轮消耗减半":State.recoverySlots>0?"病假休养 · 剩余 "+State.recoverySlots+" 格":"今晚也要好好睡觉。",21,763,211,30,14,White);
        }
        RectTransform Modal(string title,string eyebrow,float w=1000,float h=800)
        {
            if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);}modal=Rect("Modal",root,0,0,1600,1000);Box(modal,0,0,1600,1000,new Color(.07f,.12f,.16f,.72f)).raycastTarget=true;
            var p=Box(modal,(1600-w)/2,(1000-h)/2,w,h,White).rectTransform;Box(p,0,0,w,5,Orange);Text(p,eyebrow,32,26,w-64,27,16,Orange,true);Text(p,title,32,71,w-64,49,32,Navy,true);return p;
        }
        void CloseModal(){if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}}
        void DrawEvent()
        {
            if(State.eventId=="homeCG"||State.eventId=="travelCG"){DrawHolidayStory();return;}
            var e=Events.Get(State);var p=Modal(e.title,"生活来信 / "+e.location,1020,780);Text(p,e.body,33,146,950,151,23,Ink);
            for(int i=0;i<e.choices.Length;i++){int id=i;var c=e.choices[i];var b=Button(p,"",33,322+i*119,953,103,()=>{Events.Choose(State,id);SaveAndRender();},Pale,Navy,20,c.unavailable=="","EventChoice"+i);Text(b.transform,c.title,18,13,910,35,22,Navy,true);Text(b.transform,c.effect,18,57,910,33,18,Sage);}
            Text(p,"日程已暂停，作出选择后继续。选择与后果会留在回忆里。",33,727,949,29,17,Muted);
        }
        void DrawHolidayStory()
        {
            var e=Events.Get(State);var p=Modal(e.title,e.location,1080,884);var art=Resources.Load<Texture2D>(State.eventId=="homeCG"?"Art/HomeCG":"Art/TravelCG");Picture(p,art,32,143,1016,293);Text(p,e.body,32,459,1016,111,23,Ink);
            for(int i=0;i<e.choices.Length;i++){int id=i;var c=e.choices[i];var b=Button(p,"",32,596+i*111,1016,93,()=>{Events.Choose(State,id);SaveAndRender();},Pale,Navy,22,true,"EventChoice"+i);Text(b.transform,c.title,20,12,973,31,22,Navy,true);Text(b.transform,c.effect,20,53,973,28,18,Sage);}
            Text(p,"假期故事 / 选择后继续安排",32,840,1016,26,16,Muted);
        }
        void DrawSummary()
        {
            var p=Modal("第 "+State.round+" 轮 · "+(Rules.Holiday(State)?"寒假小结":"两周小结"),"休息一下，再翻下一页。",1120,854);
            var content=Scroll(p,32,145,1056,573,Math.Max(573,State.summary.Count*76));for(int i=0;i<State.summary.Count;i++){Text(content,State.summary[i],0,i*76+6,1020,63,19,Ink);Box(content,0,i*76+73,1020,1,Line);}
            Button(p,State.round==17?"结束学年，查看毕业结局  →":"进入下一轮  →",32,758,1056,58,()=>{Rules.Advance(State);tab="日程";SaveAndRender();},Orange,White,23,true,"NextRound");
        }
        void DrawJournal()
        {
            Text(root,"这一年的小事",320,130,1100,55,35,Navy,true);Text(root,"选择、分工、申请和生活里的回响，都留在这里。",320,198,1200,36,20,Muted);
            var entries=State.journal.AsEnumerable().Reverse().ToList();var list=Scroll(root,320,256,1240,642,Math.Max(642,entries.Count*74));for(int i=0;i<entries.Count;i++){Text(list,entries[i],10,i*74+8,1195,58,20,Ink);Box(list,10,i*74+71,1195,1,Line);}
        }
        void ShowGuide()
        {
            var p=Modal("先读这四条，就能开始。","新生手账指南",1030,812);
            string[] titles={"01  上课按课表，其余半天由你安排","02  八门课，分别看出勤与考核","03  休息、住房和分工都很重要","04  把人生目标写进日程提醒"};
            string[] bodies={"一键填入 8 次上课，空格可批量休息。每课每轮最多 2 次；\n替换固定课程时段会记缺勤。寒假只有 2 项特别安排，可重复。","单科合规出勤低于 70% 判 F；病假计入合规，但不增加学习。\n作业和项目可提前提交，截止轮自动交已有成果；逾期质量 ×75%。","疲劳 65 / 85、压力 70 / 90 会降低效率；透支会强制休养。\n到小组页按能力分工，到生活页买家具、摆放、用食物或赠送。","目标页选择志愿，主界面同步提醒。工作与博士需在第 11—15 轮\n实际申请；毕业需 8 门全过、CGPA ≥2.00，最后进入独立结局页。"};
            for(int i=0;i<4;i++){Text(p,titles[i],32,149+i*137,965,34,23,Navy,true);Text(p,bodies[i],32,192+i*137,965,76,20,Muted);}
            Button(p,"开始安排我的一年  →",32,729,965,49,()=>{State.tutorialDone=true;SaveAndRender();},Navy,White,22,true,"CloseGuide");
        }
        void ConfirmRestart(){var p=Modal("重新翻开一本手账？","重新开始",850,445);Text(p,"新游戏会覆盖新版自动存档。旧版存档单独保留。",32,157,785,85,23,Ink);Button(p,"先继续这一局",32,333,370,62,CloseModal,Pale,Navy,21);Button(p,"开始新的一年",424,333,393,62,NewGame,Orange,White,21,true,"ConfirmNewGame");}
        void ConfirmMenu()
        {
            var p=Modal("暂时合上手账。","当前进度已自动保存",860,550);
            Text(p,"音乐与音效",32,152,750,37,23,Navy,true);Button(p,YearAudio.Muted?"声音：关（点击开启）":"声音：开（点击关闭）",32,211,796,50,()=>{YearAudio.Toggle();ConfirmMenu();},Pale,Navy,21);
            Text(p,"返回扉页后，可从「继续我的手账」接着当前半日。",32,302,796,58,21,Muted);
            Button(p,"继续",32,434,242,60,CloseModal,Pale,Navy,21);Button(p,"重新开始",287,434,255,60,ConfirmRestart,Pale,Navy,21);Button(p,"保存并返回扉页",556,434,272,60,()=>{menu=true;SaveAndRender();},Navy,White,21);
        }
    }
}
