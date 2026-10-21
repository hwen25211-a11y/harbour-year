using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// 轻松版沿用的原版规则引擎：日历、课程、作业、考试、经济、疲劳压力、病假与事件。
namespace HarbourYear.Lite
{
    public static class Fmt
    {
        public static string N(double d){return Json.Num(d);}
        public static string F1(double d){return d.ToString("0.0",CultureInfo.InvariantCulture);}
        public static string F2(double d){return d.ToString("0.00",CultureInfo.InvariantCulture);}
        public static string Money(double d){return d.ToString("#,##0",CultureInfo.InvariantCulture);}
        public static string Signed(int e){return e>0?"+"+e:e.ToString();}
        public static string Letter(string l){int i=l.IndexOf('-');return i<0?l:l.Substring(0,i)+"−"+l.Substring(i+1);}
        public static string Floor1(double e){return F1(Math.Floor(e*10+1e-9)/10);}
        public static double R1(double e){return Math.Floor(e*10+0.5)/10;}
        public static double R2(double e){return Math.Floor(e*100+0.5)/100;}
    }
    public class Result{public string error;public static readonly Result Ok=new Result();public static Result Fail(string e){return new Result{error=e};}}
    public class CourseGrade
    {
        public string type;public string[] keys;public int[] weights;public Dictionary<string,double> scores=new Dictionary<string,double>();public List<string> pending=new List<string>(),unknown=new List<string>(),reasons=new List<string>();
        public double? total;public double counted;public bool closed,attendanceRisk,forcedFail;public string status,letter;public Grade result;public bool passed;
    }
    public static class Core
    {
        public static int Clamp(int e){return Math.Max(0,Math.Min(100,e));}
        static int Aff(int e){return Math.Max(0,Math.Min(100,e));}
        // ---------- 新开局 ----------
        public static List<Npc> Classmates(int seed)
        {
            var t=new Rng(seed);var used=new HashSet<string>();var a=new List<Npc>();
            Func<string> name=()=>{while(true){string e=Cat.Surnames[(int)Math.Floor(t.Next()*Cat.Surnames.Length)]+Cat.Given[(int)Math.Floor(t.Next()*Cat.Given.Length)];if(used.Add(e))return e;}};
            for(int e=0;e<24;e++)
            {
                double[] n;do n=Cat.Abilities.Select(_=>20+Math.Floor(t.Next()*51)).ToArray();while(n.Sum()>210||n.Max()<40);
                a.Add(new Npc{id="c"+(e+1).ToString("00"),name=name(),abilities=n,kind="pool"});
            }
            for(int e=0;e<2;e++)a.Add(new Npc{id="f"+(e+1),name=name(),abilities=new double[]{30,30,30,30},kind="fallback"});
            return a;
        }
        public static GameState New(int seed=1)
        {
            var s=new GameState{seed=seed,npcs=Classmates(seed)};
            foreach(var n in s.npcs)s.affection[n.id]=Cat.AffectionStart;
            s.initialAbilities=new double[]{30,30,30,30};s.abilities=new double[]{30,30,30,30};s.plan=Blank(0);
            for(int i=0;i<8;i++){s.tasks[i]=new Work();s.projects[i]=new Project();s.attendance[i]=new Att();}
            return s;
        }
        public static List<string> Blank(int round){return Cal.Rounds[round].kind=="winter"?new List<string>():Enumerable.Repeat((string)null,28).ToList();}
        public static string CheckAbilities(double[] a)
        {
            if(a==null||a.Length!=4)return "能力分配格式错误。";if(!a.All(e=>e==Math.Floor(e)))return "每项能力都须是整数。";if(!a.All(e=>e>=0&&e<=100))return "每项能力须在 0—100 之间。";
            double n=a.Sum();return n==120?null:"四项能力合计须恰好 120 点，当前 "+Fmt.N(n)+" 点。";
        }
        public static string SetAbilities(GameState e,double[] t)
        {
            if(e.housing!=null||e.round!=0||e.history.Count>0||e.logs.Count>0)return "开局能力只能在签约、开始生活之前分配。";
            var n=CheckAbilities(t);if(n!=null)return n;e.initialAbilities=(double[])t.Clone();e.abilities=(double[])t.Clone();return null;
        }
        public static string Sign(GameState e,string economy,string housing)
        {
            if(e.housing!=null)return "已经签约，本版本不能中途换房。";
            if(e.round!=0||e.phase!="planning"||e.history.Count>0||e.logs.Count>0)return "只能在开学前签约。";
            var r=Cat.Economies.FirstOrDefault(x=>x.id==economy);var i=Cat.HomeOf(housing);if(r==null||i==null)return "未知的经济档位或房型。";
            var a=CheckAbilities(e.initialAbilities);if(a!=null||!e.abilities.SequenceEqual(e.initialAbilities))return a??"开局能力与分配记录不一致。";
            int rent=i.monthlyRent*12,dep=i.monthlyRent;if(rent+dep>r.budget)return "预算不足：签约需要 HK$"+(rent+dep)+"（年租＋押金），初始预算只有 HK$"+r.budget+"。";
            e.economy=economy;e.housing=housing;e.budget=r.budget;e.rentPaid=rent;e.deposit=dep;e.money=r.budget-rent-dep;e.startMoney=e.money;return null;
        }
        public static string QuickStart(GameState e,string name,Look look)
        {
            string n=name.Trim();if(n.Length>20)n=n.Substring(0,20);e.name=n==""?"新同学":n;e.appearance=look.Clone();
            return SetAbilities(e,new double[]{30,30,30,30})??Sign(e,"balanced","standard");
        }
        // ---------- 基础 ----------
        public static int Pos(GameState e){return e.phase=="planning"?0:e.phase=="running"?e.cursor:e.plan.Count;}
        public static int Today(GameState e){return Cal.SlotOf(e.round,Math.Min(e.cursor,Cal.Slots(e.round)-1)).day;}
        public static int Elapsed(GameState e){return e.history.Concat(e.logs).Count(t=>t.half!="am");}
        public static int Reserve(GameState e){return (245-Elapsed(e))*Cat.DailyCost;}
        public static int[] RestGain(GameState e)
        {
            var t=e.housing!=null?Cat.HomeOf(e.housing):Cat.Homes[0];int n=e.installed.Contains("cushion")?3:0,r=e.installed.Contains("plant")?2:0;
            return new[]{t.restFatigue+Math.Min(n,Cat.FurnitureFatigueCap),t.restStress+Math.Min(r,Cat.FurnitureStressCap)};
        }
        public static int FreeSlots(GameState e){return e.housing!=null?Cat.HomeOf(e.housing).slots-e.installed.Count:0;}
        public static void Grow(GameState e,string ab,double n){int i=Cat.AbilityIndex(ab);e.abilities[i]=Math.Min(100,Fmt.R1(e.abilities[i]+n));}
        public static void Learn(GameState e,string course,double n){int i=Cat.CourseIndex(course);e.mastery[i]=Math.Min(100,Fmt.R1(e.mastery[i]+n));}
        static string GrowText(string ab,double n){return Cat.AbilityName(ab)+" +"+Fmt.N(n);}
        public static Course[] SemesterCourses(GameState e){int t=Cal.Rounds[e.round].semester;return Cat.Courses.Where(c=>c.semester==t).ToArray();}
        public static void SetReviewFocus(GameState e,string course){if(SemesterCourses(e).Any(c=>c.id==course))e.reviewFocus=course;}
        // ---------- 计划条目 ----------
        public static string Trip(string e){return e!=null&&e.StartsWith("trip:")?e.Substring(5):null;}
        public static string Makeup(string e){return e!=null&&e.StartsWith("makeup:")?e.Substring(7):null;}
        public static string JobId(string e){return e!=null&&e.StartsWith("job:")?e.Substring(4):null;}
        public static bool IsWork(string e){return e!=null&&(e.StartsWith("task:")||e.StartsWith("project:"));}
        public static string WorkKind(string e){return e==null?null:e.StartsWith("task:")?"individual":e.StartsWith("project:")?"group":null;}
        public static string WorkCourse(string e){return e.StartsWith("task:")?e.Substring(5):e.Substring(8);}
        public static string TaskItem(string c){return "task:"+c;}
        public static string ProjectItem(string c){return "project:"+c;}
        public static string WorkItem(string kind,string c){return kind=="individual"?TaskItem(c):ProjectItem(c);}
        public static int JobCount(IEnumerable<string> p){return p.Count(e=>JobId(e)!=null);}
        public static string Label(string e)
        {
            var t=Makeup(e);if(t!=null){var x=Cal.Exam(t);return x!=null?"补考 · "+Cat.CourseName(x.course)+(x.kind=="midterm"?"期中":"期末"):"补考";}
            var n=Trip(e);if(n!=null)return "全天游玩 · "+Cat.TripOf(n).name;
            var k=WorkKind(e);if(k!=null)return Cat.WorkNames[k=="individual"?0:1]+" · "+Cat.CourseName(WorkCourse(e));
            if(e!=null&&e.StartsWith("class:"))return Cat.CourseName(e.Substring(6));if(e!=null&&e.StartsWith("exam:"))return "考试 · "+Cat.CourseName(e.Substring(5));
            if(e!=null&&e.StartsWith("job:")){var j=Cat.JobOf(e.Substring(4));return "兼职 · "+(j!=null?j.name:"岗位");}
            var b=Cat.BasicOf(e);return b!=null?b.name:"留白";
        }
        public static Session PendingMakeup(GameState e,int day,string half)
        {
            var r=Cal.MakeupAt(day,half);if(r==null)return null;var x=e.Exam(r.id);return x!=null&&x.status=="excused"&&x.makeup=="pending"?r:null;
        }
        public static string FixedItem(GameState e,int round,int i)
        {
            var r=Cal.Fixed(round,i);if(r!=null||Cal.Rounds[round].kind=="winter")return r;var s=Cal.SlotOf(round,i);var o=PendingMakeup(e,s.day,s.half);return o!=null?"makeup:"+o.id:null;
        }
        public class MakeupInfo{public int round,index,day;public string half;}
        public static MakeupInfo MakeupOf(string id){var t=Cal.Makeup(id);var n=Cal.RoundOfDay(t.day);return new MakeupInfo{round=n.index,index=(t.day-n.startDay)*2+(t.half=="am"?0:1),day=t.day,half=t.half};}
        // ---------- 兼职 ----------
        public class Missing{public string ability;public int required;public double current;}
        public static List<Missing> Lacks(Job e,double[] ab){var r=new List<Missing>();for(int i=0;i<e.reqKeys.Length;i++){double cur=ab[Cat.AbilityIndex(e.reqKeys[i])];if(cur<e.reqValues[i])r.Add(new Missing{ability=e.reqKeys[i],required=e.reqValues[i],current=cur});}return r;}
        public static bool Qualified(Job e,double[] ab){return Lacks(e,ab).Count==0;}
        public static double? Chance(Job e,double[] ab)
        {
            if(!Qualified(e,ab))return null;if(e.reqKeys.Length==0)return e.baseChance;double r=0;for(int i=0;i<e.reqKeys.Length;i++)r+=ab[Cat.AbilityIndex(e.reqKeys[i])]-e.reqValues[i];r/=e.reqKeys.Length;
            double v=Math.Min(90,Math.Max(30,e.baseChance+0.5*r));return Math.Floor(v*10+0.5)/10;
        }
        public static bool TriedThisRound(GameState e){return e.lastAttempt!=null&&e.lastAttempt.round==e.round;}
        public static bool CanSeekJob(GameState e){return e.housing!=null&&Cal.Rounds[e.round].kind=="semester"&&e.phase=="planning";}
        public static string Apply(GameState e,string job,double roll,out Attempt attempt)
        {
            attempt=null;if(!CanSeekJob(e))return "只能在普通轮开始前的日程规划阶段求职；寒假不参加普通求职。";var a=Cat.JobOf(job);if(a==null)return "未知岗位。";
            if(TriedThisRound(e))return "本轮的求职机会已经用过了，下一轮再试。";if(e.hired.Contains(job))return "你已经获得这个岗位，不需要再申请。";
            var o=Chance(a,e.abilities);if(o==null)return "尚未达到申请门槛。";if(roll<0||roll>=1)throw new ArgumentOutOfRangeException("roll");
            attempt=new Attempt{round=e.round,job=job,chance=o.Value,roll=roll,success=roll<o.Value/100};if(attempt.success)e.hired.Add(job);e.lastAttempt=attempt;return null;
        }
        public static string JobUsable(GameState e,string job){return Cat.JobOf(job)==null?"未知岗位。":e.hired.Contains(job)?null:"尚未获得这个岗位；请先在“兼职求职”中申请并获录用。";}
        public class Swap{public List<string> plan;public List<int> slots=new List<int>();public int count,pay,fatigue,stress;public List<KeyValuePair<string,double>> growth=new List<KeyValuePair<string,double>>();public string error;}
        public static Swap ReplaceJobs(GameState e,string target,string from="lower")
        {
            Func<string,Swap> fail=t=>new Swap{plan=e.plan,error=t};var i=Cat.JobOf(target);if(i==null)return fail("未知岗位。");
            if(Cal.Rounds[e.round].kind!="semester"||(e.phase!="planning"&&e.phase!="running"))return fail("只能在普通轮安排或暂停日程时替换兼职。");
            if(!e.hired.Contains(target))return fail("目标岗位尚未录用；只有已录用的岗位才能换进日程。");if(from==target)return fail("来源岗位与目标岗位相同。");
            var a=new List<int>();
            for(int r=Pos(e);r<e.plan.Count;r++){var o=JobId(e.plan[r]);if(o==null||o==target)continue;if(from=="lower"?Cat.JobOf(o).pay<i.pay:from=="others"||o==from)a.Add(r);}
            if(a.Count==0)return fail(from=="lower"?"本轮尚未执行的日程里，没有工资低于「"+i.name+"」的兼职半日。":from=="others"?"本轮尚未执行的日程里，没有其他岗位的兼职半日。":"本轮尚未执行的日程里，没有「"+(Cat.JobOf(from)!=null?Cat.JobOf(from).name:"该岗位")+"」的兼职半日。");
            var s=new Swap{plan=new List<string>(e.plan),slots=a,count=a.Count};
            Action<string,double> add=(k,v)=>{int idx=s.growth.FindIndex(g=>g.Key==k);double cur=idx<0?0:s.growth[idx].Value;var nv=Fmt.R1(cur+v);if(idx<0)s.growth.Add(new KeyValuePair<string,double>(k,nv));else s.growth[idx]=new KeyValuePair<string,double>(k,nv);};
            foreach(var r in a){var n=Cat.JobOf(JobId(s.plan[r]));s.pay+=i.pay-n.pay;s.fatigue+=i.fatigue-n.fatigue;s.stress+=i.stress-n.stress;add(n.growth,-n.growthPerShift);add(i.growth,i.growthPerShift);s.plan[r]="job:"+target;}
            s.growth.RemoveAll(g=>g.Value==0);return s;
        }
        // ---------- 出勤与考试 ----------
        public static bool AttendanceRisk(Att e){return e.expected>0&&(e.attended+e.excused)*10<e.expected*7;}
        public static double? AttendanceRate(Att e){return e.expected==0?(double?)null:(double)(e.attended+e.excused)/e.expected;}
        public static double ExamScore(double mastery,double research,int fatigue,int stress)
        {
            double a=Cat.ExamPenaltyRate*Math.Max(fatigue-Cat.ExamPenaltyFrom,0)+Cat.ExamPenaltyRate*Math.Max(stress-Cat.ExamPenaltyFrom,0);
            return Math.Floor(Math.Max(0,Math.Min(100,Cat.ExamBase+Cat.ExamMastery*mastery+Cat.ExamResearch*research-a))*100+0.5)/100;
        }
        public static string ExamName(Session e){return Cat.CourseName(e.course)+(e.kind=="midterm"?"期中考试":"期末考试");}
        // ---------- 作业与项目 ----------
        public static int Total(string kind,string course){return Cat.PointsPerUnit*Cat.Load(course,kind).workloadUnits;}
        static double Pace(double ab){return Cat.ProgressScale*(Cat.ProgressBase+ab/Cat.ProgressDivisor);}
        static double Quality(double ab,double m){return Fmt.R1(Math.Max(0,Math.Min(Cat.QualityMax,Cat.QualityBase+Cat.QualityAbility*ab+Cat.QualityMastery*m)));}
        static int PolishStep(int n){return n-1<Cat.Polish.Length&&n-1>=0?Cat.Polish[n-1]:Cat.PolishAfter;}
        public static double PolishGain(Work e){if(e.quality==null)return 0;return Math.Max(0,Fmt.R1(Math.Min(Cat.QualityCap,Fmt.R1(e.quality.Value+PolishStep(e.polish+1)))-e.quality.Value));}
        public static string KindName(string kind){return kind=="individual"?"个人作业":"小组项目";}
        public static Work WorkOf(GameState e,string kind,string course){return kind=="individual"?e.Task(course):e.Proj(course);}
        public static int Due(string course,string kind){return Cat.WeekEnd(course,Cat.Load(course,kind).dueWeek);}
        public static int LateDue(string course,string kind){return Due(course,kind)+14;}
        public static bool TeamReady(Project e)
        {
            var t=new List<string>{"self"};t.AddRange(e.members);
            return e.members.Count==2&&e.members.Distinct().Count()==2&&Cat.Roles.All(n=>e.roles[n]!=null&&t.Contains(e.roles[n]))&&e.roles.Values.Distinct().Count()==3;
        }
        public static string RoleAbility(string course,string role){return role=="research"?"research":role=="writing"?"writing":Cat.LessonOf(course).type=="project"?"practice":"communication";}
        public static string RoleName(string course,string role){return role=="research"?"资料研究":role=="writing"?"报告撰写":Cat.LessonOf(course).type=="project"?"原型执行":"汇报展示";}
        static double MemberAbility(GameState e,string course,string who,string role){var i=RoleAbility(course,role);return who=="self"?e.Ab(i):e.Npc(who).Ab(i);}
        static double WorkAbility(GameState e,string kind,string course)
        {
            if(kind=="individual")return e.Ab(Cat.CourseAbility[course]);var r=e.Proj(course);return Cat.Roles.Sum(i=>MemberAbility(e,course,r.roles[i],i))/Cat.Roles.Length;
        }
        public static double Rate(GameState e,string kind,string course)
        {
            if(kind=="individual")return Fmt.R1(Pace(e.Ab(Cat.CourseAbility[course])));var r=e.Proj(course);
            return TeamReady(r)?Fmt.R1(Cat.Roles.Sum(i=>Pace(MemberAbility(e,course,r.roles[i],i)))/Cat.Roles.Length):0;
        }
        public static double? FinalScore(Work e){return e.status=="missing"?0:e.status=="submitted"?e.score:null;}
        public static string WorkLetter(Work e){var t=e.status=="submitted"||e.status=="missing"?FinalScore(e):e.quality;return t==null?null:Cat.GradeOf(t.Value).letter;}
        public static bool SemesterClosed(GameState e,int sem)
        {
            int n=Cal.Rounds.Where(x=>x.semester==sem).Last().index;return e.round>n||(e.round==n&&(e.phase=="summary"||e.phase=="finished"));
        }
        public static string PartName(string k){return k=="individual"?"个人作业":k=="group"?"小组项目":k=="midterm"?"期中考试":"期末考试";}
        public static CourseGrade Grade(GameState e,string course)
        {
            var n=Cat.LessonOf(course).type;var g=new CourseGrade{type=n,keys=Cat.WeightKeys[n],weights=Cat.WeightValues[n]};
            foreach(var k in new[]{"individual","group"}){var r=WorkOf(e,k,course);var s=FinalScore(r);if(s==null){if(r.legacy&&r.status=="submitted")g.unknown.Add(k);else g.pending.Add(k);}else g.scores[k]=s.Value;}
            foreach(var k in new[]{"midterm","final"})
            {
                if(!g.keys.Contains(k))continue;var x=e.Exam(Cal.ExamOf(course,k).id);
                if(x!=null&&x.score!=null)g.scores[k]=x.score.Value;else if(x!=null&&x.legacy)g.unknown.Add(k);else g.pending.Add(k);
            }
            double sum=0;bool all=true;for(int i=0;i<g.keys.Length;i++){double v;if(g.scores.TryGetValue(g.keys[i],out v))sum+=v*g.weights[i];else all=false;}
            g.total=all?sum/100:(double?)null;g.counted=g.scores.Sum(kv=>kv.Value*g.weights[Array.IndexOf(g.keys,kv.Key)])/100;
            g.closed=SemesterClosed(e,Cat.CourseOf(course).semester);g.attendanceRisk=AttendanceRisk(e.Attendance(course));g.forcedFail=g.closed&&g.attendanceRisk;g.status="in_progress";
            if(g.closed&&(g.forcedFail||(g.unknown.Count==0&&g.total!=null)))
            {
                g.status="official";if(g.forcedFail){g.result=new Grade{letter="F",points100=0};g.passed=false;}else{g.result=Cat.GradeOf(g.total.Value);g.passed=g.result.letter!="F";}
            }
            else if(g.closed)g.status="unknown";
            if(g.forcedFail)g.reasons.Add("普通课堂出勤率 "+Fmt.N(Math.Floor(AttendanceRate(e.Attendance(course)).Value*1000+0.5)/10)+"% 低于 70%，课程强制 F、绩点 0");
            else if(g.result!=null&&!g.passed)g.reasons.Add("课程总分 "+Fmt.Floor1(g.total.Value)+" 低于及格线 50");
            if(g.result!=null&&!g.passed)
                for(int i=0;i<g.keys.Length;i++){var k=g.keys[i];double v;if(g.scores.TryGetValue(k,out v)&&v==0&&(!(k=="individual"||k=="group")||WorkOf(e,k,course).status=="missing"))g.reasons.Add(PartName(k)+(k=="individual"||k=="group"?"缺交":"缺考")+"，0 分按原权重 "+g.weights[i]+"% 计入");}
            g.letter=g.result!=null?g.result.letter:g.total==null?null:Cat.GradeOf(g.total.Value).letter;return g;
        }
        // ---------- 组队 ----------
        public static bool Open(Work e){return e.status!="unreleased"&&e.status!="submitted"&&e.status!="missing";}
        public static List<string> Candidates(GameState e,string course)
        {
            var n=e.Proj(course);return Rng.Shuffle(e.npcs.Where(x=>x.kind=="pool").Select(x=>x.id).ToList(),(long)e.seed*31+Cat.CourseIndex(course)).Where(x=>!n.members.Contains(x)&&!n.Invited(x)).ToList();
        }
        public static List<string> NextBatch(GameState e,string course,List<string> shown){return Candidates(e,course).Where(x=>!shown.Contains(x)).Take(4).ToList();}
        public static string TeamLock(GameState e,string course)
        {
            var n=e.Proj(course);if(n==null)return "未知课程。";if(n.status=="unreleased")return "老师尚未发布这个小组项目。";if(!Open(n))return "这个小组项目已经结束。";return n.locked?"已经实际进行过小组作业，成员与分工已锁定。":null;
        }
        public static Dictionary<string,string> BestRoles(GameState e,string course)
        {
            var n=e.Proj(course);var r=new List<string>{"self"};r.AddRange(n.members);var best=n.roles;double a=-1;
            foreach(var x in r)foreach(var y in r)foreach(var z in r)
            {
                if(new HashSet<string>{x,y,z}.Count!=3)continue;var roles=new Dictionary<string,string>{{"research",x},{"writing",y},{"presentation",z}};
                double c=Cat.Roles.Sum(i=>MemberAbility(e,course,roles[i],i));if(c>a){a=c;best=roles;}
            }
            return new Dictionary<string,string>(best);
        }
        static void Reroles(GameState e,string course){var n=e.Proj(course);n.roles=n.members.Count==2?BestRoles(e,course):Cat.Roles.ToDictionary(r=>r,r=>(string)null);}
        public static string Refresh(GameState e,string course)
        {
            var n=TeamLock(e,course);if(n!=null)return n;var r=e.Proj(course);if(r.members.Count>=2)return "队伍已经满员。";if(r.refreshed)return "这个项目的额外刷新已经用过了。";
            var i=NextBatch(e,course,r.shown);if(i.Count==0)return "没有更多可以邀请的候选同学了。";r.shown=i;r.refreshed=true;Know(e,i);return null;
        }
        static void Know(GameState e,IEnumerable<string> ids){foreach(var i in ids)if(!e.known.Contains(i))e.known.Add(i);}
        static double AvgAbility(Npc n){return n.abilities.Sum()/4;}
        public static double InviteChance(GameState e,string npc)
        {
            var n=e.Npc(npc);double i=70-0.8*(AvgAbility(n)-40)+0.4*(e.affection[npc]-50)+0.2*(e.reputation-50);return Math.Floor(Math.Max(15,Math.Min(95,i))*10+0.5)/10;
        }
        public static string Invite(GameState e,string course,string npc,double roll,out bool success,out double chance)
        {
            success=false;chance=0;var a=TeamLock(e,course);if(a!=null)return a;var o=e.Proj(course);var s=e.Npc(npc);
            if(s==null||s.kind!="pool")return "只能正式邀请候选同学。";if(!e.known.Contains(npc))return "还没有认识这位同学。";if(o.members.Contains(npc))return "这位同学已经在本组。";
            if(o.Invited(npc))return "本项目已经正式邀请过这位同学，不能重复邀请。";if(o.members.Count>=2)return "队伍已经满员。";
            chance=InviteChance(e,npc);success=roll<chance/100;o.invites.Add(new Invite{npc=npc,ok=success});if(success){o.members.Add(npc);Reroles(e,course);}return null;
        }
        public static bool CanRecruit(GameState e,string course)
        {
            if(TeamLock(e,course)!=null)return false;var n=e.Proj(course);return n.members.Count<2&&(n.invites.Count>=2||Candidates(e,course).Count==0);
        }
        public static string Recruit(GameState e,string course,out List<string> added)
        {
            added=new List<string>();if(!CanRecruit(e,course)){var n=e.Proj(course);return TeamLock(e,course)??(n.members.Count>=2?"队伍已经满员。":"至少正式邀请 2 次后仍缺人，才能公开招募。");}
            var r=e.Proj(course);foreach(var x in e.npcs.Where(x=>x.kind=="fallback")){if(r.members.Count>=2)break;if(!r.members.Contains(x.id)){r.members.Add(x.id);added.Add(x.id);}}
            Reroles(e,course);Know(e,added);return null;
        }
        public static string SetRoles(GameState e,string course,Dictionary<string,string> roles)
        {
            var r=TeamLock(e,course);if(r!=null)return r;var i=e.Proj(course);if(i.members.Count<2)return "队伍满员后才能分配岗位。";
            var probe=new Project{members=i.members,roles=roles};if(!TeamReady(probe))return "三人各负责一个不同岗位。";i.roles=new Dictionary<string,string>(roles);return null;
        }
        static string Sign(int e){return e>0?"+"+e:e.ToString();}
        static string Relation(GameState e,string course)
        {
            var n=e.Proj(course);if(n.relation!="pending"||(n.status!="submitted"&&n.status!="missing"))return "";
            if(e.ev!=null&&e.ev.course==course)return " 本项目的交付关系将在小组事件选择之后结算。";
            string r=n.status=="missing"?"missing":n.late?"late":"on_time";var i=Cat.Relation[r];string a=r=="missing"?null:Cat.GradeOf(n.score.Value).letter;int o=e.reputation;
            foreach(var t in n.members)e.affection[t]=Aff(e.affection[t]+i[0]);
            e.reputation=Aff(e.reputation+i[1]+(r=="missing"||a==null?0:Cat.LetterReputation[a]));n.relation="graded";
            string s=n.members.Count==0?"":n.members.Count==1?"组员好感 "+Sign(i[0])+"、":n.members.Count+" 位组员好感各 "+Sign(i[0])+"、";
            return " "+Cat.CourseName(course)+"小组项目"+(r=="on_time"?"按时提交":r=="late"?"迟交提交":"缺交")+(a!=null?"（"+Fmt.Letter(a)+"）":"")+"："+s+"声望 "+Sign(e.reputation-o)+"。";
        }
        static string Submit(GameState e,string kind,string course,int day,bool late)
        {
            var a=WorkOf(e,kind,course);a.status="submitted";a.submittedDay=day;a.late=late;a.score=Fmt.R1(late?a.quality.Value*80/100:a.quality.Value);return kind=="group"?Relation(e,course):"";
        }
        static string Miss(GameState e,string kind,string course){var r=WorkOf(e,kind,course);r.status="missing";r.score=0;return kind=="group"?Relation(e,course):"";}
        public static string WorkBlock(GameState e,string kind,string course)
        {
            var r=Cat.CourseOf(course);if(r==null)return "未知课程。";var i=WorkOf(e,kind,course);var a=KindName(kind);
            if(i.status=="unreleased")return kind=="individual"?r.name+"的个人作业尚未布置；个人作业在第 3 教学周的课堂时段发布。":r.name+"的小组项目尚未发布；小组项目在第 4 教学周的课堂时段发布。";
            if(i.status=="submitted")return r.name+"的"+a+"已提交，不能再修改或打磨。";if(i.status=="missing")return r.name+"的"+a+"补交期已过，记为缺交（0 分），不能再做。";
            if(kind=="group"&&!TeamReady(e.Proj(course)))return "请先在“小组项目”中为"+r.name+"凑齐两位组员（邀请或公开招募），并为三人分配岗位。";return null;
        }
        public static string ItemBlock(GameState e,string t)
        {
            var n=Makeup(t);if(n!=null){var x=e.Exam(n);return x!=null&&x.makeup=="pending"?null:"这场补考已经不需要安排。";}
            var r=Trip(t);if(r!=null&&Cat.TripOf(r)==null)return "未知地点。";var k=WorkKind(t);if(k!=null)return WorkBlock(e,k,WorkCourse(t));var j=JobId(t);return j!=null?JobUsable(e,j):null;
        }
        public static string SubmitNow(GameState e,string kind,string course)
        {
            if(e.phase!="planning"&&e.phase!="running")return "请在安排或暂停日程时提交。";var r=WorkOf(e,kind,course);if(r==null)return "未知课程。";
            if(r.status!="complete")return r.status=="submitted"?"这份作业已经提交过了。":"作业完成后才能提交。";if(e.ev!=null)return "请先完成当前事件的选择。";Submit(e,kind,course,Today(e),false);return null;
        }
        // ---------- 修改日程 ----------
        public static bool Departed(GameState e,int t){int n=t-t%2;return Trip(e.plan[t])!=null&&n<Pos(e)&&Trip(e.plan[n])==Trip(e.plan[t]);}
        public static bool DayFree(GameState e,int t){return FixedItem(e,e.round,t)==null&&FixedItem(e,e.round,t+1)==null;}
        public class PlanEdit{public List<string> plan;public string error;}
        public static PlanEdit SetSlot(GameState e,int t,string n)
        {
            Func<string,PlanEdit> fail=x=>new PlanEdit{plan=e.plan,error=x};
            if(t<Pos(e)||t>=e.plan.Count)return fail("只能修改尚未执行的半日。");if(Departed(e,t))return fail("全天游玩已经出发，下午不能改成别的安排。");
            if(Trip(n)!=null)
            {
                int r0=t-t%2;if(r0<Pos(e))return fail("全天游玩要占用同一天上午和下午两个尚未执行的半日。");if(!DayFree(e,r0))return fail("这一天有课程、考试或补考，不能安排全天游玩。");
                var i0=ItemBlock(e,n);if(i0!=null)return fail(i0);var a0=new List<string>(e.plan);a0[r0]=n;a0[r0+1]=n;return new PlanEdit{plan=a0};
            }
            if(n!=null&&(n.StartsWith("class:")||n.StartsWith("exam:"))&&n!=Cal.Fixed(e.round,t))return fail("课堂和考试只能安排在课表指定的时段。");
            if(Makeup(n)!=null&&n!=FixedItem(e,e.round,t))return fail("补考只能安排在指定的补考时段。");
            var r=ItemBlock(e,n);if(r!=null)return fail(r);var b=Cat.BasicOf(n);
            if(b!=null&&n!=e.plan[t]&&e.plan.Count(x=>x==n)>=b.cap)return fail(b.name+"每轮最多 "+b.cap+" 个半日。");
            if(JobId(n)!=null&&JobId(e.plan[t])==null&&JobCount(e.plan)>=6)return fail("所有兼职合计每轮最多 6 个半日。");
            var a=new List<string>(e.plan);if(Trip(a[t])!=null){int d=t-t%2;a[d]=null;a[d+1]=null;}a[t]=n;return new PlanEdit{plan=a};
        }
        public class Fill{public List<string> plan;public int added,unfilled;public List<int> conflicts=new List<int>();public string error;}
        public static Fill FillFixed(GameState e)
        {
            var t=new List<string>(e.plan);var f=new Fill{plan=t};
            for(int i=Pos(e);i<t.Count;i++){var a=FixedItem(e,e.round,i);if(a==null||t[i]==a)continue;if(t[i]==null){t[i]=a;f.added++;}else f.conflicts.Add(i);}
            return f;
        }
        public static Fill FillMany(GameState e,string item,int count,IEnumerable<int> slots)
        {
            var a=new List<string>(e.plan);var i=ItemBlock(e,item);if(i!=null)return new Fill{plan=a,unfilled=count,error=i};
            var o=Cat.BasicOf(item);int s=JobId(item)!=null?6-JobCount(a):o!=null?o.cap-a.Count(x=>x==item):a.Count;int c=Math.Max(0,Math.Min(count,s)),l=0;
            foreach(var n in slots.Distinct().OrderBy(x=>x)){if(n<Pos(e)||n>=a.Count||a[n]!=null||l>=c)continue;a[n]=item;l++;}
            return new Fill{plan=a,added=l,unfilled=Math.Max(0,count-l)};
        }
        public static Fill ReuseLast(GameState e)
        {
            var t=e.Clone();var f=new Fill{plan=t.plan};if(e.lastPlan==null||e.lastPlan.Count!=e.plan.Count)return f;
            for(int i=Pos(e);i<e.plan.Count;i++)
            {
                var a=e.lastPlan[i];if(t.plan[i]!=null||a==null||(Trip(a)!=null&&i%2==1))continue;if(Trip(a)!=null&&t.plan[i+1]!=null){f.unfilled++;continue;}
                var o=SetSlot(t,i,a);if(o.error!=null)f.unfilled++;else{t.plan=o.plan;f.added++;}
            }
            f.plan=t.plan;return f;
        }
        public static GameState ApplyPlan(GameState e,List<string> t)
        {
            if(t.Count!=e.plan.Count)return e;var n=e.Clone();int r=Pos(e);
            for(int i=r;i<t.Count;i++)n.plan[i]=Departed(e,i)?e.plan[i]:t[i];
            for(int x=r;x<n.plan.Count;x++)
            {
                var i=Trip(n.plan[x]);if(i==null||Departed(e,x))continue;int a=x-x%2;
                if(!(a>=r&&Trip(n.plan[a])==i&&Trip(n.plan[a+1])==i&&DayFree(e,a)))n.plan[x]=null;
            }
            n.blockedReason=null;return n;
        }
        public static void Begin(GameState e){if(e.phase!="planning"||e.plan.Any(x=>x==null))return;e.phase="running";e.cursor=0;e.logs.Clear();e.startMoney=e.money;e.startFatigue=e.fatigue;e.startStress=e.stress;}
        // ---------- 随机事件 ----------
        static int RoundEvents(GameState e){return e.events.Count(t=>t.round==e.round);}
        static bool Cooling(GameState e,string id){var n=Cal.TeachingRound(e.round);return n==null||e.events.Any(x=>x.id==id&&n.Value-Cal.TeachingRound(x.round).Value<=Cat.EventCooldown);}
        static int WalkReserve(GameState e){return !e.firstWalkDone&&e.plan.Skip(e.cursor).Contains("walk")?1:0;}
        static void Raise(GameState e,string id,int day,string half,string course=null,string npc=null)
        {
            e.ev=new CurEvent{id=id,day=day,half=half,course=course,npc=npc};e.events.Add(new EventRec{id=id,round=e.round,day=day,choice=null,course=course,npc=npc});
        }
        public static string Trigger(string id){return Cat.EventOf(id).trigger;}
        static bool Room(GameState e,string cat)
        {
            if(RoundEvents(e)+WalkReserve(e)>=Cat.EventCap)return false;if(cat=="walk")return true;
            return e.events.Count(n=>n.round==e.round&&Trigger(n.id)==cat)<1;
        }
        static LifeEvent Draw(GameState e,string cat,double chance,int day,int k,Func<LifeEvent,bool> ok=null)
        {
            if(!Room(e,cat))return null;var o=Cat.Events.Where(n=>n.trigger==cat&&!Cooling(e,n.id)&&(ok==null||ok(n))).ToArray();
            if(o.Length==0||Rng.Hash(e.seed,day,k,1)>=chance)return null;return o[(int)Math.Floor(Rng.Hash(e.seed,day,k,2)*o.Length)];
        }
        static void Walked(GameState e,int day,string half)
        {
            if(!e.firstWalkDone){e.firstWalkDone=true;Raise(e,"red_bulbul",day,half);return;}var r=Draw(e,"walk",Cat.WalkChance,day,half=="am"?0:1);if(r!=null)Raise(e,r.id,day,half);
        }
        static void Teamed(GameState e,string course,int day,string half,string stage)
        {
            var a=e.Proj(course);if(a.members.Count==0)return;int k=half=="am"?4:5;var o=Draw(e,"group",Cat.GroupChance,day,k,x=>x.stage==stage);if(o==null)return;
            var s=a.members[(int)Math.Floor(Rng.Hash(e.seed,day,k,3)*a.members.Count)];Raise(e,o.id,day,half,course,s);
        }
        static void Evening(GameState e,int day,bool studied)
        {
            if(e.sickDay==day||e.events.Any(x=>x.day==day))return;var r=Draw(e,"daily",Cat.DailyChance,day,3,x=>!x.studied||studied);if(r!=null)Raise(e,r.id,day,"pm");
        }
        public static string Who(string text,string name){return text.Replace("{npc}",name);}
        public static string Choose(GameState e,int t)
        {
            if(e.ev==null)return "没有待选择的事件。";var n=Cat.EventOf(e.ev.id);if(n==null||t<0||t>=n.options.Length)return "没有这个选项。";var r=n.options[t];
            if(r.money<0&&e.money<-r.money)return "余额不足，请选择其他选项。";
            int m=e.money,f=e.fatigue,s0=e.stress;e.money+=r.money;e.fatigue=Clamp(e.fatigue+r.fatigue);e.stress=Clamp(e.stress+r.stress);
            var o=e.ev.npc;var s=o!=null&&e.Npc(o)!=null?e.Npc(o).name:"";
            if(r.affection!=0&&o!=null)e.affection[o]=Aff(e.affection[o]+r.affection);if(r.reputation!=0)e.reputation=Aff(e.reputation+r.reputation);
            e.events[e.events.Count-1].choice=t;var c=e.ev.course;e.ev=null;var l=c!=null?Relation(e,c):"";
            if(e.logs.Count>0)
            {
                var u=e.logs[e.logs.Count-1];
                u.text+=" 【"+Who(n.title,s)+"】"+r.label+" —— "+Who(r.result,s)+(r.affection!=0&&o!=null?"（"+s+"好感 "+(r.affection>0?"+":"")+r.affection+"）":"")+(r.reputation!=0?"（声望 "+(r.reputation>0?"+":"")+r.reputation+"）":"");
                u.text+=l;u.money+=e.money-m;u.fatigue+=e.fatigue-f;u.stress+=e.stress-s0;
            }
            return null;
        }
        // ---------- 半日结算 ----------
        static bool Sick(GameState e,int day)
        {
            if(e.fatigue<Cat.Illness&&e.stress<Cat.Illness)return false;e.sickDay=day;e.money-=Cat.Medical;e.fatigue-=Cat.IllnessFatigueRelief;e.stress-=Cat.IllnessStressRelief;return true;
        }
        static string Daily(GameState e){e.money-=Cat.DailyCost;e.fatigue-=Cat.DailyRecovery;e.stress-=Cat.DailyCalm;return "";}
        static string Release(GameState e,Session t,int day)
        {
            var r=t.course;var i=Cat.CourseOf(r);var ind=Cat.Load(r,"individual");var grp=Cat.Load(r,"group");
            if(t.week==ind.releaseWeek&&e.Task(r).status=="unreleased")
            {
                e.Task(r).status="active";e.Task(r).released=day;e.notice=new Notice{course=r,kind="individual"};
                return " "+i.teacher+"布置了个人作业「"+i.assignment+"」，"+Cal.Date(Due(r,"individual"))+"（第 "+ind.dueWeek+" 周末）截止。";
            }
            if(t.week==grp.releaseWeek&&e.Proj(r).status=="unreleased")
            {
                var p=e.Proj(r);p.status="active";p.released=day;p.shown=NextBatch(e,r,new List<string>());Know(e,p.shown);e.notice=new Notice{course=r,kind="group"};
                return " "+i.teacher+"发布了小组项目，"+Cal.Date(Due(r,"group"))+"（第 "+grp.dueWeek+" 周末）截止，请先邀请组员并分工。";
            }
            return "";
        }
        static string Deadlines(GameState e,int day)
        {
            string n="";
            foreach(var r in Cat.Courses)foreach(var i in new[]{"individual","group"})
            {
                var a=WorkOf(e,i,r.id);int o=Due(r.id,i);string s=r.name+KindName(i);
                if(a.status=="complete"&&day>=o){var t=Submit(e,i,r.id,o,false);n+=" "+s+"已在截止日自动提交，得分 "+Fmt.N(a.score.Value)+"（"+Fmt.Letter(WorkLetter(a))+"）。"+t;}
                else if(a.status=="active"&&day>=o){a.status="overdue";n+=" "+s+"未能在截止日前完成，进入补交期："+Cal.Date(o+14)+"前完成会立即迟交提交，得分为原始分 × 0.8。";}
                if(a.status=="overdue"&&day>=o+14){var t=Miss(e,i,r.id);n+=" "+s+"补交期结束仍未完成，记为缺交（0 分，按原权重计入课程总评）。"+t;}
            }
            return n;
        }
        static string[] DoWork(GameState e,string kind,string n,int day,string half)
        {
            var a=WorkOf(e,kind,n);var o=Cat.CourseOf(n);string c=kind=="individual"?"「"+o.assignment+"」":o.name+"小组项目";
            double l=WorkAbility(e,kind,n),u=e.Mastery(n);bool d=a.status!="complete";e.fatigue+=Cat.WorkFatigue(kind);e.stress+=Cat.WorkStress;string f="";bool p=false;
            if(a.status=="complete")
            {
                double g=PolishGain(a);a.polish++;a.quality=Fmt.R1(a.quality.Value+g);
                f=g>0?"继续打磨"+c+"（第 "+a.polish+" 次）：质量 +"+Fmt.N(g)+"，现在 "+Fmt.N(a.quality.Value)+"（预计 "+Fmt.Letter(WorkLetter(a))+"）。":"继续打磨"+c+"（第 "+a.polish+" 次）：质量已达上限 "+Fmt.N(Cat.QualityCap)+"，不再提高。";
            }
            else
            {
                int r=Total(kind,n);double i=Rate(e,kind,n);a.progress=Math.Min(r,Fmt.R1(a.progress+i));a.sumA=Fmt.R1(a.sumA+l);a.sumM=Fmt.R1(a.sumM+u);a.halves++;
                if(a.progress<r)f=kind=="individual"?"推进"+c+"，进度 +"+Fmt.N(i)+"，当前 "+Fmt.N(a.progress)+"/"+r+"。":"三人按分工协作，"+c+"进度 +"+Fmt.N(i)+"，当前 "+Fmt.N(a.progress)+"/"+r+"。";
                else
                {
                    a.quality=Quality(a.sumA/a.halves,a.sumM/a.halves);
                    if(a.status=="overdue")p=true;else{a.status="complete";f="完成"+c+"！作品质量 "+Fmt.N(a.quality.Value)+"（预计 "+Fmt.Letter(WorkLetter(a))+"）。可以在截止前手动提交，也可以继续打磨；未手动提交的会在 "+Cal.Date(Due(n,kind))+" 截止日结束时自动提交。";}
                }
            }
            if(kind=="individual"){Grow(e,"writing",Cat.GrowAbility);Learn(e,n,Cat.StudyMastery);f+=" "+GrowText("writing",Cat.GrowAbility)+"，"+o.name+"掌握度 +"+Cat.StudyMastery+"。";if(e.installed.Contains("lamp"))e.fatigue-=2;}
            else
            {
                var t=e.Proj(n);var role=Cat.Roles.First(x=>t.roles[x]=="self");var ab=RoleAbility(n,role);
                if(!t.locked){t.locked=true;f+=" 小组已开工，分工从现在起锁定。";}
                Grow(e,ab,Cat.GrowAbility);Learn(e,n,Cat.GroupMastery);f+=" 你负责"+RoleName(n,role)+"："+GrowText(ab,Cat.GrowAbility)+"，"+o.name+"掌握度 +"+Cat.GroupMastery+"。";
                Teamed(e,n,day,half,d&&!p&&a.status!="complete"?"production":"finished");
            }
            if(p){var x=Submit(e,kind,n,day,true);f="在补交期内完成"+c+"，作品质量 "+Fmt.N(a.quality.Value)+"，已自动迟交提交：得分 "+Fmt.N(a.score.Value)+"（"+Fmt.Letter(WorkLetter(a))+"）。"+x+f;}
            return new[]{Cat.WorkNames[kind=="individual"?0:1]+" · "+o.name,f};
        }
        static string Excused(GameState e,Session t,int n)
        {
            var r=MakeupOf(t.id);string i=" "+ExamName(t)+"因病假缺考，不按 0 分处理：补考安排在 "+Cal.Date(r.day)+Cal.Half(r.half)+"，与正式考试同一公式计分。";
            if(r.round==e.round&&r.index>n)
            {
                var old=e.plan[r.index];if(Trip(old)!=null){int x=r.index-r.index%2;e.plan[x]=null;e.plan[x+1]=null;}
                e.plan[r.index]="makeup:"+t.id;e.notice=new Notice{course=t.course,kind="makeup"};
                if(old!=null&&old!=e.plan[r.index])i+="那个半日原来的「"+Label(old)+"」已改为补考"+(Trip(old)!=null?"；同一天的全天游玩已取消，另一个半日请重新安排":"")+"。";
            }
            return i;
        }
        static GameState Winter(GameState e)
        {
            int t=Cal.SlotOf(e.round,e.cursor).day;int m=e.money,f=e.fatigue,s=e.stress;bool r=Sick(e,t);
            string i=r?"身体不适，医疗费 HK$180，在家休养。":"寒假在家调整状态。寒假活动（探亲、旅游、寻找实习）将在后续版本加入。";
            i+=Daily(e)+Deadlines(e,t);e.fatigue=Clamp(e.fatigue);e.stress=Clamp(e.stress);
            e.logs.Add(new LogEntry{day=t,half=null,date=Cal.Date(t),title=r?"寒假 · 生病休养":"寒假",text=i,money=e.money-m,fatigue=e.fatigue-f,stress=e.stress-s});
            e.cursor++;if(e.cursor==21)e.phase="summary";return e;
        }
        // 执行一个半日（原地修改）。
        public static GameState Step(GameState t)
        {
            if(t.phase!="running"||t.notice!=null||t.ev!=null||t.blockedReason!=null||t.cursor>=Cal.Slots(t.round))return t;
            if(Cal.Rounds[t.round].kind=="winter")return Winter(t);
            int n=t.cursor;var sl=Cal.SlotOf(t.round,n);int r=sl.day;string i=sl.half;var a=Cal.At(r,i);var o=t.plan[n];int m0=t.money,f0=t.fatigue,s0=t.stress;
            var c=PendingMakeup(t,r,i);bool l=c!=null&&o=="makeup:"+c.id;bool u=i=="am"?t.fatigue>=Cat.Illness||t.stress>=Cat.Illness:t.sickDay==r;
            if(!u)
            {
                var trip=Trip(o);string block=o==null?"这个半日尚未安排，请选择一项活动。":(ItemBlock(t,o)??(trip!=null&&i=="am"&&t.money<Cat.TripCost?"余额不足：全天游玩需要 HK$"+Cat.TripCost+"，现在只有 HK$"+t.money+"。请改成其他安排。":null));
                if(block!=null){t.blockedReason=block;return t;}
            }
            string d="",f="";
            if(u&&i=="am"){Sick(t,r);d="获批病假 · 在家休养";f="医疗费 HK$180。今天上午和下午都在家休养，原定安排未执行。";}
            else if(u){d="病假 · 继续休养";f="下午继续休养，原定安排未执行。";}
            if(a!=null)
            {
                var e=a.course;var expect=Cal.Fixed(t.round,n);
                if(a.kind=="class"){var att=t.Attendance(e);att.expected++;if(u)att.excused++;else if(o==expect)att.attended++;}
                else if(u)t.SetExam(a.id,new ExamRec{status="excused",score=null,makeup="pending",legacy=false});
                else t.SetExam(a.id,o==expect?new ExamRec{status="taken",score=ExamScore(t.Mastery(e),t.Ab("research"),t.fatigue,t.stress),makeup=null,legacy=false}:new ExamRec{status="missed",score=0,makeup=null,legacy=false});
            }
            if(c!=null)
            {
                var x=t.Exam(c.id);if(l){x.makeup="taken";x.score=ExamScore(t.Mastery(c.course),t.Ab("research"),t.fatigue,t.stress);}else{x.makeup="missed";x.score=0;}
            }
            if(l)
            {
                d="补考 · "+ExamName(c);
                f=(u?(i=="am"?"身体不适，医疗费 HK$180；":"身体不适；")+"已安排的补考照常进行，当天其余时间休养。":"")+"按时参加补考，本场得分 "+Fmt.N(t.Exam(c.id).score.Value)+"（与正式考试同一公式，不封顶压分）。";
                t.fatigue+=Cat.ClassFatigue;t.stress+=Cat.ClassStress;
            }
            else if(!u)
            {
                if(o.StartsWith("class:")||o.StartsWith("exam:"))
                {
                    var e=o.Substring(o.IndexOf(':')+1);bool exam=o.StartsWith("exam:");d=exam?"考试 · "+Cat.CourseName(e):Cat.CourseName(e);
                    f=exam?"认真作答，交卷后松了一口气。本场得分 "+Fmt.N(t.Exam(a.id).score.Value)+"。":"听课、讨论、记下新的问题。";t.fatigue+=Cat.ClassFatigue;t.stress+=Cat.ClassStress;
                    if(!exam){var ab=Cat.CourseAbility[e];Grow(t,ab,Cat.GrowAbility);Learn(t,e,Cat.ClassMastery);f+=" "+GrowText(ab,Cat.GrowAbility)+"，"+Cat.CourseName(e)+"掌握度 +"+Cat.ClassMastery+"。";}
                }
                else if(JobId(o)!=null)
                {
                    var j=Cat.JobOf(JobId(o));d="兼职 · "+j.name;t.money+=j.pay;t.fatigue+=j.fatigue;t.stress+=j.stress;t.jobShifts++;Grow(t,j.growth,j.growthPerShift);
                    f="完成半日工作，工资 HK$"+j.pay+"。"+GrowText(j.growth,j.growthPerShift)+"。";
                }
                else if(IsWork(o)){var w=DoWork(t,WorkKind(o),WorkCourse(o),r,i);d=w[0];f=w[1];}
                else if(Trip(o)!=null)
                {
                    var p=Cat.TripOf(Trip(o));d="全天游玩 · "+p.name;
                    if(i=="am"){t.money-=Cat.TripCost;f="出发去"+p.name+"，今天的费用 HK$"+Cat.TripCost+" 已付。";}
                    else{t.fatigue-=Cat.TripFatigue;t.stress-=Cat.TripStress;f=p.text+"尽兴而归：疲劳 −"+Cat.TripFatigue+"、压力 −"+Cat.TripStress+"。";}
                }
                else
                {
                    var b=Cat.BasicOf(o);d=b.name;f=b.id=="rest"?"慢慢吃饭、好好休息，这半天也有自己的价值。":b.id=="walk"?"出门走走，公园和海旁都比书桌开阔。":"";
                    t.fatigue+=b.fatigue;t.stress+=b.stress;t.money+=b.money;
                    if(b.id=="rest"){var g=RestGain(t);t.fatigue-=g[0];t.stress-=g[1];}
                    if(b.id=="walk")Walked(t,r,i);
                    if(b.id=="review"){Grow(t,"research",Cat.GrowAbility);Learn(t,t.reviewFocus,Cat.ReviewMastery);var cn=Cat.CourseName(t.reviewFocus);f="复习"+cn+"，整理笔记。"+GrowText("research",Cat.GrowAbility)+"，"+cn+"掌握度 +"+Cat.ReviewMastery+"。";}
                }
                if(a!=null&&o!=Cal.Fixed(t.round,n))f+=a.kind=="class"?" 这节课记为无故缺勤。":" 这场考试记为无故缺考（0 分，按原权重计入）。";
            }
            if(c!=null&&!l)f+=" 没有参加"+ExamName(c)+"的补考，记为缺考（0 分）。";
            if(a!=null&&a.kind!="class"&&u)f+=Excused(t,a,n);
            if(a!=null)f+=Release(t,a,r);
            if(i=="pm")
            {
                f+=Daily(t)+Deadlines(t,r);
                var prev=n>0?t.plan[n-1]:null;bool studied=new[]{prev,o}.Any(x=>x=="review"||(x!=null&&(x.StartsWith("class:")||x.StartsWith("task:"))));
                if(t.ev==null)Evening(t,r,studied);
            }
            t.fatigue=Clamp(t.fatigue);t.stress=Clamp(t.stress);
            t.logs.Add(new LogEntry{day=r,half=i,date=Cal.Date(r),title=d,text=f,money=t.money-m0,fatigue=t.fatigue-f0,stress=t.stress-s0});
            t.cursor++;if(t.cursor==28)t.phase="summary";return t;
        }
        public static GameState NextRound(GameState e)
        {
            if(e.phase!="summary")return e;var t=e.Clone();if(e.round==Cal.Last){t.phase="finished";return t;}
            if(Cal.Rounds[t.round].kind!="winter")t.lastPlan=new List<string>(t.plan);t.history.AddRange(t.logs);t.round++;t.plan=Blank(t.round);
            foreach(var x in Cal.Exams){var r=t.Exam(x.id);if(r==null||r.makeup!="pending")continue;var n=MakeupOf(x.id);if(n.round==t.round)t.plan[n.index]="makeup:"+x.id;}
            t.cursor=0;t.phase="planning";t.logs=new List<LogEntry>();var cs=SemesterCourses(t);if(cs.Length>0&&!cs.Any(c=>c.id==t.reviewFocus))t.reviewFocus=cs[0].id;t.purchased=new int[5];return t;
        }
        // 计划阶段的“轮末预估”：空白和失效任务按休息估算。
        public static int[] Forecast(GameState e)
        {
            if(e.phase!="planning"&&e.phase!="running")return null;var t=e.Clone(false);for(int i=0;i<t.plan.Count;i++)if(t.plan[i]==null)t.plan[i]="rest";
            if(t.phase=="planning")Begin(t);t.notice=null;t.ev=null;t.blockedReason=null;
            for(int i=0;i<64&&t.phase=="running";i++)
            {
                Step(t);t.notice=null;t.ev=null;
                if(t.blockedReason!=null){if(Trip(t.plan[t.cursor])!=null&&t.cursor%2==0)t.plan[t.cursor+1]="rest";t.plan[t.cursor]="rest";t.blockedReason=null;}
            }
            return new[]{t.money,t.fatigue,t.stress};
        }
        // ---------- 背包 ----------
        public static string Buy(GameState e,string id)
        {
            var n=Cat.Goods[Cat.GoodIndex(id)];int k=Cat.GoodIndex(id);if(e.phase!="planning"&&e.phase!="running")return "请先进入下一轮。";if(e.money<n.price)return "余额不足。";
            if(e.purchased[k]>=n.stock||(n.kind=="furniture"&&e.inventory[k]>0))return "本轮已售罄，或你已经拥有这件家具。";e.money-=n.price;e.inventory[k]++;e.purchased[k]++;return null;
        }
        public static bool Use(GameState e,string id)
        {
            int k=Cat.GoodIndex(id);if(e.inventory[k]<1||Cat.Goods[k].kind!="item"||(e.phase!="planning"&&e.phase!="running"))return false;e.inventory[k]--;
            if(id=="tea")e.stress=Clamp(e.stress-8);if(id=="bun")e.fatigue=Clamp(e.fatigue-6);return true;
        }
        public static bool Toggle(GameState e,string id)
        {
            int k=Cat.GoodIndex(id);if(e.inventory[k]<1||Cat.Goods[k].kind!="furniture"||(!e.installed.Contains(id)&&FreeSlots(e)<1))return false;
            if(e.installed.Contains(id))e.installed.Remove(id);else e.installed.Add(id);return true;
        }
    }
}
