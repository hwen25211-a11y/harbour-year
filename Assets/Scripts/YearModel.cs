using System;
using System.Collections.Generic;
using System.Linq;

namespace HarbourYear
{
    public enum ActionKind { Lecture, Study, Coursework, Research, PartTime, Internship, Career, Explore, Campus, Rest, Group, Training, Shopping, HomeVisit, Travel, SeekInternship }
    public enum Assessment { Project, Combined, Exam }
    [Serializable] public class Appearance { public int gender, age, front, back, outfit; }
    [Serializable] public class PlanItem
    {
        public ActionKind kind; public int course; public bool project;
        public PlanItem() { kind=ActionKind.Rest; }
        public PlanItem(ActionKind k,int c=0,bool p=false) {kind=k;course=c;project=p;}
        public PlanItem Copy(){return new PlanItem(kind,course,project);}
    }
    [Serializable] public class CourseTask { public float work, quality; public bool submitted, late; public int submittedRound; }
    [Serializable] public class CourseState
    {
        public float learning, midterm, finalExam; public bool midtermDone, finalDone;
        public int studyActions, scheduled, attended, approvedLeave, absent;
        public CourseTask assignment=new CourseTask(), project=new CourseTask();
        public int[] team={-1,-1,-1,-1,-1};
    }
    [Serializable] public class Student
    {
        public string name,habit; public Appearance look=new Appearance(); public int[] skills=new int[5]; public int reliability, goodwill;
    }
    [Serializable] public class MarketListing { public int item,price;public bool sold,rare; }
    [Serializable] public class ApplicationState
    { public string route,reason;public int submittedRound,replyRound;public bool resolved,offer; }
    [Serializable] public class YearState
    {
        public int version=2, round=1, seed=17, randomStep, economy=1, room=1;
        public string name="小林",phase="intro",eventId="",eventReturn="plan",ending="",aspiration="hk";
        public Appearance look=new Appearance();
        public float academic=18,research=8,career=12,adaptation=10,stress=12,fatigue=10;
        public float[] skills={25,25,25,25,25};
        public int money,debt,researchCount,internshipCount,careerCount,campusCount,workCount,exploreCount;
        public int cursor,recoverySlots,medicalRound,tourBuffRound,shopBought;
        public bool roundEventDone,graduationApplied,scholarshipApplied,tutorialDone,finishedRound;
        public List<PlanItem> plan=new List<PlanItem>();
        public List<CourseState> courses=new List<CourseState>();
        public List<Student> students=new List<Student>();
        public List<MarketListing> market=new List<MarketListing>();
        public List<int> owned=new List<int>(),equipped=new List<int>(),bag=new List<int>();
        public List<string> flags=new List<string>(),journal=new List<string>(),summary=new List<string>();
        public List<ApplicationState> applications=new List<ApplicationState>();
        public YearState(){for(int i=0;i<8;i++)courses.Add(new CourseState());}
        public bool Has(string f){return flags.Contains(f);} public void Flag(string f){if(!Has(f))flags.Add(f);}
    }
    public class CourseInfo
    {
        public string title,code,note;public int open,end,homeworkDue,projectDue;public Assessment type;
        public CourseInfo(string n,string c,int o,int h,Assessment t){title=n;code=c;open=o;end=o+7;homeworkDue=o+h;projectDue=o+6;type=t;note=t==Assessment.Project?"个人作业与小组项目，无考试。":t==Assessment.Combined?"个人作业、小组项目与期末考试。":"个人作业、小组项目、期中与期末考试。";}
    }
    public class ActionInfo
    {
        public ActionKind kind;public string name,tag,detail,yields;public int money,stress,fatigue;
        public ActionInfo(ActionKind k,string n,string t,string d,string y,int m,int s,int f){kind=k;name=n;tag=t;detail=d;yields=y;money=m;stress=s;fatigue=f;}
    }
    public class RoomInfo
    {
        public string name;public int rent,slots,rest;
        public RoomInfo(string n,int r,int s,int f){name=n;rent=r;slots=s;rest=f;}
    }
    public class ItemInfo
    {
        public string name,note;public int price,slots,rest,calm,study,foodFatigue,foodStress;
        public ItemInfo(string n,int p,int z,int r,int c,int st,int ff,int fs,string d){name=n;price=p;slots=z;rest=r;calm=c;study=st;foodFatigue=ff;foodStress=fs;note=d;}
        public bool Food {get{return slots==0;}}
    }
    // Playtest values, deliberately separate from the confirmed calendar and graduation rules.
    public static class Balance
    {
        public static readonly int[] StartingMoney={43000,65000,100000};
        public static readonly string[] EconomyNames={"拮据","中等均衡","富裕"};
        public const int LivingCost=650,ApplicationFee=150,MedicalCost=150,MoveCost=600;
        public const int HomeFare=800,TravelCost=2200,SeekIncome=200;
        public const float HomeworkGain=28,ProjectGain=24,LectureGain=6,StudyGain=14,LateMultiplier=.75f;
        public const float HkCareer=55,HkAdaptation=40,MainlandCareer=50,PhdResearch=60,PhdAcademic=50;
        public const int HkInternships=6,MainlandPrep=6,PhdSessions=12;
        public const float HkGpa=2.7f,MainlandGpa=2.3f,PhdGpa=3f,ScholarshipGpa=3.6f;
        public static readonly float[][] Weights={new[]{.4f,.6f,0f,0f},new[]{.3f,.3f,0f,.4f},new[]{.2f,.2f,.2f,.4f}};
        public static readonly float[] GradeCutoffs={93,85,80,75,70,65,60,50,0};
        public static readonly float[] GradePoints={4,3.7f,3.3f,3,2.7f,2.3f,2,1,0};
        public static readonly string[] GradeLetters={"A","A−","B+","B","B−","C+","C","D","F"};
    }
    public static class Catalog
    {
        public static readonly string[] Skills={"资料检索","逻辑表达","文稿写作","视觉排版","课堂演讲"};
        public static readonly string[] Roles={"搜集资料","梳理框架","撰写文稿","制作排版","课堂汇报"};
        public static readonly string[] Outfits={"上课","休闲","面试","实习"};
        public static readonly CourseInfo[] Courses={
            new CourseInfo("研究方法与写作","RES 501",1,3,Assessment.Project),new CourseInfo("数据分析实践","DAT 502",1,4,Assessment.Exam),
            new CourseInfo("传播与社会","COM 503",1,3,Assessment.Combined),new CourseInfo("设计思维工作坊","DES 504",1,4,Assessment.Project),
            new CourseInfo("城市田野观察","URB 601",10,3,Assessment.Project),new CourseInfo("数字经济分析","ECO 602",10,4,Assessment.Exam),
            new CourseInfo("组织与专业沟通","ORG 603",10,3,Assessment.Combined),new CourseInfo("专业综合项目","CAP 604",10,4,Assessment.Project)};
        public static readonly RoomInfo[] Rooms={new RoomInfo("4㎡ · 紧凑单间",1700,4,0),new RoomInfo("10㎡ · 合租房间",2900,7,3),new RoomInfo("18㎡ · 独立小屋",4300,10,6)};
        public static readonly ItemInfo[] Items={
            new ItemInfo("舒适床垫",1200,2,7,1,0,0,0,"每次休息额外恢复疲劳 7、压力 1"),
            new ItemInfo("书桌与台灯",900,2,0,0,12,0,0,"自习、作业与项目进度 +12%"),
            new ItemInfo("二手软椅",650,2,3,4,0,0,0,"每次休息额外恢复疲劳 3、压力 4"),
            new ItemInfo("绿植与小熊",240,1,0,4,0,0,0,"每次休息额外降低压力 4"),
            new ItemInfo("白噪音小音箱",450,1,3,2,0,0,0,"每次休息额外恢复疲劳 3、压力 2"),
            new ItemInfo("云吞面",48,0,0,0,0,14,5,"使用：疲劳 -14、压力 -5；也可送给同学"),
            new ItemInfo("菠萝包与奶茶",30,0,0,0,0,7,10,"使用：疲劳 -7、压力 -10；也可送给同学"),
            new ItemInfo("水果分享盒",65,0,0,0,0,10,8,"使用：疲劳 -10、压力 -8；也可送给同学")};
        public static readonly ActionInfo[] Actions={
            new ActionInfo(ActionKind.Lecture,"上课","课程","每课每周一次；只在课表指定的半日上课，不能集中刷完。","本课学习 +6 · 学业 +1",0,2,5),
            new ActionInfo(ActionKind.Study,"自习复习","课程","整理本课笔记；学习进度决定考试表现，不能靠全局属性代替。","本课学习 +14 · 学业 +1",0,2,6),
            new ActionInfo(ActionKind.Coursework,"个人作业","课程","选择课程，完成后可提前提交；截止轮自动提交已有进度。","作业约 +31 · 写作成长",0,4,8),
            new ActionInfo(ActionKind.Research,"参与科研","机会","参加导师项目，复现和研究记录会积累为申请经历。","科研 +4 · 检索成长",-20,5,8),
            new ActionInfo(ActionKind.PartTime,"兼职","生活","校园助理的半日排班；收入稳定，也占用上课和休息的时间。","职业 +1 · 适应 +1",260,3,8),
            new ActionInfo(ActionKind.Internship,"实习","机会","参加半日项目协作；实际经历计入求职申请。","职业 +3 · 实习经历 +1",320,4,9),
            new ActionInfo(ActionKind.Career,"求职准备","职业","修改简历、练习面试；准备完成后仍需去目标页实际投递。","职业 +4 · 求职准备 +1",-20,3,5),
            new ActionInfo(ActionKind.Explore,"城市探索","生活","街市、码头、山顶。认识城市，也给脑袋换个频道。","适应 +4 · 压力 -9",-80,-9,3),
            new ActionInfo(ActionKind.Campus,"校园活动","生活","听讲座、参加社团；两次活动可接触导师科研机会。","适应 +2 · 科研 +1 · 职业 +1",-30,-5,3),
            new ActionInfo(ActionKind.Rest,"休息","生活","睡一觉、做一顿饭。房型与已摆放家具会提高恢复效果。","压力 -16 · 疲劳 -24",0,-16,-24),
            new ActionInfo(ActionKind.Group,"小组项目","课程","先到小组页分配岗位；匹配技能与可靠度影响最终项目质量。","项目约 +27 · 沟通成长",0,4,7),
            new ActionInfo(ActionKind.Training,"能力培养","成长","按当前选择练习检索、逻辑、写作、排版或演讲。","选定能力 +6 · 学业 +1",-15,2,5),
            new ActionInfo(ActionKind.Shopping,"代购","机会","从城市探索中熟悉路线后解锁；占用半日，收入存在波动。","收入 $180—420 · 适应 +1",0,5,10),
            new ActionInfo(ActionKind.HomeVisit,"回内地探亲","寒假","往返路费 $800；父母的红包随机为 $300—900。","疲劳 -45 · 压力 -40",-800,-40,-45),
            new ActionInfo(ActionKind.Travel,"旅游","寒假","花费 $2200。获得「天地宽广」：第 10 轮行动新增压力与疲劳减半。","疲劳 -60 · 压力 -55",-2200,-55,-60),
            new ActionInfo(ActionKind.SeekInternship,"寻找实习","寒假","整理岗位、参加面试；职业达到 28 时拿到下学期实习机会。","职业 +10 · 求职准备 +2",-120,5,8)};
        public static string Signed(int x){return (x>=0?"+":"")+x;}
        public static string RouteName(string r){switch(r){case "hk":return "留港工作";case "hk-search":return "留港继续求职";case "mainland":return "回内地工作";case "mainland-search":return "回内地继续求职";case "phd":return "读博深造";case "delay":return "延迟毕业";default:return "继续探索未来方向";}}
    }
}
