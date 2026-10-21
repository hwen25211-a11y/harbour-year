using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// 轻松版规则层：倾向排程、自动休整、特别安排、快进停点、目标、独白与旅行照片。
namespace HarbourYear.Lite
{
    public class Stop{public string kind,message,course;public Stop(string k,string m,string c=null){kind=k;message=m;course=c;}}
    public class Advance{public GameState state;public LiteMeta meta;public Stop stop;public string error;public int changes;}
    public class SimDay{public int day,slot,fatigue,stress;}
    public class SimExam{public string course,kind;public int slot,day,fatigue,stress;}
    public class WorkNeed{public string item,kind,course;public int due,need;}
    public class Media{public string asset,src,alt,scope,body;public bool provided;}
    public class Travel{public Media scenery,selfie;public string line;}
    public class Card{public string key,title,memory,cg;public List<string> lines=new List<string>();public int[] delta;}
    public class Memory{public string key,kind,title,text,cg;public int round,day;}
    public class TripMemory{public string key,loc,name,date,text;public int round,day;public bool scenery,selfie,backfilled;}
    public class Goal{public string title,status,next,note,range;public bool done;public int round;public List<string> detail=new List<string>();}
    public class Prompt{public string kind,label,hint;}
    public class GalleryItem{public string asset,kind,src,alt,title;public int day;public List<string> notes=new List<string>();}
    public class Highlight{public string label,text;}
    public class Part{public string component,name,state,text;public int weight;public double? score;}
    public class Standing{public List<Part> parts=new List<Part>();public int covered;public string counted,next,status,headline;public List<string> reasons;public bool attendanceRisk,failed;}
    public class Protection{public bool triggered,resolved=true;public List<int> replaced=new List<int>();public Risk risk;public ConflictInfo conflict;public List<int> restedNecessary=new List<int>();public string pushed;}
    public class Relaxing{public GameState state;public LiteMeta meta;public List<int> changed=new List<int>();public string error;public List<int> confirmSlots;public List<string> confirmItems;}
    public class Summary{public string phase,round,progressText,goalTitle,goalNext,goalStatus,goalNote,attention,monologue;public int done,total;public bool goalDone;}
    public static class Pace
    {
        // ---------- 素材 ----------
        public const string MediaVersion="user-delivery-v1.0";
        static Media M(string asset,string src,string alt,string scope="all"){return new Media{asset=asset,src=src,alt=alt,scope=scope,provided=true};}
        static readonly Media Group=M("CG03","cg/group_discussion_female_young","阳光下的讨论桌，青年女主和两位背景同学一起讨论","female_young");
        public static readonly Dictionary<string,Media> Cg=new Dictionary<string,Media>{
            {"event_red_bulbul",M("CG01","cg/red_bulbul","香港公园枝头的红耳鹎")},{"event_daily_small_win",M("CG02","cg/daily_small_win","窗边书桌上的旧笔记与台灯")},
            {"event_group_outline",Group},{"event_group_delay",Group},{"event_group_credit",Group},{"event_group_rehearsal",Group},{"event_walk_sky",M("CG04","cg/walk_sky","海边被落日染成橘粉色的天空")},
            {"event_walk_shower",new Media{src="cg/walk_shower",alt="街角突然落下的阵雨",scope="all"}},{"event_walk_music",new Media{src="cg/walk_music",alt="广场边的弹唱",scope="all"}},{"event_daily_call",new Media{src="cg/daily_call",alt="睡前亮起的手机",scope="all"}}};
        public static readonly Dictionary<string,Travel> Trips=new Dictionary<string,Travel>{
            {"victoria_harbour",new Travel{scenery=M("TR01","travel/victoria_harbour_scenery","黄昏的维多利亚港，渡轮驶过海面"),line="把海港的晚霞收进今天。",selfie=new Media{asset="SF01",src="selfies/victoria_harbour_female_young",alt="维港海旁，青年女主比着手势自拍",provided=true,body="female_young"}}},
            {"maclehose",new Travel{scenery=M("TR02","travel/maclehose_scenery","麦理浩径的山脊小路与远处的海湾"),line="把山和海一起装进今天。",selfie=new Media{asset="SF02",src="selfies/maclehose_female_young",alt="麦理浩径山路上，戴遮阳帽的青年女主自拍",provided=true,body="female_young"}}}};
        public static Media CgFor(string cg,string body,bool anyBody=false)
        {
            Media n;if(cg==null||!Cg.TryGetValue(cg,out n)||!n.provided)return null;if(!anyBody&&n.scope!="all"&&n.scope!=body)return null;return n;
        }
        public static bool HasScenery(string loc){return loc!=null&&Trips.ContainsKey(loc);}
        static string SelfieKey(string loc){Travel t;return Trips.TryGetValue(loc,out t)&&t.selfie!=null?t.selfie.asset+"@"+MediaVersion:null;}
        // ---------- 旅行与形象记录 ----------
        const string TripPrefix="全天游玩 · ";
        public static string TripOfLog(LogEntry e){if(e.half!="pm"||!e.title.StartsWith(TripPrefix))return null;var t=Cat.Trips.FirstOrDefault(x=>e.title==TripPrefix+x.name);return t!=null?t.id:null;}
        static string TripKey(int round,int day,string loc){return round+":"+day+":"+loc;}
        static int RoundIndexOf(int day){var r=Cal.RoundOfDay(day);return r!=null?r.index:-1;}
        static List<TripRec> AllTrips(GameState e)
        {
            var t=new List<TripRec>();foreach(var n in e.history.Concat(e.logs)){var loc=TripOfLog(n);if(loc==null)continue;int r=RoundIndexOf(n.day);t.Add(new TripRec{key=TripKey(r,n.day,loc),loc=loc,round=r,day=n.day});}return t;
        }
        static LiteMeta RecordTrips(int prevLogs,int prevRound,GameState t,LiteMeta n)
        {
            if(t.logs.Count<=prevLogs&&t.round==prevRound)return n;var r=t.round==prevRound?t.logs.Skip(prevLogs).ToList():t.logs;
            var i=n.trips??new List<TripRec>();var a=new List<TripRec>();
            foreach(var e in r)
            {
                var loc=TripOfLog(e);if(loc==null||!HasScenery(loc))continue;var key=TripKey(t.round,e.day,loc);if(i.Concat(a).Any(x=>x.key==key))continue;
                var o=t.appearance.bodyId;var s=Trips[loc].selfie!=null&&Trips[loc].selfie.body==o?SelfieKey(loc):null;bool c=i.Concat(a).Any(x=>x.loc==loc&&x.round==t.round&&x.review!="silent");
                a.Add(new TripRec{key=key,loc=loc,round=t.round,day=e.day,body=o,selfie=s,review=c?"silent":"pending"});
            }
            if(a.Count==0)return n;var m=n.Clone();m.trips=i.Concat(a).Select(x=>x.MemberwiseCloneTrip()).ToList();
            foreach(var x in a.Where(x=>x.review=="silent"))m.digest.Add(new Digest{round=x.round,day=x.day,text=Cat.TripOf(x.loc).name+"的旅行已记录到手账。"});
            return m;
        }
        public static TripRec PendingTrip(LiteMeta e){return e.trips==null?null:e.trips.FirstOrDefault(x=>x.review=="pending");}
        public static LiteMeta KeepTrip(LiteMeta e,string key)
        {
            if(e.trips==null||!e.trips.Any(x=>x.key==key&&x.review=="pending"))return e;var m=e.Clone();m.trips.First(x=>x.key==key).review="shown";return m;
        }
        public static List<TripMemory> TripMemories(GameState e,LiteMeta t)
        {
            return AllTrips(e).Select(x=>{var n=t.trips==null?null:t.trips.FirstOrDefault(y=>y.key==x.key);var r=Cat.TripOf(x.loc);return new TripMemory{key=x.key,loc=x.loc,name=r.name,round=x.round,day=x.day,date=Cal.Date(x.day),
                text=Trips.ContainsKey(x.loc)?Trips[x.loc].line:r.text,scenery=HasScenery(x.loc),selfie=n!=null&&n.selfie!=null&&n.selfie==SelfieKey(x.loc)&&Trips.ContainsKey(x.loc)&&Trips[x.loc].selfie.body==n.body,backfilled=HasScenery(x.loc)&&n==null};}).ToList();
        }
        public static string EventKey(GameState e){return e.ev!=null?"event:"+e.round+":"+e.ev.day+":"+e.ev.id:null;}
        public static LiteMeta RecordLook(GameState e,LiteMeta t)
        {
            var n=EventKey(e);if(n==null||(t.looks!=null&&t.looks.Any(x=>x.Key==n)))return t;var m=t.Clone();if(m.looks==null)m.looks=new List<KeyValuePair<string,string>>();
            m.looks.Add(new KeyValuePair<string,string>(n,e.appearance.bodyId));if(m.looks.Count>200)m.looks=m.looks.Skip(m.looks.Count-200).ToList();return m;
        }
        public static string LookOf(LiteMeta e,string key){if(e.looks==null)return null;foreach(var x in e.looks)if(x.Key==key)return x.Value;return null;}
        // ---------- 阶段 ----------
        public static int PhaseIndex(int round){var t=round>=0&&round<Cal.Rounds.Length?Cal.Rounds[round]:null;return t==null||t.kind!="semester"?-1:(t.semester==1?0:100)+(t.roundInSemester-1)/2;}
        public static bool PhaseStart(int round){var t=round>=0&&round<Cal.Rounds.Length?Cal.Rounds[round]:null;return t!=null&&t.kind=="semester"&&(t.roundInSemester-1)%2==0;}
        public static string PhaseLabel(int round){int t=PhaseIndex(round);return t<0?"":"第 "+(t%100+1)+" 阶段";}
        public static string PhaseRounds(int round){int t=round-(Cal.Rounds[round].roundInSemester-1)%2;return "第 "+(t+1)+"—"+(t+2)+" 轮";}
        public static string FocusName(string f){return Cat.FocusOf(f).name;}
        // ---------- 预测 ----------
        static string[] ExamAt(GameState e,int t)
        {
            var n=e.plan[t];if(n!=null&&n.StartsWith("exam:")){var x=Cal.SessionAt(e.round,t);return x!=null&&x.kind!="class"?new[]{x.course,x.kind}:null;}
            var r=Core.Makeup(n);var i=r!=null?Cal.Exam(r):null;return i!=null?new[]{i.course,"makeup"}:null;
        }
        public static void Simulate(GameState e,List<SimDay> days,List<SimExam> exams)
        {
            if(e.phase!="planning"&&e.phase!="running")return;var n=e.Clone(false);for(int k=0;k<n.plan.Count;k++)if(n.plan[k]==null)n.plan[k]="rest";
            if(n.phase=="planning")Core.Begin(n);n.notice=null;n.ev=null;n.blockedReason=null;int r=n.plan.Count;
            for(int i=0;i<80&&n.phase=="running"&&n.cursor<r;i++)
            {
                int r2=n.cursor;var x=ExamAt(n,r2);if(x!=null&&exams!=null)exams.Add(new SimExam{course=x[0],kind=x[1],slot=r2,day=Cal.SlotOf(n.round,r2).day,fatigue=n.fatigue,stress=n.stress});
                Core.Step(n);n.notice=null;n.ev=null;
                if(n.blockedReason!=null){if(Core.Trip(n.plan[n.cursor])!=null&&n.cursor%2==0)n.plan[n.cursor+1]="rest";n.plan[n.cursor]="rest";n.blockedReason=null;continue;}
                if(n.cursor>r2&&r2%2==1&&days!=null)days.Add(new SimDay{day=Cal.SlotOf(e.round,r2).day,slot=r2,fatigue=n.fatigue,stress=n.stress});
            }
        }
        // ---------- 倾向排程 ----------
        class Style{public double extraWork;public int review,walks,jobs;public bool trip;}
        static readonly Dictionary<string,Style> Styles=new Dictionary<string,Style>{
            {"academic",new Style{extraWork=double.PositiveInfinity,review=4,walks=1}},{"life",new Style{walks=3,trip=true}},{"work",new Style{walks=1,jobs=6}},{"balanced",new Style{extraWork=8,review=2}}};
        public const int TriggerEnergy=30,TriggerStress=70,TargetEnergy=40,TargetStress=60;
        static int Energy(int fatigue){return 100-fatigue;}
        public static List<int> AutoSlots(GameState e){var t=new List<int>();for(int n=Core.Pos(e);n<e.plan.Count;n++)if(Core.FixedItem(e,e.round,n)==null)t.Add(n);return t;}
        public static List<WorkNeed> WorkNeeds(GameState e)
        {
            var t=new List<WorkNeed>();
            foreach(var n in Cat.Courses)foreach(var r in new[]{"individual","group"})
            {
                if(Core.WorkBlock(e,r,n.id)!=null)continue;var i=Core.WorkOf(e,r,n.id);if(i.status!="active"&&i.status!="overdue")continue;double a=Core.Rate(e,r,n.id);if(a<=0)continue;
                t.Add(new WorkNeed{item=Core.WorkItem(r,n.id),kind=r,course=n.id,due=i.status=="overdue"?Core.LateDue(n.id,r):Core.Due(n.id,r),need=(int)Math.Ceiling((Core.Total(r,n.id)-i.progress)/a)});
            }
            return t.OrderBy(x=>x.due).ThenBy(x=>Cat.CourseIndex(x.course)).ToList();
        }
        static int RoundQuota(GameState e,WorkNeed t)
        {
            int n=Cal.Rounds[e.round].endDay;if(t.due<=n)return t.need;int r=Math.Max(1,AutoSlots(e).Count);double i=(t.due-n)*(10.0/7);return Math.Min(t.need,(int)Math.Ceiling(t.need*1.1*r/(r+i)));
        }
        class Done{public int work,walks,review,trip,jobs;}
        static Done Executed(GameState e)
        {
            var t=e.plan.Take(Core.Pos(e)).ToList();return new Done{work=t.Count(Core.IsWork),walks=t.Count(x=>x=="walk"),review=t.Count(x=>x=="review"),trip=t.Any(x=>Core.Trip(x)!=null)?1:0,jobs=t.Count(x=>Core.JobId(x)!=null)};
        }
        static List<KeyValuePair<string,int>> Quota(GameState e,LiteMeta t)
        {
            var n=t.quotaRound==e.round?new List<KeyValuePair<string,int>>(t.quota):new List<KeyValuePair<string,int>>();
            foreach(var x in WorkNeeds(e))if(!n.Any(k=>k.Key==x.item))n.Add(new KeyValuePair<string,int>(x.item,RoundQuota(e,x)));return n;
        }
        static int? Q(List<KeyValuePair<string,int>> q,string item){foreach(var k in q)if(k.Key==item)return k.Value;return null;}
        static int DoneOf(GameState e,string item){return e.plan.Take(Core.Pos(e)).Count(x=>x==item);}
        static List<string> AutoPlan(GameState e,List<string> plan,List<int> auto,string focus,Done done,List<KeyValuePair<string,int>> quota)
        {
            var o=Styles[focus];var s=new List<string>(plan);int c=Core.Pos(e),l=Cal.Rounds[e.round].endDay;var u=auto.OrderBy(x=>x).ToList();
            Func<Func<int,bool>,int?> d=f=>{foreach(var x in u)if(f(x)){u.Remove(x);return x;}return null;};
            Func<string,int> fixedCount=item=>{int k=0;for(int r=0;r<s.Count;r++)if(r>=c&&!auto.Contains(r)&&s[r]==item)k++;return k;};
            Func<int,bool> weekend=x=>Cal.Weekday(Cal.SlotOf(e.round,x).day)>=5;
            var h=WorkNeeds(e);var g=new Dictionary<string,int>();
            foreach(var t in h.Where(t=>t.kind=="group"&&!e.Proj(t.course).locked)){if(fixedCount(t.item)>0)continue;var x=d(_=>true);if(x==null)break;s[x.Value]=t.item;g[t.item]=1;}
            var need=h.Select(t=>new{w=t,n=Math.Max(0,(t.due<=l?t.need:(Q(quota,t.item)??RoundQuota(e,t))-DoneOf(e,t.item))-fixedCount(t.item)-(g.ContainsKey(t.item)?g[t.item]:0))}).ToList();
            int v=Math.Min(u.Count,need.Sum(t=>t.n));var y=Enumerable.Range(0,v).Select(t=>u[(int)Math.Floor((t+0.5)*u.Count/v)]).ToList();u=u.Where(x=>!y.Contains(x)).ToList();
            int b=0,xw=done.work;foreach(var t in need)for(int k=0;k<t.n&&b<y.Count;k++,b++,xw++)s[y[b]]=t.w.item;
            foreach(var t in h)
            {
                int left=t.need-fixedCount(t.item)-auto.Count(r=>s[r]==t.item);
                while(left>0&&xw<o.extraWork){var x=d(_=>true);if(x==null)break;s[x.Value]=t.item;left--;xw++;}
            }
            if(o.jobs>0&&e.hired.Count>0)
            {
                var best=e.hired.OrderByDescending(j=>Cat.JobOf(j).pay).First();int have=0;for(int r=0;r<s.Count;r++)if(r>=c&&!auto.Contains(r)&&Core.JobId(s[r])!=null)have++;
                for(int k=done.jobs+have;k<Math.Min(o.jobs,6);k++){var x=d(weekend)??d(_=>true);if(x==null)break;s[x.Value]="job:"+best;}
            }
            if(o.trip&&done.trip==0&&!Enumerable.Range(0,s.Count).Any(r=>r>=c&&!auto.Contains(r)&&Core.Trip(s[r])!=null)&&e.money>=Cat.TripCost)
            {
                var opts=u.Where(x=>x%2==0&&u.Contains(x+1)&&weekend(x)).Concat(u.Where(x=>x%2==0&&u.Contains(x+1))).ToList();
                if(opts.Count>0){int t=opts[0];var loc=Cat.Trips[(e.round+t/2)%Cat.Trips.Length].id;s[t]="trip:"+loc;s[t+1]="trip:"+loc;u=u.Where(x=>x!=t&&x!=t+1).ToList();}
            }
            for(int k=done.walks+fixedCount("walk");k<o.walks;k++){var x=d(z=>weekend(z)&&z%2==1)??d(z=>z%2==1)??d(_=>true);if(x!=null)s[x.Value]="walk";}
            for(int k=done.review+fixedCount("review");k<o.review;k++){var x=d(z=>!weekend(z)&&z%2==0)??d(z=>!weekend(z))??d(_=>true);if(x!=null)s[x.Value]="review";}
            foreach(var x in u)s[x]="rest";return s;
        }
        static string ConflictKey(int round,IEnumerable<string> items){return round+":"+string.Join(",",items.Distinct().OrderBy(x=>x,StringComparer.Ordinal));}
        static HashSet<int> Surplus(GameState e,List<string> t,List<int> n,List<KeyValuePair<string,int>> quota)
        {
            var i=new HashSet<int>();int a=Cal.Rounds[e.round].endDay;
            foreach(var o in WorkNeeds(e))
            {
                var s=n.Where(x=>t[x]==o.item).OrderBy(x=>x).ToList();int c=o.due<=a?o.need:(Q(quota,o.item)??RoundQuota(e,o))-DoneOf(e,o.item);
                int fixedN=0;for(int r=Core.Pos(e);r<t.Count;r++)if(!n.Contains(r)&&t[r]==o.item)fixedN++;int l=Math.Max(0,c-fixedN);foreach(var x in s.Skip(l))i.Add(x);
            }
            return i;
        }
        static int Priority(GameState e,string t,int n,HashSet<int> surplus)
        {
            if(Core.JobId(t)!=null)return 0;if(t=="review")return 1;if(Core.IsWork(t)&&surplus.Contains(n))return 2;
            if(Core.IsWork(t)){var x=WorkNeeds(e).FirstOrDefault(w=>w.item==t);return x!=null&&x.due-Core.Today(e)>7?3:-1;}
            return t=="walk"?4:Core.Trip(t)!=null?5:-1;
        }
        static List<string> Protect(GameState e,LiteMeta meta,List<string> plan,List<int> r,List<KeyValuePair<string,int>> quota,Protection a)
        {
            var i=new List<string>(plan);int o=Core.Today(e);var probe=e.Clone(false);
            for(int it=0;it<40;it++)
            {
                probe.plan=i;var days=new List<SimDay>();Simulate(probe,days,null);
                if(!a.triggered){if(!days.Any(x=>Energy(x.fatigue)<TriggerEnergy||x.stress>TriggerStress))return i;a.triggered=true;}
                var s=days.Where(x=>Energy(x.fatigue)<TargetEnergy||x.stress>TargetStress).ToList();
                if(s.Count==0){a.risk=null;a.resolved=true;return i;}
                a.risk=new Risk{day=s[0].day,energy=Energy(s[0].fatigue),stress=s[0].stress};a.resolved=false;
                var c=Surplus(probe,i,r,quota);int? l=null;
                foreach(var t in s){var cand=r.Where(n=>n<=t.slot&&i[n]!="rest"&&Priority(probe,i[n],n,c)>=0).OrderBy(n=>Priority(probe,i[n],n,c)).ThenByDescending(n=>n).ToList();if(cand.Count>0){l=cand[0];break;}}
                if(l!=null)
                {
                    int x=l.Value;if(Core.Trip(i[x])!=null){int d0=x-x%2;i[d0]="rest";i[d0+1]="rest";a.replaced.Add(d0);a.replaced.Add(d0+1);}else{i[x]="rest";a.replaced.Add(x);}continue;
                }
                var u=s[s.Count-1];var needs=WorkNeeds(probe);
                var d=r.Where(x=>x<=u.slot&&Core.IsWork(i[x])).Where(x=>{var w=needs.FirstOrDefault(y=>y.item==i[x]);return w!=null&&w.due-o<=7;}).ToList();
                if(d.Count==0)return i;
                var f=needs.Where(w=>d.Any(x=>i[x]==w.item)).Select(w=>new ConflictItem{item=w.item,course=w.course,kind=w.kind,due=w.due,halves=d.Where(x=>i[x]==w.item).ToList()}).ToList();
                var p=ConflictKey(e.round,f.Select(x=>x.item));string m=null;foreach(var k in meta.conflicts)if(k.Key==p){m=k.Value;break;}
                if(m=="rest"){int x=d[d.Count-1];i[x]="rest";a.replaced.Add(x);a.restedNecessary.Add(x);continue;}
                if(m=="push")a.pushed=p;else a.conflict=new ConflictInfo{key=p,items=f,day=s[0].day,energy=Energy(s[0].fatigue),stress=s[0].stress};
                return i;
            }
            return i;
        }
        static LiteMeta WithHealth(LiteMeta e,Protection t){e.health=new Health{triggered=t.triggered,resolved=t.resolved,rested=t.replaced.Count,risk=t.risk};e.conflict=t.conflict;return e;}
        class Planned{public List<string> plan;public Protection info;public List<KeyValuePair<string,int>> quota;}
        static Planned Make(GameState e,LiteMeta t,List<string> n,List<int> r,string focus,Done done)
        {
            var q=Quota(e,t);var s=AutoPlan(e,n,r,focus,done,q);var info=new Protection();var p=Protect(e,t,s,r,q,info);return new Planned{plan=p,info=info,quota=q};
        }
        public static Advance NewRound(GameState e,LiteMeta t,string focus=null)
        {
            focus=focus??t.focus;var r=Enumerable.Range(0,e.plan.Count).Select(n=>Core.FixedItem(e,e.round,n)).ToList();var i=AutoSlots(e);var a=Make(e,t,r,i,focus,new Done());
            var m=t.Clone();m.focus=focus;m.round=e.round;m.free=new List<int>();m.auto=i;m.customized=false;m.relaxed=new List<int>();m.quotaRound=e.round;m.quota=a.quota;
            return new Advance{state=Core.ApplyPlan(e,a.plan),meta=WithHealth(m,a.info)};
        }
        public class Change{public int index;public string from,to;}
        public static Advance Replan(GameState e,LiteMeta t,string focus,out List<Change> changes)
        {
            focus=focus??t.focus;var r=t.auto.Where(x=>x>=Core.Pos(e)&&Core.FixedItem(e,e.round,x)==null).ToList();
            var i=Make(e,t,e.plan.Select((x,k)=>r.Contains(k)?null:x).ToList(),r,focus,Executed(e));
            changes=r.Where(x=>i.plan[x]!=e.plan[x]).Select(x=>new Change{index=x,from=e.plan[x],to=i.plan[x]}).ToList();
            var m=t.Clone();m.focus=focus;m.quotaRound=e.round;m.quota=i.quota;return new Advance{state=changes.Count>0?Core.ApplyPlan(e,i.plan):e,meta=WithHealth(m,i.info),changes=changes.Count};
        }
        public static Advance Replan(GameState e,LiteMeta t,string focus=null){List<Change> c;return Replan(e,t,focus,out c);}
        public static Advance ResolveConflict(GameState e,LiteMeta t,string key,string choice)
        {
            var m=t.Clone();m.conflicts.RemoveAll(x=>x.Key==key);m.conflicts.Add(new KeyValuePair<string,string>(key,choice));return Replan(e,m);
        }
        public static Advance AddSpecial(GameState e,LiteMeta t,int n,string item)
        {
            if(n<Core.Pos(e)||Core.FixedItem(e,e.round,n)!=null)return new Advance{state=e,meta=t,error="这个半日已经过去或有固定课程／考试，不能作为特别安排。"};
            if(!t.free.Contains(n)&&t.free.Count>=2)return new Advance{state=e,meta=t,error="特别安排最多 2 个半日。"};
            if(Core.Trip(item)!=null||item==null)return new Advance{state=e,meta=t,error="全天游玩要占同一天两个半日，请在“详细安排”中安排。"};
            var i=Core.SetSlot(e,n,item);if(i.error!=null)return new Advance{state=e,meta=t,error=i.error};
            var a=t.Clone();if(!a.free.Contains(n))a.free.Add(n);a.free.Sort();a.auto.Remove(n);a.relaxed.Remove(n);return Replan(Core.ApplyPlan(e,i.plan),a);
        }
        public static Advance RemoveSpecial(GameState e,LiteMeta t,int n)
        {
            if(!t.free.Contains(n)||n<Core.Pos(e))return new Advance{state=e,meta=t,error="这个特别安排已经执行，不能取消。"};
            var a=t.Clone();a.free.Remove(n);a.auto.Add(n);a.auto.Sort();return Replan(e,a);
        }
        public static List<string> SpecialOptions(GameState e)
        {
            var t=new List<string>{"walk","rest","review"};t.AddRange(WorkNeeds(e).Select(x=>x.item));t.AddRange(e.hired.Select(x=>"job:"+x));return t.Distinct().ToList();
        }
        public static Relaxing Relax(GameState e,LiteMeta t,bool force=false)
        {
            int r=Core.Pos(e),lim=Math.Min(e.plan.Count,(r/2+2)*2);var a=t.auto.Where(x=>x>=r&&x<lim&&Core.FixedItem(e,e.round,x)==null).OrderBy(x=>x).ToList();
            var o=Surplus(e,e.plan,t.auto.Where(x=>x>=r).ToList(),Quota(e,t));
            var s=a.Where(x=>e.plan[x]!="rest"&&Priority(e,e.plan[x],x,o)>=0&&Priority(e,e.plan[x],x,o)<=3).Take(4).ToList();
            var c=a.Where(x=>Core.IsWork(e.plan[x])&&Priority(e,e.plan[x],x,o)<0).ToList();
            if(s.Count==0&&c.Count>0&&!force)return new Relaxing{state=e,meta=t,confirmSlots=c.Take(4).ToList(),confirmItems=c.Select(x=>e.plan[x]).Distinct().ToList()};
            if(s.Count==0&&force)s=c.Take(4).ToList();
            if(s.Count==0)return new Relaxing{state=e,meta=t,error="接下来两天没有可以改成休息的半日（已经在休息，或者是课程、考试、特别安排和你亲手安排的事）。"};
            var l=new List<string>(e.plan);foreach(var x in s)l[x]="rest";var u=t.Clone();u.auto.RemoveAll(s.Contains);foreach(var x in s)if(!u.relaxed.Contains(x))u.relaxed.Add(x);
            var d=Replan(Core.ApplyPlan(e,l),u);return new Relaxing{state=d.state,meta=d.meta,changed=s};
        }
        public static LiteMeta Customized(GameState before,GameState after,LiteMeta n)
        {
            var r=n.auto.Where(x=>before.plan[x]!=after.plan[x]).ToList();if(r.Count==0)return n;var m=n.Clone();m.auto.RemoveAll(r.Contains);m.customized=true;return m;
        }
        public static List<int> SlotsOf(GameState e,string item){var n=new List<int>();for(int r=Core.Pos(e);r<e.plan.Count;r++)if(e.plan[r]==item)n.Add(r);return n;}
        public static int[] FindTrip(GameState e)
        {
            for(int t=Core.Pos(e);t<e.plan.Count-1;t++){if(t%2!=0)continue;var n=Core.Trip(e.plan[t]);if(n!=null&&e.plan[t+1]==e.plan[t])return new[]{t};}return null;
        }
        public static Advance ChangeTrip(GameState e,LiteMeta t,int am,string loc)
        {
            var a=FindTrip(e);if(a==null||a[0]!=am)return new Advance{state=e,meta=t,error="这一天没有尚未出发的全天游玩。"};if(Core.Trip(e.plan[am])==loc)return new Advance{state=e,meta=t,error="已经是这个地点。"};
            var o=Core.SetSlot(e,am,"trip:"+loc);if(o.error!=null)return new Advance{state=e,meta=t,error=o.error};var s=Core.ApplyPlan(e,o.plan);return new Advance{state=s,meta=Customized(e,s,t)};
        }
        // ---------- 显示用文字 ----------
        public static int EnergyOf(int fatigue){return Math.Max(0,Math.Min(100,100-fatigue));}
        public static int Neg(int e){return e==0?0:-e;}
        public static string Signed(int e){return e>0?"+"+e:e<0?"−"+(-e):"±0";}
        public const int SickEnergy=100-Cat.Illness,SickStress=Cat.Illness,ExamEnergy=40;
        public static string[] EnergyLevel(int fatigue){int t=EnergyOf(fatigue);return t<=10?new[]{"优先休整","risk"}:t<=20?new[]{"需要休息","risk"}:t<40?new[]{"有些疲惫","low"}:t<70?new[]{"状态尚可","ok"}:new[]{"精力充沛","ok"};}
        public static string[] StressLevel(int e){return e>=90?new[]{"优先休整","risk"}:e>=80?new[]{"需要放松","risk"}:e>60?new[]{"有点紧绷","low"}:e>=30?new[]{"平稳","ok"}:new[]{"轻松","ok"};}
        public static string Delta(int fatigue,int stress,int money=0,int affection=0,int reputation=0,string name="")
        {
            var p=new List<string>();if(fatigue!=0)p.Add("精力 "+Signed(Neg(fatigue)));if(stress!=0)p.Add("压力 "+Signed(stress));if(money!=0)p.Add("HK$ "+Signed(money));
            if(affection!=0)p.Add(name+"好感 "+Signed(affection));if(reputation!=0)p.Add("声望 "+Signed(reputation));return string.Join("、",p);
        }
        static readonly Regex Strip=new Regex("[：:，,]?\\s*疲劳 [+−-]\\d+(?:、压力 [+−-]\\d+)?");
        public static string Clean(string e){return Strip.Replace(e,"").Replace("。。","。");}
        public static LogEntry LastLog(GameState e){return e.logs.Count>0?e.logs[e.logs.Count-1]:e.history.Count>0?e.history[e.history.Count-1]:null;}
        public static string Changes(LogEntry e){return "现金 "+Signed(e.money)+" · 精力 "+Signed(Neg(e.fatigue))+" · 压力 "+Signed(e.stress);}
        // ---------- 课程成绩 ----------
        static Part WorkPart(GameState e,string course,string kind,int w)
        {
            var i=Core.WorkOf(e,kind,course);int a=Core.Total(kind,course);var o=new Part{component=kind,name=Core.PartName(kind),weight=w};var s=Cal.Date(Core.Due(course,kind));
            switch(i.status)
            {
                case "unreleased":o.state="unreleased";o.text="尚未发布（第 "+Cat.Load(course,kind).releaseWeek+" 教学周课堂上发布）";break;
                case "active":o.state="working";o.text="进行中 · 进度 "+Fmt.N(i.progress)+"/"+a+" · "+s+" 截止 · 待评估";break;
                case "complete":{var l=Core.WorkLetter(i);o.state="complete";o.text="已完成、未提交 · 作品质量 "+Fmt.N(i.quality.Value)+(l!=null?"（预计 "+Fmt.Letter(l)+"）":"")+"，提交后才计分，还不是课程成绩";break;}
                case "overdue":o.state="overdue";o.text="已过截止，补交中 · 进度 "+Fmt.N(i.progress)+"/"+a+" · "+Cal.Date(Core.LateDue(course,kind))+" 前完成按 ×0.8 计分，否则 0 分";break;
                case "missing":o.state="zero";o.score=0;o.text="缺交 · 0 分，按原权重 "+w+"% 计入";break;
                default:if(i.score==null){o.state="unknown";o.text="旧档作品，分数无法还原";}else{o.state="scored";o.score=i.score;o.text=(i.late?"迟交":"按时")+"提交 · 得分 "+Fmt.N(i.score.Value);}break;
            }
            return o;
        }
        static Part ExamPart(GameState e,string course,string kind,int w)
        {
            var i=Cal.ExamOf(course,kind);var a=e.Exam(i.id);var o=new Part{component=kind,name=Core.PartName(kind),weight=w};
            if(a==null){o.state="scheduled";o.text=Cal.Date(i.day)+Cal.Half(i.half)+"考试 · 待评估";}
            else if(a.legacy){o.state="unknown";o.text="旧档场次，考前状态没有记录，分数无法还原";}
            else if(a.status=="taken"){o.state="scored";o.score=a.score;o.text="得分 "+Fmt.N(a.score.Value);}
            else if(a.status=="missed"){o.state="zero";o.score=0;o.text="无故缺考 · 0 分，按原权重 "+w+"% 计入";}
            else if(a.makeup=="pending"){var m=Core.MakeupOf(i.id);o.state="makeup";o.text="病假缺考，待补考（"+Cal.Date(m.day)+Cal.Half(m.half)+"），不按 0 分处理";}
            else if(a.makeup=="taken"){o.state="scored";o.score=a.score;o.text="病假后补考 · 得分 "+Fmt.N(a.score.Value);}
            else{o.state="zero";o.score=0;o.text="病假后未参加补考 · 0 分，按原权重 "+w+"% 计入";}
            return o;
        }
        static readonly string[] NextOrder={"overdue","makeup","complete","working","scheduled","unreleased"};
        public static Standing CourseStanding(GameState e,string course)
        {
            var n=Core.Grade(e,course);var s=new Standing{reasons=n.reasons};
            for(int i=0;i<n.keys.Length;i++){var k=n.keys[i];s.parts.Add(k=="individual"||k=="group"?WorkPart(e,course,k,n.weights[i]):ExamPart(e,course,k,n.weights[i]));}
            s.covered=s.parts.Where(p=>p.score!=null).Sum(p=>p.weight);s.counted=Fmt.Floor1(n.counted);
            var o=NextOrder.Select(st=>s.parts.FirstOrDefault(p=>p.state==st)).FirstOrDefault(p=>p!=null);
            s.next=n.status=="in_progress"&&o!=null?o.name+"："+o.text:null;s.attendanceRisk=n.status=="in_progress"&&n.attendanceRisk;
            if(n.status=="official"){s.status="official";s.failed=!n.passed;s.headline="正式成绩 "+(n.total==null?"—":Fmt.Floor1(n.total.Value))+"（"+Fmt.Letter(n.result.letter)+"，绩点 "+Fmt.F2(n.result.points100/100.0)+"）";}
            else if(n.status=="unknown"){s.status="unknown";s.headline="无法评估：有旧档分数无法还原";}
            else if(s.covered==0){s.status="none";s.headline="尚未评估";}
            else{s.status="partial";s.headline="已评定部分 · 覆盖课程权重 "+s.covered+"%"+(s.covered==100?"（正式成绩在学期结束后公布）":"");}
            return s;
        }
        // ---------- 需要留意 ----------
        public static List<string> Attention(GameState e)
        {
            var t=new List<string>();int n=Core.Today(e);int sem=Cal.Rounds[e.round].semester;
            foreach(var r in Cat.Courses)
            {
                foreach(var i in new[]{"individual","group"})
                {
                    var a=Core.WorkOf(e,i,r.id);string o=i=="individual"?"「"+r.assignment+"」":"「"+r.name+"」小组项目";
                    if(a.status=="overdue")t.Add(o+"已过截止："+Cal.Date(Core.LateDue(r.id,i))+" 前完成按 ×0.8 计分，否则 0 分（进度 "+Fmt.N(a.progress)+"/"+Core.Total(i,r.id)+"）。");
                    else if(a.status=="active"&&Core.Due(r.id,i)-n<=7)t.Add(o+Cal.Date(Core.Due(r.id,i))+" 截止，进度 "+Fmt.N(a.progress)+"/"+Core.Total(i,r.id)+"。");
                }
                foreach(var k in new[]{"midterm","final"})
                {
                    var x=Cal.ExamOf(r.id,k);var a=x!=null?e.Exam(x.id):null;if(x!=null&&a!=null&&a.makeup=="pending"){var m=Core.MakeupOf(x.id);t.Add(r.name+Core.PartName(k)+"待补考："+Cal.Date(m.day)+Cal.Half(m.half)+"（改成其他安排会记为缺考）。");}
                }
            }
            foreach(var c in Cat.Courses.Where(c=>c.semester==sem)){var g=Core.Grade(e,c.id);if(g.status=="in_progress"&&g.attendanceRisk)t.Add(c.name+"出勤率目前低于 70%：学期结束时仍不足会强制 F。");}
            int absent=e.logs.Count(x=>x.text.Contains("记为无故缺勤")),missed=e.logs.Count(x=>x.text.Contains("无故缺考")||x.text.Contains("记为缺考"));
            if(absent>0)t.Add("本轮有 "+absent+" 次课堂记为无故缺勤。");if(missed>0)t.Add("本轮有 "+missed+" 场考试记为缺考（0 分）。");
            int sick=e.logs.Count(x=>x.title=="获批病假 · 在家休养");if(sick>0)t.Add("本轮因病休养 "+sick+" 天（医疗费各 HK$"+Cat.Medical+"）。");
            int c0=EnergyOf(e.fatigue);
            if(c0<=SickEnergy)t.Add("精力 "+c0+"：若下一天开始时仍在 "+SickEnergy+" 或以下（即疲劳 ≥ "+Cat.Illness+"），将安排一整天病假。");else if(c0<=20)t.Add("精力 "+c0+"，需要休息。");
            if(e.stress>=SickStress)t.Add("压力 "+e.stress+"：若下一天开始时仍在 "+SickStress+" 或以上，将安排一整天病假。");else if(e.stress>=80)t.Add("压力 "+e.stress+"，需要放松。");
            t.AddRange(ExamWarnings(e));return t;
        }
        static List<string> ExamWarnings(GameState e)
        {
            if(e.phase!="planning"&&e.phase!="running")return new List<string>();int t=Core.Today(e),n=e.phase=="planning"?0:e.cursor;
            bool soon=Enumerable.Range(0,e.plan.Count).Any(i=>i>=n&&e.plan[i]!=null&&(e.plan[i].StartsWith("exam:")||e.plan[i].StartsWith("makeup:"))&&Cal.Rounds[e.round].startDay+i/2<=t+2);
            if(!soon)return new List<string>();var exams=new List<SimExam>();Simulate(e,null,exams);
            return exams.Where(x=>x.day<=t+2&&(x.fatigue>Cat.ExamPenaltyFrom||x.stress>Cat.ExamPenaltyFrom)).Select(x=>{var c=Cat.CourseOf(x.course);var k=x.kind=="makeup"?"补考":Core.PartName(x.kind);
                return "预测："+Cal.Date(x.day)+Cal.Half(x.slot%2==0?"am":"pm")+c.name+k+"开考前精力约 "+EnergyOf(x.fatigue)+"、压力约 "+x.stress+"。精力低于 "+ExamEnergy+" 或压力高于 "+Fmt.N(Cat.ExamPenaltyFrom)+" 的部分会影响分数（以开考前的实际状态为准），可以考虑考前多休息。";}).ToList();
        }
        // ---------- 回忆与结果 ----------
        public static bool SampleEnded(GameState e){return e.round>=3&&(e.phase=="summary"||e.phase=="finished");}
        static List<LogEntry> AllLogs(GameState e){return e.history.Concat(e.logs).ToList();}
        static string MemKey(EventRec e){return "event:"+e.round+":"+e.day+":"+e.id;}
        static string TeamKey(string course){return "team:"+course;}
        public static List<Memory> Memories(GameState e)
        {
            var t=new List<Memory>();
            foreach(var n in e.events)
            {
                if(n.choice==null)continue;var r=Cat.EventOf(n.id);if(r==null)continue;var who=n.npc!=null?(e.Npc(n.npc)!=null?e.Npc(n.npc).name:"组员"):"";var a=r.options[n.choice.Value];
                t.Add(new Memory{key=MemKey(n),round=n.round,day=n.day,kind=r.trigger,title=Core.Who(r.title,who),text=Core.Who(a.label,who)+"——"+Core.Who(a.result,who),cg=r.cg});
            }
            foreach(var n in Cat.Courses)
            {
                if(!e.Proj(n.id).locked)continue;var r=AllLogs(e).FirstOrDefault(x=>x.title=="小组作业 · "+n.name);int i=r!=null?Cal.RoundOfDay(r.day).index:e.round;
                t.Add(new Memory{key=TeamKey(n.id),round=i,day=r!=null?r.day:0,kind="team",title="和同学开始「"+n.name+"」小组项目",text=TeamText(e,n.id)});
            }
            return t.OrderBy(x=>x.day).ToList();
        }
        static string TeamText(GameState e,string course)
        {
            var n=e.Proj(course);var r=Cat.Roles.Select(x=>{var i=n.roles[x];return (i=="self"?"你":(e.Npc(i)!=null?e.Npc(i).name:"组员"))+"负责"+Core.RoleName(course,x);});
            return string.Join("、",n.members.Select(x=>e.Npc(x).name))+"和你一起开工："+string.Join("，",r)+"。目前进度 "+Fmt.N(n.progress)+"。";
        }
        public static Card PendingCard(GameState e,LiteMeta t)
        {
            foreach(var n in Memories(e))
            {
                if(t.seen.Contains(n.key))continue;
                if(n.kind=="team")return new Card{key=n.key,title="合作开始了",lines=new List<string>{n.text},memory=n.title};
                var r=e.events.First(x=>MemKey(x)==n.key);var i=Cat.EventOf(r.id).options[r.choice.Value];var a=AllLogs(e).FirstOrDefault(x=>x.day==r.day&&x.text.Contains("【"+n.title));
                var o=Delta(i.fatigue,i.stress,i.money,i.affection,i.reputation);
                return new Card{key=n.key,title=n.title,lines=new List<string>{n.text,"这个选项本身："+(o==""?"没有数值变化":o)+"（叠加在这个半日的活动上）。"},memory=n.title,delta=a!=null?new[]{a.money,a.fatigue,a.stress}:null,cg=n.cg};
            }
            return null;
        }
        public static LiteMeta MarkSeen(LiteMeta e,string key){if(e.seen.Contains(key))return e;var m=e.Clone();m.seen.Add(key);return m;}
        // ---------- 目标 ----------
        public static string FirstProjectCourse(GameState e)
        {
            int sem=Cal.Rounds[e.round].semester;if(sem==0)sem=1;var t=Cat.Courses.Where(c=>c.semester==sem).ToList();
            var r=t.Where(c=>e.Proj(c.id).released!=null).OrderBy(c=>e.Proj(c.id).released).FirstOrDefault();return (r??t[0]).id;
        }
        static string When(GameState e,int t){return Cal.Date(Cal.Rounds[e.round].startDay+t/2)+Cal.Half(t%2==0?"am":"pm");}
        public static string SlotName(GameState e,int t){return When(e,t);}
        static readonly string[] Titles={"在香港走一走，留下第一份生活回忆","找到合作伙伴，开始第一份小组任务","把第一份个人作业按时交出去","参加期中考试，小组项目继续推进"};
        public static readonly Dictionary<string,string> GoalLabels=new Dictionary<string,string>{{"done","已达成"},{"current","进行中"},{"missed","本轮未达成"},{"pending","待补考"},{"upcoming","之后的轮次"},{"unknown","记录不足，无法判断"}};
        public static Goal CurrentGoal(GameState e){return e.round<4?GoalFor(e,e.round):new Goal{title="照自己的节奏过好这两周",status="current",next="按本轮安排继续。"};}
        static Goal G(string title,string status,string next,string note=null){return new Goal{title=title,status=status,done=status=="done",next=next,note=note};}
        public static Goal GoalFor(GameState e,int t)
        {
            string n=Titles[t];if(t>e.round)return G(n,"upcoming","");int i=Cal.Rounds[t].endDay;bool a=t<e.round||e.phase=="summary"||e.phase=="finished";
            Func<bool,string,string,string,string,Goal> o=(ok,doneText,missText,note,cur)=>ok?G(n,"done",doneText):a?G(n,"missed",missText,note):G(n,"current",cur);
            if(t==0)
            {
                var m=Memories(e).FirstOrDefault(x=>x.kind=="walk");bool r=m!=null&&m.day<=i;var walks=t==e.round?SlotsOf(e,"walk"):new List<int>();
                string s=e.firstWalkDone?"散步途中遇到了一件事，选择后会记下回忆。":walks.Count>0?When(e,walks[0])+"外出散步（HK$45，精力 +4、压力 −8）。":"本轮还没有安排散步：在“特别安排”里加一次“外出散步”，就会出门走走；不去也可以，不会凭空记下回忆。";
                return o(r,"已留下回忆：「"+(m!=null?m.title:"")+"」。","这一轮没有出门散步，没有留下第一份生活回忆。",m!=null?"后来在 "+Cal.Date(m.day)+" 留下了回忆：「"+m.title+"」。":null,s);
            }
            return t==1?TeamGoal(e,i,a,n):t==2?SubmitGoal(e,i,a,n):ExamGoal(e,i,a,n);
        }
        static Goal TeamGoal(GameState e,int end,bool past,string n)
        {
            var days=Cat.Courses.Where(c=>e.Proj(c.id).locked).Select(c=>{var lg=AllLogs(e).FirstOrDefault(x=>x.title=="小组作业 · "+c.name);return lg!=null?(int?)lg.day:null;}).ToList();
            bool any=days.Count>0;int? day=days.Any(x=>x==null)?null:any?days.Min():null;var a=Cat.Courses.FirstOrDefault(c=>e.Proj(c.id).locked);
            if(any&&day==null&&past)return G(n,"unknown","旧存档没有记录小组作业开始的日期，无法判断是否在这一轮开始。");
            if(any&&(day==null||day<=end))return G(n,"done","已和 "+string.Join("、",e.Proj(a.id).members.Select(x=>e.Npc(x).name))+" 开始「"+a.name+"」小组项目。");
            if(past)return G(n,"missed","这一轮没有开始小组作业。",any?"后来在 "+Cal.Date(day.Value)+" 开始了小组作业。":null);
            var o=FirstProjectCourse(e);var s=e.Proj(o);var c0=Cat.CourseOf(o);
            if(s.status=="unreleased")
            {
                int t=Cat.Load(o,"group").releaseWeek,sem=Cal.Rounds[e.round].semester;if(sem==0)sem=1;
                var g=G(n,"current","小组项目在第 "+t+" 教学周的课堂上发布（本轮后半，最早是"+c0.name+"，约 "+Cal.Date(Cat.WeekEnd(o,t)-6)+"）。发布前不能提前开工。");g.detail.Add("第 "+sem+" 学期各课的个人作业在第 3 周、小组项目在第 4 周按课表发布。");return g;
            }
            if(!Core.TeamReady(s))return G(n,"current","「"+c0.name+"」小组项目已发布：接受默认队伍，或到“小组项目”手动邀请同学。");
            var l=SlotsOf(e,Core.ProjectItem(o));
            return G(n,"current",l.Count>0?"已安排 "+l.Count+" 个小组作业半日，第一次在 "+When(e,l[0])+"（当前进度 "+Fmt.N(s.progress)+"）。":"队伍已组好，但本轮剩下的半日里还没有小组作业：请在特别安排或“详细安排”中放一个“小组作业”。");
        }
        static bool OnTime(GameState e,string c){var n=e.Task(c);return n.status=="submitted"&&!n.late&&n.submittedDay!=null&&n.submittedDay<=Core.Due(c,"individual");}
        static Goal SubmitGoal(GameState e,int end,bool past,string n)
        {
            var i=Cal.Rounds[2];var a=Cat.Courses.Where(c=>c.semester==i.semester&&Core.Due(c.id,"individual")<=end).ToList();
            if(a.Count==0)return G(n,"unknown","这一轮没有截止的个人作业。");
            Func<Course,string> o=c=>{var x=e.Task(c.id);return OnTime(e,c.id)?"「"+c.assignment+"」按时提交":x.status=="submitted"?"「"+c.assignment+"」"+Cal.Date(x.submittedDay.Value)+"迟交补交":x.status=="missing"?"「"+c.assignment+"」缺交":"「"+c.assignment+"」未按时提交";};
            if(a.All(c=>OnTime(e,c.id)))return G(n,"done",string.Join("；",a.Select(o))+"。");
            if(past)
            {
                var later=a.Where(c=>!OnTime(e,c.id)&&e.Task(c.id).status=="submitted"&&e.Task(c.id).submittedDay>end).Select(c=>"「"+c.assignment+"」后来在 "+Cal.Date(e.Task(c.id).submittedDay.Value)+" 迟交补交（不算按时）").ToList();
                var miss=a.Where(c=>!OnTime(e,c.id)).Select(c=>{var r=e.Task(c.id);return r.status=="submitted"&&r.submittedDay<=end?"「"+c.assignment+"」"+Cal.Date(r.submittedDay.Value)+"已补交（迟交）":"「"+c.assignment+"」本轮结束时仍未提交";});
                return G(n,"missed",string.Join("；",a.Where(c=>OnTime(e,c.id)).Select(o).Concat(miss))+"。",later.Count>0?string.Join("；",later)+"。":null);
            }
            var s=a.First(c=>!OnTime(e,c.id));var w=e.Task(s.id);
            return G(n,"current",w.status=="complete"?"「"+s.assignment+"」已完成，"+Cal.Date(Core.Due(s.id,"individual"))+" 截止时自动提交（也可以先打磨或手动提交）。":w.status=="overdue"?"「"+s.assignment+"」已过截止，进入补交期（完成后按 ×0.8 计分，不算按时）。":w.status=="submitted"?"「"+s.assignment+"」已迟交补交，不算按时。":"「"+s.assignment+"」进度 "+Fmt.N(w.progress)+"/"+Core.Total("individual",s.id)+"，"+Cal.Date(Core.Due(s.id,"individual"))+" 截止；已安排 "+SlotsOf(e,Core.TaskItem(s.id)).Count+" 个作业半日。");
        }
        static Goal ExamGoal(GameState e,int end,bool past,string n)
        {
            var i=Cal.Rounds[3];var a=Cat.Courses.Select(c=>new{c,x=Cal.ExamOf(c.id,"midterm")}).Where(p=>p.x!=null&&p.x.day>=i.startDay&&p.x.day<=end).ToList();
            if(a.Count==0)return G(n,"unknown","这一轮没有期中考试。");
            Func<string,ExamRec> o=id=>e.Exam(id);
            Func<Course,Session,string> s=(c,x)=>{var r=o(x.id);return r==null?c.name+"期中在 "+Cal.Date(x.day)+Cal.Half(x.half):r.status=="taken"?c.name+"期中得分 "+Fmt.N(r.score.Value):r.status=="excused"?(r.makeup=="taken"?c.name+"期中病假后补考完成":r.makeup=="missed"?c.name+"期中病假后补考缺考":c.name+"期中病假，待补考"):c.name+"期中缺考（0 分）";};
            Func<string,bool> ok=id=>o(id)!=null&&(o(id).status=="taken"||(o(id).status=="excused"&&o(id).makeup=="taken"));
            string l=string.Join("；",a.Select(p=>s(p.c,p.x)))+"。";
            if(a.All(p=>ok(p.x.id)))return G(n,"done",l);
            if(a.Any(p=>o(p.x.id)!=null&&(o(p.x.id).status=="missed"||o(p.x.id).makeup=="missed")))return G(n,past?"missed":"current",l);
            if(a.Any(p=>o(p.x.id)!=null&&o(p.x.id).makeup=="pending"))return G(n,past||a.All(p=>o(p.x.id)!=null)?"pending":"current",l);
            if(past)return G(n,"unknown",l);
            var u=a.First(p=>o(p.x.id)==null);var taken=a.Where(p=>o(p.x.id)!=null).ToList();
            return G(n,"current",Cal.Date(u.x.day)+Cal.Half(u.x.half)+"是"+u.c.name+"期中考试；考试按开考前的真实精力和压力计分。"+(taken.Count>0?"已考："+string.Join("；",taken.Select(p=>s(p.c,p.x)))+"。":""));
        }
        public static List<Goal> Goals(GameState e)
        {
            return Enumerable.Range(0,4).Select(n=>{var r=GoalFor(e,n);r.round=n;r.next=r.status=="upcoming"?null:r.next;r.range=Cal.Date(Cal.Rounds[n].startDay)+"—"+Cal.Date(Cal.Rounds[n].endDay);return r;}).ToList();
        }
        // ---------- 独白与回顾 ----------
        static readonly Dictionary<string,string> Lines=new Dictionary<string,string>{{"review","这段内容，终于不像刚打开时那么陌生了。"},{"rest","今天给自己留了一点空白。"},{"tired","先缓一缓，明天再继续。"},{"team","有人一起分担，事情也有了眉目。"},{"walk","走出门，这座城市好像近了一点。"},{"job","这里的第一份工资到手了，日子有了实在的分量。"}};
        static readonly Regex ReviewOf=new Regex("复习(.+?)，");
        public static string[] Monologue(GameState e,LiteMeta t)
        {
            string n=null;foreach(var m in t.monologues)if(m.Key==e.round-1){n=m.Value;break;}
            int r=e.startFatigue;bool i=e.logs.Aggregate(r,(x,l)=>Math.Max(0,Math.Min(100,x+l.fatigue)))==e.fatigue;var a=new Dictionary<string,int>();var o=e.history;
            for(int idx=0;idx<e.logs.Count;idx++)
            {
                var l=e.logs[idx];r=Math.Max(0,Math.Min(100,r+l.fatigue));var s=new List<string>();
                if(l.title=="预习与复习"){var mm=ReviewOf.Match(l.text);var k=mm.Success?mm.Groups[1].Value:"";a[k]=(a.ContainsKey(k)?a[k]:0)+1;if(a[k]==2)s.Add("review");}
                if(l.title=="在家休息"&&-l.fatigue>=5)s.Add("rest");
                if(l.title.StartsWith("获批病假")||(i&&100-r<30))s.Add("tired");
                if(l.title.StartsWith("小组作业 ·")&&!o.Any(x=>x.title.StartsWith("小组作业 ·"))&&e.logs.First(x=>x.title.StartsWith("小组作业 ·"))==l)s.Add("team");
                if(l.title=="外出散步"||(l.title.StartsWith("全天游玩 ·")&&l.half=="pm"))s.Add("walk");
                if(l.title.StartsWith("兼职 ·")&&!o.Concat(e.logs.Take(idx)).Any(x=>x.title.StartsWith("兼职 ·")))s.Add("job");
                var c=s.FirstOrDefault(x=>x!=n);if(c!=null)return new[]{c,Lines[c],l.day.ToString()};
            }
            return null;
        }
        public static List<Highlight> Highlights(GameState e)
        {
            var t=e.logs;Func<Func<LogEntry,bool>,int> n=pred=>t.Count(pred);
            int r=n(x=>Cat.Courses.Any(cc=>x.title==cc.name)),i=n(x=>x.title.StartsWith("考试 ·")||x.title.StartsWith("补考 ·")),a=n(x=>x.title.StartsWith("个人作业 ·")||x.title.StartsWith("小组作业 ·"));
            int o=n(x=>x.title=="预习与复习"),s=n(x=>x.title=="外出散步"),c=n(x=>x.title.StartsWith("全天游玩 ·")&&x.half=="pm");
            var l=new List<string>();if(r>0)l.Add("上课 "+r+" 次");if(i>0)l.Add("考试 "+i+" 场");if(a>0)l.Add("作业 "+a+" 个半日");if(o>0)l.Add("复习 "+o+" 次");if(s>0)l.Add("散步 "+s+" 次");if(c>0)l.Add("出游 "+c+" 天");
            var u=new List<Highlight>();if(l.Count>0)u.Add(new Highlight{label="做成了什么",text=string.Join("、",l)+"。"});
            var d=Memories(e).Where(x=>x.round==e.round).ToList();var f=d.FirstOrDefault(x=>x.kind=="team");if(f!=null)u.Add(new Highlight{label="与谁开始合作",text=f.text});
            var p=d.Where(x=>x.kind!="team").ToList();if(p.Count>0)u.Add(new Highlight{label="留下的回忆",text=string.Join("、",p.Select(x=>"「"+x.title+"」"))});
            return u.Take(3).ToList();
        }
        public static string NetCash(GameState e,out int net)
        {
            net=0;if(e.phase!="summary"&&e.phase!="finished")return null;net=e.money-e.startMoney;var n=Fmt.Money(Math.Abs(net));
            return net>0?"本轮现金净增加 HK$"+n:net<0?"本轮现金净减少 HK$"+n:"本轮收支持平";
        }
        // ---------- 组队（轻松版：默认队伍） ----------
        public static List<string> NeedTeams(GameState e)
        {
            int sem=Cal.Rounds[e.round].semester;if(sem==0)sem=1;
            return Cat.Courses.Where(c=>c.semester==sem).Select(c=>c.id).Where(t=>{var n=e.Proj(t);return n.status!="unreleased"&&!Core.TeamReady(n)&&Core.TeamLock(e,t)==null&&n.status!="submitted"&&n.status!="missing";}).ToList();
        }
        public static Advance DefaultTeam(GameState e,LiteMeta t,string course,out List<string> names)
        {
            names=new List<string>();var i=Core.TeamLock(e,course);if(i!=null)return new Advance{state=e,meta=t,error=i};var a=e.Proj(course);if(a.members.Count>=2)return new Advance{state=e,meta=t,error="队伍已经满员。"};
            int o=2-a.members.Count;var s=a.shown.Where(x=>!a.members.Contains(x)&&!a.Invited(x)).ToList();var c=s.Concat(Core.Candidates(e,course).Where(x=>!s.Contains(x))).Take(o).ToList();
            if(c.Count<o)return new Advance{state=e,meta=t,error="没有足够的候选同学，请到“小组项目”页公开招募。"};
            var l=e.Clone();var u=l.Proj(course);foreach(var x in c){u.invites.Add(new Invite{npc=x,ok=true});u.members.Add(x);}u.roles=Core.BestRoles(l,course);foreach(var x in c)if(!l.known.Contains(x))l.known.Add(x);
            var d=Replan(l,t);var m=d.meta;if(!m.defaultTeams.Contains(course)){m=m.Clone();m.defaultTeams.Add(course);}names=c.Select(x=>l.Npc(x).name).ToList();return new Advance{state=d.state,meta=m};
        }
        public static Advance DefaultTeams(GameState e,LiteMeta t,out List<string> done,out List<string> errors)
        {
            var n=e;var r=t;done=new List<string>();errors=new List<string>();
            foreach(var c in NeedTeams(e)){List<string> names;var x=DefaultTeam(n,r,c,out names);if(x.error!=null)errors.Add(Cat.CourseName(c)+"："+x.error);else{n=x.state;r=x.meta;done.Add(Cat.CourseName(c)+"（"+string.Join("、",names)+"）");}}
            return new Advance{state=n,meta=r};
        }
        // ---------- 快进 ----------
        static LiteMeta Note(GameState e,LiteMeta t,string text)
        {
            int r=Cal.Rounds[e.round].startDay+Math.Min(e.cursor,e.plan.Count-1)/2;var m=t.Clone();m.digest.Add(new Digest{round=e.round,day=r,text=text});return m;
        }
        static bool ReleasedThisRound(GameState e,string c){var n=e.Proj(c).released;return n!=null&&n>=Cal.Rounds[e.round].startDay&&n<=Cal.Rounds[e.round].endDay;}
        static Advance HandleNotice(GameState e,LiteMeta t)
        {
            var n=e.notice;var r=Cat.CourseOf(n.course);var i=t;var a=e.Clone();
            if(n.kind=="makeup")
            {
                var before=t.free.Select(x=>e.plan[x]).ToList();i=Note(e,i,r.name+"：病假缺考，补考已排入本轮日程。");a.notice=null;
                for(int k=0;k<t.free.Count;k++)if(a.plan[t.free[k]]!=before[k])return new Advance{state=a,meta=i,stop=new Stop("adjust","补考占用了你的一个特别安排，请确认本轮剩余安排。")};
            }
            else
            {
                int due=Core.Due(r.id,n.kind);i=Note(e,i,n.kind=="individual"?r.teacher+"布置了「"+r.assignment+"」（"+Cal.Date(due)+"截止）。":r.name+"发布了小组项目（"+Cal.Date(due)+"截止）。");a.notice=null;
            }
            List<Pace.Change> changes;var o=Replan(a,i,null,out changes);a=o.state;i=o.meta;
            if(changes.Count>0)i=Note(a,i,"已为新任务补排 "+changes.Count(x=>x.to!=null&&(x.to.StartsWith("task:")||x.to.StartsWith("project:")))+" 个未来半日。");
            if(n.kind=="group"&&!Core.TeamReady(a.Proj(n.course))&&!i.teamPrompted.Contains(n.course))
            {
                bool earlier=i.teamPrompted.Any(x=>ReleasedThisRound(a,x));i=i.Clone();i.teamPrompted.Add(n.course);
                if(!earlier)return new Advance{state=a,meta=i,stop=new Stop("team","「"+r.name+"」小组项目发布了：可以直接接受默认队伍，或到“小组项目”手动组队。本轮之后发布的项目会一并列在这里，不再逐个打断。",n.course)};
            }
            return new Advance{state=a,meta=i};
        }
        static Stop NoTeamWork(GameState e)
        {
            if(e.round!=1)return null;var t=FirstProjectCourse(e);var n=e.Proj(t);
            return !Core.TeamReady(n)||n.locked||n.status=="submitted"||n.status=="missing"||SlotsOf(e,Core.ProjectItem(t)).Count>0?null:new Stop("adjust","队伍已组好，但本轮剩余的半日里没有小组作业：请在特别安排或“详细安排”中放一个“小组作业”。",t);
        }
        static Stop ConflictStop(LiteMeta e){return new Stop("conflict","休整和"+(e.conflict.items.Count>1?"几份":"一份")+"临近截止的作业撞在一起了，请先选一个方向。");}
        public static Advance Run(GameState e,LiteMeta t,bool untilDayEnd=false,int? steps=null,int limit=400)
        {
            var x=Loop(e,t,untilDayEnd,steps,limit);if(x.state.ev!=null)x.meta=RecordLook(x.state,x.meta);return x;
        }
        static Advance Loop(GameState e,LiteMeta t,bool untilDayEnd,int? steps,int limit)
        {
            var r=e.Clone();var i=t;int a=0,o=r.cursor/2;
            Func<string,string,Advance> stop=(k,m)=>new Advance{state=r,meta=i,stop=new Stop(k,m)};
            for(int it=0;it<limit;it++)
            {
                if(r.phase=="planning")return stop("planning","本轮还没有开始。");
                if((r.phase=="summary"||r.phase=="finished")&&r.ev!=null)return stop("event","遇到了一件事，需要你选择。");
                if(r.phase=="summary"||r.phase=="finished"){var c=PendingCard(r,i);if(c!=null)return stop("result",c.title);if(PendingTrip(i)!=null)return stop("trip","旅行回来了。");return SampleEnded(r)?stop("sample_end","四轮样例结束。"):stop("round_end","这一轮结束了。");}
                if(r.notice!=null){var h=HandleNotice(r,i);r=h.state;i=h.meta;if(h.stop!=null)return new Advance{state=r,meta=i,stop=h.stop};var t2=NoTeamWork(r);if(t2!=null)return new Advance{state=r,meta=i,stop=t2};continue;}
                var card=PendingCard(r,i);if(card!=null)return stop("result",card.title);
                if(r.ev!=null)return stop("event","遇到了一件事，需要你选择。");
                if(r.blockedReason!=null)return stop("blocked",r.blockedReason);
                if(i.conflict!=null)return new Advance{state=r,meta=i,stop=ConflictStop(i)};
                if(PendingTrip(i)!=null)return stop("trip","旅行回来了。");
                if(untilDayEnd&&r.cursor/2>o)return stop("day","过完了一天。");
                string q=r.round+":"+r.cursor;
                if(r.cursor%2==0&&i.dayCheck!=q){var j=Replan(r,i);r=j.state==r?r:j.state;i=j.meta;i.dayCheck=r.round+":"+r.cursor;continue;}
                if(steps!=null&&a>=steps.Value)return stop("tick","继续中。");
                int before=r.cursor,logs=r.logs.Count,round=r.round;if(ReferenceEquals(r,e))r=r.Clone();Core.Step(r);a++;i=RecordTrips(logs,round,r,i);
                if(r.cursor==before&&r.ev==null&&r.notice==null&&r.blockedReason==null&&r.phase=="running")return stop("blocked","这个半日无法执行，请检查安排。");
            }
            return stop("limit","已推进很多步，暂停一下。");
        }
        public static Advance SetFocus(GameState e,LiteMeta t,string focus)
        {
            if(e.phase!="planning"||!PhaseStart(e.round))return new Advance{state=e,meta=t,error="倾向在每个阶段开始时选择，本阶段沿用已选的倾向。"};
            List<Change> c;var r=Replan(e,t,focus,out c);r.changes=c.Count;return r;
        }
        public static Advance StartRound(GameState e,LiteMeta t)
        {
            var n=e;var r=t;if(r.round!=n.round){var x=NewRound(n,r);n=x.state;r=x.meta;}
            if(PhaseStart(n.round)&&r.focus=="balanced")return new Advance{state=e,meta=t,error="新阶段开始：请先从“学业成长／香港生活／工作体验”中选一个倾向。"};
            if(r.conflict!=null)return new Advance{state=e,meta=t,error="休整和临近截止的作业有冲突，请先选“先休整”或“继续赶进度”。"};
            if(n.plan.Any(x=>x==null))return new Advance{state=e,meta=t,error="还有未安排的半日。"};
            var m=r.Clone();m.focusPhase=PhaseIndex(n.round);var s=n.Clone();Core.Begin(s);return new Advance{state=s,meta=m};
        }
        public static Advance NextRound(GameState e,LiteMeta t)
        {
            if(SampleEnded(e))return new Advance{state=e,meta=t,error="四轮样例已经结束。"};if(e.phase!="summary")return new Advance{state=e,meta=t,error="这一轮还没有结束。"};
            if(e.ev!=null)return new Advance{state=e,meta=t,error="先完成这件事的选择，再进入下一轮。"};if(PendingCard(e,t)!=null)return new Advance{state=e,meta=t,error="先看完这件事的结果，再进入下一轮。"};
            if(PendingTrip(t)!=null)return new Advance{state=e,meta=t,error="先把这次旅行收进手账，再进入下一轮。"};
            var n=Monologue(e,t);var r=t;if(n!=null&&!t.monologues.Any(x=>x.Key==e.round)){r=t.Clone();r.monologues.Add(new KeyValuePair<int,string>(e.round,n[0]));}
            return NewRound(Core.NextRound(e),r);
        }
        public class JobCard{public string kind,text;public Job job;public double chance;}
        public static JobCard JobOffer(GameState e,LiteMeta t)
        {
            if(t.focus!="work"||e.hired.Count>0)return null;
            if(Core.TriedThisRound(e))return new JobCard{kind="info",text="本轮的求职机会已经用过（"+(e.lastAttempt.success?"已录用":"这次没有被录用")+"），下一轮规划时可以再申请。没有兼职的半日照常安排作业与休息。"};
            if(!Core.CanSeekJob(e))return new JobCard{kind="info",text="求职只能在每轮开始前的规划阶段进行；下一轮开始前可以申请。"};
            if(t.jobCardDismissed==e.round)return null;
            var n=Cat.Jobs.Where(j=>Core.Qualified(j,e.abilities)).OrderByDescending(j=>j.pay).ToList();
            if(n.Count==0)
            {
                var c=Cat.Jobs.OrderBy(j=>Core.Lacks(j,e.abilities).Count).First();
                return new JobCard{kind="none",text="目前没有达到条件的岗位（最接近的「"+c.name+"」还差："+string.Join("、",Core.Lacks(c,e.abilities).Select(m=>Cat.AbilityName(m.ability)+" "+Fmt.F1(m.required-m.current)))+"）。这一轮照常安排作业与休息。"};
            }
            return new JobCard{kind="offer",job=n[0],chance=Core.Chance(n[0],e.abilities).Value};
        }
        public static LiteMeta DismissJob(GameState e,LiteMeta t){var m=t.Clone();m.jobCardDismissed=e.round;return m;}
        public static string PendingKind(GameState e,LiteMeta t)
        {
            if(e.notice!=null)return "notice";if((e.phase=="summary"||e.phase=="finished")&&e.ev!=null)return "event";if(PendingCard(e,t)!=null)return "result";if(e.ev!=null)return "event";return PendingTrip(t)!=null?"trip":null;
        }
        public static Prompt MainAction(GameState e,LiteMeta t,Stop stop,bool focusSeen)
        {
            var r=PendingKind(e,t);
            if(r=="notice")return new Prompt{kind="notice",label="看看新通知",hint="老师发布了新任务，确认后再继续。"};
            if(r=="event")return new Prompt{kind="event",label="看看发生了什么",hint="遇到了一件事，需要你选择。"};
            if(r=="result")return new Prompt{kind="result",label="查看这次结果",hint=PendingCard(e,t).title};
            if(r=="trip")return new Prompt{kind="trip",label="看看这次旅行",hint="旅行回来了，把今天收进手账。"};
            if(e.phase=="summary"||e.phase=="finished")return SampleEnded(e)?new Prompt{kind="sample_end",label="回看这段生活",hint="四轮样例已经结束，不会进入第 5 轮。"}:new Prompt{kind="round_end",label="看看这两周",hint="第 "+(e.round+1)+" 轮结束了，回顾后再开始下一轮。"};
            if(t.conflict!=null)return new Prompt{kind="conflict",label="处理眼前的事",hint="休整和临近截止的作业撞在一起了，先选一个方向。"};
            if(e.blockedReason!=null)return new Prompt{kind="handle",label="处理眼前的事",hint=e.blockedReason};
            if(stop!=null&&(stop.kind=="team"||stop.kind=="adjust")&&(stop.kind!="team"||NeedTeams(e).Count>0))return new Prompt{kind="handle",label="处理眼前的事",hint=stop.message};
            if(e.phase=="planning")
                return (PhaseStart(e.round)&&t.focus=="balanced")||(PhaseStart(e.round)&&t.focusPhase!=PhaseIndex(e.round)&&!focusSeen)?new Prompt{kind="focus",label="选择这段时间的重心",hint=PhaseLabel(e.round)+"开始：学业成长、香港生活或工作体验，选一个倾向。"}
                    :new Prompt{kind="start",label="开始这两周",hint="倾向："+FocusName(t.focus)+"。到下一件需要你决定的事时停下。"};
            return new Prompt{kind="continue",label="继续生活",hint="到下一件需要你决定的事时停下。"};
        }
        public static Summary Home(GameState e,LiteMeta t)
        {
            var n=CurrentGoal(e);int r=e.plan.Count;bool i=e.phase=="summary"||e.phase=="finished";var s=new Summary{total=r};
            if(e.phase=="planning"){s.progressText="本轮尚未开始";s.done=0;}else if(i){s.progressText="本轮已结束";s.done=r;}else{s.progressText="本轮已过 "+e.cursor+" / "+r+" 个半日";s.done=e.cursor;}
            var o=i?new List<string>():NeedTeams(e);
            s.attention=t.conflict!=null?"休整和临近截止的作业撞在一起了，需要你选一个方向。":o.Count>0?string.Join("、",o.Select(Cat.CourseName))+"的小组项目还没有组队。":Attention(e).FirstOrDefault();
            var c=Monologue(e,t);s.phase=(PhaseLabel(e.round)==""?"样例":PhaseLabel(e.round))+" · "+FocusName(t.focus);s.round="第 "+(e.round+1)+" / 4 轮";
            s.goalTitle=n.title;s.goalNext=n.next;s.goalDone=n.done;s.goalStatus=n.status;s.goalNote=n.note;s.monologue=c!=null?c[1]:null;return s;
        }
        public static List<string> Todo(GameState e,LiteMeta t)
        {
            var n=new List<string>();if(e.notice!=null)n.Add("有一则新的课程通知待确认。");
            if(e.ev!=null)n.Add("有一件事等你选择。");else if(PendingCard(e,t)!=null)n.Add("有一件事的结果还没看。");else if(PendingTrip(t)!=null)n.Add("有一次旅行还没收进手账。");
            if(t.conflict!=null)n.Add("休整和临近截止的作业撞在一起了，需要你选一个方向。");
            if(e.phase!="summary"&&e.phase!="finished"){var x=NeedTeams(e);if(x.Count>0)n.Add(x.Count+" 个小组项目还没有组队。");}
            if(e.blockedReason!=null)n.Add(e.blockedReason);return n;
        }
        public static List<GalleryItem> Gallery(GameState e,LiteMeta t)
        {
            var n=new List<GalleryItem>();
            Action<GalleryItem,string,int> add=(x,note,day)=>{var i=n.FirstOrDefault(y=>y.asset==x.asset);if(i!=null){if(!i.notes.Contains(note))i.notes.Add(note);i.day=Math.Min(i.day,day);return;}x.notes.Add(note);x.day=day;n.Add(x);};
            foreach(var m in Memories(e))
            {
                if(m.cg==null)continue;Media c;if(!Cg.TryGetValue(m.cg,out c)||!c.provided||c.asset==null)continue;var body=LookOf(t,m.key);if(c.scope!="all"&&c.scope!=body)continue;
                add(new GalleryItem{asset=c.asset,kind="event",src=c.src,alt=c.alt,title=c.asset=="CG03"?"小组讨论":m.title},Cal.Date(m.day)+" · "+m.title,m.day);
            }
            foreach(var x in TripMemories(e,t))
            {
                Travel tr;if(!Trips.TryGetValue(x.loc,out tr)||!x.scenery)continue;
                add(new GalleryItem{asset=tr.scenery.asset,kind="travel",src=tr.scenery.src,alt=tr.scenery.alt,title=x.name+" · 风景"},x.date+" · 全天游玩",x.day);
                if(x.selfie&&tr.selfie!=null)add(new GalleryItem{asset=tr.selfie.asset,kind="travel",src=tr.selfie.src,alt=tr.selfie.alt,title=x.name+" · 自拍"},x.date+" · 全天游玩",x.day);
            }
            return n.OrderBy(x=>x.kind=="event"?0:1).ThenBy(x=>x.day).ToList();
        }
        public static string HealthNote(GameState e,LiteMeta t)
        {
            var n=t.health;if(n==null||!n.triggered)return null;
            if(n.resolved)return "按预测，本轮有几天会比较累，已自动把 "+n.rested+" 个半日改成休息；预计每天结束时精力不低于 40、压力不高于 60。";var r=n.risk;
            return (n.rested>0?"已经把能换的半日都改成休息（"+n.rested+" 个）":"接下来没有可以换成休息的推荐半日")+"，预计 "+Cal.Date(r.day)+" 结束时精力仍约 "+r.energy+"、压力约 "+r.stress+"：剩下的是课程、考试、必要作业、特别安排或你亲手安排的事。"+(t.conflicts.Any(x=>x.Key.StartsWith(e.round+":")&&x.Value=="push")?"（你选了继续赶进度。）":"");
        }
        public static string PlanSummary(GameState e,LiteMeta t)
        {
            var n=new List<KeyValuePair<string,int>>();
            foreach(var r in t.auto)if(r>=Core.Pos(e)){var l=Core.Label(e.plan[r]);int i=n.FindIndex(x=>x.Key==l);if(i<0)n.Add(new KeyValuePair<string,int>(l,1));else n[i]=new KeyValuePair<string,int>(l,n[i].Value+1);}
            int fixedN=Enumerable.Range(0,e.plan.Count).Count(i=>i>=Core.Pos(e)&&Core.FixedItem(e,e.round,i)!=null);
            var items=n.OrderByDescending(x=>x.Value).Select(x=>x.Key+" "+x.Value).ToList();
            return "本轮共 28 个半日：固定课程与考试 "+fixedN+" 个"+(t.free.Count>0?"，特别安排 "+t.free.Count+" 个":"")+(t.relaxed.Count>0?"，“放轻松”休息 "+t.relaxed.Count+" 个":"")+"，系统推荐 "+(items.Count>0?string.Join("、",items):"无")+"。"+(t.customized?"（你在详细安排里改过部分推荐。）":"");
        }
        public static LiteMeta NewMeta(){return new LiteMeta();}
    }
}
