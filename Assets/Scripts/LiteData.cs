using System;
using System.Collections.Generic;
using System.Linq;
// 轻松版四轮样例：日历、课表与数值目录。规则与数值逐项移植自《港硕生活模拟器 · 轻松版四轮样例 a320095》（源码提交 02dac96）。
namespace HarbourYear.Lite
{
    public class RoundInfo{public int index,semester,roundInSemester,startDay,endDay;public string kind;}
    public class Session{public string id,course,code,type,kind,half,plannedHalf,moveReason;public int semester,week,day,plannedDay;}
    public class Slot{public int day;public string half;}
    public static class Cal
    {
        static readonly DateTime Origin=new DateTime(2026,8,31,0,0,0,DateTimeKind.Utc);
        public static int SemStart(int sem){return sem==1?0:133;}
        public static readonly RoundInfo[] Rounds=BuildRounds();
        public static int Last{get{return Rounds.Length-1;}}
        static RoundInfo[] BuildRounds()
        {
            var r=new List<RoundInfo>();
            for(int t=0;t<8;t++)r.Add(new RoundInfo{index=t,kind="semester",semester=1,roundInSemester=t+1,startDay=t*14,endDay=t*14+13});
            r.Add(new RoundInfo{index=8,kind="winter",semester=0,startDay=112,endDay=132});
            for(int t=0;t<8;t++){int n=133+t*14;r.Add(new RoundInfo{index=9+t,kind="semester",semester=2,roundInSemester=t+1,startDay=n,endDay=n+13});}
            return r.ToArray();
        }
        public static RoundInfo RoundOfDay(int day){return Rounds.FirstOrDefault(t=>day>=t.startDay&&day<=t.endDay);}
        public static DateTime DateOf(int day){return Origin.AddDays(day);}
        public static string Iso(int day){return DateOf(day).ToString("yyyy-MM-dd");}
        public static int Weekday(int day){return ((day%7)+7)%7;}
        public static int DayOf(string iso){var p=iso.Split('-').Select(int.Parse).ToArray();return (int)Math.Round((new DateTime(p[0],p[1],p[2],0,0,0,DateTimeKind.Utc)-Origin).TotalDays);}
        public static string Date(int day){var d=DateOf(day);return d.Month+"月"+d.Day+"日";}
        public static readonly string[] Weekdays={"周一","周二","周三","周四","周五","周六","周日"};
        public static string Half(string h){return h=="am"?"上午":h=="pm"?"下午":"";}
        static readonly Dictionary<int,string> Holidays=new[]{
            new[]{"2026-09-26","中秋节翌日"},new[]{"2026-10-01","国庆日"},new[]{"2026-10-19","重阳节翌日（重阳节逢周日）"},new[]{"2026-12-25","圣诞节"},new[]{"2026-12-26","圣诞节后第一个周日"},
            new[]{"2027-01-01","一月一日"},new[]{"2027-02-06","农历年初一"},new[]{"2027-02-08","农历年初三"},new[]{"2027-02-09","农历年初四（年初二逢周日）"},new[]{"2027-03-26","耶稣受难节"},
            new[]{"2027-03-27","耶稣受难节翌日"},new[]{"2027-03-29","复活节星期一"},new[]{"2027-04-05","清明节"},new[]{"2027-05-01","劳动节"}}.ToDictionary(e=>DayOf(e[0]),e=>e[1]);
        public static string Holiday(int day){string n;return Holidays.TryGetValue(day,out n)?n:null;}
        public class Lesson{public string course,code,type,half;public int weekday;}
        public static readonly Dictionary<int,Lesson[]> Timetable=new Dictionary<int,Lesson[]>{
            {1,new[]{new Lesson{course="research",code="S1-E",type="exam",weekday=0,half="am"},new Lesson{course="culture",code="S1-C",type="comprehensive",weekday=1,half="pm"},new Lesson{course="media",code="S1-P1",type="project",weekday=2,half="am"},new Lesson{course="design",code="S1-P2",type="project",weekday=3,half="pm"}}},
            {2,new[]{new Lesson{course="s2-exam",code="S2-E",type="exam",weekday=0,half="am"},new Lesson{course="s2-comprehensive",code="S2-C",type="comprehensive",weekday=1,half="pm"},new Lesson{course="s2-project-1",code="S2-P1",type="project",weekday=2,half="am"},new Lesson{course="s2-project-2",code="S2-P2",type="project",weekday=3,half="pm"}}}};
        static string ExamKind(string type,int week){if(type=="exam"&&week==8)return "midterm";if(type=="exam"&&week==16)return "final";if(type=="comprehensive"&&week==16)return "final";return "class";}
        static List<Session> Build(int sem)
        {
            var t=new List<Session>();
            for(int n=1;n<=16;n++)foreach(var r in Timetable[sem])
            {
                int i=SemStart(sem)+(n-1)*7+r.weekday;string a=ExamKind(r.type,n);
                t.Add(new Session{id=r.code+"-w"+n.ToString("00")+(a=="class"?"":"-"+a),semester=sem,week=n,course=r.course,code=r.code,type=r.type,kind=a,day=i,half=r.half,plannedDay=i,plannedHalf=r.half});
            }
            int end=SemStart(sem)+112-1;var moved=t.Where(e=>Holiday(e.day)!=null).ToList();
            var taken=new HashSet<string>(t.Where(e=>!moved.Contains(e)).Select(e=>e.day+":"+e.half));var courseDays=new HashSet<string>(t.Where(e=>!moved.Contains(e)).Select(e=>e.course+":"+e.day));
            foreach(var e in moved)
            {
                string name=Holiday(e.plannedDay),other=e.plannedHalf=="am"?"pm":"am";bool done=false;
                for(int s=e.plannedDay+1;s<=end&&!done;s++)
                {
                    if(Weekday(s)>=5||Holiday(s)!=null||courseDays.Contains(e.course+":"+s))continue;
                    foreach(var h in new[]{e.plannedHalf,other})if(!taken.Contains(s+":"+h)){e.day=s;e.half=h;e.moveReason=Iso(e.plannedDay)+" 为公众假期（"+name+"），顺延至本学期最早的空闲半日";taken.Add(s+":"+h);courseDays.Add(e.course+":"+s);done=true;break;}
                }
                if(!done)throw new Exception("无法为 "+e.id+" 找到补课时段");
            }
            return t.OrderBy(e=>e.day).ThenBy(e=>e.half=="am"?0:1).ToList();
        }
        public static readonly List<Session> Sessions=Build(1).Concat(Build(2)).ToList();
        public static readonly List<Session> Exams=Sessions.Where(e=>e.kind!="class").ToList();
        public static Session At(int day,string half){return Sessions.FirstOrDefault(n=>n.day==day&&n.half==half);}
        public static Session Exam(string id){return Exams.FirstOrDefault(t=>t.id==id);}
        public static Session ExamOf(string course,string kind){return Exams.FirstOrDefault(n=>n.course==course&&n.kind==kind);}
        public static Slot Makeup(string id)
        {
            var t=Exam(id);if(t==null)return null;int wd=t.type=="exam"&&t.kind=="midterm"?3:4;string h=t.type=="comprehensive"?"pm":"am";
            return new Slot{day=SemStart(t.semester)+105+wd,half=h};
        }
        public static Session MakeupAt(int day,string half){return Exams.FirstOrDefault(n=>{var r=Makeup(n.id);return r.day==day&&r.half==half;});}
        public static int Slots(int round){return Rounds[round].kind=="winter"?21:28;}
        public static Slot SlotOf(int round,int i)
        {
            var n=Rounds[round];return n.kind=="winter"?new Slot{day=n.startDay+i,half=null}:new Slot{day=n.startDay+i/2,half=i%2==0?"am":"pm"};
        }
        public static Session SessionAt(int round,int i){var s=SlotOf(round,i);return s.half!=null?At(s.day,s.half):null;}
        public static string Fixed(int round,int i){var n=SessionAt(round,i);return n==null?null:(n.kind=="class"?"class:":"exam:")+n.course;}
        // 课堂/考试顺序编号（不含寒假）
        public static int? TeachingRound(int round){if(Rounds[round].kind=="winter")return null;return round-Rounds.Take(round).Count(e=>e.kind=="winter");}
    }
    public class Job{public string id,name,growth;public string[] reqKeys;public int[] reqValues;public int pay,fatigue,stress;public double baseChance,growthPerShift;}
    public class Course{public string id,name,teacher,assignment,requirement,project;public int semester;}
    public class Workload{public int releaseWeek,dueWeek,workloadUnits;}
    public class Trip{public string id,name,text,cg;}
    public class Basic{public string id,name,mark,description;public int fatigue,stress,money,cap;}
    public class Good{public string id,name,kind,description;public int price,stock;}
    public class Economy{public string id,name,recommended,description;public int budget;}
    public class Housing{public string id,name,area;public int monthlyRent,slots,restFatigue,restStress;}
    public class Option{public string label,result;public int fatigue,stress,money,affection,reputation;}
    public class LifeEvent{public string id,trigger,stage,title,text,cg;public bool studied;public Option[] options;}
    public class Focus{public string id,name,hint,detail;}
    public class Grade{public double min;public string letter;public int points100;}
    public static class Cat
    {
        public static readonly Job[] Jobs={
            new Job{id="fast_food",name="快餐店店员",reqKeys=new string[0],reqValues=new int[0],pay=220,fatigue=10,stress=3,baseChance=85,growth="practice",growthPerShift=0.2},
            new Job{id="retail",name="零售店员",reqKeys=new[]{"communication"},reqValues=new[]{25},pay=260,fatigue=8,stress=4,baseChance=70,growth="communication",growthPerShift=0.2},
            new Job{id="event_assistant",name="活动助理",reqKeys=new[]{"communication"},reqValues=new[]{40},pay=320,fatigue=9,stress=6,baseChance=65,growth="communication",growthPerShift=0.3},
            new Job{id="admin",name="行政文员",reqKeys=new[]{"writing","practice"},reqValues=new[]{40,30},pay=340,fatigue=6,stress=5,baseChance=65,growth="writing",growthPerShift=0.3},
            new Job{id="tutor",name="补习导师",reqKeys=new[]{"research","communication"},reqValues=new[]{55,50},pay=360,fatigue=6,stress=7,baseChance=60,growth="communication",growthPerShift=0.4},
            new Job{id="social_media",name="社交媒体运营",reqKeys=new[]{"writing","practice"},reqValues=new[]{55,50},pay=400,fatigue=6,stress=6,baseChance=55,growth="writing",growthPerShift=0.4},
            new Job{id="research_assistant",name="大学兼职研究助理",reqKeys=new[]{"research","writing"},reqValues=new[]{70,55},pay=500,fatigue=5,stress=8,baseChance=50,growth="research",growthPerShift=0.4}};
        public static Job JobOf(string id){return Jobs.FirstOrDefault(t=>t.id==id);}
        public static readonly Course[] Courses={
            new Course{id="research",semester=1,name="研究方法",teacher="陈老师",assignment="把问题问清楚",requirement="选择一个香港日常生活现象，写出研究问题、访谈提纲与研究伦理说明。",project="题目待定"},
            new Course{id="culture",semester=1,name="城市与文化",teacher="林老师",assignment="街区观察札记",requirement="观察一处街区，将课堂的空间与身份概念用于一篇图文分析。",project="题目待定"},
            new Course{id="media",semester=1,name="数字传播",teacher="黄老师",assignment="平台案例分析",requirement="选择一个本地传播案例，分析受众、内容策略与平台机制。",project="一份给新来港同学的城市指南"},
            new Course{id="design",semester=1,name="设计思维",teacher="周老师",assignment="从痛点到原型",requirement="记录一项学生生活需求，提出方案，并用简图呈现测试方法。",project="题目待定"},
            new Course{id="s2-exam",semester=2,name="第二学期考试主导课",teacher="任课老师",assignment="个人作业（题目待定）",requirement="课程名称与作业要求待定。",project="题目待定"},
            new Course{id="s2-comprehensive",semester=2,name="第二学期综合课",teacher="任课老师",assignment="个人作业（题目待定）",requirement="课程名称与作业要求待定。",project="题目待定"},
            new Course{id="s2-project-1",semester=2,name="第二学期项目课一",teacher="任课老师",assignment="个人作业（题目待定）",requirement="课程名称与作业要求待定。",project="题目待定"},
            new Course{id="s2-project-2",semester=2,name="第二学期项目课二",teacher="任课老师",assignment="个人作业（题目待定）",requirement="课程名称与作业要求待定。",project="题目待定"}};
        public static int CourseIndex(string id){return Array.FindIndex(Courses,t=>t.id==id);}
        public static Course CourseOf(string id){return Courses.FirstOrDefault(t=>t.id==id);}
        public static string CourseName(string id){return CourseOf(id).name;}
        static readonly Dictionary<string,Workload[]> Loads=new Dictionary<string,Workload[]>{
            {"E",new[]{new Workload{releaseWeek=3,dueWeek=6,workloadUnits=2},new Workload{releaseWeek=4,dueWeek=10,workloadUnits=2}}},
            {"C",new[]{new Workload{releaseWeek=3,dueWeek=7,workloadUnits=3},new Workload{releaseWeek=4,dueWeek=12,workloadUnits=3}}},
            {"P1",new[]{new Workload{releaseWeek=3,dueWeek=9,workloadUnits=4},new Workload{releaseWeek=4,dueWeek=13,workloadUnits=6}}},
            {"P2",new[]{new Workload{releaseWeek=3,dueWeek=10,workloadUnits=4},new Workload{releaseWeek=4,dueWeek=14,workloadUnits=6}}}};
        public static Cal.Lesson LessonOf(string course){return Cal.Timetable[CourseOf(course).semester].First(t=>t.course==course);}
        public static Workload Load(string course,string kind){return Loads[LessonOf(course).code.Split('-')[1]][kind=="individual"?0:1];}
        public static int WeekEnd(string course,int week){return Cal.SemStart(CourseOf(course).semester)+(week-1)*7+6;}
        public static readonly Trip[] Trips={
            new Trip{id="disney",name="迪士尼乐园",text="暂时把 DDL 留在手机里，看一次巡游与灯光。",cg="trip_disney"},
            new Trip{id="peak",name="太平山顶",text="从高处看城市与海港，天色慢慢变化。",cg="trip_peak"},
            new Trip{id="maclehose",name="麦理浩径",text="离开密集楼宇，沿山路走进更开阔的风景。",cg="trip_maclehose"},
            new Trip{id="wong_tai_sin",name="黄大仙庙",text="在人流与庭院之间放慢步子，为接下来的学期许个愿。",cg="trip_wong_tai_sin"},
            new Trip{id="palace_museum",name="香港故宫博物馆",text="看展、看海，挑一件想记住的展品。",cg="trip_palace_museum"},
            new Trip{id="victoria_harbour",name="维多利亚港",text="看渡轮经过，等天色从蓝色变成夜景。",cg="trip_victoria_harbour"}};
        public static Trip TripOf(string id){return Trips.FirstOrDefault(t=>t.id==id);}
        public const int TripCost=300,TripFatigue=20,TripStress=30;
        public static readonly Basic[] Basics={
            new Basic{id="review",name="预习与复习",mark="读",description="整理课堂笔记；不推进尚未布置的作业。",fatigue=4,stress=1,money=0,cap=28},
            new Basic{id="rest",name="在家休息",mark="休",description="让这半天慢一点；恢复量取决于房型与已摆放的家具。",fatigue=0,stress=0,money=0,cap=28},
            new Basic{id="walk",name="外出散步",mark="逛",description="去公园或海旁走走。花费 HK$45，精力 +4、压力 −8；可能遇到小事件。",fatigue=-4,stress=-8,money=-45,cap=28}};
        public static Basic BasicOf(string id){return id==null?null:Basics.FirstOrDefault(t=>t.id==id);}
        public const int ClassFatigue=3,ClassStress=2;
        public const double ExamBase=10,ExamMastery=0.7,ExamResearch=0.2,ExamPenaltyFrom=60,ExamPenaltyRate=0.15;
        public static readonly string[] Abilities={"research","writing","communication","practice"};
        public static readonly string[] AbilityNames={"研究分析","写作表达","沟通展示","实践执行"};
        public static int AbilityIndex(string id){return Array.IndexOf(Abilities,id);}
        public static string AbilityName(string id){return AbilityNames[AbilityIndex(id)];}
        public static readonly Dictionary<string,string> CourseAbility=new Dictionary<string,string>{{"research","research"},{"culture","writing"},{"media","communication"},{"design","practice"},{"s2-exam","research"},{"s2-comprehensive","writing"},{"s2-project-1","communication"},{"s2-project-2","practice"}};
        public const double GrowAbility=0.5;public const int ClassMastery=5,ReviewMastery=8,StudyMastery=2,GroupMastery=1;
        public const int StartFatigue=18,StartStress=15,DailyCost=75,DailyRecovery=3,DailyCalm=1,Medical=180,Illness=90,IllnessFatigueRelief=32,IllnessStressRelief=20;
        public static readonly string[] WorkNames={"个人作业","小组作业"};public static readonly string[] WorkMarks={"写","组"};
        public static int WorkFatigue(string kind){return kind=="individual"?6:5;}
        public const int WorkStress=3;
        public const int PointsPerUnit=20;public const double ProgressScale=10,ProgressBase=0.75,ProgressDivisor=120,QualityBase=50,QualityAbility=0.25,QualityMastery=0.15,QualityMax=90,QualityCap=100;
        public static readonly int[] Polish={4,4,3,3,2,2};public const int PolishAfter=1;
        public static readonly string[] Roles={"research","writing","presentation"};
        public static readonly Good[] Goods={
            new Good{id="tea",name="港式凉茶",kind="item",price=18,description="使用后压力 −8",stock=2},
            new Good{id="bun",name="菠萝包",kind="item",price=12,description="使用后精力 +6",stock=2},
            new Good{id="lamp",name="二手阅读灯",kind="furniture",price=120,description="摆放后，每个个人作业半日少消耗 2 精力",stock=1},
            new Good{id="plant",name="窗边绿植",kind="furniture",price=60,description="摆放后，每个休息半日额外恢复 2 压力",stock=1},
            new Good{id="cushion",name="柔软靠垫",kind="furniture",price=85,description="摆放后，每个休息半日额外恢复 3 精力",stock=1}};
        public static int GoodIndex(string id){return Array.FindIndex(Goods,t=>t.id==id);}
        public static readonly Economy[] Economies={
            new Economy{id="frugal",name="拮据",budget=100000,recommended="compact",description="基本覆盖便宜住房和必要开销；购物、旅游及改善生活通常需要兼职额外赚钱。"},
            new Economy{id="balanced",name="中等均衡",budget=125000,recommended="standard",description="足以覆盖正常房租和生活开销，可偶尔额外消费；频繁旅游、购物或升级住房时需兼职补贴。"},
            new Economy{id="wealthy",name="富裕",budget=200000,recommended="comfort",description="生活费较充裕，可负担日常开支，并有更多购物、旅游空间；兼职压力较小。"}};
        public static readonly Housing[] Homes={
            new Housing{id="compact",name="紧凑单间",area="4㎡",monthlyRent=6000,slots=4,restFatigue=12,restStress=8},
            new Housing{id="standard",name="标准房间",area="10㎡",monthlyRent=7500,slots=7,restFatigue=18,restStress=12},
            new Housing{id="comfort",name="舒适房间",area=null,monthlyRent=12000,slots=10,restFatigue=24,restStress=16}};
        public static Housing HomeOf(string id){return Homes.FirstOrDefault(t=>t.id==id);}
        public const int FurnitureFatigueCap=6,FurnitureStressCap=4;
        public const int AffectionStart=50;
        public static readonly Dictionary<string,int[]> Relation=new Dictionary<string,int[]>{{"on_time",new[]{5,2}},{"late",new[]{-3,-2}},{"missing",new[]{-6,-12}}};
        public static readonly Dictionary<string,int> LetterReputation=new Dictionary<string,int>{{"A",4},{"A-",3},{"B+",2},{"B",1},{"B-",0},{"C+",-1},{"C",-2},{"C-",-3},{"F",-4}};
        public static readonly Dictionary<string,string[]> WeightKeys=new Dictionary<string,string[]>{{"project",new[]{"individual","group"}},{"comprehensive",new[]{"individual","group","final"}},{"exam",new[]{"individual","group","midterm","final"}}};
        public static readonly Dictionary<string,int[]> WeightValues=new Dictionary<string,int[]>{{"project",new[]{40,60}},{"comprehensive",new[]{30,30,40}},{"exam",new[]{20,20,20,40}}};
        public static readonly Grade[] Grades={new Grade{min=85,letter="A",points100=400},new Grade{min=80,letter="A-",points100=367},new Grade{min=75,letter="B+",points100=333},new Grade{min=70,letter="B",points100=300},new Grade{min=65,letter="B-",points100=267},new Grade{min=60,letter="C+",points100=233},new Grade{min=55,letter="C",points100=200},new Grade{min=50,letter="C-",points100=167},new Grade{min=0,letter="F",points100=0}};
        public static Grade GradeOf(double total){return Grades.First(t=>total+1e-9>=t.min);}
        public const int EventCap=2,EventCooldown=2;public const double WalkChance=0.25,GroupChance=0.25,DailyChance=0.05;
        const string Draft="Astra 初稿，用户批准启用（D85）";
        static Option O(string label,string result,int fatigue,int stress,int money,int affection=0,int reputation=0){return new Option{label=label,result=result,fatigue=fatigue,stress=stress,money=money,affection=affection,reputation=reputation};}
        public static readonly LifeEvent[] Events={
            new LifeEvent{id="red_bulbul",trigger="walk",title="在公园看到了红耳鹎！",text="公园的绿荫里，一只红耳鹎停在枝头，歪着头看你。",cg="event_red_bulbul",options=new[]{O("呼唤：嘬嘬嘬~","小鸟给了你一个鄙视的眼神后飞走了，不是万物皆可嘬嘬。",-3,-5,0),O("拍照：关闭闪光灯，记录美好时刻。","相册里多了一只杀马特造型腮红鸡！每次看到它心情都会好起来。",-4,-8,0)}},
            new LifeEvent{id="walk_shower",trigger="walk",title="突然下起短暂阵雨",text="刚走到街角，雨点就落了下来。",cg="event_walk_shower",options=new[]{O("到檐下听雨","雨声把脑子里的待办清单冲淡了一点。",-2,-3,0),O("买一杯热饮等雨停","热饮下肚，雨也停了。",-3,-5,-20)}},
            new LifeEvent{id="walk_music",trigger="walk",title="广场边传来熟悉的旋律",text="有人在广场边弹唱一首你很熟悉的歌。",cg="event_walk_music",options=new[]{O("坐下听完","一曲终了，你跟着周围的人一起鼓掌。",0,-5,0),O("跟着轻声哼唱","你小声跟着哼完了副歌。",0,-3,0)}},
            new LifeEvent{id="walk_sky",trigger="walk",title="海边的云被落日染亮",text="回头时，海面上的云已经变成了橘粉色。",cg="event_walk_sky",options=new[]{O("拍一张发给家人","家人很快回了一个大拇指。",0,-5,0),O("把手机收起来多看一会","天色一点点暗下去，你多站了一会儿。",-3,-4,0)}},
            new LifeEvent{id="group_outline",trigger="group",stage="production",title="组员认为切入点应该重做",text="讨论时，{npc}认为报告的切入点不够清楚，想重新收窄问题。",cg="event_group_outline",options=new[]{O("认真听完，再一起收窄问题","花了些心力，但{npc}觉得被认真对待了。",0,2,0,5),O("先沿用现有框架，把建议记进讨论","进度没停下来，只是{npc}有点失落。",0,0,0,-2)}},
            new LifeEvent{id="group_delay",trigger="group",stage="production",title="组员临时忙不过来",text="{npc}这周事情太多，问你能不能先接一小段。",cg="event_group_delay",options=new[]{O("接过来，并说明分工不变","多忙了一会儿，{npc}很感激。",3,0,0,5),O("说明自己也满负荷，维持分工","你守住了自己的节奏，{npc}有些无奈。",0,2,0,-2)}},
            new LifeEvent{id="group_credit",trigger="group",stage="finished",title="汇报页漏了组员的贡献",text="整理最后的汇报页时，你发现{npc}负责的部分没有署名。",cg="event_group_credit",options=new[]{O("主动补上，并向大家说明","{npc}看到后向你道谢，大家也觉得你做事周到。",0,0,0,6,1),O("提醒负责汇总的人补上","署名补上了，{npc}也知道是你提醒的。",0,0,0,2)}},
            new LifeEvent{id="group_rehearsal",trigger="group",stage="finished",title="队友提议多练一次问答",text="{npc}有点担心课堂提问，想再练一次。",cg="event_group_rehearsal",options=new[]{O("留下来快速演练","练完心里踏实多了，{npc}也松了口气。",2,-3,0,3),O("按计划结束，互发提示卡","你们各自带着提示卡回去准备。",0,-1,0,1)}},
            new LifeEvent{id="daily_call",trigger="daily",title="家人发来消息",text="睡前，手机亮了一下：“今天过得怎么样？”",cg="event_daily_call",options=new[]{O("认真聊一会","聊完以后，这一天好像没那么累了。",0,-4,0),O("回一张今天的照片","家人回了一个笑脸。",0,-2,0)}},
            new LifeEvent{id="daily_small_win",trigger="daily",studied=true,title="看懂了以前没懂的一段",text="翻旧笔记时，你突然看懂了之前卡住的一段。",cg="event_daily_small_win",options=new[]{O("把发现写成一句提醒","你在笔记边上写下了这句提醒。",0,-3,0),O("给自己泡杯茶，庆祝一下","小小的进步也值得庆祝。",-2,-2,0)}}};
        public static LifeEvent EventOf(string id){return Events.FirstOrDefault(t=>t.id==id);}
        public static readonly Focus[] Foci={
            new Focus{id="academic",name="学业成长",hint="作业提早完成，多一些复习；休息照样安排。",detail="已发布、能开展的作业尽量提早完成，每轮最多 4 次复习、1 次散步，其余休息。"},
            new Focus{id="life",name="香港生活",hint="必要作业按进度完成，其余时间多出门走走。",detail="作业按截止进度分配（保证节奏、不提早赶完），每轮 3 次散步；余额足够时安排 1 天全天游玩（HK$300），其余休息。"},
            new Focus{id="work",name="工作体验",hint="已录用的兼职排进日程，作业按进度完成。",detail="已录用岗位每轮最多 6 个兼职半日（原规则上限，按实际执行发薪）；作业按截止进度分配，每轮 1 次散步，其余休息。还没录用时先给出一张可申请的岗位卡。"}};
        public static readonly Focus Balanced=new Focus{id="balanced",name="平衡生活（旧存档）",hint="旧存档的安排，延续到本阶段结束。",detail="每轮最多 8 个作业半日，2 次复习，其余休息；下个阶段开始时请从三种倾向中选一个。"};
        public static Focus FocusOf(string id){return id=="balanced"?Balanced:Foci.First(t=>t.id==id);}
        public static readonly string[] Bodies={"female_young","female_middle","male_young","male_middle"};
        public static readonly string[] Hairs={"01","02","03","04"};
        public static readonly string[] Outfits={"class","casual","interview","internship"};
        public static readonly Dictionary<string,string> BodyNames=new Dictionary<string,string>{{"female_young","女 · 青年"},{"female_middle","女 · 中年"},{"male_young","男 · 青年"},{"male_middle","男 · 中年"}};
        public static readonly Dictionary<string,string> OutfitNames=new Dictionary<string,string>{{"class","上课"},{"casual","休闲"},{"interview","面试"},{"internship","实习"}};
        public static readonly string[] Surnames={"陈","林","黄","张","李","王","吴","刘","蔡","杨","郑","梁","何","许","周","叶","罗","冯","邓","谢"};
        public static readonly string[] Given="嘉怡.子轩.晓彤.俊熙.思颖.浩然.咏琪.家乐.美琳.志豪.雅婷.卓谦.凯琳.永康.芷晴.锦华.颖欣.伟诚.可欣.柏言.诗韵.国栋.佩珊.天佑.慧敏.启明.若彤.展鹏".Split('.');
    }
    // 与原版一致的可复现随机数（mulberry 变体 + FNV 散列）
    public class Rng
    {
        uint t;public Rng(long seed){t=unchecked((uint)seed);}
        public double Next(){unchecked{t+=1831565813u;uint e=t;e=(e^(e>>15))*(e|1u);e^=e+(e^(e>>7))*(e|61u);return (e^(e>>14))/4294967296.0;}}
        public static double Hash(params long[] values){unchecked{uint t=2166136261u;foreach(var n in values)t=(t^(uint)(int)n)*16777619u;return new Rng(t).Next();}}
        public static List<T> Shuffle<T>(IList<T> list,long seed){var n=new Rng(seed);var r=list.ToList();for(int e=r.Count-1;e>0;e--){int t=(int)Math.Floor(n.Next()*(e+1));var x=r[e];r[e]=r[t];r[t]=x;}return r;}
    }
}
