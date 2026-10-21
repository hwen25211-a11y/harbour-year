using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarbourYear.Lite;
// 轻松版规则检查：脚本化地玩完四轮，可与网页版引擎逐项比对（Tools/test-lite.sh）。
namespace HarbourYear.Tests
{
    public static class LiteChecks
    {
        public class Case{public string name,conflict,gender="female",grind;public int seed,choice;public int? sickDay;public string[] focus;public double roll;public bool extras,dayByDay;}
        static int checks;
        static void Assert(bool yes,string message){checks++;if(!yes)throw new Exception("FAILED: "+message);}
        public static JObj Play(Case c,List<string> trace=null)
        {
            var s=Core.New(c.seed);var err=Core.QuickStart(s,"测试同学",Look.Default(c.gender));if(err!=null)throw new Exception(err);
            var lite=Pace.NewMeta();var a=Pace.NewRound(s,lite);s=a.state;lite=a.meta;var snaps=new List<object>();trace=trace??new List<string>();
            System.Action<string> snap=tag=>{var o=new JObj();o["tag"]=tag;o["state"]=s.ToJ();o["lite"]=lite.ToJ();snaps.Add(o);};
            for(int guard=0;guard<2000;guard++)
            {
                if(s.phase=="planning")
                {
                    if(Pace.PhaseStart(s.round)){var x=Pace.SetFocus(s,lite,c.focus[Pace.PhaseIndex(s.round)]);if(x.error==null){s=x.state;lite=x.meta;}}
                    if(lite.focus=="work")
                    {
                        var card=Pace.JobOffer(s,lite);
                        if(card!=null&&card.kind=="offer"){var t=s.Clone();Attempt at;if(Core.Apply(t,card.job.id,c.roll,out at)==null){s=t;if(at.success&&lite.round==s.round&&s.phase=="planning"){var z=Pace.Replan(s,lite);s=z.state;lite=z.meta;}}}
                    }
                    if(c.extras)
                    {
                        if(s.round==0){var t=s.Clone();if(Core.Buy(t,"bun")==null)s=t;t=s.Clone();if(Core.Buy(t,"plant")==null)s=t;t=s.Clone();Core.Toggle(t,"plant");s=t;}
                        if(s.round==1){var slot=Pace.AutoSlots(s).Where(x=>x%2==1&&!lite.free.Contains(x)&&x>6).Cast<int?>().FirstOrDefault();if(slot!=null){var n=Pace.AddSpecial(s,lite,slot.Value,"review");if(n.error==null){s=n.state;lite=n.meta;}}}
                        if(s.round==2){var i=Pace.Relax(s,lite);if(i.confirmSlots!=null)i=Pace.Relax(s,lite,true);if(i.error==null&&i.confirmSlots==null){s=i.state;lite=i.meta;}var t=s.Clone();Core.Use(t,"bun");s=t;}
                    }
                    if(Pace.NeedTeams(s).Count>0){List<string> d,e;var p=Pace.DefaultTeams(s,lite,out d,out e);s=p.state;lite=p.meta;}
                    if(c.grind!=null){int z=Core.Pos(s);var plan=s.plan.Select((x,i)=>i>=z&&x=="rest"?(c.grind=="job"&&s.hired.Count>0&&i%3==0?"job:"+s.hired[0]:"review"):x).ToList();var s2=Core.ApplyPlan(s,plan);lite=Pace.Customized(s,s2,lite);s=s2;}
                    if(lite.conflict!=null){var m=Pace.ResolveConflict(s,lite,lite.conflict.key,c.conflict);s=m.state;lite=m.meta;}
                    var z2=Pace.StartRound(s,lite);
                    if(z2.error!=null){if(lite.conflict!=null){var m=Pace.ResolveConflict(s,lite,lite.conflict.key,c.conflict);s=m.state;lite=m.meta;continue;}throw new Exception("start failed: "+z2.error);}
                    s=z2.state;lite=z2.meta;
                }
                var st=Pace.Run(s,lite,c.dayByDay);s=st.state;lite=st.meta;trace.Add(st.stop.kind+"@"+s.round+":"+s.cursor);
                switch(st.stop.kind)
                {
                    case "event":
                    {
                        var ev=Cat.EventOf(s.ev.id);int ch=c.choice%ev.options.Length;if(ev.options[ch].money<0&&s.money<-ev.options[ch].money)ch=(ch+1)%ev.options.Length;
                        lite=Pace.RecordLook(s,lite);var t=s.Clone();var e=Core.Choose(t,ch);if(e!=null)throw new Exception(e);s=t;break;
                    }
                    case "result":lite=Pace.MarkSeen(lite,Pace.PendingCard(s,lite).key);break;
                    case "trip":lite=Pace.KeepTrip(lite,Pace.PendingTrip(lite).key);break;
                    case "team":{List<string> d,e;var p=Pace.DefaultTeams(s,lite,out d,out e);s=p.state;lite=p.meta;break;}
                    case "conflict":{var m=Pace.ResolveConflict(s,lite,lite.conflict.key,c.conflict);s=m.state;lite=m.meta;break;}
                    case "blocked":
                    {
                        var plan=new List<string>(s.plan);if(Core.Trip(plan[s.cursor])!=null&&s.cursor%2==0)plan[s.cursor+1]="rest";plan[s.cursor]="rest";var o=s;s=Core.ApplyPlan(s,plan);lite=Pace.Customized(o,s,lite);break;
                    }
                    case "round_end":{snap("round"+s.round);var n=Pace.NextRound(s,lite);if(n.error!=null)throw new Exception(n.error);s=n.state;lite=n.meta;break;}
                    case "day":if(c.sickDay!=null&&Cal.SlotOf(s.round,s.cursor).day==c.sickDay&&s.phase=="running"){s=s.Clone();s.fatigue=95;}break;
                    case "sample_end":{snap("end");var r=new JObj();r["name"]=c.name;r["snaps"]=snaps;r["trace"]=trace.Cast<object>().ToList();return r;}
                }
            }
            throw new Exception("guard");
        }
        static Case FromJ(JObj o)
        {
            return new Case{name=o.S("name"),seed=o.I("seed"),focus=o.A("focus").Cast<string>().ToArray(),choice=o.I("choice"),conflict=o.S("conflict"),roll=o.D("roll"),extras=o.B("extras"),gender=o.S("gender")??"female",dayByDay=o.Has("dayByDay")&&o.B("dayByDay"),grind=o.Has("grind")?o.S("grind"):null,sickDay=o.Has("sickDay")?(int?)o.I("sickDay"):null};
        }
        // 不依赖网页版的基本断言
        public static void Basics()
        {
            Assert(Cal.Sessions.Count==128,"128 timetable sessions");Assert(Cal.At(31,"pm")==null,"national day class moved");
            var moved=Cal.Sessions.First(x=>x.plannedDay==31);Assert(moved.day!=31&&moved.moveReason.Contains("国庆日"),"holiday makeup lesson");
            var s=Core.New(7);Assert(s.npcs.Count==26&&s.npcs.Select(n=>n.name).Distinct().Count()==26,"26 unique classmates");
            Assert(Core.QuickStart(s,"  小明  ",Look.Default("male"))==null&&s.money==27500&&s.name=="小明","quick start budget");
            var m=Pace.NewMeta();var a=Pace.NewRound(s,m);Assert(a.state.plan.All(x=>x!=null)&&a.meta.auto.Count==28-8,"round plan filled; 8 fixed lessons");
            var json=Json.Write(a.state.ToJ());var back=GameState.From((JObj)Json.Parse(json));Assert(Json.Write(back.ToJ())==json,"state JSON round trip");
            var mj=Json.Write(a.meta.ToJ());Assert(Json.Write(LiteMeta.From((JObj)Json.Parse(mj)).ToJ())==mj,"meta JSON round trip");
            Assert(Core.ExamScore(50,30,70,40)==Math.Floor((10+35+6-1.5)*100+0.5)/100,"exam penalty formula");
            foreach(var f in new[]{"academic","life","work"}){var c=new Case{name=f,seed=5,focus=new[]{f,f},choice=0,conflict="rest",roll=0.1};var r=Play(c);Assert(r.A("snaps").Count==4,f+" plays four rounds");}
        }
#if HARBOUR_STANDALONE
        public static int Main(string[] args)
        {
            try
            {
                Basics();Console.WriteLine("LITE_BASICS_OK "+checks);
                if(args.Length==3&&args[0]=="--roundtrip")
                {
                    // 网页版导出的存档读入再写回，应逐字段一致
                    int n=0;var all=(List<object>)Json.Parse(File.ReadAllText(args[1]));
                    foreach(JObj c in all)foreach(JObj snap in c.A("snaps")){snap["state"]=GameState.From(snap.O("state")).ToJ();snap["lite"]=LiteMeta.From(snap.O("lite")).ToJ();n++;}
                    File.WriteAllText(args[2],Json.Write(all));Console.WriteLine("LITE_ROUNDTRIP_OK "+n);return 0;
                }
                if(args.Length>=2){var cases=((List<object>)Json.Parse(File.ReadAllText(args[0]))).Cast<JObj>().Select(FromJ).ToList();File.WriteAllText(args[1],Json.Write(cases.Select(c=>(object)Play(c)).ToList()));Console.WriteLine("LITE_CASES_OK "+cases.Count);}
                return 0;
            }
            catch(Exception e){Console.WriteLine(e);return 1;}
        }
#endif
    }
}
