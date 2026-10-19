using System;
using System.Collections.Generic;
using System.Linq;

namespace HarbourYear
{
    public enum ActionKind { Lecture, Study, Coursework, Research, PartTime, Internship, Career, Explore, Campus, Rest }
    [Serializable] public class PlanItem
    {
        public ActionKind kind; public int course; public bool project;
        public PlanItem() {} public PlanItem(ActionKind k, int c = 0, bool p = false) { kind = k; course = c; project = p; }
    }
    [Serializable] public class CourseTask
    {
        public float work; public bool submitted; public bool late; public float quality; public int submittedRound;
    }
    [Serializable] public class CourseState
    {
        public float learning; public int studyActions; public CourseTask assignment = new CourseTask(); public CourseTask project = new CourseTask();
    }
    public class CourseInfo
    {
        public string title, code, note; public int open, homeworkDue, projectDue;
        public CourseInfo(string n, string c, int o, int h, int p, string d) { title=n; code=c; open=o; homeworkDue=h; projectDue=p; note=d; }
    }
    [Serializable] public class ApplicationState
    {
        public string route; public int submittedRound, replyRound; public bool resolved, offer; public string reason;
    }
    [Serializable] public class YearState
    {
        public int version = 1, round = 1, seed = 17;
        public float academic = 18, research = 8, career = 12, adaptation = 10, stress = 15;
        public int money = 12000, debt, researchCount, internshipCount, careerCount, campusCount, workCount, exploreCount;
        public string phase = "event", eventId = "arrival", eventReturn = "plan", ending = "";
        public bool graduationApplied, scholarshipApplied, tutorialDone;
        public List<PlanItem> plan = new List<PlanItem>();
        public List<CourseState> courses = new List<CourseState>();
        public List<string> flags = new List<string>(), journal = new List<string>(), summary = new List<string>();
        public List<ApplicationState> applications = new List<ApplicationState>();
        public YearState() { for(int i=0;i<4;i++) courses.Add(new CourseState()); }
        public bool Has(string f) { return flags.Contains(f); }
        public void Flag(string f) { if(!Has(f)) flags.Add(f); }
    }
    public class ActionInfo
    {
        public ActionKind kind; public string name, tag, detail, yields; public int money, stress;
        public ActionInfo(ActionKind k, string n, string t, string d, string y, int m, int s) { kind=k;name=n;tag=t;detail=d;yields=y;money=m;stress=s; }
    }
    public static class Catalog
    {
        public static readonly CourseInfo[] Courses = {
            new CourseInfo("研究方法与学术写作", "RES 501",1,6,12,"从问题意识到文献综述，把「我觉得」变成有依据的论证。"),
            new CourseInfo("数据分析与实践", "DAT 502",1,8,13,"清洗数据、解释结果，也学会和凌晨的报错和平相处。"),
            new CourseInfo("城市与社会观察", "URB 601",14,18,24,"带着问题走进街区，完成一份有温度的田野报告。"),
            new CourseInfo("专业综合项目", "CAP 602",14,20,25,"从提案到最终答辩，把这一年的所学做成作品。")
        };
        public static readonly ActionInfo[] Actions = {
            new ActionInfo(ActionKind.Lecture,"上课","学习","带着问题进教室，课后总算知道该问什么。","学业 +5 · 本课学习 +24",0,6),
            new ActionInfo(ActionKind.Study,"自习","学习","在图书馆找到靠窗的位置，把难点重新梳理一遍。","学业 +4 · 本课学习 +20 · 科研 +2",0,4),
            new ActionInfo(ActionKind.Coursework,"完成课业","学习","单独选择阶段作业或期末项目；完成后记得提交。","学业 +3 · 作业 +55 / 项目 +45",0,9),
            new ActionInfo(ActionKind.Research,"参与科研","机会","查文献、复现实验、写记录。第四次参与形成论文初稿。","科研 +9 · 学业 +2 · 科研经历 +1",-40,10),
            new ActionInfo(ActionKind.PartTime,"兼职","生活","校园助理或合规短工。排班很实在，体力消耗也很实在。","职业 +3 · 适应 +2 · 兼职经历 +1",650,9),
            new ActionInfo(ActionKind.Internship,"实习","机会","把课堂里的模型带到真实需求里，也学会写周报。","职业 +8 · 适应 +3 · 实习经历 +1",900,12),
            new ActionInfo(ActionKind.Career,"求职准备","职业","修改简历、练习面试、整理作品；申请去目标页操作。","职业 +8 · 求职准备 +1",-60,6),
            new ActionInfo(ActionKind.Explore,"城市探索","生活","坐天星小轮、逛街市。地图上的地名开始有了记忆。","适应 +9 · 探索经历 +1",-120,-14),
            new ActionInfo(ActionKind.Campus,"校园活动","生活","参加讲座或社团，不必每一次社交都变成交换名片。","适应 +4 · 职业 +2 · 科研 +1",-60,-6),
            new ActionInfo(ActionKind.Rest,"休息","生活","给自己煮顿饭，洗好床单，好好睡一觉。","压力 -28 · 无额外花费",0,-28)
        };
        public static string Signed(int n) { return (n>=0?"+":"")+n; }
        public static string RouteName(string route) { return route=="hk"?"留港工作":route=="mainland"?"回内地工作":route=="phd"?"读博深造":route=="delay"?"延期完成学业":"继续探索未来方向"; }
    }
    public static class Rules
    {
        public const int TotalRounds = 26, LivingCost = 450;
        public static float SoftGain(float before,float after) { float gain=after-before;return before+(before>=70?gain*.4f:before+gain>70?70-before+(before+gain-70)*.4f:gain); }
        public static float Clamp(float v) { return Math.Max(0, Math.Min(100,v)); }
        public static void Normalize(YearState s) { s.academic=Clamp(s.academic);s.research=Clamp(s.research);s.career=Clamp(s.career);s.adaptation=Clamp(s.adaptation);s.stress=Clamp(s.stress); }
        public static string Month(int r) { string[] m={"九月","十月","十一月","十二月","一月","二月","三月","四月","五月","六月","七月","八月","八月"}; return m[Math.Min(12,(r-1)/2)]+(r%2==1?" · 上旬":" · 下旬"); }
        public static string PlanName(PlanItem p) { return Catalog.Actions[(int)p.kind].name + ((int)p.kind<3?" / "+Catalog.Courses[p.course].title+(p.kind==ActionKind.Coursework?(p.project?" · 项目":" · 作业"):""):""); }
        public static float Efficiency(YearState s, int repeats) { return (s.stress>=90?.5f:s.stress>=70?.7f:1f)*Math.Max(.45f,1f-.2f*repeats); }
        public static string ActionLock(YearState s, PlanItem p)
        {
            if((int)p.kind<3 && (p.course<0||p.course>=4||s.round<Catalog.Courses[p.course].open)) return "本课程第 "+Catalog.Courses[Math.Max(0,Math.Min(3,p.course))].open+" 回合开放";
            if(p.kind==ActionKind.Coursework && (p.project?s.courses[p.course].project:s.courses[p.course].assignment).submitted) return "这份任务已提交，请选择其他任务";
            if(p.kind==ActionKind.Research && !(s.academic>=24 && (s.Has("mentor")||s.campusCount>=2))) return "需学业 24，以及导师联系或 2 次校园活动";
            if(p.kind==ActionKind.Internship && !(s.career>=28 && (s.Has("interview")||s.careerCount>=3))) return "需职业 28，以及面试通过或 3 次求职准备";
            return "";
        }
        public static bool AddPlan(YearState s, PlanItem p) { if(s.phase!="plan"||s.plan.Count>=4||ActionLock(s,p)!="")return false;s.plan.Add(p);return true; }
        public static bool MovePlan(YearState s,int from,int to) { if(s.phase!="plan"||from<0||to<0||from>=s.plan.Count||to>=s.plan.Count)return false;var p=s.plan[from];s.plan.RemoveAt(from);s.plan.Insert(to,p);return true; }
        public static void Log(YearState s,string text) { s.summary.Add(text); }
        public static void Experience(YearState s,string text) { s.journal.Add("第 "+s.round+" 回合 · "+text); }
        public static bool Submit(YearState s,int c,bool project)
        {
            if(s.phase!="plan"||c<0||c>=4||s.round<Catalog.Courses[c].open)return false;
            var t=project?s.courses[c].project:s.courses[c].assignment;
            if(t.submitted||t.work<=0)return false;
            CommitTask(s,c,project);return true;
        }
        static void CommitTask(YearState s,int c,bool project)
        {
            var t=project?s.courses[c].project:s.courses[c].assignment; var def=Catalog.Courses[c];
            t.late=s.round>(project?def.projectDue:def.homeworkDue);t.submitted=true;t.submittedRound=s.round;t.quality=t.work*(t.late?.75f:1f);
            Experience(s,def.title+(project?"期末项目":"阶段作业")+"已"+(t.late?"补交":"提交")+"，质量 "+t.quality.ToString("0"));
        }
        public static float Score(YearState s,int c,bool projected=false)
        {
            var x=s.courses[c];float a=x.assignment.submitted?x.assignment.quality:(projected?x.assignment.work:0),p=x.project.submitted?x.project.quality:(projected?x.project.work:0);
            return .4f*x.learning+.25f*a+.35f*p;
        }
        public static float Points(float score) { return score>=93?4f:score>=85?3.7f:score>=80?3.3f:score>=75?3f:score>=70?2.7f:score>=65?2.3f:score>=60?2f:score>=50?1f:0f; }
        public static string Letter(float x) { return x>=93?"A":x>=85?"A−":x>=80?"B+":x>=75?"B":x>=70?"B−":x>=65?"C+":x>=60?"C":x>=50?"D":"F"; }
        public static float GPA(YearState s,bool projected=false) { var ids=Enumerable.Range(0,4).Where(i=>!projected||s.round>=Catalog.Courses[i].open).ToArray();return ids.Length==0?0:ids.Average(i=>Points(Score(s,i,projected))); }
        public static bool CanGraduate(YearState s) { return s.graduationApplied&&GPA(s)>=2f&&Enumerable.Range(0,4).All(i=>Score(s,i)>=50&&s.courses[i].assignment.submitted&&s.courses[i].project.submitted); }
        public static string GraduationReason(YearState s)
        {
            var l=new List<string>();if(!s.graduationApplied)l.Add("未提交毕业申请");
            for(int i=0;i<4;i++){var c=s.courses[i];if(Score(s,i)<50)l.Add(Catalog.Courses[i].title+"低于 50 分");else if(!c.assignment.submitted||!c.project.submitted)l.Add(Catalog.Courses[i].title+"仍有未提交任务");}
            if(GPA(s)<2)l.Add("GPA 低于 2.00");return l.Count==0?"四门课均合格、所有任务已交、GPA 达标，且已申请毕业。":string.Join("；",l.ToArray());
        }
        public static string ApplicationLock(YearState s,string route)
        {
            if(s.applications.Any(a=>a.route==route))return "已提交申请";
            if(s.round<16)return "第 16 回合开始接受申请";if(s.round>24)return "本年度申请已截止（第 24 回合）";
            if(route=="hk") { if(s.career<55||s.adaptation<40||s.internshipCount<2)return "需职业 55、适应 40、实习 2 次"; }
            if(route=="mainland") { if(s.career<50||s.careerCount<3)return "需职业 50、求职准备 3 次"; }
            if(route=="phd") { if(s.research<60||s.academic<50||s.researchCount<6||!s.Has("mentor"))return "需科研 60、学业 50、科研 6 次、导师联系"; }
            return "";
        }
        public static bool Apply(YearState s,string route)
        {
            if(s.phase!="plan"||ApplicationLock(s,route)!="")return false;
            s.applications.Add(new ApplicationState { route=route,submittedRound=s.round,replyRound=s.round+2,reason="申请已送出，第 "+(s.round+2)+" 回合回复；届时复核课程成绩。" });
            s.money-=150;s.stress+=6;Normalize(s);Experience(s,"向「"+Catalog.RouteName(route)+"」提交申请（材料费 $150）");return true;
        }
        public static bool Execute(YearState s)
        {
            if(s.phase!="plan"||s.plan.Count!=4)return false;
            s.summary.Clear();var repeats=new Dictionary<ActionKind,int>();
            foreach(var p in s.plan)
            {
                string locked=ActionLock(s,p);if(locked!=""){s.stress=Clamp(s.stress-28);Log(s,PlanName(p)+"无法执行（"+locked+"），已改为休息。压力 -28。");continue;}
                var d=Catalog.Actions[(int)p.kind];int n=repeats.ContainsKey(p.kind)?repeats[p.kind]:0;float e=Efficiency(s,n);repeats[p.kind]=n+1;
                float a0=s.academic,r0=s.research,c0=s.career,u0=s.adaptation;
                s.money+=d.money;s.stress+=d.stress;var course=s.courses[p.course];float l0=course.learning,w0=(p.project?course.project:course.assignment).work;
                switch(p.kind)
                {
                    case ActionKind.Lecture: s.academic+=5*e;course.learning=Clamp(course.learning+24*e);course.studyActions++;break;
                    case ActionKind.Study: s.academic+=4*e;s.research+=2*e;course.learning=Clamp(course.learning+20*e);course.studyActions++;break;
                    case ActionKind.Coursework: s.academic+=3*e;var task=p.project?course.project:course.assignment;task.work=Clamp(task.work+(p.project?45:55)*e);break;
                    case ActionKind.Research: s.research+=9*e;s.academic+=2*e;s.researchCount++;if(s.researchCount==4){s.Flag("paper");Experience(s,"整理出第一份论文初稿；终于不是 final_final_v3 空文件了。");}break;
                    case ActionKind.PartTime: s.career+=3*e;s.adaptation+=2*e;s.workCount++;break;
                    case ActionKind.Internship: s.career+=8*e;s.adaptation+=3*e;s.internshipCount++;if(s.internshipCount==4)Experience(s,"实习负责人认可了你的独立交付。");break;
                    case ActionKind.Career: s.career+=8*e;s.careerCount++;break;
                    case ActionKind.Explore:s.adaptation+=9*e;s.exploreCount++;break;
                    case ActionKind.Campus:s.adaptation+=4*e;s.career+=2*e;s.research+=1*e;s.campusCount++;break;
                }
                s.academic=SoftGain(a0,s.academic);s.research=SoftGain(r0,s.research);s.career=SoftGain(c0,s.career);s.adaptation=SoftGain(u0,s.adaptation);
                Normalize(s);var gains=new List<string>();
                if(s.academic>a0)gains.Add("学业 +"+(s.academic-a0).ToString("0.0"));if(s.research>r0)gains.Add("科研 +"+(s.research-r0).ToString("0.0"));if(s.career>c0)gains.Add("职业 +"+(s.career-c0).ToString("0.0"));if(s.adaptation>u0)gains.Add("适应 +"+(s.adaptation-u0).ToString("0.0"));
                if(course.learning>l0)gains.Add("本课学习 +"+(course.learning-l0).ToString("0.0"));if(p.kind==ActionKind.Coursework)gains.Add("任务进度 +"+((p.project?course.project:course.assignment).work-w0).ToString("0.0"));
                Log(s,PlanName(p)+"："+string.Join(" · ",gains.ToArray())+"；现金 "+Catalog.Signed(d.money)+"，压力 "+Catalog.Signed(d.stress)+(e<.999f?"（成长与课业效率 "+(e*100).ToString("0")+"%）":""));
            }
            s.plan.Clear();s.eventId=Events.ForRound(s);s.eventReturn="settle";s.phase="event";return true;
        }
        public static void FinishRound(YearState s)
        {
            s.money-=LivingCost;Log(s,"两周生活费 -$450（学费与基础住宿费已付）。");
            for(int i=0;i<4;i++)foreach(bool p in new[]{false,true}){var t=p?s.courses[i].project:s.courses[i].assignment;int due=p?Catalog.Courses[i].projectDue:Catalog.Courses[i].homeworkDue;if(s.round==due&&!t.submitted){if(t.work>0){CommitTask(s,i,p);Log(s,Catalog.Courses[i].title+(p?"项目":"作业")+"已在截止日按现有质量自动提交。");}else{Log(s,Catalog.Courses[i].title+(p?"项目":"作业")+"错过截止，可继续完成并补交；补交质量按 75% 计。");s.stress+=8;}}}
            if(s.money<0){int need=-s.money;s.debt+=need;s.money=0;s.stress+=10;Log(s,"学生应急生活金垫付 $"+need+"；累计待还 $"+s.debt+"。没有利息，不会中断学业；之后收入优先还款。");Experience(s,"申请了应急生活金 $"+need);}
            else if(s.debt>0&&s.money>300){int pay=Math.Min(s.debt,s.money-300);s.money-=pay;s.debt-=pay;Log(s,"自动偿还应急生活金 $"+pay+"，保留 $300 应急现金。");}
            foreach(var a in s.applications.Where(x=>!x.resolved&&s.round>=x.replyRound))
            {
                float required=a.route=="phd"?3f:a.route=="hk"?2.7f:2.3f;
                a.resolved=true;a.offer=GPA(s,true)>=required;
                a.reason=a.offer?"申请材料与经历达标，当前预计 GPA "+GPA(s,true).ToString("0.00")+"，收到有条件"+(a.route=="phd"?"录取":"聘用")+"，毕业后生效。":"本次申请未通过：回复时预计 GPA "+GPA(s,true).ToString("0.00")+"，低于 "+required.ToString("0.00")+"。其他去向仍可申请。";
                Log(s,Catalog.RouteName(a.route)+"："+a.reason);Experience(s,Catalog.RouteName(a.route)+"："+(a.offer?"收到 offer":"申请未通过"));
            }
            if(s.stress>=90)Log(s,"压力已达 90：下次成长与课业效率降至 50%。可先休息或城市探索；日程按顺序执行。");
            else if(s.stress>=70)Log(s,"压力达到 70：下次成长与课业效率为 70%，先安排减压会更有效。");
            Normalize(s);s.phase="summary";s.eventId="";
        }
        public static void Advance(YearState s)
        {
            if(s.phase!="summary")return;
            if(s.round==26){s.phase="ending";Experience(s,"这一年的手账写到了最后一页。");}else{s.round++;s.phase="plan";}
        }
        public static List<string> Endings(YearState s)
        {
            if(!CanGraduate(s))return new List<string>{"delay"};
            var l=s.applications.Where(a=>a.resolved&&a.offer).Select(a=>a.route).ToList();l.Add("explore");return l;
        }
        public static List<string> Honors(YearState s)
        {
            var l=new List<string>();if(!CanGraduate(s))return l;
            if(s.scholarshipApplied&&GPA(s)>=3.6f&&s.academic>=65)l.Add("学业奖学金");
            if(GPA(s)>=3.6f&&Enumerable.Range(0,4).All(i=>Score(s,i)>=80)&&(s.researchCount>=4||s.internshipCount>=4))l.Add("优秀毕业表现");return l;
        }
        public static string EndingStory(string r)
        {
            if(r=="hk")return "工牌上的新照片还有点拘谨。下班走到海边，你发现自己已经不再需要导航。留下来不是这一年的标准答案，只是此刻你愿意继续写的下一页。";
            if(r=="mainland")return "回程箱子比来时重，里面除了书，还有一份签好的聘用通知。面试时讲过的项目，终于变成了新工作的起点。你带走的不只是一个学位。";
            if(r=="phd")return "导师的录取邮件只有短短几行，你却读了好几遍。研究问题还没解决，方法也还要改；但这一次，你决定给好奇心更长一点的时间。";
            if(r=="delay")return "手账上还有几项没有打勾。教务老师帮你列好补交与重修清单，生活并没有被一个截止日期判定失败。你会带着更明确的节奏，把学业的最后一段完成。";
            return "最后一次借书归还，最后一次穿过校园。你顺利完成了学业，也给自己留下了一段不急着命名的时间。方向可以再想一想，这一年已经算数。";
        }
    }
}
