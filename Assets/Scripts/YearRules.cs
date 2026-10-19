using System;
using System.Collections.Generic;
using System.Linq;
namespace HarbourYear
{
    public static class Rules
    {
        public const int TotalRounds=17;
        public static float Clamp(float x){return Math.Max(0,Math.Min(100,x));}
        public static int Next(YearState s,int min,int max){unchecked{s.randomStep++;uint x=(uint)(s.seed+s.randomStep*747796405);x=(x^(x>>16))*2246822519u;x^=x>>13;return min+(int)(x%(uint)(max-min));}}
        public static void Normalize(YearState s){s.academic=Clamp(s.academic);s.research=Clamp(s.research);s.career=Clamp(s.career);s.adaptation=Clamp(s.adaptation);s.stress=Clamp(s.stress);s.fatigue=Clamp(s.fatigue);for(int i=0;i<5;i++)s.skills[i]=Clamp(s.skills[i]);}
        public static bool Holiday(YearState s){return s.round==9;}
        public static int Slots(YearState s){return Holiday(s)?2:28;}
        public static int SemesterStart(YearState s){return s.round<10?0:4;}
        public static string Month(int r){string[] dates={"9.01—9.14","9.15—9.28","9.29—10.12","10.13—10.26","10.27—11.09","11.10—11.23","11.24—12.07","12.08—12.21","12.21—1.10 · 寒假","1.11—1.24","1.25—2.07","2.08—2.21","2.22—3.07","3.08—3.21","3.22—4.04","4.05—4.18","4.19—5.02"};return dates[Math.Max(0,Math.Min(16,r-1))];}
        public static void Log(YearState s,string t){s.summary.Add(t);}
        public static void Experience(YearState s,string t){s.journal.Add("第 "+s.round+" 轮 · "+t);}
        public static void Start(YearState s)
        {
            s.name=string.IsNullOrWhiteSpace(s.name)?"小林":s.name.Trim();if(s.name.Length>12)s.name=s.name.Substring(0,12);
            s.money=Balance.StartingMoney[s.economy];Life.CreateStudents(s);Life.RefreshMarket(s);BlankPlan(s);
            s.phase="event";s.eventId="arrival";s.eventReturn="plan";Experience(s,"带着行李来到香港，租下了"+Catalog.Rooms[s.room].name+"。");
        }
        public static void BlankPlan(YearState s){s.plan.Clear();for(int i=0;i<Slots(s);i++)s.plan.Add(null);s.cursor=0;s.finishedRound=false;s.roundEventDone=false;}
        public static int ScheduledCourse(YearState s,int slot)
        {
            if(Holiday(s)||slot<0||slot>=28)return -1;
            int[] fixedSlots={0,3,4,7};int local=Array.IndexOf(fixedSlots,slot%14);return local<0?-1:SemesterStart(s)+local;
        }
        public static string SlotName(YearState s,int slot){return Holiday(s)?"寒假安排 "+(slot+1):"第 "+(slot/2+1)+" 天"+(slot%2==0?"上午":"下午");}
        public static bool IsCourse(ActionKind k){return k==ActionKind.Lecture||k==ActionKind.Study||k==ActionKind.Coursework||k==ActionKind.Group;}
        public static string PlanName(PlanItem p){if(p==null)return "尚未安排";return Catalog.Actions[(int)p.kind].name+(IsCourse(p.kind)?" · "+Catalog.Courses[p.course].title:p.kind==ActionKind.Training?" · "+Catalog.Skills[p.course%5]:"");}
        public static string ActionLock(YearState s,PlanItem p,int slot=-1)
        {
            if(p==null)return "请选择日程";
            bool winter=(int)p.kind>=13;
            if(Holiday(s)&&!winter)return "寒假仅安排探亲、旅游或寻找实习";
            if(!Holiday(s)&&winter)return "仅在第 9 轮寒假开放";
            if(IsCourse(p.kind))
            {
                if(p.course<0||p.course>=8)return "请选择课程";
                var d=Catalog.Courses[p.course];
                if(s.round<d.open||s.round>d.end)return "仅在该课程所属学期安排";
                if(p.kind==ActionKind.Lecture&&slot>=0&&ScheduledCourse(s,slot)!=p.course)return "上课须安排在课表指定半日，每课每轮最多 2 次";
                if(p.kind==ActionKind.Lecture&&slot<0&&!Enumerable.Range(0,Slots(s)).Any(i=>ScheduledCourse(s,i)==p.course&&(s.plan[i]==null||s.plan[i].kind!=ActionKind.Lecture)))return "本轮这门课的 2 次上课已排满";
                if(p.kind==ActionKind.Coursework&&s.courses[p.course].assignment.submitted)return "该课个人作业已提交";
                if(p.kind==ActionKind.Group&&s.courses[p.course].project.submitted)return "该课小组项目已提交";
            }
            if(p.kind==ActionKind.Research&&(s.academic<24||(!s.Has("mentor")&&s.campusCount<2)))return "需学业 24，以及导师联系或 2 次校园活动";
            if(p.kind==ActionKind.Internship&&(s.career<28||(!s.Has("interview")&&s.careerCount<3)))return "需职业 28，以及实习面试通过或 3 次求职准备";
            if(p.kind==ActionKind.Shopping&&(s.adaptation<30||s.exploreCount<3))return "需城市适应 30、城市探索 3 次";
            return "";
        }
        public static bool SetPlan(YearState s,int slot,PlanItem p){if(s.phase!="plan"||slot<0||slot>=Slots(s)||ActionLock(s,p,slot)!="")return false;s.plan[slot]=p.Copy();return true;}
        public static bool MovePlan(YearState s,int a,int b)
        {
            if(s.phase!="plan"||a<0||b<0||a>=Slots(s)||b>=Slots(s))return false;
            var x=s.plan[a];var y=s.plan[b];if((x!=null&&ActionLock(s,x,b)!="")||(y!=null&&ActionLock(s,y,a)!=""))return false;
            s.plan[a]=y;s.plan[b]=x;return true;
        }
        public static int FillClasses(YearState s){if(s.phase!="plan"||Holiday(s))return 0;int n=0;for(int i=0;i<28;i++){int c=ScheduledCourse(s,i);if(c>=0){s.plan[i]=new PlanItem(ActionKind.Lecture,c);n++;}}return n;}
        public static int FillEmpty(YearState s,PlanItem p){int n=0;for(int i=0;i<Slots(s);i++)if(s.plan[i]==null&&SetPlan(s,i,p))n++;return n;}
        public static float Efficiency(YearState s,int repeats){return (s.stress>=90?.55f:s.stress>=70?.8f:1)*(s.fatigue>=85?.6f:s.fatigue>=65?.85f:1)*Math.Max(.6f,1-.08f*repeats);}
        static float Grow(float value,float gain){return Clamp(value+gain*(value>=70?.45f:1));}
        public static bool Begin(YearState s){if(s.phase!="plan"||s.plan.Count!=Slots(s)||s.plan.Any(p=>p==null))return false;s.summary.Clear();s.cursor=0;s.phase="executing";s.finishedRound=false;return true;}
        public static bool Step(YearState s)
        {
            if(s.phase!="executing")return false;
            if(s.cursor>=Slots(s)){if(!s.roundEventDone){s.roundEventDone=true;s.eventId=Events.ForRound(s);s.eventReturn="executing";s.phase="event";}else FinishRound(s);return true;}
            if(!Holiday(s)&&s.recoverySlots==0&&s.medicalRound!=s.round&&(s.fatigue>=95||s.stress>=98))
            {s.medicalRound=s.round;s.phase="event";s.eventId="medical";s.eventReturn="executing";return true;}
            int slot=s.cursor;var p=s.plan[slot];int scheduled=ScheduledCourse(s,slot);bool sick=s.recoverySlots>0;
            if(scheduled>=0){var c=s.courses[scheduled];c.scheduled++;if(sick)c.approvedLeave++;else if(p.kind==ActionKind.Lecture&&p.course==scheduled)c.attended++;else{c.absent++;Log(s,SlotName(s,slot)+" · "+Catalog.Courses[scheduled].title+" 缺课，合规出勤 "+Attendance(c).ToString("0")+"%。");}}
            if(sick){s.recoverySlots--;Rest(s);Log(s,SlotName(s,slot)+" · 获批病假休养；取消原日程，不增加学习或收入。");}
            else
            {
                string locked=ActionLock(s,p,slot);if(locked!=""){Rest(s);Log(s,SlotName(s,slot)+" · "+PlanName(p)+"未执行（"+locked+"），改为休息。");}
                else Perform(s,p,slot);
            }
            s.cursor++;if(!Holiday(s)&&s.cursor%2==0){s.fatigue-=5;s.stress-=2;Normalize(s);}
            if(Holiday(s)&&!sick&&(p.kind==ActionKind.HomeVisit||p.kind==ActionKind.Travel)){s.eventId=p.kind==ActionKind.HomeVisit?"homeCG":"travelCG";s.eventReturn="executing";s.phase="event";}
            else if(!s.roundEventDone&&s.cursor>=(Holiday(s)?2:14)){s.roundEventDone=true;s.eventId=Events.ForRound(s);s.eventReturn="executing";s.phase="event";}
            else if(s.cursor==Slots(s))FinishRound(s);
            return true;
        }
        static void Rest(YearState s){s.fatigue-=24+Catalog.Rooms[s.room].rest+Life.Bonus(s,0);s.stress-=16+Life.Bonus(s,1);Normalize(s);}
        public static void Perform(YearState s,PlanItem p,int slot)
        {
            int n=s.plan.Take(slot).Count(x=>x!=null&&x.kind==p.kind&&(!IsCourse(p.kind)||x.course==p.course));
            float e=Efficiency(s,p.kind==ActionKind.Lecture?0:n),boost=1+Life.Bonus(s,2)/100f;
            var d=Catalog.Actions[(int)p.kind];int cash=d.money;float beforeF=s.fatigue,beforeS=s.stress;string extra="";
            if(p.kind==ActionKind.Rest)Rest(s);else
            {
                float buff=s.tourBuffRound==s.round?.5f:1;
                s.stress+=d.stress*(d.stress>0?buff:1);s.fatigue+=d.fatigue*(d.fatigue>0?buff:1);
                CourseState c=IsCourse(p.kind)?s.courses[p.course]:null;
                switch(p.kind)
                {
                    case ActionKind.Lecture:c.learning=Clamp(c.learning+Balance.LectureGain*e);c.studyActions++;s.academic=Grow(s.academic,1*e);extra="到课 · 学习 "+c.learning.ToString("0")+"%";break;
                    case ActionKind.Study:c.learning=Clamp(c.learning+Balance.StudyGain*e*boost);c.studyActions++;s.academic=Grow(s.academic,e);s.skills[0]+=e;extra="本课学习 "+c.learning.ToString("0")+"%";break;
                    case ActionKind.Coursework:c.assignment.work=Clamp(c.assignment.work+(Balance.HomeworkGain+s.skills[2]*.1f)*e*boost);s.skills[2]+=e;s.academic=Grow(s.academic,e);extra="作业进度 "+c.assignment.work.ToString("0")+"%";break;
                    case ActionKind.Group:c.project.work=Clamp(c.project.work+(Balance.ProjectGain+Life.TeamFit(s,p.course)*.08f)*e*boost);s.skills[1]+=e;s.skills[4]+=.5f*e;extra="项目进度 "+c.project.work.ToString("0")+"% · 分工匹配 "+Life.TeamFit(s,p.course).ToString("0");break;
                    case ActionKind.Research:s.research=Grow(s.research,4*e);s.skills[0]+=2*e;s.researchCount++;if(s.researchCount==8){s.Flag("paper");Experience(s,"整理出一份研究初稿，文件名终于不只是 final_final。");}break;
                    case ActionKind.PartTime:s.workCount++;s.career=Grow(s.career,e);s.adaptation=Grow(s.adaptation,e);break;
                    case ActionKind.Internship:s.internshipCount++;s.career=Grow(s.career,3*e);s.adaptation=Grow(s.adaptation,e);break;
                    case ActionKind.Career:s.careerCount++;s.career=Grow(s.career,4*e);s.skills[4]+=e;break;
                    case ActionKind.Explore:s.exploreCount++;s.adaptation=Grow(s.adaptation,4*e);break;
                    case ActionKind.Campus:s.campusCount++;s.adaptation=Grow(s.adaptation,2*e);s.research=Grow(s.research,e);s.career=Grow(s.career,e);if(s.campusCount==2)s.Flag("mentor");break;
                    case ActionKind.Training:s.skills[p.course%5]+=6*e;s.academic=Grow(s.academic,e);break;
                    case ActionKind.Shopping:cash=Next(s,180,421);s.adaptation=Grow(s.adaptation,e);break;
                    case ActionKind.HomeVisit:cash=-Balance.HomeFare+Next(s,300,901);Experience(s,"回家吃了顿热饭，父母往包里塞了红包。净现金 "+Catalog.Signed(cash)+"。");s.Flag("homeCG");break;
                    case ActionKind.Travel:cash=-Balance.TravelCost;s.tourBuffRound=10;s.adaptation=Grow(s.adaptation,8);s.Flag("travelCG");Experience(s,"旅行让天地变宽。第 10 轮新增压力与疲劳减半，不能叠加。");break;
                    case ActionKind.SeekInternship:s.career=Grow(s.career,10);s.careerCount+=2;if(s.career>=28){s.Flag("interview");extra="获得下学期实习机会";}else extra="简历获得反馈，继续准备可解锁实习";break;
                }
            }
            s.money+=cash;Normalize(s);
            Log(s,SlotName(s,slot)+" · "+PlanName(p)+"："+extra+"  现金 "+Catalog.Signed(cash)+"，疲劳 "+(s.fatigue-beforeF).ToString("+0;-0;0")+"，压力 "+(s.stress-beforeS).ToString("+0;-0;0")+(e<.99f?"；效率 "+(e*100).ToString("0")+"%":""));
        }
        public static float Attendance(CourseState c){return c.scheduled==0?100:100f*(c.attended+c.approvedLeave)/c.scheduled;}
        public static float TaskQuality(YearState s,int i,bool project)
        {var t=project?s.courses[i].project:s.courses[i].assignment;float q=t.work;if(project)q*=.75f+.25f*Life.TeamFit(s,i)/100f;int due=project?Catalog.Courses[i].projectDue:Catalog.Courses[i].homeworkDue;return q*(s.round>due?Balance.LateMultiplier:1);}
        public static bool Submit(YearState s,int i,bool project)
        {if(s.phase!="plan"||i<0||i>=8||s.round<Catalog.Courses[i].open||s.round>Catalog.Courses[i].end)return false;return Commit(s,i,project);}
        static bool Commit(YearState s,int i,bool project)
        {
            var t=project?s.courses[i].project:s.courses[i].assignment;if(t.submitted||t.work<=0)return false;
            t.quality=TaskQuality(s,i,project);t.late=s.round>(project?Catalog.Courses[i].projectDue:Catalog.Courses[i].homeworkDue);t.submitted=true;t.submittedRound=s.round;
            Experience(s,Catalog.Courses[i].title+(project?"小组项目":"个人作业")+(t.late?"补交":"提交")+"，质量 "+t.quality.ToString("0.0"));return true;
        }
        public static float RawScore(YearState s,int i,bool projected=false)
        {
            var c=s.courses[i];var w=Balance.Weights[(int)Catalog.Courses[i].type];
            float a=c.assignment.submitted?c.assignment.quality:projected?TaskQuality(s,i,false):0;
            float p=c.project.submitted?c.project.quality:projected?TaskQuality(s,i,true):0;
            return a*w[0]+p*w[1]+(c.midtermDone?c.midterm:projected?c.learning:0)*w[2]+(c.finalDone?c.finalExam:projected?c.learning:0)*w[3];
        }
        public static float Score(YearState s,int i,bool projected=false){return Attendance(s.courses[i])<70?0:RawScore(s,i,projected);}
        public static float Points(float score){for(int i=0;i<Balance.GradeCutoffs.Length;i++)if(score>=Balance.GradeCutoffs[i])return Balance.GradePoints[i];return 0;}
        public static string Letter(float score){for(int i=0;i<Balance.GradeCutoffs.Length;i++)if(score>=Balance.GradeCutoffs[i])return Balance.GradeLetters[i];return "F";}
        public static float GPA(YearState s,bool projected=false){var ids=Enumerable.Range(0,8).Where(i=>!projected||s.round>=Catalog.Courses[i].open).ToArray();return ids.Length==0?0:ids.Average(i=>Points(Score(s,i,projected)));}
        public static bool CanGraduate(YearState s){return Enumerable.Range(0,8).All(i=>Score(s,i)>=50)&&GPA(s)>=2;}
        public static string GraduationReason(YearState s)
        {
            var reasons=new List<string>();for(int i=0;i<8;i++){if(Attendance(s.courses[i])<70)reasons.Add(Catalog.Courses[i].title+"出勤低于 70%");else if(Score(s,i)<50)reasons.Add(Catalog.Courses[i].title+"总分低于 50");}
            if(GPA(s)<2)reasons.Add("CGPA 低于 2.00");return reasons.Count==0?"8 门课全部通过，CGPA ≥2.00，满足毕业标准。":string.Join("；",reasons.ToArray());
        }
        public static string ApplicationLock(YearState s,string r)
        {
            if(r!="hk"&&r!="mainland"&&r!="phd")return "无此申请方向";
            if(s.applications.Any(a=>a.route==r))return "本年度已申请此方向";
            if(s.round<11||s.round>15)return "申请窗口：第 11—15 轮，2 轮后回复";
            if(r=="hk"&&(s.career<Balance.HkCareer||s.adaptation<Balance.HkAdaptation||s.internshipCount<Balance.HkInternships))return "需职业 55、适应 40、实习 6 次";
            if(r=="mainland"&&(s.career<Balance.MainlandCareer||s.careerCount<Balance.MainlandPrep))return "需职业 50、求职准备 6 次";
            if(r=="phd"&&(s.research<Balance.PhdResearch||s.academic<Balance.PhdAcademic||s.researchCount<Balance.PhdSessions||!s.Has("mentor")))return "需科研 60、学业 50、科研 12 次、导师联系";
            return "";
        }
        public static bool Apply(YearState s,string r)
        {if(s.phase!="plan"||ApplicationLock(s,r)!="")return false;s.applications.Add(new ApplicationState{route=r,submittedRound=s.round,replyRound=s.round+2,reason="已投递，2 轮后复核实际课程表现。"});s.money-=Balance.ApplicationFee;Experience(s,"提交"+Catalog.RouteName(r)+"申请。");return true;}
        public static void FinishRound(YearState s)
        {
            if(s.finishedRound)return;s.finishedRound=true;
            int bill=Life.Bill(s);s.money-=bill;Log(s,(Holiday(s)?"寒假三周":"两周")+"账单：房租与生活费共 $"+bill+"。");
            for(int i=0;i<8;i++)
            {
                var d=Catalog.Courses[i];var c=s.courses[i];
                if(s.round==d.homeworkDue&&!c.assignment.submitted){if(!Commit(s,i,false))Log(s,d.title+"作业零进度逾期；本学期仍可补交，质量 ×75%。");}
                if(s.round==d.projectDue&&!c.project.submitted){if(!Commit(s,i,true))Log(s,d.title+"项目零进度逾期；本学期仍可补交，质量 ×75%。");}
                if(d.type==Assessment.Exam&&s.round==d.open+3&&!c.midtermDone){c.midtermDone=true;c.midterm=c.learning;Log(s,d.title+"期中考试："+c.midterm.ToString("0.0")+"（按本课学习进度结算）。");}
                if(d.type!=Assessment.Project&&s.round==d.end&&!c.finalDone){c.finalDone=true;c.finalExam=c.learning;Log(s,d.title+"期末考试："+c.finalExam.ToString("0.0")+"。");}
                if(s.round==d.end)Log(s,d.title+"学期成绩："+Score(s,i).ToString("0.0")+" / "+Letter(Score(s,i))+"；到课 "+c.attended+"，病假 "+c.approvedLeave+"，旷课 "+c.absent+"。");
            }
            foreach(var a in s.applications.Where(x=>!x.resolved&&s.round>=x.replyRound))
            {
                float need=a.route=="hk"?Balance.HkGpa:a.route=="phd"?Balance.PhdGpa:Balance.MainlandGpa;
                a.resolved=true;a.offer=GPA(s,true)>=need;a.reason="回复时预计 CGPA "+GPA(s,true).ToString("0.00")+"，要求 "+need.ToString("0.00")+"。"+(a.offer?"获得有条件 offer，顺利毕业后生效。":"本次未获 offer；仍可沿所选方向继续求职。");Log(s,Catalog.RouteName(a.route)+"："+a.reason);Experience(s,Catalog.RouteName(a.route)+(a.offer?"收到 offer":"未获 offer"));
            }
            if(s.money<0){int need=-s.money;s.debt+=need;s.money=0;s.stress+=8;Log(s,"应急生活金垫付 $"+need+"，无利息。收入恢复后自动还款，保留 $800；游戏继续。");}
            else if(s.debt>0&&s.money>800){int pay=Math.Min(s.debt,s.money-800);s.money-=pay;s.debt-=pay;Log(s,"偿还应急生活金 $"+pay+"。");}
            if(s.tourBuffRound==s.round){s.tourBuffRound=0;Log(s,"「天地宽广」已在本轮生效，现在结束。");}
            Normalize(s);s.phase="summary";s.eventId="";
        }
        public static void Advance(YearState s)
        {
            if(s.phase!="summary")return;
            if(s.round==TotalRounds){s.phase="ending";s.ending=PreferredEnding(s);Experience(s,"学年结束。"+GraduationReason(s));}
            else{s.round++;s.phase="plan";s.shopBought=0;BlankPlan(s);Life.RefreshMarket(s);}
        }
        public static List<string> Endings(YearState s)
        {if(!CanGraduate(s))return new List<string>{"delay"};var l=s.applications.Where(a=>a.resolved&&a.offer).Select(a=>a.route).ToList();l.Add("hk-search");l.Add("mainland-search");l.Add("explore");return l;}
        public static string PreferredEnding(YearState s)
        {
            var list=Endings(s);if(list.Contains("delay"))return "delay";if(list.Contains(s.aspiration))return s.aspiration;
            if(s.aspiration=="hk"||s.aspiration=="mainland")return s.aspiration+"-search";
            // PhD without admission cannot be awarded: use the strongest feasible offer, with stable tie order.
            if(list.Contains("hk")&&(!list.Contains("mainland")||s.adaptation>=40))return "hk";
            if(list.Contains("mainland"))return "mainland";return "explore";
        }
        public static List<string> Honors(YearState s)
        {var l=new List<string>();if(!CanGraduate(s))return l;if(s.scholarshipApplied&&GPA(s)>=Balance.ScholarshipGpa&&s.academic>=65)l.Add("学业奖学金");if(GPA(s)>=3.6f&&Enumerable.Range(0,8).All(i=>Score(s,i)>=80)&&(s.researchCount>=12||s.internshipCount>=8))l.Add("优秀毕业表现");return l;}
        public static string EndingStory(string r)
        {
            switch(r){case "hk":return "工牌上的新照片还有点拘谨。下班走到海边，你发现自己已经不再需要导航。你选择继续在这座城市写下一页。";case "hk-search":return "学位拿到了，工作还在路上。你决定继续留港寻找机会，整理作品，约下一场面试。未收到 offer，并不抹去已经走完的一年。";case "mainland":return "回程的行李里，除了书还有一份聘用通知。课堂里的项目成为了新工作的起点，你带走的不只是一个学位。";case "mainland-search":return "你决定回内地，把这年的经历整理进简历。工作尚未落实，但接下来在哪座城市开始，由你继续选择。";case "phd":return "导师的录取邮件只有几行，你读了好几遍。研究问题还没解决，但这一次，你决定给好奇心更长一点的时间。";case "delay":return "手账里还有几项没有打勾。教务老师列出了补交、补考或重修的后续清单。这里结束本学年的记录，延期完成并不等于人生被判了不及格。";default:return "归还最后一本借书，走出熟悉的校园。你完成了学业，也给未来留下一段不急着命名的时间。这一年已经算数。";}
        }
    }
}
