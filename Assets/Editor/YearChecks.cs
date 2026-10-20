using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
namespace HarbourYear.Tests
{
    public static class YearChecks
    {
        static int checks;
        static void Assert(bool yes,string message){checks++;if(!yes)throw new Exception("FAILED: "+message);}
        public static YearState Copy(YearState s){using(var m=new MemoryStream()){var f=new BinaryFormatter();f.Serialize(m,s);m.Position=0;return (YearState)f.Deserialize(m);}}
        public static YearState New(int seed=17,int economy=1,int room=1){var s=new YearState{seed=seed,economy=economy,room=room};Rules.Start(s);Events.Choose(s,2);return s;}
        public static int EventOption(YearState s){return 0;}
        public static void Drain(YearState s){int guard=0;while(s.phase=="event"||s.phase=="executing"){if(++guard>100)throw new Exception("execution stuck");if(s.phase=="event")Events.Choose(s,EventOption(s));else Rules.Step(s);}}
        public static PlanItem Pick(YearState s,string route)
        {
            if(s.fatigue>=48||s.stress>=58)return new PlanItem(ActionKind.Rest);
            if(s.money<Life.Bill(s)+800)return new PlanItem(ActionKind.PartTime);
            var ids=Enumerable.Range(Rules.SemesterStart(s),4).ToArray();
            foreach(int i in ids.OrderBy(i=>Catalog.Courses[i].homeworkDue)){var c=s.courses[i];var d=Catalog.Courses[i];if(!c.assignment.submitted&&c.assignment.work<99&&s.round>=d.homeworkDue-3)return new PlanItem(ActionKind.Coursework,i);}
            foreach(int i in ids){var c=s.courses[i];var d=Catalog.Courses[i];if(d.type==Assessment.Exam&&!c.midtermDone&&c.learning<96)return new PlanItem(ActionKind.Study,i);}
            foreach(int i in ids){var c=s.courses[i];var d=Catalog.Courses[i];if(!c.project.submitted&&c.project.work<99&&s.round>=d.projectDue-3)return new PlanItem(ActionKind.Group,i);}
            foreach(int i in ids)if(Catalog.Courses[i].type!=Assessment.Project&&s.courses[i].learning<98)return new PlanItem(ActionKind.Study,i);
            if(route=="phd"||route=="all")if(s.research<68||s.researchCount<14){var p=new PlanItem(ActionKind.Research);if(Rules.ActionLock(s,p)=="")return p;return new PlanItem(ActionKind.Campus);}
            if(route=="hk"||route=="mainland"||route=="all")
            {
                if(s.careerCount<7||s.career<32)return new PlanItem(ActionKind.Career);
                if((route=="hk"||route=="all")&&(s.internshipCount<9||s.career<62))return new PlanItem(ActionKind.Internship);
                if(s.career<58)return new PlanItem(ActionKind.Career);
                if(s.adaptation<45)return new PlanItem(ActionKind.Explore);
            }
            if(s.researchCount<12)return new PlanItem(ActionKind.Research);
            return new PlanItem(ActionKind.Rest);
        }
        public static void Prepare(YearState s,string route)
        {
            if(Rules.Holiday(s)){s.plan[0]=new PlanItem(ActionKind.Travel);s.plan[1]=new PlanItem(ActionKind.HomeVisit);return;}
            for(int c=Rules.SemesterStart(s);c<Rules.SemesterStart(s)+4;c++)
            {
                var used=new HashSet<int>();for(int role=0;role<5;role++){int r=role;int best=Enumerable.Range(0,6).Where(n=>!used.Contains(n)).OrderByDescending(n=>s.students[n].skills[r]*(.6f+.4f*s.students[n].reliability/100f)).First();Life.Assign(s,c,role,best);used.Add(best);}
            }
            Rules.FillClasses(s);Rules.FillEmpty(s,new PlanItem(ActionKind.Rest));var preview=Copy(s);Rules.Begin(preview);
            while(preview.phase=="executing"||preview.phase=="event")
            {
                if(preview.phase=="event"){Events.Choose(preview,EventOption(preview));continue;}
                int slot=preview.cursor;if(slot<28&&Rules.ScheduledCourse(preview,slot)<0){var item=Pick(preview,route);if(Rules.ActionLock(preview,item,slot)!="")item=new PlanItem(ActionKind.Rest);preview.plan[slot]=item;}
                Rules.Step(preview);
            }
            s.plan=preview.plan.Select(p=>p.Copy()).ToList();
        }
        public static YearState Play(string route,List<string> trace=null,int seed=17,int economy=1,int room=1)
        {
            var s=New(seed,economy,room);s.aspiration=route=="all"?"phd":route;s.scholarshipApplied=true;
            for(int r=1;r<=17;r++)
            {
                Assert(s.round==r&&s.phase=="plan","round "+r+" available");
                foreach(string name in new[]{"hk","mainland","phd"})if((route==name||route=="all")&&Rules.ApplicationLock(s,name)==""&&Rules.GPA(s,true)>=(name=="phd"?Balance.PhdGpa:name=="hk"?Balance.HkGpa:Balance.MainlandGpa))Assert(Rules.Apply(s,name),"application "+name);
                if(route=="delay")Rules.FillEmpty(s,new PlanItem(Rules.Holiday(s)?ActionKind.HomeVisit:ActionKind.Rest));else Prepare(s,route);
                if(trace!=null){trace.Add("第 "+r+" 轮 "+Rules.Month(r));for(int i=0;i<s.plan.Count;i++)trace.Add("  "+Rules.SlotName(s,i)+"："+Rules.PlanName(s.plan[i]));}
                Assert(Rules.Begin(s),"full plan starts");Drain(s);Assert(s.phase=="summary","round settles");
                if(trace!=null)trace.Add("结算：现金 "+s.money+"，疲劳 "+s.fatigue.ToString("0")+"，压力 "+s.stress.ToString("0")+"，预计 CGPA "+Rules.GPA(s,true).ToString("0.00"));Rules.Advance(s);
            }
            return s;
        }
        public static void Run()
        {
            checks=0;Directory.CreateDirectory("docs/paths");var report=new List<string>();
            foreach(string route in new[]{"hk","mainland","phd","explore","delay","all","hk-search","mainland-search"})
            {
                var trace=new List<string>();var s=Play(route,trace);string actual=route=="all"?"phd":route;
                string line=route+" CGPA="+Rules.GPA(s).ToString("0.00")+" ending="+s.ending+" offers="+string.Join(",",s.applications.Where(a=>a.offer).Select(a=>a.route).ToArray())+" money="+s.money+" stats="+s.academic+","+s.research+","+s.career+","+s.adaptation;
                Console.WriteLine(line);report.Add(line);trace.Add(line);trace.Add(Rules.GraduationReason(s));File.WriteAllLines("docs/paths/"+route+".txt",trace.ToArray());
                Assert(s.phase=="ending"&&s.round==17,"17 rounds finish");Assert(s.ending==actual,"reachable "+actual+" / "+Rules.GraduationReason(s));
                if(route=="all")Assert(new[]{"hk","mainland","phd"}.All(r=>Rules.Endings(s).Contains(r)),"multiple offers selectable");
                if(route=="phd")Assert(Rules.Honors(s).Count==2,"honors stack");
                if(route!="delay")Assert(s.courses.All(c=>c.scheduled==16&&c.attended+c.approvedLeave+c.absent==16),"per course attendance accounting");
            }
            var preference=Play("explore");preference.aspiration="hk";Assert(Rules.PreferredEnding(preference)=="hk-search","HK preference without offer stays HK");preference.aspiration="mainland";Assert(Rules.PreferredEnding(preference)=="mainland-search","mainland preference without offer stays mainland");preference.aspiration="phd";Assert(Rules.PreferredEnding(preference)=="explore","no PhD admission cannot award PhD");preference.courses[0].absent+=8;preference.courses[0].scheduled+=8;Assert(Rules.PreferredEnding(preference)=="delay","graduation has priority over aspiration");
            var n=New();Assert(!Rules.Begin(n),"empty plan blocked");Assert(Rules.FillClasses(n)==8,"eight fixed classes each round");Assert(!Rules.SetPlan(n,1,new PlanItem(ActionKind.Lecture,0)),"cannot spam lecture outside timetable");Assert(!Rules.MovePlan(n,0,1),"class cannot be moved off schedule");Assert(Rules.MovePlan(n,0,14),"same course weekly slots can swap");
            Rules.FillEmpty(n,new PlanItem(ActionKind.Rest));Assert(n.plan.Count(p=>p.kind==ActionKind.Lecture&&p.course==0)==2,"two lectures per course");
            var c0=new CourseState{scheduled=10,attended=7,absent=3};Assert(Rules.Attendance(c0)==70,"70 percent passes boundary");c0.attended=6;c0.approvedLeave=1;Assert(Rules.Attendance(c0)==70,"approved leave counts as compliant");
            var absent=New();absent.courses[0].scheduled=10;absent.courses[0].attended=6;absent.courses[0].absent=4;absent.courses[0].assignment.submitted=true;absent.courses[0].assignment.quality=100;absent.courses[0].project.submitted=true;absent.courses[0].project.quality=100;Assert(Rules.Score(absent,0)==0,"attendance below threshold forces F");Assert(Rules.Attendance(absent.courses[1])==100,"attendance not cross contaminated");
            var sick=New();Rules.FillClasses(sick);Rules.FillEmpty(sick,new PlanItem(ActionKind.Career));sick.fatigue=99;Rules.Begin(sick);Rules.Step(sick);Assert(sick.phase=="event"&&sick.cursor==0,"illness pauses before action");Events.Choose(sick,0);Rules.Step(sick);Assert(sick.courses[0].approvedLeave==1&&sick.courses[0].attended==0&&sick.courses[0].learning==0,"sick leave no learning windfall");Assert(sick.researchCount==0,"sick leave cancels income and work");Drain(sick);Assert(sick.phase=="summary","illness never game over");
            var winter=New();winter.round=9;Rules.BlankPlan(winter);Assert(Rules.Slots(winter)==2,"winter uses two slots");Assert(!Rules.SetPlan(winter,0,new PlanItem(ActionKind.Lecture)),"winter no courses");Assert(Rules.SetPlan(winter,0,new PlanItem(ActionKind.Travel))&&Rules.SetPlan(winter,1,new PlanItem(ActionKind.Travel)),"winter allows repeat");Rules.Begin(winter);Drain(winter);Assert(winter.tourBuffRound==10,"travel buff scheduled");Rules.Advance(winter);Assert(winter.plan.Count==28,"second semester resets slots");
            var normal=Copy(winter);normal.tourBuffRound=0;winter.stress=normal.stress=0;winter.fatigue=normal.fatigue=0;Rules.Perform(winter,new PlanItem(ActionKind.Career),0);Rules.Perform(normal,new PlanItem(ActionKind.Career),0);Assert(winter.stress==normal.stress/2&&winter.fatigue==normal.fatigue/2,"travel halves positive additions");Rules.FinishRound(winter);Assert(winter.tourBuffRound==0,"buff expires after first semester2 round");
            var late=New();late.round=6;late.courses[0].assignment.work=100;Assert(Rules.Submit(late,0,false)&&late.courses[0].assignment.quality==75,"late submission penalty");Assert(!Rules.Submit(late,0,false),"submission locked");
            var grades=New();grades.academic=100;Assert(Rules.Score(grades,0)==0,"global skill cannot replace coursework");Assert(Catalog.Courses.Count(d=>d.type==Assessment.Project)==4&&Catalog.Courses.Count(d=>d.type==Assessment.Combined)==2&&Catalog.Courses.Count(d=>d.type==Assessment.Exam)==2,"4 2 2 assessment distribution");
            var poor=New();poor.money=0;Rules.FinishRound(poor);Assert(poor.money==0&&poor.debt==Life.Bill(poor),"money rescue");int debt=poor.debt;Rules.FinishRound(poor);Assert(poor.debt==debt,"settlement idempotent");
            var home=New();home.money=10000;Assert(Life.Buy(home,0).StartsWith("买到了"),"buy mattress");Assert(Life.Equip(home,0)&&Life.UsedSlots(home)==2,"equip furniture");var empty=Copy(home);empty.equipped.Clear();home.fatigue=empty.fatigue=70;Rules.Perform(home,new PlanItem(ActionKind.Rest),0);Rules.Perform(empty,new PlanItem(ActionKind.Rest),0);Assert(home.fatigue<empty.fatigue,"furniture changes recovery");
            home.owned.Add(1);home.owned.Add(2);Life.Equip(home,1);Life.Equip(home,2);Assert(Life.RoomLock(home,0)=="先收起部分家具","smaller room protects slots");Life.Buy(home,5);Assert(!Life.Consume(home,0,99),"invalid gift rejected");Assert(Life.Consume(home,0,0)&&home.students[0].goodwill==6,"food gifting changes cooperation");
            var team=New();float before=Life.TeamFit(team,0);Life.Assign(team,0,0,0);float after=Life.TeamFit(team,0);Assert(before!=after,"assignment changes project quality");Life.Assign(team,0,1,0);Assert(team.courses[0].team[0]==-1&&team.courses[0].team[1]==0,"one student one role swap");var appearance=team.students[0].look.front;team.round++;Life.RefreshMarket(team);Assert(team.students[0].look.front==appearance,"NPC appearance persists");
            foreach(int seed in new[]{7,88})foreach(int econ in new[]{0,2}){var s=Play("explore",null,seed,econ,econ==0?0:2);Assert(Rules.CanGraduate(s),"economy and NPC variation viable");}
            foreach(ActionKind kind in new[]{ActionKind.Rest,ActionKind.Lecture,ActionKind.PartTime,ActionKind.Research}){var s=New();for(int round=1;round<=17;round++){Rules.FillEmpty(s,new PlanItem(Rules.Holiday(s)?ActionKind.HomeVisit:kind));Rules.FillEmpty(s,new PlanItem(Rules.Holiday(s)?ActionKind.HomeVisit:ActionKind.Rest));Rules.Begin(s);Drain(s);Rules.Advance(s);}Assert(!Rules.CanGraduate(s),"single action spam fails "+kind);}
            report.Add("PASS "+checks+" assertions");Console.WriteLine(report.Last());File.WriteAllLines("docs/rules-results.txt",report.ToArray());
        }
#if HARBOUR_STANDALONE
        public static int Main(){try{Run();return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
#endif
    }
}
