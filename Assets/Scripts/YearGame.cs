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
    public class YearGame : MonoBehaviour
    {
        public YearState State { get; private set; }
        RectTransform root,modal;Texture2D harbour;string tab="日程",toast="";bool menu=true;int selectedCourse,replaceIndex=-1;bool selectedProject;ActionKind selectedAction;
        float toastUntil; Canvas canvas;
        public bool MenuVisible {get{return menu;}}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(FindObjectOfType<YearGame>()==null)new GameObject("Harbour Year / Game").AddComponent<YearGame>();}
        void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;harbour=Resources.Load<Texture2D>("Art/Harbour");NotebookUI.Font=Resources.Load<TMP_FontAsset>("Fonts/Notebook SDF");
            if(NotebookUI.Font==null){var source=Resources.Load<UnityEngine.Font>("Fonts/NotebookSans");NotebookUI.Font=TMP_FontAsset.CreateFontAsset(source);NotebookUI.Font.atlasPopulationMode=AtlasPopulationMode.Dynamic;NotebookUI.Font.isMultiAtlasTexturesEnabled=true;}
            var go=new GameObject("Notebook Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,1000);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var rect=(RectTransform)go.transform;root=Rect("Notebook",rect,0,0,1600,1000);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            if(FindObjectOfType<EventSystem>()==null)new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            var camera=new GameObject("Paper Camera",typeof(Camera)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Paper;camera.orthographic=true;
            Render();
        }
        void Update(){if(Input.GetKeyDown(KeyCode.Escape)){if(modal!=null){Destroy(modal.gameObject);modal=null;if(!menu&&State.phase!="plan")Render();}else if(!menu)ConfirmMenu();}if(toast!=""&&Time.unscaledTime>toastUntil){toast="";Render();}}
        void OnApplicationPause(bool paused){if(paused&&State!=null)YearSave.Write(State);}
        void OnApplicationQuit(){if(State!=null)YearSave.Write(State);}
        void Clear(){for(int i=root.childCount-1;i>=0;i--){root.GetChild(i).gameObject.SetActive(false);Destroy(root.GetChild(i).gameObject);}modal=null;}
        public void NewGame(){State=new YearState();menu=false;tab="日程";YearSave.Write(State);Render();ShowGuide();}
        public void ContinueGame(){State=YearSave.Read();if(State==null){Notice("暂时没有可读取的存档，请开始新游戏。");return;}menu=false;tab="日程";Render();if(YearSave.LastError!="")Notice(YearSave.LastError);}
        public void SaveAndRender(){if(State!=null)YearSave.Write(State);Render();}
        void Notice(string text){toast=text;toastUntil=Time.unscaledTime+5;Render();}
        public void Render()
        {
            Clear();Box(root,0,0,1600,1000,Paper);
            if(menu){DrawMenu();return;}
            Header();Sidebar();
            if(State.phase=="ending"){DrawEnding();return;}
            if(tab=="课程")DrawCourses();else if(tab=="目标")DrawGoals();else if(tab=="回忆")DrawJournal();else DrawPlanner();
            Text(root,"HONG KONG, ONE YEAR     /     每一个普通的日子，都在成为你。",40,954,920,27,15,Muted);
            Text(root,YearSave.LastError!=""?YearSave.LastError:"已自动保存 · 本机单人存档",1200,953,360,30,16,YearSave.LastError!=""?Red:Muted,false,TextAlignmentOptions.TopRight);
            if(State.phase=="event")DrawEvent();else if(State.phase=="summary")DrawSummary();
            if(toast!=""){Box(root,460,908,1040,40,Navy);Text(root,toast,478,914,1005,30,17,White);}
        }
        void DrawMenu()
        {
            Picture(root,harbour,595,0,1005,1000);Box(root,0,0,625,1000,Paper);Box(root,62,70,42,5,Orange);
            Text(root,"一本关于成长、选择与日常的手账",62,102,485,40,22,Muted);
            Text(root,"港硕\n这一年",55,185,545,255,100,Navy,true);Text(root,"HONG KONG, ONE YEAR",64,465,500,45,21,Orange,true);
            Text(root,"带着一只行李箱来到香港。\n在课业、机会与生活之间，\n写下属于自己的二十六个两周。",65,547,480,126,24,Ink);
            Button(root,"开始新的一年  →",65,717,477,65,()=>{if(YearSave.Exists)ConfirmRestart();else NewGame();},Navy,White,23,true,"NewGame");
            Button(root,"继续我的手账",65,798,477,62,ContinueGame,Pale,Navy,22,YearSave.Exists,"Continue");
            Text(root,"26 回合  /  多种人生去向  /  无恋爱攻略",65,896,495,35,18,Muted);Text(root,"第一册 · 香港",1370,940,180,34,19,White);
        }
        void Header()
        {
            Text(root,"港硕这一年",40,33,300,54,35,Navy,true);Text(root,"学生手账  /  2026—2027",300,48,300,30,16,Muted);
            string[] tabs={"日程","课程","目标","回忆"};for(int i=0;i<4;i++){string t=tabs[i];Button(root,t,835+i*137,34,120,48,()=>{tab=t;Render();},tab==t?Navy:Paper,tab==t?White:Ink,21,true,"Tab"+t);}
            Button(root,"帮助",1407,34,75,48,ShowGuide,Pale,Navy,18);Button(root,"菜单",1495,34,65,48,ConfirmMenu,Pale,Navy,18);
            Box(root,40,99,1520,1,Line);
        }
        void Sidebar()
        {
            var p=Box(root,40,122,250,803,Navy).transform;
            Text(p,"STUDENT NOTEBOOK",23,23,220,25,14,Hex("B8C5C6"));Text(p,"在港 · 第一年",23,67,210,45,28,White,true);
            Box(p,23,128,204,1,Hex("546875"));Text(p,"第 "+State.round.ToString("00")+" / 26 回合",23,151,212,45,27,White);Text(p,Rules.Month(State.round),23,202,210,30,18,Hex("C6D2CC"));
            string[] n={"学业能力","科研能力","职业能力","城市适应"};float[] v={State.academic,State.research,State.career,State.adaptation};
            for(int i=0;i<4;i++){float y=265+i*64;Text(p,n[i],23,y,150,27,18,White);Text(p,v[i].ToString("0"),179,y,45,30,22,White,true,TextAlignmentOptions.TopRight);Bar(p,23,y+38,204,v[i],i%2==0?Hex("DAA477"):Hex("93B3A0"),4);}
            Box(p,23,531,204,1,Hex("546875"));Text(p,"生活费余额",23,551,203,28,17,Hex("C6D2CC"));Text(p,"$ "+State.money.ToString("N0"),23,584,210,47,31,White,true);Text(p,State.debt>0?"应急金待还 $"+State.debt:"每两周生活费 $450",23,634,215,27,16,State.debt>0?Hex("EDBA93"):Hex("C6D2CC"));
            Text(p,"压力",23,691,115,27,18,White);Text(p,State.stress.ToString("0")+" / 100",120,691,105,28,19,White,false,TextAlignmentOptions.TopRight);Bar(p,23,734,204,State.stress,State.stress>=70?Hex("E2A075"):Hex("93B3A0"),7);
            Text(p,State.stress>=70?"先休息一下，也是一种安排。":"慢慢来，也记得抬头看海。",23,756,208,30,14,Hex("C6D2CC"));
        }
        void DrawPlanner()
        {
            Picture(root,harbour,318,122,875,242);Box(root,340,145,346,195,new Color(.99f,.97f,.91f,.96f));
            Text(root,State.round<=13?"上学期 · 从陌生到熟悉":"下学期 · 把想法变成方向",361,165,307,30,16,Orange,true);
            Text(root,"把日子，\n过成自己的答案。",358,206,320,90,31,Navy,true);Text(root,"一回合两周，每一步都算数。",362,308,310,25,16,Muted);
            Text(root,"这两周，怎么过？",318,393,420,48,31,Navy,true);Text(root,"安排四件事 · 按顺序执行",846,404,347,30,18,Muted,false,TextAlignmentOptions.TopRight);
            for(int i=0;i<4;i++)
            {
                int index=i;float x=318+i*223;bool filled=i<State.plan.Count;var p=Box(root,x,455,207,164,White).transform;Box(p,0,0,207,4,filled?Orange:Line);
                Text(p,(i+1).ToString("00"),14,17,56,36,23,Orange,true);
                if(filled){var item=State.plan[i];Text(p,Catalog.Actions[(int)item.kind].name,15,61,176,34,23,Navy,true);Text(p,(int)item.kind<3?Catalog.Courses[item.course].title+(item.kind==ActionKind.Coursework?(item.project?" / 项目":" / 作业"):""):Catalog.Actions[(int)item.kind].tag+" · "+Catalog.Actions[(int)item.kind].yields,15,99,177,42,14,Muted);
                    Button(p,"改",125,16,31,30,()=>OpenActionPicker(index),Pale,Navy,14,State.phase=="plan");Button(p,"×",163,16,30,30,()=>{State.plan.RemoveAt(index);SaveAndRender();},Pale,Navy,17,State.phase=="plan");
                    Button(p,"←",15,136,37,23,()=>{Rules.MovePlan(State,index,index-1);SaveAndRender();},White,Muted,18,State.phase=="plan"&&index>0);Button(p,"→",57,136,37,23,()=>{Rules.MovePlan(State,index,index+1);SaveAndRender();},White,Muted,18,State.phase=="plan"&&index<State.plan.Count-1);
                }else{Button(p,"＋",60,49,83,55,()=>OpenActionPicker(-1),White,Muted,35,State.phase=="plan");Text(p,"从下方挑选一项行动",14,123,183,28,14,Muted,false,TextAlignmentOptions.Midline);}
            }
            Text(root,"把想做的事放进日程",318,648,500,31,21,Navy,true);Text(root,"点击卡片查看收益与条件",865,652,329,25,16,Muted,false,TextAlignmentOptions.TopRight);
            for(int i=0;i<Catalog.Actions.Length;i++)
            {
                var a=Catalog.Actions[i];float x=318+(i%5)*177,y=695+(i/5)*88;var p=Box(root,x,y,167,77,White).transform;
                var b=p.gameObject.AddComponent<Button>();p.GetComponent<Image>().raycastTarget=true;b.targetGraphic=p.GetComponent<Image>();b.interactable=State.phase=="plan";b.onClick.AddListener(()=>OpenAction(a.kind));
                Text(p,a.name,12,9,144,31,21,Navy,true);Text(p,(a.money>0?"+$"+a.money:a.money<0?"−$"+-a.money:"$0")+"   压力 "+Catalog.Signed(a.stress),12,48,148,23,14,a.stress<0?Sage:Muted);
            }
            Button(root,"确认日程，度过这两周  →",318,879,875,46,ConfirmTurn,Orange,White,21,State.phase=="plan"&&State.plan.Count==4,"ExecuteTurn");
            DrawDeadlines();
        }
        void DrawDeadlines()
        {
            var p=Box(root,1221,122,339,803,Hex("EEE8DA")).transform;Text(p,"别忘了这些事",22,23,296,40,25,Navy,true);Text(p,"截止日会自动提交已有成果",22,73,296,26,16,Muted);Box(p,22,114,293,1,Line);
            var tasks=new List<Tuple<int,int,bool>>();for(int i=0;i<4;i++)if(State.round>=Catalog.Courses[i].open)foreach(bool pr in new[]{false,true})if(!(pr?State.courses[i].project:State.courses[i].assignment).submitted)tasks.Add(Tuple.Create(pr?Catalog.Courses[i].projectDue:Catalog.Courses[i].homeworkDue,i,pr));
            int count=0;foreach(var t in tasks.OrderBy(x=>x.Item1).Take(4))
            {
                var c=Catalog.Courses[t.Item2];var task=t.Item3?State.courses[t.Item2].project:State.courses[t.Item2].assignment;float y=140+count*126;bool late=t.Item1<State.round;
                Text(p,late?"已过期 · 仍可补交":"第 "+t.Item1+" 回合截止",22,y,290,28,17,late?Red:Orange,true);Text(p,c.title,22,y+34,291,32,19,Navy);Text(p,(t.Item3?"期末项目":"阶段作业")+"  "+task.work.ToString("0")+"%",22,y+72,284,26,16,Muted);Bar(p,22,y+106,293,task.work,Sage,3);count++;
            }
            if(count==0)Text(p,"当前学期的任务\n已经全部提交。\n\n给自己一个小小的勾。",23,170,290,190,21,Sage);
            Button(p,"打开课程清单  →",22,670,293,49,()=>{tab="课程";Render();},White,Navy,18);
            Text(p,State.round>=16?"申请窗口：第 16—24 回合\n高属性之外，也记得真的投递。":"下一步：探索你的年度目标\n不必现在就决定最终的去向。",22,741,293,52,15,Muted);
        }
        void OpenActionPicker(int replace)
        {
            if(State.phase!="plan")return;var p=Modal(replace>=0?"这一格，换成什么？":"挑选一件想做的事。","行动目录",1050,625);
            for(int i=0;i<Catalog.Actions.Length;i++){var a=Catalog.Actions[i];float x=38+(i%5)*198,y=158+(i/5)*134;var b=Button(p,"",x,y,181,116,()=>OpenAction(a.kind,replace),Pale,Navy,20);Text(b.transform,a.name,14,17,156,38,23,Navy,true);Text(b.transform,a.tag+" · 压力 "+Catalog.Signed(a.stress),14,72,156,29,15,Muted);}
            Text(p,"选好行动后，还可以查看基础收益、费用与前置条件。",38,454,970,46,20,Muted);Button(p,"回到日程",38,535,973,49,CloseModal,Navy,White,21);
        }
        void OpenAction(ActionKind kind,int replace=-1)
        {
            if(State.phase!="plan")return;selectedAction=kind;replaceIndex=replace;
            if(replace>=0){selectedCourse=State.plan[replace].course;selectedProject=State.plan[replace].project;}else if(State.round>=14&&selectedCourse<2)selectedCourse=2;
            DrawActionDialog();
        }
        RectTransform Modal(string heading,string eyebrow,float w=890,float h=720)
        {
            if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);}
            modal=Rect("Modal",root,0,0,1600,1000);var bg=Box(modal,0,0,1600,1000,new Color(.08f,.15f,.2f,.64f));bg.raycastTarget=true;
            var p=Box(modal,(1600-w)/2,(1000-h)/2,w,h,White).rectTransform;Box(p,0,0,w,6,Orange);Text(p,eyebrow,38,29,w-90,30,16,Orange,true);Text(p,heading,38,74,w-90,60,33,Navy,true);return p;
        }
        void CloseModal(){if(modal!=null){Destroy(modal.gameObject);modal=null;}}
        void DrawActionDialog()
        {
            var d=Catalog.Actions[(int)selectedAction];var p=Modal(d.name,"安排行动  /  "+d.tag,900,766);
            Text(p,d.detail,38,139,820,62,21,Ink);Text(p,"基础收益："+d.yields+"\n现金 "+Catalog.Signed(d.money)+" · 压力 "+Catalog.Signed(d.stress),38,211,820,75,21,Sage,true);
            if((int)selectedAction<3)
            {
                Text(p,"选择课程",38,315,800,30,20,Navy,true);
                for(int i=0;i<4;i++){int id=i;var c=Catalog.Courses[i];Button(p,c.title+(State.round<c.open?"（第 "+c.open+" 回合）":""),38+(i%2)*420,357+(i/2)*61,401,50,()=>{selectedCourse=id;DrawActionDialog();},selectedCourse==i?Navy:Pale,selectedCourse==i?White:Navy,18,State.round>=c.open);}
                if(selectedAction==ActionKind.Coursework){Button(p,"阶段作业",38,494,399,47,()=>{selectedProject=false;DrawActionDialog();},!selectedProject?Sage:Pale,!selectedProject?White:Navy,18);Button(p,"期末项目",458,494,399,47,()=>{selectedProject=true;DrawActionDialog();},selectedProject?Sage:Pale,selectedProject?White:Navy,18);}
            }
            else Text(p,selectedAction==ActionKind.Research?"参与 4 次科研形成论文初稿。\n博士申请还需要 6 次科研、导师联系与课程成绩。":selectedAction==ActionKind.Internship?"实习收入到账，但压力也会累积。\n留港申请需要至少 2 次实习，拿到回复后才能选择去向。":"同回合重复同类行动，成长效率依次为 100%、80%、60%、45%。\n压力达到 70 / 90 时，成长与课业效率再降至 70% / 50%。\n减压与现金收益不打折；把休息排在前面会改变后续效率。",38,330,812,170,21,Muted);
            var item=new PlanItem(selectedAction,selectedCourse,selectedProject);string locked=Rules.ActionLock(State,item);Text(p,locked!=""?locked:"属性达到 70 后，行动带来的该属性成长按 40% 计；课业进度不受此影响。",38,577,820,55,18,locked!=""?Red:Muted);
            Button(p,"返回",38,666,194,58,CloseModal,Pale,Navy,20);
            Button(p,replaceIndex>=0?"替换这个行动":"加入日程  +",251,666,606,58,()=>{if(replaceIndex>=0)State.plan[replaceIndex]=item;else Rules.AddPlan(State,item);SaveAndRender();},Orange,White,22,locked==""&&(replaceIndex>=0||State.plan.Count<4),"AddPlan");
        }
        void ConfirmTurn()
        {
            if(State.phase!="plan"||State.plan.Count!=4)return;var p=Modal("这两周，就这样安排。","确认日程",900,690);
            for(int i=0;i<4;i++)Text(p,(i+1)+".  "+Rules.PlanName(State.plan[i]),38,158+i*66,820,53,22,Navy);
            int delta=State.plan.Sum(a=>Catalog.Actions[(int)a.kind].money)-Rules.LivingCost;Text(p,"预计现金变化 $"+Catalog.Signed(delta)+"（含生活费；不含事件与应急金偿还）\n收益按执行时压力与重复次数计算，之后会遇到本回合事件。",38,456,816,78,18,Muted);
            Button(p,"再调整一下",38,585,270,58,CloseModal,Pale,Navy,21);Button(p,"确定执行  →",329,585,528,58,()=>{Rules.Execute(State);SaveAndRender();},Orange,White,22,true,"ConfirmExecute");
        }
        void DrawCourses()
        {
            Text(root,"课程与成绩",320,130,700,55,37,Navy,true);Text(root,"学习占 40% · 作业占 25% · 项目占 35%   /   每门课都需要真实投入",321,197,1220,35,20,Muted);
            for(int i=0;i<4;i++)
            {
                int id=i;var c=Catalog.Courses[i];var s=State.courses[i];float x=320+(i%2)*631,y=255+(i/2)*305;var p=Box(root,x,y,607,283,White).transform;bool open=State.round>=c.open;
                Text(p,c.code+"  /  "+(i<2?"上学期":"下学期"),22,15,360,25,15,Orange,true);Text(p,c.title,22,53,480,35,25,Navy,true);Text(p,open?Rules.Score(State,i,true).ToString("0.0")+" 分":"未开放",462,17,121,34,21,open?Sage:Muted,true,TextAlignmentOptions.TopRight);
                Text(p,"学习进度 "+s.learning.ToString("0")+"% · 实际投入 "+s.studyActions+" 次",22,102,550,28,18,Muted);Bar(p,22,138,560,s.learning,Sage,5);
                Text(p,TaskLine(s.assignment,"作业",c.homeworkDue),22,161,558,31,17,Ink);Text(p,TaskLine(s.project,"项目",c.projectDue),22,201,558,31,17,Ink);
                Button(p,open?"管理任务 / 提交  →":"第 "+c.open+" 回合开放",22,240,559,32,()=>CourseDetail(id),Pale,Navy,16,open);
            }
            Text(root,"毕业要求：四门课均 ≥50 分、8 项任务全部提交、最终 GPA ≥2.00，并提交毕业申请。",320,889,1227,37,20,Navy);
        }
        string TaskLine(CourseTask t,string name,int due){return name+" "+(t.submitted?(t.late?"已补交":"已提交")+" · 质量 "+t.quality.ToString("0"):t.work.ToString("0")+"% · "+(State.round>due?"已逾期，可补交":"待提交"))+"   /   第 "+due+" 回合截止";}
        void CourseDetail(int index)
        {
            var c=Catalog.Courses[index];var s=State.courses[index];var p=Modal(c.title,c.code+"  /  课程档案",950,750);Text(p,c.note,38,143,865,62,21,Muted);
            Text(p,"学习进度 "+s.learning.ToString("0")+"%    ·    已提交成绩 "+Rules.Score(State,index).ToString("0.0")+"    ·    预计 "+Rules.Score(State,index,true).ToString("0.0"),38,227,865,42,22,Sage,true);
            for(int i=0;i<2;i++){bool project=i==1;var t=project?s.project:s.assignment;int due=project?c.projectDue:c.homeworkDue;float y=310+i*120;Text(p,TaskLine(t,project?"期末项目":"阶段作业",due),38,y,600,57,20,Navy);Bar(p,38,y+73,605,t.work,Sage,5);
                Button(p,t.submitted?"已提交":State.round>due?"确认补交":"立即提交",674,y,226,66,()=>{Rules.Submit(State,index,project);SaveAndRender();CourseDetail(index);},Orange,White,20,State.phase=="plan"&&!t.submitted&&t.work>0);}
            Text(p,"截止日自动提交有进度的任务；零进度任务将逾期。补交按完成质量的 75% 计分。\n提交后不能继续修改该任务，提前提交前请确认完成度。",38,568,866,70,18,Muted);Button(p,"收好课程档案",38,660,865,51,CloseModal,Navy,White,21);
        }
        void DrawGoals()
        {
            Text(root,"这一年，想走向哪里？",320,130,1125,58,37,Navy,true);Text(root,"条件始终在这里。去向需要主动申请，收到 offer 后，毕业时由你决定。",321,198,1215,36,20,Muted);
            var list=Scroll(root,320,257,1240,656,1080);
            string[] routes={"hk","mainland","phd"};string[] desc={"职业 ≥55 · 适应 ≥40 · 实习 ≥2 次\n回复时预计 GPA ≥2.70；正式毕业后聘用生效。","职业 ≥50 · 求职准备 ≥3 次\n回复时预计 GPA ≥2.30；正式毕业后聘用生效。","科研 ≥60 · 学业 ≥50 · 科研 ≥6 次 · 导师联系\n回复时预计 GPA ≥3.00；正式毕业后录取生效。"};
            for(int i=0;i<3;i++)
            {
                string route=routes[i];float y=i*196;var p=Box(list,0,y,1220,178,White).transform;Text(p,Catalog.RouteName(route),25,20,385,36,27,Navy,true);Text(p,desc[i],25,70,786,70,20,Muted);
                var app=State.applications.FirstOrDefault(a=>a.route==route);string locked=Rules.ApplicationLock(State,route);
                Text(p,app==null?(locked==""?"条件已满足，材料费 $150":locked):app.offer?"已获有条件 offer":app.resolved?"申请未通过":"第 "+app.replyRound+" 回合回复",820,18,371,60,18,app!=null&&app.offer?Sage:Orange);
                Button(p,app==null?"提交申请  →":"查看申请",820,108,372,47,()=>{if(app!=null)ShowApplication(app);else{Rules.Apply(State,route);SaveAndRender();}},Navy,White,19,State.phase=="plan"&&(app!=null||locked==""));
            }
            var grad=Box(list,0,588,1220,159,Hex("E5EBDD")).transform;Text(grad,"顺利毕业 · 是所有去向的起点",25,19,990,37,26,Navy,true);Text(grad,"四门课各 ≥50 · 8 项任务已交 · GPA ≥2.00 · 毕业申请已提交\n当前已提交成绩 GPA "+Rules.GPA(State).ToString("0.00")+"  /  预计 GPA "+Rules.GPA(State,true).ToString("0.00"),25,69,790,70,19,Ink);
            Button(grad,State.graduationApplied?"毕业申请已提交":State.round<22?"第 22 回合开放":"提交毕业申请",820,87,372,46,()=>{State.graduationApplied=true;Rules.Experience(State,"在目标页提交毕业申请。");SaveAndRender();},Sage,White,18,State.phase=="plan"&&!State.graduationApplied&&State.round>=22);
            var h=Box(list,0,765,1220,205,White).transform;Text(h,"可叠加的荣誉",25,18,850,37,25,Navy,true);Text(h,"学业奖学金：主动申请 + 最终 GPA ≥3.60 + 学业 ≥65 + 顺利毕业。\n优秀毕业表现：GPA ≥3.60、四课各 ≥80，且科研或实习 ≥4 次。\n荣誉独立授予，可以和工作、博士或探索方向同时获得。",25,71,788,107,19,Muted);
            Button(h,State.scholarshipApplied?"奖学金申请已记录":"申请学业奖学金",820,111,372,48,()=>{State.scholarshipApplied=true;Rules.Experience(State,"提交了学业奖学金申请。");SaveAndRender();},Orange,White,18,State.phase=="plan"&&!State.scholarshipApplied);
            Text(list,"无 offer 也可顺利毕业、继续探索；课程或手续不达标会延期，并显示补修原因。\n申请窗口为第 16—24 回合，2 回合后回复，每个去向本年度可申请一次。",25,990,1170,73,19,Muted);
        }
        void ShowApplication(ApplicationState a){var p=Modal(Catalog.RouteName(a.route),"申请档案",850,490);Text(p,"第 "+a.submittedRound+" 回合投递  /  第 "+a.replyRound+" 回合回复\n\n"+a.reason,38,153,771,192,23,Ink);Button(p,"收好这封邮件",38,389,772,58,CloseModal);}
        void DrawJournal()
        {
            Text(root,"这一年的小事",320,130,1115,57,37,Navy,true);Text(root,"事件选择、课程提交和申请消息，都留在这里。",320,201,1210,34,20,Muted);
            var lines=State.journal.AsEnumerable().Reverse().ToList();var content=Scroll(root,320,263,1240,654,Math.Max(654,lines.Count*84));for(int i=0;i<lines.Count;i++){Text(content,lines[i],21,i*84+12,1172,61,20,Ink);Box(content,20,i*84+78,1174,1,Line);}
        }
        void DrawEvent()
        {
            var e=Events.Get(State);var p=Modal(e.title,"生活来信  /  "+e.location,1050,814);Text(p,e.body,38,157,972,163,24,Ink);
            for(int i=0;i<e.choices.Length;i++){int id=i;var c=e.choices[i];float y=344+i*124;var b=Button(p,"",38,y,973,107,()=>{Events.Choose(State,id);SaveAndRender();},Pale,Navy,20,c.unavailable=="","EventChoice"+i);Text(b.transform,c.title,22,15,925,36,22,Navy,true);Text(b.transform,c.effect,22,60,925,33,18,Sage);}
            Text(p,"选择会被记入手账，部分经历将在之后的日子再次出现。",38,758,970,31,17,Muted);
        }
        void DrawSummary()
        {
            var p=Modal("第 "+State.round+" 回合 · 两周小结","翻过这一页，也看看自己走了多远。",1110,838);var content=Scroll(p,38,154,1035,549,Math.Max(549,State.summary.Count*92));
            for(int i=0;i<State.summary.Count;i++){Text(content,State.summary[i],0,i*92+8,1009,80,19,Ink);Box(content,0,i*92+89,1005,1,Line);}
            Button(p,State.round==26?"去看看这一年的结局  →":"收好这一页，进入下一回合  →",38,743,1035,56,()=>{Rules.Advance(State);SaveAndRender();},Orange,White,23,true,"NextRound");
        }
        void DrawEnding()
        {
            var paths=Rules.Endings(State);if(State.ending==""&&paths.Count==1){State.ending=paths[0];YearSave.Write(State);}
            string r=State.ending;Text(root,r==""?"几个方向，都在等你。":r=="delay"?"慢一点，也会抵达。":"这一年，成为了你。",321,129,1200,58,38,Navy,true);
            if(r=="")
            {
                Text(root,"你已完成毕业要求。收到的机会并不替你做决定，选一个想去的方向。",321,205,1220,48,22,Muted);
                for(int i=0;i<paths.Count;i++){string path=paths[i];Button(root,Catalog.RouteName(path)+"  →",321,300+i*115,1220,86,()=>{State.ending=path;Rules.Experience(State,"选择最终去向："+Catalog.RouteName(path));SaveAndRender();},i==0?Orange:White,i==0?White:Navy,26);}return;
            }
            var content=Scroll(root,320,213,1240,642,Math.Max(1040,935+State.journal.Count*47));
            Picture(content,harbour,0,0,1208,190);Box(content,21,26,468,133,new Color(.99f,.97f,.91f,.96f));Text(content,Catalog.RouteName(r),43,43,425,50,34,Navy,true);Text(content,"最终 GPA  "+Rules.GPA(State).ToString("0.00")+" / 4.00",45,103,415,35,21,Sage,true);
            Text(content,Rules.EndingStory(r),0,219,1198,119,23,Ink);Text(content,"为什么走到这里",0,357,1180,37,25,Navy,true);string reason=r=="delay"?Rules.GraduationReason(State):r=="explore"?Rules.GraduationReason(State)+"你选择给下一段生活留出探索空间。":Rules.GraduationReason(State)+"已主动申请「"+Catalog.RouteName(r)+"」并收到 offer。";
            Text(content,reason,0,405,1198,91,20,Muted);
            for(int i=0;i<4;i++){float x=(i%2)*609,y=521+(i/2)*67;Text(content,Catalog.Courses[i].title, x,y,416,32,21,Navy);Text(content,Rules.Score(State,i).ToString("0.0")+"  /  "+Rules.Letter(Rules.Score(State,i)),x+414,y,180,35,22,Sage,true,TextAlignmentOptions.TopRight);}
            var honors=Rules.Honors(State);Text(content,"荣誉  /  "+(honors.Count>0?string.Join(" · ",honors.ToArray()):"完成自己的这一程，也值得被记住。"),0,685,1200,47,23,Orange,true);
            Text(content,"科研 "+State.researchCount+" 次 · 实习 "+State.internshipCount+" 次 · 求职准备 "+State.careerCount+" 次 · 城市探索 "+State.exploreCount+" 次\n余额 $"+State.money+" · 应急金待还 $"+State.debt+" · 属性与压力见左侧最终面板",0,749,1200,80,20,Muted);
            Text(content,"重要经历 · 这一年留下的脚注",0,853,1200,44,25,Navy,true);for(int i=0;i<State.journal.Count;i++)Text(content,State.journal[i],0,917+i*47,1197,42,18,Ink);
            Button(root,"重新开始一年",320,879,492,46,ConfirmRestart,Orange,White,20);Button(root,"返回扉页",833,879,722,46,()=>{menu=true;SaveAndRender();},Pale,Navy,20);
        }
        void ShowGuide()
        {
            var p=Modal("先把这一页读完。","新生手账指南",1010,803);
            string[] titles={"01  排好四件事，再确认两周日程","02  课程需要投入，也需要提交","03  给生活留余地，给未来发申请"};string[] bodies={"从行动卡选内容；学习行动要选课程，课业还要选作业或项目。\n日程上的「改」「×」「← →」可替换、删除和调整顺序。","学习、作业、项目分别影响成绩。截止日自动提交有进度的任务，\n也可在课程页提前提交；错过后仍可补交，但质量按 75% 计。","压力 70 / 90 时效率下降；同回合重复行动的成长逐次减弱。\n目标页列出所有条件。第 16—24 回合申请去向，第 22 回合起申请毕业。"};
            for(int i=0;i<3;i++){Text(p,titles[i],38,161+i*157,932,40,24,Navy,true);Text(p,bodies[i],38,216+i*157,932,82,21,Muted);}
            Text(p,"学费、基础住宿费已付；每回合生活费 $450。应急金可兜底，不会卡住游戏。",38,649,932,57,18,Sage);
            Button(p,"明白了，翻开我的手账  →",38,719,932,48,()=>{if(State!=null){State.tutorialDone=true;YearSave.Write(State);}Render();},Orange,White,22,true,"CloseGuide");
        }
        void ConfirmRestart()
        {
            var p=Modal("重新翻开一本手账？","重新开始",840,458);Text(p,"新的一年会覆盖当前自动存档。\n如果还想继续这一程，可以先返回。",38,158,760,117,24,Ink);Button(p,"保留当前手账",38,348,362,61,CloseModal,Pale,Navy,21);Button(p,"开始新的一年",423,348,374,61,NewGame,Orange,White,21,true,"ConfirmNewGame");
        }
        void ConfirmMenu()
        {
            var p=Modal("暂时合上手账。","游戏菜单",840,458);Text(p,"当前进度已自动保存。\n下次从「继续我的手账」接着这一页。",38,155,760,119,24,Ink);Button(p,"继续这一页",38,347,230,60,CloseModal,Pale,Navy,20);Button(p,"重新开始",282,347,246,60,ConfirmRestart,Pale,Navy,20);Button(p,"保存并返回扉页",542,347,256,60,()=>{menu=true;SaveAndRender();},Navy,White,20);
        }
    }
}
