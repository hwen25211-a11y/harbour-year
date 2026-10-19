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
        static void Assert(bool ok,string message){checks++;if(!ok)throw new Exception("FAILED: "+message);}
        static YearState Copy(YearState s){using(var ms=new MemoryStream()){var f=new BinaryFormatter();f.Serialize(ms,s);ms.Position=0;return (YearState)f.Deserialize(ms);}}
        public static YearState Preview(YearState s)
        {
            var c=Copy(s);int count=c.plan.Count;float stress=s.stress;foreach(var p in s.plan)stress=Rules.Clamp(stress+Catalog.Actions[(int)p.kind].stress);
            while(c.plan.Count<4)c.plan.Add(new PlanItem(ActionKind.Rest));Rules.Execute(c);c.phase="plan";c.stress=stress;return c;
        }
        public static PlanItem Pick(YearState original,string route)
        {
            var s=Preview(original);
            if(s.stress>=55)return new PlanItem(s.adaptation<46?ActionKind.Explore:ActionKind.Rest);
            if(s.money<650)return new PlanItem(ActionKind.PartTime);
            int[] indices=Enumerable.Range(0,4).Where(i=>s.round>=Catalog.Courses[i].open).ToArray();
            foreach(var i in indices.OrderBy(i=>Catalog.Courses[i].homeworkDue))
            {
                var c=Catalog.Courses[i];var v=s.courses[i];
                if(!v.assignment.submitted&&v.assignment.work<90&&s.round>=c.homeworkDue-4)return new PlanItem(ActionKind.Coursework,i,false);
                if(!v.project.submitted&&v.project.work<90&&s.round>=c.projectDue-4)return new PlanItem(ActionKind.Coursework,i,true);
            }
            foreach(var i in indices)if(s.courses[i].learning<95)return new PlanItem(original.plan.Any(p=>p.kind==ActionKind.Lecture)?ActionKind.Study:ActionKind.Lecture,i);
            if(route=="phd"||route=="all")if(s.researchCount<7||s.research<66){var p=new PlanItem(ActionKind.Research);if(Rules.ActionLock(s,p)=="")return p;}
            if(route=="hk"||route=="mainland"||route=="all")
            {
                if(s.careerCount<3||s.career<32)return new PlanItem(ActionKind.Career);
                if(route!="mainland"&&(s.internshipCount<4||s.career<62)){var p=new PlanItem(ActionKind.Internship);if(Rules.ActionLock(s,p)=="")return p;}
                if(s.career<58)return new PlanItem(ActionKind.Career);
            }
            if(s.adaptation<48)return new PlanItem(ActionKind.Explore);
            if(s.researchCount<4&&Rules.ActionLock(s,new PlanItem(ActionKind.Research))=="")return new PlanItem(ActionKind.Research);
            return new PlanItem(s.money<2000?ActionKind.PartTime:ActionKind.Campus);
        }
        public static int EventOption(YearState s)
        {
            if(s.eventId=="arrival")return 2;
            if(s.eventId=="r1"||s.eventId=="r3"||s.eventId=="r5"||s.eventId=="r8"||s.eventId=="r9"||s.eventId=="r10"||s.eventId=="r11"||s.eventId=="r12"||s.eventId=="r13"||s.eventId=="r15"||s.eventId=="r18"||s.eventId=="r19"||s.eventId=="r20"||s.eventId=="r22"||s.eventId=="r23"||s.eventId=="r24"||s.eventId=="r25"||s.eventId=="r26")return 0;
            if(s.eventId=="r7"||s.eventId=="r21")return 1;
            return 0;
        }
        public static YearState Play(string route,List<string> trace=null)
        {
            var s=new YearState();Events.Choose(s,2);s.scholarshipApplied=true;
            for(int r=1;r<=26;r++)
            {
                Assert(s.round==r&&s.phase=="plan","turn sequence "+r);
                if(route!="delay")
                {
                    foreach(var name in new[]{"hk","mainland","phd"})if((route==name||route=="all")&&Rules.ApplicationLock(s,name)==""&&Rules.GPA(s,true)>= (name=="phd"?3f:name=="hk"?2.7f:2.3f))Rules.Apply(s,name);
                    if(r>=22)s.graduationApplied=true;
                }
                for(int slot=0;slot<4;slot++){var p=route=="delay"?new PlanItem(ActionKind.Rest):Pick(s,route);if(Rules.ActionLock(s,p)!="")p=new PlanItem(ActionKind.Career);Assert(Rules.AddPlan(s,p),"legal action "+Rules.PlanName(p));}
                if(trace!=null)trace.Add("回合 "+r+" | "+string.Join(" → ",s.plan.Select(Rules.PlanName).ToArray()));
                Assert(Rules.Execute(s),"execute full plan");int opt=EventOption(s);if(trace!=null)trace.Add("事件："+Events.Get(s).title+" / "+Events.Get(s).choices[opt].title);Events.Choose(s,opt);
                Assert(s.phase=="summary","summary reached");Rules.Advance(s);
            }
            if(trace!=null){trace.Add("最终 GPA "+Rules.GPA(s).ToString("0.00")+"；属性 "+s.academic.ToString("0")+" / "+s.research.ToString("0")+" / "+s.career.ToString("0")+" / "+s.adaptation.ToString("0"));trace.Add("可选结局："+string.Join("、",Rules.Endings(s).Select(Catalog.RouteName).ToArray()));trace.Add("荣誉："+string.Join("、",Rules.Honors(s).ToArray()));}
            return s;
        }
        public static void Run()
        {
            Directory.CreateDirectory("docs/paths");
            foreach(string r in new[]{"hk","mainland","phd","explore","delay","all"})
            {
                var trace=new List<string>();var s=Play(r,trace);File.WriteAllLines("docs/paths/"+r+".txt",trace.ToArray());
                string result=r+" GPA="+Rules.GPA(s).ToString("0.00")+" endings="+string.Join(",",Rules.Endings(s).ToArray())+" honors="+string.Join(",",Rules.Honors(s).ToArray())+" stats="+s.academic+","+s.research+","+s.career+","+s.adaptation;
                Console.WriteLine(result);File.AppendAllText("docs/rules-results.txt",result+"\n");
                Assert(s.phase=="ending","full game ended");if(r=="all")Assert(Rules.Endings(s).Count==4,"all offers remain selectable");else Assert(Rules.Endings(s).Contains(r),"reachable "+r+" / "+Rules.GraduationReason(s));
                if(r=="phd")Assert(Rules.Honors(s).Count==2,"honors stack with phd");
            }
            var n=new YearState();Events.Choose(n,0);Assert(!Rules.Execute(n),"cannot execute empty plan");for(int i=0;i<4;i++)Rules.AddPlan(n,new PlanItem(ActionKind.Rest));Assert(!Rules.AddPlan(n,new PlanItem(ActionKind.Rest)),"four slots only");Assert(Rules.MovePlan(n,0,3),"reorder plan");
            var high=new YearState();high.academic=100;Assert(Rules.Score(high,0)==0,"global academic cannot substitute course investment");
            var late=new YearState{round=9,phase="plan"};late.courses[0].assignment.work=100;Assert(Rules.Submit(late,0,false),"late submission allowed");Assert(late.courses[0].assignment.late&&late.courses[0].assignment.quality==75,"late penalty");Assert(!Rules.Submit(late,0,false),"cannot resubmit");
            var deadline=new YearState{round=6,phase="plan"};deadline.courses[0].assignment.work=55;Rules.FinishRound(deadline);Assert(deadline.courses[0].assignment.submitted&&!deadline.courses[0].assignment.late,"deadline auto submission");
            var zero=new YearState{round=6,phase="plan"};Rules.FinishRound(zero);Assert(!zero.courses[0].assignment.submitted,"zero work stays overdue");
            var poor=new YearState{money=0};Rules.FinishRound(poor);Assert(poor.money==0&&poor.debt==450&&poor.phase=="summary","budget rescue nonblocking");poor.money=1000;Rules.FinishRound(poor);Assert(poor.money==300&&poor.debt==200,"income repays debt but keeps buffer");
            Assert(Rules.Efficiency(new YearState{stress=90},0)==.5f,"stress penalty");Assert(Rules.Efficiency(new YearState{stress=0},3)<.5f,"repetition penalty");
            var ordering=new YearState{stress=90,phase="plan"};ordering.plan=new List<PlanItem>{new PlanItem(ActionKind.Rest),new PlanItem(ActionKind.Lecture),new PlanItem(ActionKind.Rest),new PlanItem(ActionKind.Rest)};var reversed=Copy(ordering);reversed.plan=new List<PlanItem>{new PlanItem(ActionKind.Lecture),new PlanItem(ActionKind.Rest),new PlanItem(ActionKind.Rest),new PlanItem(ActionKind.Rest)};Rules.Execute(ordering);Rules.Execute(reversed);Assert(ordering.courses[0].learning>reversed.courses[0].learning,"order simulation stable");
            var noapply=Play("explore");Assert(!Rules.Endings(noapply).Contains("phd")&&!Rules.Endings(noapply).Contains("hk"),"attributes never auto award offers");
            var echo=new YearState{phase="event",eventId="r3",eventReturn="plan"};Events.Choose(echo,0);echo.eventId="r8";echo.phase="event";float before=echo.courses[1].assignment.work;Events.Choose(echo,0);Assert(echo.courses[1].assignment.work-before==25,"group choice echoes");
            var wrong=new YearState{round=24,phase="plan",career=80,careerCount=4};Assert(Rules.Apply(wrong,"mainland"),"actual application required");wrong.round=26;Rules.FinishRound(wrong);Assert(wrong.applications[0].resolved&&!wrong.applications[0].offer,"low course results reject application");
            foreach(ActionKind kind in Enum.GetValues(typeof(ActionKind)))
            {
                var spam=new YearState();Events.Choose(spam,2);for(int r=1;r<=26;r++){for(int i=0;i<4;i++){var p=new PlanItem(kind,r<14?0:2);if(!Rules.AddPlan(spam,p))Rules.AddPlan(spam,new PlanItem(ActionKind.Rest));}Rules.Execute(spam);Events.Choose(spam,0);Rules.Advance(spam);}Assert(!Rules.CanGraduate(spam),"single action spam cannot graduate "+kind);
            }
            Console.WriteLine("PASS "+checks+" assertions");File.AppendAllText("docs/rules-results.txt","PASS "+checks+" assertions\n");
        }
#if HARBOUR_STANDALONE
        public static int Main(){try{if(File.Exists("docs/rules-results.txt"))File.Delete("docs/rules-results.txt");Run();return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
#endif
    }
}
