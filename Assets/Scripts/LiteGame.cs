using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 《港硕这一年》v3：轻松版四轮样例（同步自网页版 a320095）。开始页面 → 开场 → 建档 → 主页。
namespace HarbourYear.Lite
{
    public partial class LiteGame:MonoBehaviour
    {
        public const string Version="v3.0.0",Build="港硕这一年 v3.0.0 · 轻松版四轮样例（同步自网页版 a320095）";
        public static LiteGame Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){Launch();}
        public static void Launch(bool fromClassic=false){if(Instance==null&&(fromClassic||FindObjectOfType<YearGame>()==null))new GameObject("Lite Game").AddComponent<LiteGame>();}
        RectTransform root;GameObject canvasObject,cameraObject;
        // ---------- 状态 ----------
        public GameState S;public LiteMeta M;public bool hasSave,unreadable,saveFailed;
        public string screen="title",win="room";string titleLayer,dialog,laterKey,setupStep="quick",setupEconomy="balanced",setupHousing="standard",setupError,toast="";
        Stop stop;int? focusSeen;bool autoRun;int speed=1,week,slotSel,batchCount=2;float runTimer,toastUntil;string replaceTarget,replaceFrom="lower",batchItem="rest",batchRange="all",batchHalf="all",teamCourse="media";
        readonly List<List<string>> undo=new List<List<string>>();GameState draft;double[] alloc;SaveData retry;string quickName="",quickGender="female";
        string titleTab="sound",gameTab="save",memoryFilter="all",galleryFilter="all";bool growthOpen=true,journalFull;Relaxing relaxAsk;int? specialSlot;string specialItem="walk";
        readonly HashSet<string> open=new HashSet<string>(),eventSounds=new HashSet<string>(),failedCg=new HashSet<string>();readonly HashSet<int> moneyRounds=new HashSet<int>();readonly Dictionary<string,string> photoMode=new Dictionary<string,string>();
        Texture zoomTex;string zoomAlt;GalleryItem galleryZoom;Action<string> pickDone;string pickTitle;List<KeyValuePair<string,string>> pickOptions;Dictionary<string,string> roleDraft;string roleDraftFor;
        readonly Dictionary<string,float> scrolls=new Dictionary<string,float>();
        public bool BigText{get{return PlayerPrefs.GetInt("HarbourYear.Lite.BigText",0)==1;}}
        void Awake()
        {
            Instance=this;Application.targetFrameRate=60;Application.runInBackground=true;
            UI.Font=Resources.Load<TMP_FontAsset>("Fonts/Notebook SDF");if(UI.Font==null){UI.Font=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("Fonts/NotebookSans"));UI.Font.atlasPopulationMode=AtlasPopulationMode.Dynamic;}
            UI.Scale=BigText?1.12f:1;
            canvasObject=new GameObject("Lite Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var sc=canvasObject.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            root=UI.Stretch("Lite Root",canvasObject.transform);
            if(FindObjectOfType<EventSystem>()==null)new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            cameraObject=new GameObject("Lite Camera",typeof(Camera));var cam=cameraObject.GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=UI.Hex("24271F");cam.orthographic=true;
            LiteAudio.Boot(gameObject);bool bad;var save=LiteSave.Load(out bad);unreadable=bad;hasSave=save!=null;S=save!=null?save.state:Core.New(0);M=save!=null?save.lite:Pace.NewMeta();
            if(hasSave){var k=Pace.EventKey(S);if(k!=null)eventSounds.Add(EventSoundKey(S));if(S.phase=="summary"||S.phase=="finished")moneyRounds.Add(S.round);}
            if(unreadable)Toast("轻松版存档无法读取，已另存一份备份。可以在开始页面的“设置 · 存档”导入存档，或开始新生活（替换前会再备份一次）。");
            teamCourse=Cat.Courses.FirstOrDefault(c=>S.Proj(c.id).status!="unreleased"&&!Core.TeamReady(S.Proj(c.id)))?.id??"media";Render();
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(canvasObject!=null)Destroy(canvasObject);if(cameraObject!=null)Destroy(cameraObject);}
        void OnApplicationQuit(){if(hasSave&&screen=="game")LiteSave.Write(S,M);}
        float lastW,lastH;
        void Update()
        {
            var r=root.rect;if(Mathf.Abs(r.width-lastW)>1||Mathf.Abs(r.height-lastH)>1){lastW=r.width;lastH=r.height;Render();}
            if(toast!=""&&Time.unscaledTime>toastUntil){toast="";Render();}
            if(Input.GetKeyDown(KeyCode.Escape))Escape();
            if(screen=="game"&&autoRun)
            {
                if(S.phase!="running"||S.notice!=null||S.ev!=null||S.blockedReason!=null||dialog!=null||M.conflict!=null||Pace.PendingCard(S,M)!=null){autoRun=false;Render();return;}
                runTimer+=Time.unscaledDeltaTime;if(runTimer<1.3f/speed)return;runTimer=0;var e=Pace.Run(S,M,false,1);
                if(e.stop.kind!="tick"){autoRun=false;if(!new[]{"event","result","round_end","sample_end","day"}.Contains(e.stop.kind))toast=e.stop.message;}Commit(e.state,e.meta,null,false);
            }
        }
        void Escape()
        {
            if(zoomTex!=null||galleryZoom!=null){zoomTex=null;galleryZoom=null;Render();return;}if(pickDone!=null){pickDone=null;Render();return;}
            if(dialog!=null){dialog=null;Render();return;}
            if(screen=="game"&&Pace.PendingKind(S,M)=="event"&&laterKey!=EventDialogKey(S)){laterKey=EventDialogKey(S);Render();return;}
            if(screen=="title"&&titleLayer!=null){titleLayer=null;Render();return;}if(screen=="game"&&win!="room"){Nav("room");return;}
        }
        // ---------- 通用 ----------
        public void Toast(string m){toast=m??"";toastUntil=Time.unscaledTime+6.5f;}
        void Commit(GameState s,LiteMeta m,string msg=null,bool render=true)
        {
            bool newRound=s.round!=S.round||s.phase!=S.phase;if(s.cursor!=S.cursor||newRound){undo.Clear();if(s.phase=="running")week=s.cursor>=14?1:0;}
            S=s;M=m;if(S.notice!=null||S.ev!=null||S.blockedReason!=null||S.phase=="summary"||S.phase=="finished")autoRun=false;
            if(hasSave&&screen=="game"){bool ok=LiteSave.Write(S,M);saveFailed=!ok&&!((S.phase=="summary"||S.phase=="finished")&&S.ev!=null);if(saveFailed)Toast("这一步的进度没有写入存档（"+LiteSave.LastError+"），上一份存档保持不变。请导出存档并反馈。");}
            if(msg!=null)Toast(msg);if(render)Render();
        }
        static string EventSoundKey(GameState e){return e.ev!=null?"event:"+e.round+":"+e.ev.day+":"+e.ev.half+":"+e.ev.id:null;}
        static string EventDialogKey(GameState e){return EventSoundKey(e);}
        bool Ended{get{return S.phase=="summary"||S.phase=="finished";}}
        void EnsureRound()
        {
            if(screen!="game"||S.phase!="planning"||M.round==S.round||Cal.Rounds[S.round].kind!="semester"||Pace.SampleEnded(S))return;var e=Pace.NewRound(S,M);S=e.state;M=e.meta;if(hasSave)LiteSave.Write(S,M);
        }
        void Sounds()
        {
            if(screen!="game")return;var kind=Pace.PendingKind(S,M);
            if(kind=="event"&&laterKey!=EventDialogKey(S)){var k=EventSoundKey(S);if(k!=null&&eventSounds.Add(k))LiteAudio.Sfx("sfx_event");}
            if(Ended&&kind==null&&(win=="lite"||win=="planner"||win=="review")&&moneyRounds.Add(S.round)){int net;if(Pace.NetCash(S,out net)!=null&&net!=0)LiteAudio.Sfx(net>0?"sfx_money_up":"sfx_money_down");}
            bool trip=kind=="trip"&&laterKey!=(Pace.PendingTrip(M)!=null?Pace.PendingTrip(M).key:null);if(LiteAudio.Instance!=null)LiteAudio.Instance.SetScene(screen!="game"?"menu":trip?"outdoor":"room");
        }
        // ---------- 绘制 ----------
        float W{get{return root.rect.width>0?root.rect.width:1600;}}float H{get{return root.rect.height>0?root.rect.height:900;}}
        public void Render()
        {
            foreach(var sr in root.GetComponentsInChildren<ScrollRect>())scrolls[sr.name]=sr.content.anchoredPosition.y;
            for(int i=root.childCount-1;i>=0;i--){var c=root.GetChild(i).gameObject;c.SetActive(false);Destroy(c);}
            EnsureRound();Sounds();UI.Box(root,0,0,W,H,UI.Hex("24271F"),0,"Backdrop");
            if(screen=="game"){DrawHome();DrawWindow();DrawGameOverlays();}
            else{DrawStartBackground();if(screen=="title")DrawTitle();else if(screen=="prologue")DrawPrologue();else if(screen=="setup")DrawSetup();}
            if(pickDone!=null)DrawPick();if(zoomTex!=null)DrawZoom(zoomTex,zoomAlt);if(galleryZoom!=null)DrawZoom(UI.Tex(galleryZoom.src),galleryZoom.alt);
            if(toast!=""){var t=UI.Anchor("Toast",root,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,24),new Vector2(Mathf.Min(980,W-60),10));var tx=UI.Text(t,toast,22,12,t.sizeDelta.x-80,10,17,UI.White);float h=UI.Measure(tx,t.sizeDelta.x-80)+24;t.sizeDelta=new Vector2(t.sizeDelta.x,h);UI.Backdrop(t,new Color(.17f,.1f,.07f,.94f),12);UI.Btn(t,"×",t.sizeDelta.x-50,(h-36)/2,36,36,()=>{toast="";Render();},Look2.Plain,true,"CloseToast","none",22).GetComponentInChildren<TextMeshProUGUI>().color=UI.White;}
        }
        RectTransform Overlay(string name){var o=UI.Stretch(name,root);var dim=UI.Fill(o,new Color(.165f,.1f,.07f,.74f));dim.raycastTarget=true;return o;}
        // 纸张窗口（window.png）与普通奶油色面板；body 在可滚动区域内用 Flow 排版。
        public Flow Panel(RectTransform parent,string key,string title,string eyebrow,bool paper,float width,float height,Action close,string closeLabel="返回",Action<RectTransform,float> tabs=null,float tabsH=0)
        {
            width=Mathf.Min(width,W-30);if(paper)height=width*788f/1100f;height=Mathf.Min(height,H-30);if(paper){width=Mathf.Min(width,height*1100f/788f);height=width*788f/1100f;}
            var p=UI.Anchor("Panel:"+key,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(width,height));float top,side,bottom;
            if(paper){var bg=p.gameObject.AddComponent<RawImage>();bg.texture=UI.Tex("ui/window");bg.raycastTarget=true;top=width*.165f;side=width*.056f;bottom=width*.042f;}
            else{var bg=UI.Fill(p,UI.Brown,12);bg.raycastTarget=true;var inner=UI.Stretch("Fill",p);inner.offsetMin=new Vector2(2,2);inner.offsetMax=new Vector2(-2,-2);UI.Fill(inner,UI.Cream,11);top=22;side=30;bottom=20;}
            float cw=width-side*2,hx=paper?width*.135f:0,bw=closeLabel.Length>3?186:104;UI.Text(p,eyebrow,side+6+hx,top,cw-170-hx,24,14,paper?UI.Hex("A0705A"):UI.Brick,true,TextAlignmentOptions.TopLeft,true);UI.Text(p,title,side+6+hx,top+22,cw-190-hx,40,paper?25:28,UI.Hex("5A2A20"),true,TextAlignmentOptions.TopLeft,true);
            if(close!=null)UI.Btn(p,"‹ "+closeLabel,paper?width*.84f-bw:side+cw-bw-4,top+6,bw,42,close,Look2.Tab,true,"Back:"+closeLabel,"close",15);
            float y=top+(paper?66:72);UI.Box(p,side,y,cw,1.5f,UI.Hex("D8BF98"));y+=8;if(tabs!=null){tabs(UI.Rect("Tabs",p,side,y,cw,tabsH),cw);y+=tabsH+6;}
            ScrollRect sr;var content=UI.Scroll(p,side,y,cw,height-y-bottom,out sr);sr.name="Scroll:"+key;var f=new Flow(content,6,cw-26,8);flowScroll[f]=sr;return f;
        }
        readonly Dictionary<Flow,ScrollRect> flowScroll=new Dictionary<Flow,ScrollRect>();
        public void EndPanel(Flow f)
        {
            ScrollRect sr;if(!flowScroll.TryGetValue(f,out sr))return;flowScroll.Remove(f);UI.FinishScroll(sr,f.y+10);
            float keep;if(scrolls.TryGetValue(sr.name,out keep)){float max=Mathf.Max(0,sr.content.sizeDelta.y-sr.viewport.sizeDelta.y);sr.content.anchoredPosition=new Vector2(0,Mathf.Clamp(keep,0,max));}
        }
        void Tabs(RectTransform r,float w,IList<KeyValuePair<string,string>> items,string current,Action<string> pick)
        {
            float x=0;foreach(var it in items){float bw=Mathf.Max(96,UI.Width(it.Value,16)+30);var k=it.Key;UI.Btn(r,it.Value,x,0,bw,40,()=>pick(k),current==it.Key?Look2.Selected:Look2.Tab,true,"Tab:"+it.Key,current==it.Key?"none":"page",16);x+=bw+8;}
        }
        // ---------- 选择器（下拉菜单的替代） ----------
        void Pick(string title,List<KeyValuePair<string,string>> options,Action<string> done){pickTitle=title;pickOptions=options;pickDone=done;Render();}
        void DrawPick()
        {
            var o=Overlay("Picker");var f=Panel(o,"picker",pickTitle,"请选择",true,820,600,()=>{pickDone=null;Render();},"取消");
            foreach(var opt in pickOptions){var v=opt.Key;f.Wide(opt.Value,()=>{var d=pickDone;pickDone=null;d(v);Render();},Look2.Secondary,true,"Pick:"+v,44);}EndPanel(f);
        }
        void DrawZoom(Texture tex,string alt)
        {
            var o=Overlay("Zoom");var close=o.gameObject.AddComponent<Button>();close.onClick.AddListener(()=>{zoomTex=null;galleryZoom=null;LiteAudio.Sfx("sfx_close");Render();});
            if(tex!=null)UI.Picture(o,tex,40,30,W-80,H-120,false);else UI.Text(o,alt+"（图片没有加载出来）",40,H/2-20,W-80,40,20,UI.White,false,TextAlignmentOptions.Center);
            var b=UI.Btn(o,"关闭大图",W/2-90,H-76,180,48,()=>{zoomTex=null;galleryZoom=null;Render();},Look2.Pill,true,"CloseZoom","close",17);
            // 大图层把键盘焦点留在“关闭大图”上：方向键不会移到底下被遮住的按钮，Enter／Esc 关闭。
            var none=new Navigation{mode=Navigation.Mode.None};b.navigation=none;close.navigation=none;if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(b.gameObject);
        }
        void Zoom(Texture t,string alt){zoomTex=t;zoomAlt=alt;Render();}
        // ---------- 开始页面 ----------
        void DrawStartBackground()
        {
            var bg=UI.Picture(root,UI.Tex("start/start_background_blur"),0,0,W,H,true);bg.name="Start background";
            var sound=LiteAudio.Instance!=null&&LiteAudio.Instance.prefs.enabled;
            UI.Btn(root,sound?"声音已开启":"开启声音",W-170,14,152,44,()=>{var a=LiteAudio.Instance;a.prefs.enabled=!a.prefs.enabled;a.SavePrefs();Render();},sound?Look2.Primary:Look2.Pill,true,"StartSound","none",16);
            UI.Text(root,"轻松版 · 前八周／四轮试玩 · "+Version,0,H-34,W,26,15,UI.White,false,TextAlignmentOptions.Center);
        }
        string SaveSummary()
        {
            if(hasSave){var r=Cal.Rounds[S.round];string t=S.phase=="planning"?"这一轮还没开始":S.phase=="running"?Cal.Date(r.startDay+Math.Min(S.cursor,S.plan.Count-1)/2):"这一轮已结束";return UI.Safe(S.name)+" · 第 "+(S.round+1)+" 轮 · "+t;}
            return unreadable?"本地存档无法读取，不能继续。原文件已另存备份，可以在“设置 · 存档”导入导出的存档，或开始新生活（会先备份）。":"暂无可继续的进度";
        }
        void DrawTitle()
        {
            float u=Mathf.Min(H/100f,W*.009f);float y=Mathf.Max(14,3.2f*u);
            var title=UI.Tex("start/start_title");float ar=UI.Aspect(title),th=34*u,tw=th*ar;if(tw>W-32){tw=W-32;th=tw/ar;}
            UI.Picture(root,title,(W-tw)/2,y,tw,th,false);y+=th+.5f*u;float bh=12.6f*u,bw=bh*3;
            Action<string,string,Action,bool> button=(id,img,act,en)=>{UI.ImageButton(root,UI.Tex("start/"+img),(W-bw)/2,y-.4f*u,bw,bh,act,en,id,"open");y+=bh-.8f*u;};
            button("NewGame","button_new_game",NewGameFromTitle,true);button("Continue","button_continue",ContinueGame,hasSave);
            var note=SaveSummary();float nw=Mathf.Min(560,UI.Width(note,15)+30);var n=UI.Box(root,(W-nw)/2,y-1.2f*u,nw,28,new Color(1,.96f,.9f,.85f),14);
            UI.Text(n.transform,note,0,2,nw,24,15,unreadable&&!hasSave?UI.Hex("9A3B2C"):UI.Hex("6B4434"),false,TextAlignmentOptions.Center,true);y+=28-1.2f*u+.6f*u;
            button("Gallery","button_gallery",()=>{titleLayer="gallery";galleryFilter="all";Render();},true);button("Settings","button_settings",()=>{titleLayer="settings";Render();},true);
            if(titleLayer=="confirm")DrawConfirmNew();else if(titleLayer=="settings")DrawTitleSettings();else if(titleLayer=="gallery")DrawGallery();
        }
        void NewGameFromTitle(){if(hasSave||unreadable){titleLayer="confirm";Render();return;}titleLayer=null;screen="prologue";Render();}
        void ContinueGame(){if(!hasSave)return;titleLayer=null;win="room";screen="game";Render();}
        void DrawConfirmNew()
        {
            var o=Overlay("Confirm");var f=Panel(o,"confirm","开始新生活？","开始页面",true,900,0,()=>{titleLayer=null;Render();});
            f.P("开始新生活会替换当前进度"+(hasSave?"（"+UI.Safe(S.name)+" · 第 "+(S.round+1)+" 轮）":"（本地有一份无法读取的存档）")+"。先看一段开场，再到建档；<b>完成建档时才替换</b>，替换前会把旧进度另存一份备份。中途返回或刷新页面，当前进度都还在。",19);
            f.Row(hasSave?Flow.Bt("先导出",Export):Flow.Bt("保存无法读取的原文件",SaveUnreadable),Flow.Bt("取消",()=>{titleLayer=null;Render();},Look2.Secondary,true,"CancelNew","close"),Flow.Bt("开始新生活",()=>{titleLayer=null;screen="prologue";Render();},Look2.Primary,true,"ConfirmNewGame"));
            EndPanel(f);
        }
        void DrawTitleSettings()
        {
            var o=Overlay("Settings");var items=new List<KeyValuePair<string,string>>{new KeyValuePair<string,string>("display","显示"),new KeyValuePair<string,string>("sound","声音"),new KeyValuePair<string,string>("save","存档"),new KeyValuePair<string,string>("classic","经典版")};
            var f=Panel(o,"title-settings","设置","开始页面",true,980,0,()=>{titleLayer=null;Render();},"返回",(r,w)=>Tabs(r,w,items,titleTab,k=>{titleTab=k;Render();}),42);
            if(titleTab=="display")DisplaySettings(f);else if(titleTab=="sound")SoundSettings(f);
            else if(titleTab=="classic")
            {
                f.P("经典版《港硕这一年》v2：17 轮完整学年",21,UI.Hex("5A2A20"),true);
                f.P("v3 把轻松版四轮样例作为主线；原来的 17 轮完整学年（课程、小组、生活、目标与毕业结局）仍然保留，存档与轻松版互不读写。",18,UI.Muted);
                f.Row(Flow.Bt("打开经典版（17 轮）",OpenClassic,Look2.Primary,true,"OpenClassic"));
            }
            else
            {
                f.P("存档",21,UI.Hex("5A2A20"),true);
                f.P(hasSave?"当前进度："+SaveSummary()+"。进度自动保存在"+Where+"（只用轻松版自己的存档）。":unreadable?"本地存档无法读取：原文件已另存备份。可以保存原文件留底，或导入一份导出的存档。":"还没有进度。开始新生活后会自动保存。",18);
                f.Row(Flow.Bt("导出存档",Export,Look2.Secondary,hasSave),Flow.Bt("导入存档",Import),unreadable?Flow.Bt("保存无法读取的原文件",SaveUnreadable):null);
                f.P(ImportHelp()+"导入会先检查文件，成功后旧进度另存一份备份；失败时当前进度不变。原版存档不能导入轻松版。版本："+Build,16,UI.Muted);
            }
            EndPanel(f);
        }
        static string Where
        {
#if UNITY_WEBGL&&!UNITY_EDITOR
            get{return "当前浏览器";}
#else
            get{return "这台电脑";}
#endif
        }
        static string ImportHelp()
        {
#if UNITY_WEBGL&&!UNITY_EDITOR
            return "导出会下载一个 HK-Master-Life-LITE-save 开头的 .json 文件；导入时请选择这个文件。";
#else
            return "导出会把 HK-Master-Life-LITE-save 开头的 .json 文件存到桌面；导入时读取桌面或“下载”文件夹里最新的这类文件（网页版导出的也可以）。";
#endif
        }
        void DisplaySettings(Flow f)
        {
            f.P("显示",21,UI.Hex("5A2A20"),true);
            f.Row(Flow.Bt("文字：标准",()=>SetBig(false),BigText?Look2.Secondary:Look2.Selected,true,"TextNormal",BigText?"click":"none"),Flow.Bt("文字：较大",()=>SetBig(true),BigText?Look2.Selected:Look2.Secondary,true,"TextLarge",BigText?"none":"click"));
            f.P("文字大小只保存在"+Where+"，不写入游戏存档。",16,UI.Muted);
        }
        void SetBig(bool b){PlayerPrefs.SetInt("HarbourYear.Lite.BigText",b?1:0);PlayerPrefs.Save();UI.Scale=b?1.12f:1;Render();}
        void SoundSettings(Flow f)
        {
            var a=LiteAudio.Instance;f.P("声音",21,UI.Hex("5A2A20"),true);
            f.Row(Flow.Bt(a.prefs.enabled?"开启声音 ✓ 已开启":"开启声音 · 静音（默认）",()=>{a.prefs.enabled=!a.prefs.enabled;a.SavePrefs();Render();},a.prefs.enabled?Look2.Selected:Look2.Secondary,true,"SoundToggle","none"));
            var row=f.Block(34);UI.Text(row,"背景音乐",0,4,120,28,18);UI.Slider(row,130,2,360,Mathf.Round(a.prefs.bgm*100),v=>{a.prefs.bgm=v/100f;a.SavePrefs();});UI.Text(row,Mathf.RoundToInt(a.prefs.bgm*100)+"%",510,4,80,28,17,UI.Muted);
            row=f.Block(34);UI.Text(row,"音效",0,4,120,28,18);UI.Slider(row,130,2,360,Mathf.Round(a.prefs.sfx*100),v=>{a.prefs.sfx=v/100f;a.SavePrefs();});UI.Text(row,Mathf.RoundToInt(a.prefs.sfx*100)+"%",510,4,80,28,17,UI.Muted);
            f.P(a.prefs.enabled?"背景音乐 "+Mathf.RoundToInt(a.prefs.bgm*100)+"%、音效 "+Mathf.RoundToInt(a.prefs.sfx*100)+"%；拖到 0 就是这一组静音。":"目前静音：关闭时保留两组音量，重新开启后按原来的音量播放。",16,UI.Muted);
            if(Details(f,"sound-list","声音清单与说明（9 / 9 个已接入）"))
                f.P("开始页面、开场与建档用开场音乐，进入房间换成房间音乐（短暂淡出再淡入）；看旅行风景时暂停；窗口切到后台会暂停，回来后按你的设置继续。金币声只在一轮结束、看到回顾时按本轮现金净变化响一次。声音设置只保存在"+Where+"，不写入游戏存档，导入存档也不会改它。\n房间音乐 MU01 · 开始菜单／开场音乐 MU02 · 打开页面 SE01 · 关闭／返回 SE02 · 按钮（复用 SE01，低增益）· 翻页／切换页签 SE03 · 新事件提示（复用 SE01）· 本轮现金净增加 SE04 · 本轮现金净减少（复用 SE02）",16,UI.Muted);
        }
        bool Details(Flow f,string key,string label)
        {
            bool on=open.Contains(key);UI.Btn(f.root,(on?"▼ ":"▶ ")+label,f.x,f.y,f.w,40,()=>{if(!open.Remove(key))open.Add(key);Render();},Look2.Plain,true,"Details:"+key,"page",17).GetComponentInChildren<TextMeshProUGUI>().alignment=TextAlignmentOptions.MidlineLeft;f.y+=44;return on;
        }
        // ---------- CG 画廊 ----------
        void DrawGallery()
        {
            var o=Overlay("Gallery");var items=hasSave?Pace.Gallery(S,M):new List<GalleryItem>();
            var f=Panel(o,"gallery","CG 画廊","开始页面 · 当前进度",true,1040,0,()=>{titleLayer=null;Render();},"返回",items.Count>0?(Action<RectTransform,float>)((r,w)=>Tabs(r,w,new[]{new KeyValuePair<string,string>("all","全部 "+items.Count),new KeyValuePair<string,string>("event","事件 "+items.Count(x=>x.kind=="event")),new KeyValuePair<string,string>("travel","旅行 "+items.Count(x=>x.kind=="travel"))},galleryFilter,k=>{galleryFilter=k;Render();})):null,items.Count>0?42:0);
            if(items.Count==0){f.Space(40);f.P("你的香港故事，还没有留下照片。去生活里走走吧。",21,UI.Hex("6B4434"),false,24,TextAlignmentOptions.Center);f.Row(Flow.Bt("返回",()=>{titleLayer=null;Render();},Look2.Primary,true,"GalleryBack","close"));EndPanel(f);return;}
            f.P("展示当前进度已经留下的图片（共 "+items.Count+" 张）；新游戏或换存档后会随进度变化。点图片可以看大图。",16,UI.Muted);
            var shown=items.Where(x=>galleryFilter=="all"||x.kind==galleryFilter).ToList();if(shown.Count==0)f.P("这一类还没有图片。",17,UI.Muted);
            float cw=(f.w-20)/2,ch=cw*941/1672;
            for(int i=0;i<shown.Count;i+=2)
            {
                float rowH=0;
                for(int k=0;k<2&&i+k<shown.Count;k++)
                {
                    var it=shown[i+k];float x=f.x+k*(cw+20);var b=UI.ImageButton(f.root,UI.Tex(it.src),x,f.y,cw,ch,()=>{galleryZoom=it;Render();},true,"Gallery:"+it.asset,"open",false);
                    var t=UI.Text(f.root,"<b>"+it.title+"</b>\n<size=85%>"+string.Join("\n",it.notes)+"</size>",x,f.y+ch+6,cw,10,16);float th=UI.Measure(t,cw);t.rectTransform.sizeDelta=new Vector2(cw,th);rowH=Mathf.Max(rowH,ch+6+th);
                }
                f.y+=rowH+18;
            }
            EndPanel(f);
        }
        // ---------- 开场 ----------
        void DrawPrologue()
        {
            var corner=UI.Box(root,16,16,236,34,new Color(1,.965f,.918f,.88f),17,"Corner");UI.Text(corner.transform,"当前试玩：前八周，共四轮",0,5,236,26,15,UI.Hex("6B4434"),false,TextAlignmentOptions.Center);
            float w=Mathf.Min(720,W-40);var card=UI.Anchor("Prologue",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(w,10));var f=new Flow(card,42,w-84,36);
            foreach(var p in new[]{"拖着行李箱，你来到了一座陌生的城市——香港。","接下来一年的港硕生活，有课堂上的新发现，也有街角的烟火与海边的晚风。","怎样安排日子，慢慢成为怎样的人，都由你决定。不必一次想好，先从今天开始。"})f.P(p,21,UI.Hex("4A2C22"),false,14);
            f.Space(8);f.Row(Flow.Bt("开启这段生活",StartSetup,Look2.Primary,true,"PrologueStart"),Flow.Bt("跳过剧情",StartSetup,Look2.Secondary,true,"PrologueSkip"),Flow.Bt("返回开始页面",BackToTitle,Look2.Text,true,"PrologueBack","close"));
            float h=f.y+20;card.sizeDelta=new Vector2(w,h);UI.Backdrop(card,new Color(1,.973f,.925f,.9f),16);
        }
        void StartSetup(){draft=Core.New(UnityEngine.Random.Range(0,int.MaxValue));alloc=(double[])draft.initialAbilities.Clone();setupStep="quick";setupEconomy="balanced";setupHousing="standard";setupError=null;retry=null;quickName="";quickGender="female";screen="setup";Render();}
        void BackToTitle(){draft=null;retry=null;setupError=null;titleLayer=null;screen="title";Render();}
        // ---------- 建档 ----------
        void DrawSetup()
        {
            var f=Panel(root,"setup","你好，香港。","A NEW CHAPTER BEGINS",false,860,840,BackToTitle,"返回开始页面");
            if(setupError!=null)
            {
                var c=f.Card(UI.AlertBg,UI.AlertLine);c.P("<b>新生活还没有开始</b>",19,UI.Hex("8C3A2A"));c.P(setupError,17,UI.Hex("8C3A2A"));
                c.Row(retry!=null?Flow.Bt("重试",()=>CommitNewGame(retry.state),Look2.Primary):null,hasSave?Flow.Bt("导出旧进度",Export):null,Flow.Bt("取消，回到开始页面",BackToTitle,Look2.Secondary,true,null,"close"));c.End();
            }
            else if(hasSave)f.P("当前进度保持不变，直到你在这里完成建档（完成时会先另存一份旧进度备份）。中途返回，旧进度都还在。",16,UI.Muted);
            if(setupStep=="quick")QuickSetup(f);else if(setupStep=="profile")ProfileSetup(f);else if(setupStep=="economy")EconomySetup(f);else HousingSetup(f);
            f.P("全年 17 轮开发版 · 预览和切换不会扣钱，确认签约时才付款；能力分配在离开人物设置时确定",14,UI.Muted);EndPanel(f);
        }
        void QuickSetup(Flow f)
        {
            f.P("你刚来到香港读一年制硕士。接下来四轮（八周、两个阶段），先熟悉这座城市、和同学开始合作，再迎来第一批作业截止和期中考试。每个阶段只需选一次倾向，日常排程交给系统，也会替你留出休息。",19);
            f.P("你的名字",17,UI.Muted,true,6);UI.Input(f.root,f.x,f.y,Mathf.Min(420,f.w),48,quickName,20,v=>quickName=v,"新同学");f.y+=60;
            var row=f.Block(300);float pw=200;
            foreach(var g in new[]{"female","male"})
            {
                float x=g=="female"?0:pw+24;bool sel=quickGender==g;var gg=g;var b=UI.Btn(row,null,x,0,pw,300,()=>{quickGender=gg;Render();},sel?Look2.Selected:Look2.Secondary,true,"Look:"+g,sel?"none":"click");
                UI.Picture(b.transform,Portrait(Look.Default(g).bodyId),10,10,pw-20,240,false);UI.Text(b.transform,g=="female"?"女生":"男生",0,258,pw,32,19,sel?UI.White:UI.Ink,true,TextAlignmentOptions.Center);
            }
            var note=f.Card(UI.Hex("F6EBDA"),UI.Line,14);note.P("默认开局：四项能力各 30；中等均衡预算 HK$125,000，签约标准房间（一次付清 12 个月租金与押金，开学后可用现金约 HK$27,500）。主页使用完整立绘（试装图）；发型、衣装等分层部件尚无正式图片，可在“设置 · 角色与说明”中预览与保存选择。",16,UI.Muted);note.End();
            f.Wide("开始这段生活  →",()=>{var d=draft.Clone();var err=Core.QuickStart(d,quickName.Replace("<","＜").Replace(">","＞"),Look.Default(quickGender));if(err!=null){Toast(err);Render();return;}CommitNewGame(d);},Look2.Primary,true,"StartLife",54);
            f.Row(Flow.Bt("详细设置（能力分配、经济条件与住房）",()=>{draft.name=quickName.Trim()==""?"新同学":quickName.Trim();draft.appearance=Look.Default(quickGender);setupStep="profile";Render();},Look2.Text,true,"Advanced"));
        }
        Texture Portrait(string body)
        {
            switch(body){case "female_young":return UI.Tex("ui/char_female_young");case "male_young":return UI.Tex("ui/char_male_young");case "female_middle":return UI.Tex("proto/student_female");default:return UI.Tex("proto/student_male");}
        }
        void ProfileEditor(Flow f,GameState s,Action<GameState> change)
        {
            f.P("你的名字",17,UI.Muted,true,6);UI.Input(f.root,f.x,f.y,Mathf.Min(420,f.w),48,s.name=="新同学"?"":s.name,20,v=>{s.name=v.Trim()==""?"新同学":v.Replace("<","＜").Replace(">","＞");},"新同学");f.y+=60;
            var block=f.Block(372);UI.Picture(block,Portrait(s.appearance.bodyId),0,0,200,290,false);UI.Text(block,"主页使用完整立绘（试装）：女青年、男青年各一张新图，中年体型暂用原型整图。发型与衣装的选择会保存，但完整立绘不会随之变化。",0,296,206,76,13,UI.Muted);
            float x=220,y=0;
            Action<string,string[],Func<string,string>,Func<string>,Action<string>> rowOf=(label,vals,name,get,set)=>{UI.Text(block,label,x,y+8,60,30,17,UI.Muted,true);float bx=x+64;foreach(var v in vals){var vv=v;float bw=Mathf.Max(70,UI.Width(name(v),15)+22);if(bx+bw>f.w){bx=x+64;y+=42;}UI.Btn(block,name(v),bx,y,bw,38,()=>{set(vv);change(s);},get()==v?Look2.Selected:Look2.Secondary,true,label+v,get()==v?"none":"click",15);bx+=bw+6;}y+=48;};
            rowOf("体型",Cat.Bodies,v=>Cat.BodyNames[v],()=>s.appearance.bodyId,v=>s.appearance.bodyId=v);rowOf("前发",Cat.Hairs,v=>"前发 "+v,()=>s.appearance.frontHair,v=>s.appearance.frontHair=v);
            rowOf("后发",Cat.Hairs,v=>"后发 "+v,()=>s.appearance.backHair,v=>s.appearance.backHair=v);rowOf("衣装",Cat.Outfits,v=>Cat.OutfitNames[v],()=>s.appearance.outfit,v=>s.appearance.outfit=v);
            block.sizeDelta=new Vector2(f.w,Mathf.Max(372,y));f.y+=Mathf.Max(0,y-372);
        }
        void ProfileSetup(Flow f)
        {
            ProfileEditor(f,draft,d=>Render());
            f.P("分配开局能力 <size=80%><color="+UI.Rich(UI.Muted)+">四项合计 120 点，每项 0—100 的整数；开始生活后不能重新分配</color></size>",20,UI.Hex("5A2A20"),true);
            double sum=alloc.Sum(),left=120-sum;
            for(int i=0;i<4;i++)
            {
                var row=f.Block(42,6);int k=i;UI.Text(row,Cat.AbilityNames[i],0,8,120,30,18);
                Action<int> add=d=>{double v=alloc[k]+(d>0?Math.Min(d,Math.Max(0,120-alloc.Sum())):d);alloc[k]=Math.Max(0,Math.Min(100,Math.Round(v)));Render();};
                UI.Btn(row,"−5",130,0,58,40,()=>add(-5),Look2.Secondary,alloc[i]>0,"Minus5:"+i);UI.Btn(row,"−1",194,0,58,40,()=>add(-1),Look2.Secondary,alloc[i]>0,"Minus1:"+i);
                UI.Text(row,Fmt.N(alloc[i]),258,6,70,30,20,UI.Ink,true,TextAlignmentOptions.Center);
                UI.Btn(row,"+1",334,0,58,40,()=>add(1),Look2.Secondary,alloc[i]<100&&left>0,"Plus1:"+i);UI.Btn(row,"+5",398,0,58,40,()=>add(5),Look2.Secondary,alloc[i]<100&&left>0,"Plus5:"+i);
            }
            var err=Core.CheckAbilities(alloc);f.P("已分配 "+Fmt.N(sum)+" / 120"+(left>0?"，还剩 "+Fmt.N(left)+" 点":left<0?"，超出 "+Fmt.N(-left)+" 点":"，刚好分配完"),17,err!=null?UI.Warn:UI.Good,true);
            f.Row(Flow.Bt("恢复均衡 30×4",()=>{alloc=new double[]{30,30,30,30};Render();}));
            var ok=Cat.Jobs.Where(j=>Core.Qualified(j,alloc)).Select(j=>j.name).ToList();f.P("按这个分配，开局可申请："+(ok.Count>0?string.Join("、",ok):"无")+"（仍须申请并获录用）。能力不因经济条件、外观改变；之后通过上课、复习、作业和兼职成长，上限 100。",16,UI.Muted);
            f.Wide("下一步：经济条件  →",()=>{var d=draft.Clone();var e=Core.SetAbilities(d,alloc);if(e!=null){Toast(e);Render();return;}draft=d;setupStep="economy";Render();},Look2.Primary,err==null,"ToEconomy");
        }
        void EconomySetup(Flow f)
        {
            f.P("选择经济条件 <size=80%><color="+UI.Rich(UI.Muted)+">开局一次到账，之后没有固定家庭补助</color></size>",20,UI.Hex("5A2A20"),true);
            foreach(var e in Cat.Economies)
            {
                var ee=e;bool sel=setupEconomy==e.id;var c=Choice(f,"<b>"+e.name+" · HK$ "+Fmt.Money(e.budget)+"</b>"+(sel?"  ✓":"")+"\n<size=85%>"+e.description+" 推荐："+Cat.HomeOf(e.recommended).name+"。</size>",sel,()=>{setupEconomy=ee.id;setupHousing=ee.recommended;Render();},true,"Economy:"+e.id);
            }
            f.Row(Flow.Bt("‹ 上一步",()=>{setupStep="profile";Render();}),Flow.Bt("下一步：选择住房  →",()=>{setupStep="housing";Render();},Look2.Primary,true,"ToHousing"));
        }
        Button Choice(Flow f,string text,bool sel,Action click,bool enabled,string id)
        {
            var t=UI.Text(f.root,text,f.x+18,f.y+12,f.w-36,10,18,sel?UI.White:UI.Ink);float h=UI.Measure(t,f.w-36)+24;Destroy(t.gameObject);
            var b=UI.Btn(f.root,null,f.x,f.y,f.w,h,click,sel?Look2.Selected:Look2.Secondary,enabled,id,sel?"none":"click");var tx=UI.Text(b.transform,text,18,12,f.w-36,h-24,18,enabled?(sel?UI.White:UI.Ink):UI.Muted);f.y+=h+10;return b;
        }
        void HousingSetup(Flow f)
        {
            f.P("选择住房 <size=80%><color="+UI.Rich(UI.Muted)+">签约时一次付清 12 个月租金＋一个月押金；本版本不能中途换房</color></size>",20,UI.Hex("5A2A20"),true);
            int budget=Cat.Economies.First(e=>e.id==setupEconomy).budget;
            foreach(var t in Cat.Homes)
            {
                int rent=t.monthlyRent*12,dep=t.monthlyRent,left=budget-rent-dep,year=left-245*Cat.DailyCost;bool lack=left<0,sel=setupHousing==t.id;var tt=t;
                string sub=lack?"预算不足：签约需要 HK$ "+Fmt.Money(rent+dep)+"。":"签约支付 HK$ "+Fmt.Money(rent+dep)+"（年租 "+Fmt.Money(rent)+"＋押金 "+Fmt.Money(dep)+"），签约后可用现金 HK$ "+Fmt.Money(left)+"；只算每日生活费，学年结束约剩 HK$ "+Fmt.Money(year)+(year<0?"（生活预算不足）":"")+"。休息半日 "+Pace.Delta(-t.restFatigue,-t.restStress)+"；家具槽位 "+t.slots+"。";
                Choice(f,"<b>"+t.name+"（"+(t.area??"面积待定")+"）· 月租折算 HK$ "+Fmt.Money(t.monthlyRent)+"</b>"+(sel?"  ✓":"")+"\n<size=85%>"+sub+"</size>",sel,()=>{setupHousing=tt.id;Render();},!lack,"Housing:"+t.id);
            }
            f.Row(Flow.Bt("‹ 上一步",()=>{setupStep="economy";Render();}),Flow.Bt("签约，放下行李开始生活  →",()=>{var d=draft.Clone();if(d.name.Trim()=="")d.name="新同学";var e=Core.Sign(d,setupEconomy,setupHousing);if(e!=null){Toast(e);Render();return;}CommitNewGame(d);},Look2.Primary,true,"SignLease"));
        }
        void CommitNewGame(GameState state)
        {
            var lite=Pace.NewMeta();var e=Pace.NewRound(state,lite);var next=new SaveData{state=e.state,lite=e.meta};
            var err=LiteSave.SuppressWrites?null:LiteSave.Replace(next,hasSave?new SaveData{state=S,lite=M}:null,LiteSave.BeforeNewGame);
            if(err!=null){retry=new SaveData{state=state};setupError=err;Render();return;}
            bool backedUp=hasSave||unreadable;eventSounds.Clear();moneyRounds.Clear();laterKey=null;stop=null;focusSeen=null;S=next.state;M=next.lite;hasSave=true;unreadable=false;draft=null;retry=null;setupError=null;autoRun=false;undo.Clear();dialog=null;win="room";week=0;titleLayer=null;screen="game";
            Toast(backedUp?"新生活开始了。之前的进度已另存一份备份。":"");Render();
        }
        // ---------- 存档导入导出 ----------
        void Export()
        {
            if(!hasSave)return;try{var path=LiteSave.Export(LiteSave.FileName(S),LiteSave.Encode(S,M,true));Toast("轻松版存档已导出（文件名以 HK-Master-Life-LITE-save 开头）："+path);}catch(Exception e){Toast("没能导出存档："+e.Message);}Render();
        }
        void SaveUnreadable()
        {
            var raw=PlayerPrefs.GetString(LiteSave.Unreadable,PlayerPrefs.GetString(LiteSave.Key,""));if(raw==""){Toast("没有找到无法读取的原文件。");Render();return;}
            try{var p=LiteSave.Export("HK-Master-Life-LITE-unreadable-backup.json",raw);Toast("已保存无法读取的存档原文件，可交给开发者排查："+p);}catch(Exception e){Toast("没能保存："+e.Message);}Render();
        }
        void Import(){string text;var r=LiteSave.PickImport(gameObject.name,"OnImportText",out text);if(text!=null)OnImportText(text,r!=null&&r.StartsWith("file:")?r.Substring(5):null);else if(r!=null){Toast("没有导入："+r+"当前进度未改变。");Render();}}
        [UnityEngine.Scripting.Preserve] public void OnImportText(string text){OnImportText(text,null);}
        void OnImportText(string text,string file)
        {
            if(string.IsNullOrEmpty(text)||text[0]=='\u0000'){Toast("没有导入：文件无法读取。当前进度未改变。");Render();return;}
            string err;var save=LiteSave.Decode(text,out err);if(save==null){Toast("没有导入："+err+"当前进度未改变。");Render();return;}
            var rep=LiteSave.Replace(save,hasSave?new SaveData{state=S,lite=M}:null,LiteSave.BeforeImport);if(rep!=null){Toast("没有导入："+rep+"当前进度未改变。");Render();return;}
            bool backed=hasSave;eventSounds.Clear();var k=EventSoundKey(save.state);if(k!=null)eventSounds.Add(k);if(save.state.phase=="summary"||save.state.phase=="finished")moneyRounds.Add(save.state.round);
            laterKey=null;stop=null;focusSeen=null;S=save.state;M=save.lite;hasSave=true;unreadable=false;autoRun=false;undo.Clear();
            Toast("轻松版存档已导入（"+UI.Safe(S.name)+"，第 "+(S.round+1)+" 轮）"+(file!=null?"："+file:"")+(save.upgraded?"，旧格式已升级到四轮样例，进度与回忆保留":"")+(save.repaired?"；本轮兼职超过每轮 6 个半日的上限，已把超出的、尚未执行的兼职半日改成休息（"+(save.repairedSlots.Count>0?"第 "+(S.round+1)+" 轮 "+string.Join("、",save.repairedSlots.Select(SlotTitle))+"，共 "+save.repairedSlots.Count+" 个半日":"具体位置未能列出")+"），已执行的兼职与工资不变；你导入的原文件没有被改动":"")+"。"+(backed?"之前的进度已另存备份。":"")+(screen!="game"?"点“继续游戏”就能接着玩。":""));Render();
        }
        // ---------- 经典版 ----------
        public void OpenClassic()
        {
            if(hasSave&&screen=="game")LiteSave.Write(S,M);Destroy(gameObject);YearGame.Launch();
        }
    }
}
