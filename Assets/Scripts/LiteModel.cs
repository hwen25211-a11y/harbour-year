using System;
using System.Collections.Generic;
using System.Linq;
// 轻松版存档结构：字段名与网页版一致；空值用 C# null 表示。
namespace HarbourYear.Lite
{
    static class J
    {
        public static JObj Map(IEnumerable<string> keys,Func<string,object> value){var o=new JObj();foreach(var k in keys)o[k]=value(k);return o;}
        public static List<object> L<T>(IEnumerable<T> items,Func<T,object> f){return items.Select(f).ToList();}
        public static List<object> L(IEnumerable<int> items){return items.Select(i=>(object)(double)i).ToList();}
        public static List<object> L(IEnumerable<string> items){return items.Select(i=>(object)i).ToList();}
        public static List<int> Ints(List<object> a){return a.Select(x=>{double d=(double)x;if(d!=Math.Floor(d))throw new FormatException("需要整数");return (int)d;}).ToList();}
        public static List<string> Strs(List<object> a){return a.Select(x=>x==null?null:(string)x).ToList();}
        public static object N(int? v){return v.HasValue?(object)(double)v.Value:null;}
        public static object N(double? v){return v.HasValue?(object)v.Value:null;}
    }
    public class Npc
    {
        public string id,name,kind;public double[] abilities=new double[4];
        public double Ab(string id){return abilities[Cat.AbilityIndex(id)];}
        public Npc Clone(){var c=(Npc)MemberwiseClone();c.abilities=(double[])abilities.Clone();return c;}
        public JObj ToJ(){var o=new JObj();o["id"]=id;o["name"]=name;o["abilities"]=J.Map(Cat.Abilities,k=>abilities[Cat.AbilityIndex(k)]);o["kind"]=kind;return o;}
        public static Npc From(JObj o){var a=o.O("abilities");return new Npc{id=o.S("id"),name=o.S("name"),kind=o.S("kind"),abilities=Cat.Abilities.Select(k=>a.D(k)).ToArray()};}
    }
    public class Look
    {
        public string bodyId="female_young",frontHair="01",backHair="01",outfit="class";
        public static Look Default(string g){return new Look{bodyId=g=="male"?"male_young":"female_young"};}
        public Look Clone(){return (Look)MemberwiseClone();}
        public bool Valid(){return Cat.Bodies.Contains(bodyId)&&Cat.Hairs.Contains(frontHair)&&Cat.Hairs.Contains(backHair)&&Cat.Outfits.Contains(outfit);}
        public bool Female{get{return bodyId.StartsWith("female");}}
        public JObj ToJ(){var o=new JObj();o["bodyId"]=bodyId;o["frontHair"]=frontHair;o["backHair"]=backHair;o["outfit"]=outfit;return o;}
        public static Look From(JObj o){if(o.Count!=4)throw new FormatException("外观格式错误");return new Look{bodyId=o.S("bodyId"),frontHair=o.S("frontHair"),backHair=o.S("backHair"),outfit=o.S("outfit")};}
    }
    public class Work
    {
        public string status="unreleased";public int? released,submittedDay;public double progress;public int polish;public bool late;public double? quality,score;public double sumA,sumM;public int halves;public bool legacy;
        public Work CloneWork(){return (Work)MemberwiseClone();}
        public virtual JObj ToJ(){var o=new JObj();o["status"]=status;o["released"]=J.N(released);o["progress"]=progress;o["polish"]=(double)polish;o["submittedDay"]=J.N(submittedDay);o["late"]=late;o["quality"]=J.N(quality);o["sumA"]=sumA;o["sumM"]=sumM;o["halves"]=(double)halves;o["score"]=J.N(score);o["legacy"]=legacy;return o;}
        protected void Read(JObj o){status=o.S("status");released=o.NI("released");progress=o.D("progress");polish=o.I("polish");submittedDay=o.NI("submittedDay");late=o.B("late");quality=o.ND("quality");sumA=o.D("sumA");sumM=o.D("sumM");halves=o.I("halves");score=o.ND("score");legacy=o.B("legacy");}
        public static Work From(JObj o){var w=new Work();w.Read(o);return w;}
    }
    public class Invite{public string npc;public bool ok;}
    public class Project:Work
    {
        public List<string> members=new List<string>();public Dictionary<string,string> roles=Cat.Roles.ToDictionary(r=>r,r=>(string)null);public bool locked;public List<Invite> invites=new List<Invite>();public List<string> shown=new List<string>();public bool refreshed;public string relation="pending";
        public bool Invited(string npc){return invites.Any(i=>i.npc==npc);}
        public Project Clone(){var c=(Project)MemberwiseClone();c.members=new List<string>(members);c.roles=new Dictionary<string,string>(roles);c.invites=invites.Select(i=>new Invite{npc=i.npc,ok=i.ok}).ToList();c.shown=new List<string>(shown);return c;}
        public override JObj ToJ(){var o=base.ToJ();o["members"]=J.L(members);o["roles"]=J.Map(Cat.Roles,k=>roles[k]);o["locked"]=locked;var inv=new JObj();foreach(var i in invites)inv[i.npc]=i.ok;o["invites"]=inv;o["shown"]=J.L(shown);o["refreshed"]=refreshed;o["relation"]=relation;return o;}
        public static new Project From(JObj o)
        {
            var p=new Project();p.Read(o);p.members=J.Strs(o.A("members"));var r=o.O("roles");foreach(var k in Cat.Roles){if(!r.Has(k))throw new FormatException("分工缺失");p.roles[k]=r.S(k);}
            p.locked=o.B("locked");var inv=o.O("invites");p.invites=inv.Order.Select(k=>new Invite{npc=k,ok=inv.B(k)}).ToList();p.shown=J.Strs(o.A("shown"));p.refreshed=o.B("refreshed");p.relation=o.S("relation");return p;
        }
    }
    public class Att
    {
        public int attended,excused,expected;public Att Clone(){return (Att)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["attended"]=(double)attended;o["excused"]=(double)excused;o["expected"]=(double)expected;return o;}
        public static Att From(JObj o){return new Att{attended=o.I("attended"),excused=o.I("excused"),expected=o.I("expected")};}
    }
    public class ExamRec
    {
        public string status,makeup;public double? score;public bool legacy;public ExamRec Clone(){return (ExamRec)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["status"]=status;o["score"]=J.N(score);o["makeup"]=makeup;o["legacy"]=legacy;return o;}
        public static ExamRec From(JObj o){return new ExamRec{status=o.S("status"),score=o.ND("score"),makeup=o.S("makeup"),legacy=o.B("legacy")};}
    }
    public class CurEvent
    {
        public string id,half,course,npc;public int day;public CurEvent Clone(){return (CurEvent)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["id"]=id;o["day"]=(double)day;o["half"]=half;o["course"]=course;o["npc"]=npc;return o;}
        public static CurEvent From(JObj o){return o==null?null:new CurEvent{id=o.S("id"),day=o.I("day"),half=o.S("half"),course=o.S("course"),npc=o.S("npc")};}
    }
    public class EventRec
    {
        public string id,course,npc;public int round,day;public int? choice;public EventRec Clone(){return (EventRec)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["id"]=id;o["round"]=(double)round;o["day"]=(double)day;o["choice"]=J.N(choice);o["course"]=course;o["npc"]=npc;return o;}
        public static EventRec From(JObj o){return new EventRec{id=o.S("id"),round=o.I("round"),day=o.I("day"),choice=o.NI("choice"),course=o.S("course"),npc=o.S("npc")};}
    }
    public class Attempt
    {
        public int round;public string job;public double chance,roll;public bool success;public Attempt Clone(){return (Attempt)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["round"]=(double)round;o["job"]=job;o["chance"]=chance;o["roll"]=roll;o["success"]=success;return o;}
        public static Attempt From(JObj o){return o==null?null:new Attempt{round=o.I("round"),job=o.S("job"),chance=o.D("chance"),roll=o.D("roll"),success=o.B("success")};}
    }
    public class LogEntry
    {
        public int day,money,fatigue,stress;public string half,date,title,text;public LogEntry Clone(){return (LogEntry)MemberwiseClone();}
        public JObj ToJ(){var o=new JObj();o["day"]=(double)day;o["half"]=half;o["date"]=date;o["title"]=title;o["text"]=text;o["money"]=(double)money;o["fatigue"]=(double)fatigue;o["stress"]=(double)stress;return o;}
        public static LogEntry From(JObj o){return new LogEntry{day=o.I("day"),half=o.S("half"),date=o.S("date"),title=o.S("title"),text=o.S("text"),money=o.I("money"),fatigue=o.I("fatigue"),stress=o.I("stress")};}
    }
    public class Notice{public string course,kind;public JObj ToJ(){var o=new JObj();o["course"]=course;o["kind"]=kind;return o;}}
    public class GameState
    {
        public int version=9,jobShifts,seed,reputation=Cat.AffectionStart,budget,rentPaid,deposit,round,cursor,money,fatigue=Cat.StartFatigue,stress=Cat.StartStress;
        public string destination,economy,housing,reviewFocus="research",phase="planning",blockedReason,name="新同学";
        public JObj ending;
        public List<Npc> npcs=new List<Npc>();public Dictionary<string,int> affection=new Dictionary<string,int>();public List<string> known=new List<string>();
        public bool firstWalkDone;public CurEvent ev;public List<EventRec> events=new List<EventRec>();
        public double[] initialAbilities=new double[4],abilities=new double[4],mastery=new double[8];
        public List<string> hired=new List<string>();public Attempt lastAttempt;
        public List<string> plan=new List<string>(),lastPlan;
        public Work[] tasks=new Work[8];public Project[] projects=new Project[8];public Att[] attendance=new Att[8];
        public List<KeyValuePair<string,ExamRec>> exams=new List<KeyValuePair<string,ExamRec>>();
        public int? sickDay;public Notice notice;public List<LogEntry> logs=new List<LogEntry>(),history=new List<LogEntry>();
        public Look appearance=Look.Default("female");public int[] inventory=new int[5],purchased=new int[5];public List<string> installed=new List<string>();
        public int startMoney,startFatigue=Cat.StartFatigue,startStress=Cat.StartStress;
        public double Ab(string id){return abilities[Cat.AbilityIndex(id)];}
        public double Mastery(string course){return mastery[Cat.CourseIndex(course)];}
        public Work Task(string course){return tasks[Cat.CourseIndex(course)];}
        public Project Proj(string course){return projects[Cat.CourseIndex(course)];}
        public Att Attendance(string course){return attendance[Cat.CourseIndex(course)];}
        public ExamRec Exam(string id){foreach(var e in exams)if(e.Key==id)return e.Value;return null;}
        public void SetExam(string id,ExamRec r){for(int i=0;i<exams.Count;i++)if(exams[i].Key==id){exams[i]=new KeyValuePair<string,ExamRec>(id,r);return;}exams.Add(new KeyValuePair<string,ExamRec>(id,r));}
        public Npc Npc(string id){return npcs.FirstOrDefault(e=>e.id==id);}
        public GameState Clone(bool withLogs=true)
        {
            var c=(GameState)MemberwiseClone();
            c.npcs=npcs.Select(n=>n.Clone()).ToList();c.affection=new Dictionary<string,int>(affection);c.known=new List<string>(known);c.ev=ev==null?null:ev.Clone();c.events=events.Select(e=>e.Clone()).ToList();
            c.initialAbilities=(double[])initialAbilities.Clone();c.abilities=(double[])abilities.Clone();c.mastery=(double[])mastery.Clone();c.hired=new List<string>(hired);c.lastAttempt=lastAttempt==null?null:lastAttempt.Clone();
            c.plan=new List<string>(plan);c.lastPlan=lastPlan==null?null:new List<string>(lastPlan);c.tasks=tasks.Select(t=>t.CloneWork()).ToArray();c.projects=projects.Select(p=>p.Clone()).ToArray();c.attendance=attendance.Select(a=>a.Clone()).ToArray();
            c.exams=exams.Select(e=>new KeyValuePair<string,ExamRec>(e.Key,e.Value.Clone())).ToList();c.notice=notice==null?null:new Notice{course=notice.course,kind=notice.kind};
            c.logs=withLogs?logs.Select(l=>l.Clone()).ToList():new List<LogEntry>();c.history=withLogs?history.Select(l=>l.Clone()).ToList():new List<LogEntry>();
            c.appearance=appearance.Clone();c.inventory=(int[])inventory.Clone();c.purchased=(int[])purchased.Clone();c.installed=new List<string>(installed);c.ending=ending;return c;
        }
        static JObj Abil(double[] a){return J.Map(Cat.Abilities,k=>a[Cat.AbilityIndex(k)]);}
        static double[] Abil(JObj o){return Cat.Abilities.Select(k=>o.D(k)).ToArray();}
        public JObj ToJ()
        {
            var o=new JObj();var ids=Cat.Courses.Select(c=>c.id).ToList();
            o["version"]=(double)version;o["jobShifts"]=(double)jobShifts;o["destination"]=destination;o["ending"]=ending;o["seed"]=(double)seed;o["npcs"]=J.L(npcs,n=>n.ToJ());
            o["affection"]=J.Map(npcs.Select(n=>n.id).Where(affection.ContainsKey),k=>(double)affection[k]);o["reputation"]=(double)reputation;o["known"]=J.L(known);o["firstWalkDone"]=firstWalkDone;o["event"]=ev==null?null:ev.ToJ();o["events"]=J.L(events,e=>e.ToJ());
            o["initialAbilities"]=Abil(initialAbilities);o["abilities"]=Abil(abilities);o["mastery"]=J.Map(ids,k=>mastery[Cat.CourseIndex(k)]);
            var jobs=new JObj();jobs["hired"]=J.L(hired);jobs["lastAttempt"]=lastAttempt==null?null:lastAttempt.ToJ();o["jobs"]=jobs;
            o["reviewFocus"]=reviewFocus;o["economy"]=economy;o["housing"]=housing;o["budget"]=(double)budget;o["rentPaid"]=(double)rentPaid;o["deposit"]=(double)deposit;o["round"]=(double)round;o["phase"]=phase;o["cursor"]=(double)cursor;
            o["plan"]=J.L(plan);o["lastPlan"]=lastPlan==null?null:J.L(lastPlan);o["money"]=(double)money;o["fatigue"]=(double)fatigue;o["stress"]=(double)stress;
            o["tasks"]=J.Map(ids,k=>Task(k).ToJ());o["projects"]=J.Map(ids,k=>Proj(k).ToJ());o["attendance"]=J.Map(ids,k=>Attendance(k).ToJ());
            var ex=new JObj();foreach(var e in exams)ex[e.Key]=e.Value.ToJ();o["exams"]=ex;
            o["sickDay"]=J.N(sickDay);o["notice"]=notice==null?null:notice.ToJ();o["blockedReason"]=blockedReason;o["logs"]=J.L(logs,l=>l.ToJ());o["history"]=J.L(history,l=>l.ToJ());
            o["name"]=name;o["appearance"]=appearance.ToJ();o["inventory"]=J.Map(Cat.Goods.Select(g=>g.id),k=>(double)inventory[Cat.GoodIndex(k)]);o["installed"]=J.L(installed);o["purchased"]=J.Map(Cat.Goods.Select(g=>g.id),k=>(double)purchased[Cat.GoodIndex(k)]);
            var rs=new JObj();rs["money"]=(double)startMoney;rs["fatigue"]=(double)startFatigue;rs["stress"]=(double)startStress;o["roundStart"]=rs;return o;
        }
        public static GameState From(JObj o)
        {
            var s=new GameState();var ids=Cat.Courses.Select(c=>c.id).ToList();
            s.version=o.I("version");s.jobShifts=o.I("jobShifts");s.destination=o.S("destination");s.ending=o["ending"] as JObj;s.seed=o.I("seed");s.npcs=o.A("npcs").Select(x=>HarbourYear.Lite.Npc.From((JObj)x)).ToList();
            var af=o.O("affection");s.affection=af.Order.ToDictionary(k=>k,k=>af.I(k));s.reputation=o.I("reputation");s.known=J.Strs(o.A("known"));s.firstWalkDone=o.B("firstWalkDone");s.ev=CurEvent.From(o["event"] as JObj);s.events=o.A("events").Select(x=>EventRec.From((JObj)x)).ToList();
            s.initialAbilities=Abil(o.O("initialAbilities"));s.abilities=Abil(o.O("abilities"));var m=o.O("mastery");s.mastery=ids.Select(k=>m.D(k)).ToArray();
            var jobs=o.O("jobs");s.hired=J.Strs(jobs.A("hired"));s.lastAttempt=Attempt.From(jobs["lastAttempt"] as JObj);
            s.reviewFocus=o.S("reviewFocus");s.economy=o.S("economy");s.housing=o.S("housing");s.budget=o.I("budget");s.rentPaid=o.I("rentPaid");s.deposit=o.I("deposit");s.round=o.I("round");s.phase=o.S("phase");s.cursor=o.I("cursor");
            s.plan=J.Strs(o.A("plan"));s.lastPlan=o["lastPlan"]==null?null:J.Strs(o.A("lastPlan"));s.money=o.I("money");s.fatigue=o.I("fatigue");s.stress=o.I("stress");
            var t=o.O("tasks");var p=o.O("projects");var a=o.O("attendance");s.tasks=ids.Select(k=>Work.From(t.O(k))).ToArray();s.projects=ids.Select(k=>Project.From(p.O(k))).ToArray();s.attendance=ids.Select(k=>Att.From(a.O(k))).ToArray();
            var ex=o.O("exams");s.exams=ex.Order.Select(k=>new KeyValuePair<string,ExamRec>(k,ExamRec.From(ex.O(k)))).ToList();
            s.sickDay=o.NI("sickDay");var n=o["notice"] as JObj;s.notice=n==null?null:new Notice{course=n.S("course"),kind=n.S("kind")};s.blockedReason=o.S("blockedReason");
            s.logs=o.A("logs").Select(x=>LogEntry.From((JObj)x)).ToList();s.history=o.A("history").Select(x=>LogEntry.From((JObj)x)).ToList();
            s.name=o.S("name");s.appearance=Look.From(o.O("appearance"));var inv=o.O("inventory");var pu=o.O("purchased");s.inventory=Cat.Goods.Select(g=>inv.I(g.id)).ToArray();s.purchased=Cat.Goods.Select(g=>pu.I(g.id)).ToArray();s.installed=J.Strs(o.A("installed"));
            var rs=o.O("roundStart");s.startMoney=rs.I("money");s.startFatigue=rs.I("fatigue");s.startStress=rs.I("stress");return s;
        }
    }
    public class Digest{public int round,day;public string text;}
    public class Risk{public int day,energy,stress;}
    public class Health{public bool triggered,resolved;public int rested;public Risk risk;}
    public class ConflictItem{public string item,course,kind;public int due;public List<int> halves=new List<int>();}
    public class ConflictInfo{public string key;public List<ConflictItem> items=new List<ConflictItem>();public int day,energy,stress;}
    public class TripRec{public string key,loc,body,selfie,review;public int round,day;}
    public class LiteMeta
    {
        public int v=2,focusPhase=-1,round=-1,jobCardDismissed=-1,quotaRound=-1;public string focus="life",dayCheck;public bool customized;
        public List<int> free=new List<int>(),auto=new List<int>(),relaxed=new List<int>();public List<string> seen=new List<string>(),teamPrompted=new List<string>(),defaultTeams=new List<string>();
        public List<Digest> digest=new List<Digest>();public List<KeyValuePair<string,string>> conflicts=new List<KeyValuePair<string,string>>();public List<KeyValuePair<int,string>> monologues=new List<KeyValuePair<int,string>>();
        public Health health;public ConflictInfo conflict;public List<KeyValuePair<string,int>> quota=new List<KeyValuePair<string,int>>();public List<TripRec> trips;public List<KeyValuePair<string,string>> looks;
        public int? Quota(string item){foreach(var q in quota)if(q.Key==item)return q.Value;return null;}
        public LiteMeta Clone()
        {
            var c=(LiteMeta)MemberwiseClone();c.free=new List<int>(free);c.auto=new List<int>(auto);c.relaxed=new List<int>(relaxed);c.seen=new List<string>(seen);c.teamPrompted=new List<string>(teamPrompted);c.defaultTeams=new List<string>(defaultTeams);
            c.digest=digest.Select(d=>new Digest{round=d.round,day=d.day,text=d.text}).ToList();c.conflicts=new List<KeyValuePair<string,string>>(conflicts);c.monologues=new List<KeyValuePair<int,string>>(monologues);
            c.health=health==null?null:new Health{triggered=health.triggered,resolved=health.resolved,rested=health.rested,risk=health.risk==null?null:new Risk{day=health.risk.day,energy=health.risk.energy,stress=health.risk.stress}};
            c.conflict=conflict==null?null:new ConflictInfo{key=conflict.key,day=conflict.day,energy=conflict.energy,stress=conflict.stress,items=conflict.items.Select(i=>new ConflictItem{item=i.item,course=i.course,kind=i.kind,due=i.due,halves=new List<int>(i.halves)}).ToList()};
            c.quota=new List<KeyValuePair<string,int>>(quota);c.trips=trips==null?null:trips.Select(t=>(TripRec)t.MemberwiseCloneTrip()).ToList();c.looks=looks==null?null:new List<KeyValuePair<string,string>>(looks);return c;
        }
        public JObj ToJ()
        {
            var o=new JObj();o["v"]=(double)v;o["focus"]=focus;o["focusPhase"]=(double)focusPhase;o["round"]=(double)round;o["free"]=J.L(free);o["auto"]=J.L(auto);o["customized"]=customized;o["seen"]=J.L(seen);o["teamPrompted"]=J.L(teamPrompted);o["defaultTeams"]=J.L(defaultTeams);
            o["digest"]=J.L(digest,d=>{var x=new JObj();x["round"]=(double)d.round;x["day"]=(double)d.day;x["text"]=d.text;return x;});
            o["conflicts"]=J.L(conflicts,c=>{var x=new JObj();x["key"]=c.Key;x["choice"]=c.Value;return x;});o["monologues"]=J.L(monologues,c=>{var x=new JObj();x["round"]=(double)c.Key;x["key"]=c.Value;return x;});
            o["jobCardDismissed"]=(double)jobCardDismissed;o["relaxed"]=J.L(relaxed);
            if(health==null)o["health"]=null;else{var h=new JObj();h["triggered"]=health.triggered;h["resolved"]=health.resolved;h["rested"]=(double)health.rested;h["risk"]=RiskJ(health.risk);o["health"]=h;}
            if(conflict==null)o["conflict"]=null;else{var c=new JObj();c["key"]=conflict.key;c["items"]=J.L(conflict.items,i=>{var x=new JObj();x["item"]=i.item;x["course"]=i.course;x["kind"]=i.kind;x["due"]=(double)i.due;x["halves"]=J.L(i.halves);return x;});c["day"]=(double)conflict.day;c["energy"]=(double)conflict.energy;c["stress"]=(double)conflict.stress;o["conflict"]=c;}
            var wq=new JObj();wq["round"]=(double)quotaRound;var items=new JObj();foreach(var q in quota)items[q.Key]=(double)q.Value;wq["items"]=items;o["workQuota"]=wq;
            if(trips!=null)o["trips"]=J.L(trips,t=>{var x=new JObj();x["key"]=t.key;x["loc"]=t.loc;x["round"]=(double)t.round;x["day"]=(double)t.day;x["body"]=t.body;x["selfie"]=t.selfie;x["review"]=t.review;return x;});
            if(looks!=null)o["looks"]=J.L(looks,l=>{var x=new JObj();x["key"]=l.Key;x["body"]=l.Value;return x;});
            if(dayCheck!=null)o["dayCheck"]=dayCheck;return o;
        }
        static object RiskJ(Risk r){if(r==null)return null;var x=new JObj();x["day"]=(double)r.day;x["energy"]=(double)r.energy;x["stress"]=(double)r.stress;return x;}
        public static LiteMeta From(JObj o)
        {
            var m=new LiteMeta{v=o.I("v"),focus=o.S("focus"),focusPhase=o.I("focusPhase"),round=o.I("round"),free=J.Ints(o.A("free")),auto=J.Ints(o.A("auto")),customized=o.B("customized"),seen=J.Strs(o.A("seen")),teamPrompted=J.Strs(o.A("teamPrompted")),defaultTeams=J.Strs(o.A("defaultTeams"))};
            m.digest=o.A("digest").Cast<JObj>().Select(d=>new Digest{round=d.I("round"),day=d.I("day"),text=d.S("text")}).ToList();
            m.conflicts=o.A("conflicts").Cast<JObj>().Select(c=>new KeyValuePair<string,string>(c.S("key"),c.S("choice"))).ToList();m.monologues=o.A("monologues").Cast<JObj>().Select(c=>new KeyValuePair<int,string>(c.I("round"),c.S("key"))).ToList();
            m.jobCardDismissed=o.I("jobCardDismissed");m.relaxed=J.Ints(o.A("relaxed"));
            var h=o["health"] as JObj;if(h!=null){var r=h["risk"] as JObj;m.health=new Health{triggered=h.B("triggered"),resolved=h.B("resolved"),rested=h.I("rested"),risk=r==null?null:new Risk{day=r.I("day"),energy=r.I("energy"),stress=r.I("stress")}};}
            var c0=o["conflict"] as JObj;if(c0!=null)m.conflict=new ConflictInfo{key=c0.S("key"),day=c0.I("day"),energy=c0.I("energy"),stress=c0.I("stress"),items=c0.A("items").Cast<JObj>().Select(i=>new ConflictItem{item=i.S("item"),course=i.S("course"),kind=i.S("kind"),due=i.I("due"),halves=J.Ints(i.A("halves"))}).ToList()};
            var wq=o.O("workQuota");m.quotaRound=wq.I("round");var items=wq.O("items");m.quota=items.Order.Select(k=>new KeyValuePair<string,int>(k,items.I(k))).ToList();
            if(o["trips"]!=null)m.trips=o.A("trips").Cast<JObj>().Select(t=>new TripRec{key=t.S("key"),loc=t.S("loc"),round=t.I("round"),day=t.I("day"),body=t.S("body"),selfie=t.S("selfie"),review=t.S("review")}).ToList();
            if(o["looks"]!=null)m.looks=o.A("looks").Cast<JObj>().Select(l=>new KeyValuePair<string,string>(l.S("key"),l.S("body"))).ToList();
            m.dayCheck=o.S("dayCheck");return m;
        }
    }
    static class TripRecExt{public static TripRec MemberwiseCloneTrip(this TripRec t){return new TripRec{key=t.key,loc=t.loc,body=t.body,selfie=t.selfie,review=t.review,round=t.round,day=t.day};}}
}
