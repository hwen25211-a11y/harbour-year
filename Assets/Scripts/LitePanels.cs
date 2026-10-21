using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// 轻松版功能页：日程（本轮安排、详细日历、求职）、背包、作业、同学录、手账、目标与右上角三个窗口。
namespace HarbourYear.Lite
{
    public partial class LiteGame
    {
        static readonly Dictionary<string,string[][]> Groups=new Dictionary<string,string[][]>{
            {"日程",new[]{new[]{"lite","本轮安排"},new[]{"planner","详细日历"},new[]{"jobs","求职与兼职"}}},{"背包",new[]{new[]{"items","随身道具"},new[]{"market","二手市场"},new[]{"furniture","家具布置"}}},
            {"作业",new[]{new[]{"courses","课程、作业与成绩"}}},{"同学录",new[]{new[]{"team","小组与同学"}}},{"手账",new[]{new[]{"journal","本轮重点与完整记录"},new[]{"memories","生活回忆"}}},{"目标",new[]{new[]{"goals","样例目标"}}}};
        static readonly Dictionary<string,string> PaperTitles=new Dictionary<string,string>{{"settings","设置与存档"},{"review","本轮回顾"},{"messages","消息与待处理"},{"status","状态与预算"}};
        static string SemesterName(int s){return s==1?"第一学期":s==2?"第二学期":"寒假";}
        Color Title2=UI.Hex("5A2A20");
        void Step(Flow f,string text,string small=null){f.P(text+(small!=null?"  <size=78%><color="+UI.Rich(UI.Muted)+">"+small+"</color></size>":""),20,Title2,true,8);}
        void Intro(Flow f,string h,string p){f.P(h,24,Title2,true,6);f.P(p,17,UI.Muted,false,16);}
        Flow Alert(Flow f){return f.Card(UI.AlertBg,UI.AlertLine,14,8);}
        Flow Note(Flow f){return f.Card(UI.Hex("F6EBDA"),UI.Line,14,8);}
        void DrawWindow()
        {
            if(win=="room")return;var o=Overlay("Window");string group=Groups.Keys.FirstOrDefault(g=>Groups[g].Any(x=>x[0]==win));var r=Cal.Rounds[S.round];bool paper=PaperTitles.ContainsKey(win);
            string title=PaperTitles.ContainsKey(win)?PaperTitles[win]:win=="lite"&&Ended?"本轮回顾":group??"";
            string eyebrow=win=="planner"?SemesterName(r.semester)+" · 第 "+(S.round+1)+" 轮 · "+Cal.Date(r.startDay)+"—"+Cal.Date(r.endDay):SemesterName(r.semester)+" · 第 "+(S.round+1)+" 轮 · "+Pace.PhaseLabel(S.round);
            Action<RectTransform,float> tabs=null;if(group!=null&&Groups[group].Length>1)tabs=(rt,w)=>Tabs(rt,w,Groups[group].Select(x=>new KeyValuePair<string,string>(x[0],x[1])).ToList(),win,Nav);
            var f=Panel(o,win,title,eyebrow,paper,paper?1000:1360,paper?0:860,()=>Nav("room"),"返回",tabs,tabs!=null?42:0);
            switch(win)
            {
                case "lite":if(Ended)RoundReview(f);else PlanPage(f);break;case "planner":if(Ended)RoundReview(f);else Planner(f);break;case "jobs":JobsPage(f);break;
                case "items":ItemsPage(f);break;case "market":MarketPage(f);break;case "furniture":FurniturePage(f);break;case "courses":CoursesPage(f);break;case "team":TeamPage(f);break;
                case "journal":JournalPage(f);break;case "memories":MemoriesPage(f);break;case "goals":GoalsPage(f);break;case "review":if(Ended)RoundReview(f);else ReviewPage(f);break;
                case "messages":MessagesPage(f);break;case "status":StatusPage(f);break;case "settings":GameSettings(f);break;
            }
            EndPanel(f);
        }
        void Apply(Advance a,string ok){if(a.error!=null){Toast(a.error);Render();return;}Commit(a.state,a.meta,ok);}
        // ---------- 本轮回顾 ----------
        void RoundReview(Flow f)
        {
            bool c=Pace.SampleEnded(S);var mono=Pace.Monologue(S,M);int net;var cash=Pace.NetCash(S,out net);bool next=!c&&Pace.PhaseStart(S.round+1);
            f.P(c?"SAMPLE COMPLETE":"ROUND COMPLETE",14,UI.Brick,true,2);f.P(c?"四轮样例结束":"第 "+(S.round+1)+" 轮结束",28,Title2,true);
            if(cash!=null)f.P("<b>"+cash+"</b>  <size=80%><color="+UI.Rich(UI.Muted)+">（本轮开始 HK$"+Fmt.Money(S.startMoney)+" → 现在 HK$"+Fmt.Money(S.money)+"，含生活费、工资、购物与事件；不是工资收入）</color></size>",19,net>0?UI.Good:net<0?UI.Warn:UI.Ink);
            var hl=Pace.Highlights(S);if(hl.Count==0)f.P("这一轮平平静静地过去了。",18);else foreach(var h in hl){f.P(h.label,15,UI.Muted,true,2);f.P(h.text,18);}
            if(mono!=null)f.P("「"+mono[1]+"」",19,UI.Hex("6A5040"));
            if(next)f.P("下一轮是"+Pace.PhaseLabel(S.round+1)+"的开始，可以重新选择倾向。",16,UI.Muted);
            if(c)f.P("这是轻松版的四轮试玩样例（第一学期前八周、两个阶段），到这里结束：不是毕业，也不是完整通关。课程总评、期末考试与结局要在之后的轮次才会发生。",16,UI.Muted);
            f.Row(!c?Flow.Bt("开始第 "+(S.round+2)+" 轮  →",()=>{var e=Pace.NextRound(S,M);if(e.error!=null){Toast(e.error);Render();return;}week=0;win="lite";Commit(e.state,e.meta);},Look2.Primary,true,"NextRound"):null,
                Flow.Bt("回看回忆",()=>Nav("memories")),Flow.Bt("完整手账",()=>Nav("journal")),c?Flow.Bt("导出存档",Export):null,c?Flow.Bt("重新开始",()=>{dialog="reset";Render();},Look2.Primary):null);
        }
        // ---------- 本轮安排 ----------
        void PlanPage(Flow f)
        {
            var g=Pace.CurrentGoal(S);var digest=M.digest.Where(d=>d.round==S.round).ToList();
            if(Cal.Rounds[S.round].kind!="semester"){f.P("寒假不在四轮样例范围内。");return;}
            if(M.round!=S.round&&S.phase=="planning"){f.P("正在生成本轮推荐……",18,UI.Muted);return;}
            var gc=f.Card(g.done?UI.Hex("EEF3E6"):UI.Hex("FFF8EC"),UI.Hex("D8BF98"));gc.P("当前目标 · 第 "+(S.round+1)+" 轮 · "+Pace.PhaseLabel(S.round)+"（四轮样例）",14,UI.Brick,true,2);
            gc.P((g.done?"✓ ":"")+g.title,22,g.done?UI.Good:Title2,true,4);if(g.status!="done"&&g.status!="current")gc.P(Pace.GoalLabels[g.status],15,UI.Warn,true,4);
            gc.P("<b>下一步：</b>"+g.next,17);foreach(var d in g.detail)gc.P(d,15,UI.Muted);gc.End();
            var conflict=M.conflict;bool busy=S.ev!=null||S.notice!=null;
            if(conflict!=null)
            {
                var c=Alert(f);c.P("<b>休整和临近截止的作业撞在一起了</b>",19,UI.Hex("8C3A2A"));
                c.P("按预测，"+Cal.Date(conflict.day)+" 结束时精力约 "+conflict.energy+"、压力约 "+conflict.stress+"。能换成休息的半日已经都换了，剩下的是 7 天内截止的作业：",16);
                foreach(var it in conflict.items){var cc=Cat.CourseOf(it.course);c.P("· "+(it.kind=="individual"?"「"+cc.assignment+"」":"「"+cc.name+"」小组项目")+"，"+Cal.Date(it.due)+" 截止；本轮其中 "+it.halves.Count+" 个半日在这段时间（"+string.Join("、",it.halves.Select(h=>Pace.SlotName(S,h)))+"）。",16);}
                c.Row(Flow.Bt("先休整（这些半日改成休息，作业会慢一些）",()=>Conflict("rest"),Look2.Primary,true,"ConflictRest"),Flow.Bt("继续赶进度（保持安排，状态可能更低）",()=>Conflict("push"),Look2.Secondary,true,"ConflictPush"));
                c.P("只问这一次；之后出现新的作业冲突才会再问。休整不保证不生病，赶进度也不保证按时完成。",14,UI.Muted);c.End();
            }
            var teams=S.phase=="summary"?new List<string>():Pace.NeedTeams(S);
            if(teams.Count>0)
            {
                var c=Note(f);c.P("<b>"+teams.Count+" 个小组项目还没有组队</b>",19);
                c.P(string.Join("、",teams.Select(Cat.CourseName))+"。可以一键接受默认队伍（轻松版规则：同学直接答应加入，分工按能力推荐），也可以去“小组项目”自己邀请。没组队的项目不能开工。",16);
                c.Row(Flow.Bt("全部接受默认队伍",DefaultTeams,Look2.Primary,true,"AcceptTeams"),Flow.Bt("手动组队",()=>{teamCourse=Pace.FirstProjectCourse(S);Nav("team");}));c.End();
            }
            var health=Pace.HealthNote(S,M);
            if(S.phase=="planning")
            {
                if(Pace.PhaseStart(S.round))
                {
                    Step(f,Pace.PhaseLabel(S.round)+"的倾向",Pace.PhaseRounds(S.round)+"，开始前可以改，下一轮沿用");
                    if(M.focus=="balanced"){var a=Alert(f);a.P("旧存档的“平衡生活”到这里结束：请从下面三种倾向中选一个（不会自动改成工作体验）。",16);a.End();}
                    var row=f.Block(110);float bw=(f.w-24)/3;
                    for(int i=0;i<3;i++){var fo=Cat.Foci[i];bool sel=M.focus==fo.id;var b=UI.Btn(row,null,i*(bw+12),0,bw,110,()=>SetFocus(fo.id),sel?Look2.Selected:Look2.Secondary,true,"Focus:"+fo.id,sel?"none":"click");UI.Text(b.transform,"<b>"+fo.name+"</b>\n<size=82%>"+fo.hint+"</size>",16,12,bw-32,90,19,sel?UI.White:UI.Ink);}
                }
                else f.P("<b>"+Pace.PhaseLabel(S.round)+" · 倾向："+Pace.FocusName(M.focus)+"</b>  <size=80%><color="+UI.Rich(UI.Muted)+">（"+Pace.PhaseRounds(S.round)+"，本轮沿用；下个阶段开始时再选）</color></size>",18);
                if(Details(f,"focus-detail","“"+Pace.FocusName(M.focus)+"”具体怎么排"))f.P(Cat.FocusOf(M.focus).detail+"三种倾向都会按截止进度安排必要作业，并在预测会太累时自动换成休息；不保证作业一定按时或不生病。",16,UI.Muted);
                var job=Pace.JobOffer(S,M);
                if(job!=null&&job.kind=="offer")
                {
                    var c=Note(f);c.P("<b>工作体验：可以申请「"+job.job.name+"」</b>",19);
                    c.P("半日工资 HK$ "+job.job.pay+"，"+Pace.Delta(job.job.fatigue,job.job.stress)+"；按你现在的能力，录用概率约 "+Fmt.N(job.chance)+"%。每轮只有一次求职机会，录用后才会排兼职；申请失败就照常生活，下一轮可再试。",16);
                    var jid=job.job.id;c.Row(Flow.Bt("申请（用掉本轮求职机会）",()=>ApplyJob(jid),Look2.Primary,true,"ApplyJob"),Flow.Bt("稍后再说",()=>Commit(S,Pace.DismissJob(S,M))));c.End();
                }
                else if(job!=null)f.P(job.text,16,UI.Muted);
                if(Details(f,"specials","特别安排（可选，最多 2 个半日"+(M.free.Count>0?"，已有 "+M.free.Count+" 个":"")+"）"))Specials(f);
                if(health!=null)f.P("♡ "+health,16,UI.Hex("4F6A45"));
                TripNote(f);f.P(Pace.PlanSummary(S,M),16,UI.Muted);
                f.Row(Flow.Bt("▶ 开始这一轮",()=>Go(false),Look2.Primary,conflict==null,"StartRound"),Flow.Bt("只过第一天",()=>Go(true),Look2.Secondary,conflict==null,"FirstDay"),Flow.Bt("这段时间放轻松一点",()=>Relax(false),Look2.Secondary,!busy,"Relax"),Flow.Bt("详细安排（28 格日历）",()=>Nav("planner"),Look2.Secondary,true,"OpenPlanner"));
            }
            if(S.phase=="running")
            {
                f.P(Cal.Date(Cal.Rounds[S.round].startDay+S.cursor/2)+Cal.Half(S.cursor%2==0?"am":"pm")+" · 本轮第 "+(S.cursor+1)+" / 28 个半日 · "+Pace.PhaseLabel(S.round)+" · "+Pace.FocusName(M.focus)+"\n现金 HK$ "+Fmt.Money(S.money)+" · 精力 "+Pace.EnergyOf(S.fatigue)+" · 压力 "+S.stress,17,UI.Hex("5B4231"),true);
                if(stop!=null&&!new[]{"event","result","trip","conflict"}.Contains(stop.kind))f.P(stop.message,16,stop.kind=="blocked"||stop.kind=="adjust"?UI.Warn:UI.Muted);
                if(health!=null)f.P("♡ "+health,16,UI.Hex("4F6A45"));LastLog(f,S.logs.LastOrDefault(),"");
                if(Pace.FindTrip(S)!=null)TripNote(f);
                f.Row(Flow.Bt("▶ 继续到下一个要紧时刻",()=>Go(false),Look2.Primary,conflict==null,"Continue"),Flow.Bt("推进一天",()=>Go(true),Look2.Secondary,conflict==null,"OneDay"),Flow.Bt("这段时间放轻松一点",()=>Relax(false),Look2.Secondary,!busy,"Relax"),Flow.Bt("详细安排",()=>Nav("planner")));
                f.P("快进只是连续执行每个半日：生活费、上课出勤、作业进度、病假与截止都照常逐个结算；每天开始时会按真实状态重新检查是否需要休息。遇到需要你选择或值得停下的事会自动暂停。",15,UI.Muted);
            }
            if(relaxAsk!=null)
            {
                var a=Alert(f);a.P("<b>接下来两天剩下的都是必要作业</b>",18,UI.Hex("8C3A2A"));
                a.P(string.Join("、",relaxAsk.confirmItems.Select(Core.Label))+" 在 "+string.Join("、",relaxAsk.confirmSlots.Select(s=>Pace.SlotName(S,s)))+"。仍要把这些半日改成休息吗？作业会慢一些，可能影响按时完成。",16);
                a.Row(Flow.Bt("仍然改成休息",()=>Relax(true),Look2.Primary),Flow.Bt("先不改",()=>{relaxAsk=null;Render();}));a.End();
            }
            var att=Pace.Attention(S);if(att.Count>0){var a=Alert(f);a.P("<b>需要留意</b>",18,UI.Hex("8C3A2A"));foreach(var x in att)a.P("· "+x,16);a.End();}
            if(digest.Count>0)
            {
                var c=Note(f);c.P("<b>本轮通知（最近 "+Math.Min(3,digest.Count)+" 条，共 "+digest.Count+" 条）</b>",18);foreach(var d in Enumerable.Reverse(digest).Take(3))c.P(Cal.Date(d.day)+"："+d.text,16);
                if(digest.Count>3&&Details(c,"digest-more","其余 "+(digest.Count-3)+" 条通知"))foreach(var d in Enumerable.Reverse(digest).Skip(3))c.P(Cal.Date(d.day)+"："+d.text,16);
                c.Row(Flow.Bt("查看本轮重点与完整手账",()=>Nav("journal"),Look2.Text));c.End();
            }
        }
        void Go(bool day)
        {
            laterKey=null;var e=S;var t=M;if(e.phase=="planning"){var n=Pace.StartRound(e,t);if(n.error!=null){Toast(n.error);Render();return;}e=n.state;t=n.meta;}
            var s=Pace.Run(e,t,day);stop=s.stop;Commit(s.state,s.meta);
        }
        void SetFocus(string f){var i=Pace.SetFocus(S,M,f);if(i.error!=null){Toast(i.error);Render();return;}Commit(i.state,i.meta,"本阶段倾向："+Pace.FocusName(f)+"（调整了 "+i.changes+" 个推荐半日；特别安排与手动修改保持不变）。");}
        void DefaultTeams(){List<string> d,e;var r=Pace.DefaultTeams(S,M,out d,out e);stop=null;Commit(r.state,r.meta,string.Join(" ",new[]{d.Count>0?"已组队："+string.Join("；",d)+"。按能力给出分工并安排开工半日。":"",string.Join("；",e)}.Where(x=>x!="")));}
        void Conflict(string choice){var i=Pace.ResolveConflict(S,M,M.conflict.key,choice);stop=null;Commit(i.state,i.meta,choice=="rest"?"已先安排休整：相关作业半日改成休息，进度会慢一些。":"保持赶进度的安排；本轮不会再为这几份作业追问。");}
        void Relax(bool force)
        {
            var i=Pace.Relax(S,M,force);if(i.confirmSlots!=null){relaxAsk=i;Render();return;}relaxAsk=null;
            if(i.error!=null){Toast(i.error);Render();return;}Commit(i.state,i.meta,"已把 "+i.changed.Count+" 个半日改成休息："+string.Join("、",i.changed.Select(t=>Pace.SlotName(S,t)))+"。");
        }
        void Specials(Flow f)
        {
            f.P("想在某个半日做指定的事（比如散步、复习某门课、兼职），就在这里加；系统不会改动它。不加也完全可以。",16,UI.Muted);
            foreach(var r in M.free){int rr=r;var row=f.Block(42,6);UI.Text(row,Pace.SlotName(S,r)+" · "+Core.Label(S.plan[r]),0,8,f.w-120,30,17);if(r>=Core.Pos(S))UI.Btn(row,"取消",f.w-100,0,96,40,()=>{var i=Pace.RemoveSpecial(S,M,rr);Apply(i,"已取消这个特别安排，交还给推荐计划。");},Look2.Text);}
            if(M.free.Count<2)
            {
                var slots=Pace.AutoSlots(S).Where(x=>!M.free.Contains(x)).ToList();var opts=Pace.SpecialOptions(S);if(!opts.Contains(specialItem))specialItem=opts[0];
                f.Row(Flow.Bt(specialSlot!=null&&slots.Contains(specialSlot.Value)?"半日："+Pace.SlotName(S,specialSlot.Value)+"（现在："+Core.Label(S.plan[specialSlot.Value])+"）":"半日：选择一个半日",()=>Pick("选择一个半日",slots.Select(x=>new KeyValuePair<string,string>(x.ToString(),Pace.SlotName(S,x)+"（现在："+Core.Label(S.plan[x])+"）")).ToList(),v=>specialSlot=int.Parse(v)),Look2.Secondary,true,"SpecialSlot"),
                    Flow.Bt("做什么："+Core.Label(specialItem),()=>Pick("做什么",opts.Select(x=>new KeyValuePair<string,string>(x,Core.Label(x))).ToList(),v=>specialItem=v),Look2.Secondary,true,"SpecialItem"),
                    Flow.Bt("加入特别安排",()=>{if(specialSlot==null)return;var slot=specialSlot.Value;var r=Pace.AddSpecial(S,M,slot,specialItem);if(r.error==null)specialSlot=null;Apply(r,"特别安排："+Pace.SlotName(S,slot)+" "+Core.Label(specialItem)+"。");},Look2.Primary,specialSlot!=null&&slots.Contains(specialSlot.Value),"AddSpecial"));
            }
        }
        void TripNote(Flow f)
        {
            var r=Pace.FindTrip(S);int z=Core.Pos(S);
            if(r==null)
            {
                bool gone=S.plan.Take(z).Any(x=>Core.Trip(x)!=null);bool free=Enumerable.Range(0,S.plan.Count/2).Select(i=>i*2).Any(t=>t>=z&&Core.FixedItem(S,S.round,t)==null&&Core.FixedItem(S,S.round,t+1)==null);
                string a=gone?"本轮的全天游玩已经去过了":M.focus=="life"?(S.money<Cat.TripCost?"余额不足 HK$"+Cat.TripCost+"，系统没有推荐全天游玩":free?"本轮推荐里没有全天游玩":"本轮余下没有上午和下午都没课的日子"):"“"+Pace.FocusName(M.focus)+"”倾向不推荐全天游玩";
                f.P("☀ "+a+"；如果想去，可以在“详细安排”里选一个整天空着的日子安排（维多利亚港、麦理浩径有旅行照片）。",15,UI.Muted);return;
            }
            int am=r[0];string loc=Core.Trip(S.plan[am]);int day=Cal.Rounds[S.round].startDay+am/2;var c=Note(f);
            c.P("<b>☀ 全天游玩："+Cal.Date(day)+" · "+Cat.TripOf(loc).name+"</b>",18);c.P("上午出发时付 HK$"+Cat.TripCost+"，下午结束时恢复。可以换一个地点，仍是同一天，费用与恢复不变；换过后这一天按你的安排保留。",15);
            c.Row(Flow.Bt("更换地点："+Cat.TripOf(loc).name,()=>Pick("更换地点",Cat.Trips.Select(t=>new KeyValuePair<string,string>(t.id,t.name+(Pace.HasScenery(t.id)?"（有旅行照片）":""))).ToList(),v=>{var s=Pace.ChangeTrip(S,M,am,v);if(s.error!=null)Toast(s.error);else{S=s.state;M=s.meta;Commit(S,M,"已改为"+Cat.TripOf(v).name+"：仍是 "+Cal.Date(day)+" 的上午和下午。",false);}})));c.End();
        }
        void LastLog(Flow f,LogEntry l,string prefix)
        {
            if(l==null)return;var c=f.Card(UI.Hex("FFFCF5"),UI.Line,14,8);c.P(prefix+l.date+Cal.Half(l.half)+" · "+l.title,15,UI.Brick,true,2);c.P(Pace.Clean(l.text),16);c.P(Pace.Changes(l),14,UI.Muted);c.End();
        }
        void ApplyJob(string id)
        {
            LiteSave.SaveBeforeJob(S,M);var t=S.Clone();Attempt at;var err=Core.Apply(t,id,UnityEngine.Random.value*0.9999999,out at);if(err!=null){Toast(err);Render();return;}
            var meta=M;if(at.success&&M.round==t.round&&t.phase=="planning"){var n=Pace.Replan(t,M);t=n.state;meta=n.meta;}
            Commit(t,meta,at.success?"录用成功："+Cat.JobOf(id).name+"。现在可以在本轮日程中安排兼职了。":"这次没有被录用（录用概率 "+Fmt.N(at.chance)+"%）。本轮求职机会已用完，下一轮可再试。");
        }
        // ---------- 详细日历 ----------
        void PlanChange(List<string> plan,string msg)
        {
            if(Ended)return;if(!plan.SequenceEqual(S.plan)){undo.Add(new List<string>(S.plan));if(undo.Count>20)undo.RemoveAt(0);var t=Core.ApplyPlan(S,plan);var m=Pace.Customized(S,t,M);Commit(t,m,msg);}else{Toast(msg);Render();}
        }
        string SlotTitle(int i){var s=Cal.SlotOf(S.round,i);return Cal.Date(s.day)+Cal.Half(s.half);}
        void Planner(Flow f)
        {
            var r=Cal.Rounds[S.round];int z=Core.Pos(S),empty=S.plan.Skip(z).Count(x=>x==null);
            f.P("<b>把时间留给重要的事。</b>  <size=80%><color="+UI.Rich(UI.Muted)+">每天分上午、下午两格。一门课约 3 小时，只占半天。</color></size>",22,Title2);
            f.P(S.phase=="running"?"已度过 "+S.cursor+" / 28 个半日":"已安排 "+(28-empty)+" / 28 个半日",15,UI.Brick,true);
            f.Row(Flow.Bt("一键填课",FillClasses,Look2.Primary,!autoRun,"FillClasses"),Flow.Bt("批量填充活动",()=>{dialog="batch";autoRun=false;Render();},Look2.Secondary,!autoRun),
                Flow.Bt("沿用上轮",()=>{var e=Core.ReuseLast(S);PlanChange(e.plan,"沿用了 "+e.added+" 个半日，跳过 "+e.unfilled+" 项课表不同、未解锁或已完成的安排。");},Look2.Secondary,!autoRun&&S.lastPlan!=null),
                Flow.Bt("撤销",()=>{var p=undo[undo.Count-1];undo.RemoveAt(undo.Count-1);Commit(Core.ApplyPlan(S,p),M,"已撤销本次日程修改。");},Look2.Secondary,!autoRun&&undo.Count>0),
                Flow.Bt("兼职求职",()=>Nav("jobs"),Look2.Secondary,!autoRun),Flow.Bt("替换本轮兼职",OpenReplace,Look2.Secondary,!autoRun&&S.hired.Count>0),
                Flow.Bt("清空余下日程",()=>PlanChange(S.plan.Select((x,i)=>i>=z?null:x).ToList(),"已清空尚未执行的日程，可撤销。"),Look2.Text,!autoRun));
            var n=Note(f);n.P("每门课的个人作业在第 3 教学周、小组项目在第 4 教学周，按课表在该课课堂时段发布；缺课也会收到通知。安排作业时要指定课程。考试在固定时段进行，替换当周课堂。",15,UI.Muted);n.End();
            f.Row(Flow.Bt("本轮复习针对课程："+Cat.CourseName(S.reviewFocus)+"（掌握度 "+Fmt.F1(S.Mastery(S.reviewFocus))+"）",()=>Pick("本轮复习针对课程",Core.SemesterCourses(S).Select(c=>new KeyValuePair<string,string>(c.id,c.name+"（掌握度 "+Fmt.F1(S.Mastery(c.id))+"）")).ToList(),v=>{var t=S.Clone();Core.SetReviewFocus(t,v);Commit(t,M);}),Look2.Secondary,!Ended));
            f.P("兼职：已录用 "+(S.hired.Count>0?string.Join("、",S.hired.Select(j=>Cat.JobOf(j).name)):"无")+" · 本轮已排 "+Core.JobCount(S.plan)+" / 6 个半日 · 求职机会"+(Core.TriedThisRound(S)?"已用":Core.CanSeekJob(S)?"可用":"仅在规划阶段可用"),15,UI.Muted);
            var tabs=f.Block(46);for(int k=0;k<2;k++){int kk=k;UI.Btn(tabs,"第 "+(k+1)+" 周  "+Cal.Date(r.startDay+k*7)+"—"+Cal.Date(r.startDay+k*7+6),k*262,0,250,44,()=>{week=kk;Render();},week==k?Look2.Selected:Look2.Tab,true,"Week"+k,week==k?"none":"page",16);}
            float cw=(f.w-6*8)/7,ch=262;var grid=f.Block(ch);
            for(int d=0;d<7;d++)
            {
                int day=r.startDay+week*7+d,idx=(week*7+d)*2;float x=d*(cw+8);var col=UI.Frame(grid,x,0,cw,ch,UI.Hex("FFFCF5"),UI.Line,8,1.5f).rectTransform;var hol=Cal.Holiday(day);
                UI.Text(col,Cal.Weekdays[Cal.Weekday(day)]+"  <b>"+Cal.Date(day)+"</b>",8,6,cw-16,24,15,UI.Ink,false,TextAlignmentOptions.TopLeft,true);
                if(hol!=null)UI.Text(col,"公众假期 · "+hol,8,28,cw-16,18,11,UI.Brick,false,TextAlignmentOptions.TopLeft,true);
                for(int h=0;h<2;h++)HalfSlot(col,idx+h,6,52+h*104,cw-12,98);
            }
            if(S.blockedReason!=null){var a=Alert(f);a.P("<b>日程暂停</b>",18,UI.Hex("8C3A2A"));a.P(S.blockedReason,16);a.Row(Flow.Bt("调整这个半日的安排",()=>{slotSel=S.cursor;dialog="slot";Render();}));a.End();}
            var fc=Core.Forecast(S);f.P("按当前计划预估轮末 <size=80%><color="+UI.Rich(UI.Muted)+">空白与失效任务按休息估算</color></size>：<b>HK$ "+Fmt.Money(fc!=null?fc[0]:S.money)+"</b> · 精力 "+Pace.EnergyOf(fc!=null?fc[1]:S.fatigue)+" · 压力 "+(fc!=null?fc[2]:S.stress),16);
            int res=Core.Reserve(S);f.P("可用现金 <b>HK$ "+Fmt.Money(S.money)+"</b> · 基础生活预留 HK$ "+Fmt.Money(res)+" <size=80%>（余下 "+res/Cat.DailyCost+" 天 × HK$75，只作提示）</size> · 预计自由预算 <b><color="+(S.money-res<0?"#A7432F":"#3E2C22")+">HK$ "+Fmt.Money(S.money-res)+"</color></b>",16);
            var bar=f.Card(UI.Hex("F3E6CF"),UI.Hex("D8BF98"),14,10);bar.P("<b>"+(autoRun?"日子正在向前……":S.phase=="running"?"已暂停，可以调整未来的安排。":"准备好，开始这一轮生活。")+"</b>\n<size=82%>每天生活费 HK$75，按自然日结算一次；12 个月房租已在签约时付清，不再按月扣。</size>",17);
            bar.Row(Flow.Bt("速度 "+speed+"×",()=>{speed=speed==1?2:1;Render();},Look2.Secondary,true,"Speed"),autoRun?Flow.Bt("暂停",()=>{autoRun=false;Render();},Look2.Primary,true,"Pause"):Flow.Bt("▶ "+(S.phase=="running"?"继续日程":"开始日程"),()=>{dialog="review";Render();},Look2.Primary,true,"StartSchedule"));bar.End();
            LastLog(f,S.logs.LastOrDefault(),"");
        }
        void HalfSlot(RectTransform col,int i,float x,float y,float w,float h)
        {
            var t=S.plan[i];var b=Cat.BasicOf(t);var sess=Cal.SessionAt(S.round,i);bool done=i<Core.Pos(S);var fix=Core.FixedItem(S,S.round,i);bool off=fix!=null&&t!=fix;bool makeup=fix!=null&&fix.StartsWith("makeup:");
            Color fill=t==null?UI.Hex("FBF7EE"):t.StartsWith("class:")?UI.Hex("E5EEF6"):t.StartsWith("exam:")||t.StartsWith("makeup:")?UI.Hex("F6E2DC"):Core.JobId(t)!=null?UI.Hex("F3EBD6"):Core.Trip(t)!=null?UI.Hex("FCE9D2"):b!=null&&b.id=="rest"?UI.Hex("E9F0E2"):UI.Hex("F4EEE2");
            bool today=i==S.cursor&&S.phase=="running";var btn=UI.Btn(col,null,x,y,w,h,()=>{autoRun=false;slotSel=i;dialog="slot";Render();},Look2.Plain,!done,"Slot"+i);
            var bg=UI.Frame(btn.transform,0,0,w,h,fill,today?UI.Brick:UI.Hex("E3D5BD"),7,today?2.5f:1);bg.transform.SetAsFirstSibling();
            string mark=t==null?"＋":t.StartsWith("class:")?"课":t.StartsWith("exam:")||t.StartsWith("makeup:")?"考":Core.JobId(t)!=null?"工":Core.Trip(t)!=null?"游":Core.IsWork(t)?Cat.WorkMarks[Core.WorkKind(t)=="individual"?0:1]:b!=null?b.mark:"";
            UI.Text(btn.transform,Cal.Half(i%2==0?"am":"pm")+(done?" ✓":""),8,4,w-16,18,12,UI.Muted);
            UI.Text(btn.transform,"<color="+UI.Rich(UI.Brick)+">"+mark+"</color> "+Core.Label(t),8,22,w-16,46,15,done?UI.Muted:UI.Ink,true,TextAlignmentOptions.TopLeft,true);
            string note=makeup?"病假补考时段":sess!=null?(sess.kind=="class"?"课表：":"考试：")+Cat.CourseName(sess.course)+(sess.moveReason!=null?(sess.kind=="class"?"（假期补课）":"（因假期顺延）"):""):null;
            if(note!=null)UI.Text(btn.transform,note,8,h-26,w-16,22,11,off&&!done?UI.Warn:UI.Muted,false,TextAlignmentOptions.TopLeft,true);
        }
        void FillClasses(){var e=Core.FillFixed(S);PlanChange(e.plan,"已填入 "+e.added+" 个课堂／考试半日。"+(e.conflicts.Count>0?e.conflicts.Count+" 处冲突已保留原安排："+string.Join("、",e.conflicts.Select(SlotTitle))+"。":"已有安排保持不变。"));}
        void OpenReplace(){autoRun=false;if(replaceTarget==null||!S.hired.Contains(replaceTarget))replaceTarget=S.hired.OrderByDescending(j=>Cat.JobOf(j).pay).First();dialog="replace";Render();}
        // ---------- 求职 ----------
        void JobsPage(Flow f)
        {
            f.P("能力达标只获得申请资格。每个普通轮，所有岗位共享一次求职机会：查看、关闭不消耗；点“尝试求职”无论成败都会用掉。录用后当轮即可在日程里安排，所有兼职合计每轮最多 6 个半日。工资按岗位固定。",16,UI.Muted);
            var n=Note(f);n.P((Core.CanSeekJob(S)?(Core.TriedThisRound(S)?"本轮求职机会已用：申请「"+Cat.JobOf(S.lastAttempt.job).name+"」，"+(S.lastAttempt.success?"已录用":"未录用")+"（概率 "+Fmt.N(S.lastAttempt.chance)+"%）。":"本轮求职机会：可用。"):Cal.Rounds[S.round].kind=="winter"?"寒假不参加普通求职。":"求职只能在普通轮开始前的规划阶段进行。")+" 每次求职前会自动保存一份“求职前存档”，可在下方读取后重新求职（会恢复当时的完整进度）。",16);n.End();
            f.Row(Flow.Bt("读取求职前存档（替换当前进度）",LoadBeforeJob));
            float cw=(f.w-20)/2;int col=0;float rowY=f.y,rowH=0;
            foreach(var j in Cat.Jobs)
            {
                var lack=Core.Lacks(j,S.abilities);var ch=Core.Chance(j,S.abilities);bool hired=S.hired.Contains(j.id),can=Core.CanSeekJob(S)&&!Core.TriedThisRound(S)&&!hired&&ch!=null;
                var sub=new Flow(f.root,f.x+col*(cw+20),cw,rowY);var c=sub.Card(hired?UI.Hex("EEF3E6"):UI.Hex("FFFCF5"),UI.Line,14,8);
                c.P("<b>"+j.name+"</b>  <size=80%><color="+(hired?"#4F7A43":"#7D6858")+">"+(hired?"已录用":ch==null?"未达门槛":"可申请")+"</color></size>",19);
                c.P("半日工资 <b>HK$ "+j.pay+"</b> · "+Pace.Delta(j.fatigue,j.stress)+" · 每半日 "+Cat.AbilityName(j.growth)+" +"+Fmt.N(j.growthPerShift),15);
                c.P("门槛："+(j.reqKeys.Length==0?"无":string.Join("，",j.reqKeys.Select((k,i)=>Cat.AbilityName(k)+" ≥"+j.reqValues[i]+"（你 "+Fmt.F1(S.Ab(k))+"）"))),15);
                if(lack.Count>0)c.P("还差："+string.Join("，",lack.Select(m=>Cat.AbilityName(m.ability)+" "+Fmt.F1(m.required-m.current))),15,UI.Warn);else if(!hired)c.P("当前录用概率 <b>"+Fmt.N(ch.Value)+"%</b>",15);
                var jid=j.id;if(!hired)c.Row(Flow.Bt("尝试求职（用掉本轮机会）",()=>ApplyJob(jid),Look2.Primary,can,"Job:"+j.id));c.End();
                rowH=Mathf.Max(rowH,sub.y-rowY);col++;if(col==2){col=0;rowY+=rowH;rowH=0;}
            }
            f.y=rowY+rowH;f.Row(Flow.Bt("替换本轮兼职",OpenReplace,Look2.Secondary,S.hired.Count>0&&!Ended));f.P("本轮已排兼职 "+Core.JobCount(S.plan)+" / 6 个半日",15,UI.Muted);
        }
        void LoadBeforeJob()
        {
            var e=LiteSave.LoadBeforeJob();if(e==null){Toast("没有可读取的求职前存档。");Render();return;}var k=EventSoundKey(e.state);if(k!=null)eventSounds.Add(k);if(e.state.phase=="summary"||e.state.phase=="finished")moneyRounds.Add(e.state.round);
            autoRun=false;undo.Clear();Commit(e.state,e.lite,"已读取求职前存档：恢复到当时的完整进度，可以重新求职。");
        }
        // ---------- 背包 ----------
        static string GoodMark(string id){return id=="tea"?"茶":id=="bun"?"包":id=="lamp"?"灯":id=="plant"?"绿":"垫";}
        void Goods(Flow f,IEnumerable<Good> goods,Action<Flow,Good> body)
        {
            var list=goods.ToList();float cw=(f.w-40)/3;int col=0;float rowY=f.y,rowH=0;
            foreach(var g in list)
            {
                var sub=new Flow(f.root,f.x+col*(cw+20),cw,rowY);var c=sub.Card(UI.Hex("FFFCF5"),UI.Line,14,8);var icon=c.Block(56,6);var ic=UI.Box(icon,0,0,56,56,g.kind=="furniture"?UI.Hex("E9F0E2"):UI.Hex("FCE9D2"),12);
                UI.Text(ic.transform,GoodMark(g.id),0,0,56,56,24,UI.Brown2,true,TextAlignmentOptions.Center);UI.Text(icon,g.kind=="furniture"?"学长姐闲置":"楼下小店",68,16,cw-100,24,14,UI.Muted);
                body(c,g);c.End();rowH=Mathf.Max(rowH,sub.y-rowY);col++;if(col==3){col=0;rowY+=rowH;rowH=0;}
            }
            f.y=rowY+rowH;
        }
        void MarketPage(Flow f)
        {
            Intro(f,"给小屋添一点生活。","家具来自学长学姐的闲置；门口小店也备了些补给。");f.P("可用 HK$ "+Fmt.Money(S.money),17,UI.Brick,true);
            Goods(f,Cat.Goods,(c,g)=>{int k=Cat.GoodIndex(g.id);bool sold=S.purchased[k]>=g.stock||(g.kind=="furniture"&&S.inventory[k]>0);c.P("<b>"+g.name+"</b>",19,null,false,4);c.P(g.description,15,UI.Muted);
                c.Row(Flow.Bt("HK$ "+g.price+" · "+(S.inventory[k]>0&&g.kind=="furniture"?"已拥有":sold?"本轮售罄":"买下"),()=>{var t=S.Clone();var e=Core.Buy(t,g.id);if(e!=null){Toast(e);Render();return;}Commit(t,M,"已买下"+g.name+"，可在"+(g.kind=="item"?"随身道具":"家具布置")+"中使用。");},Look2.Primary,!sold&&S.money>=g.price&&!Ended,"Buy:"+g.id));
                c.P(g.kind=="furniture"?"每件家具限拥有一件":"本轮剩余 "+(g.stock-S.purchased[k])+" 份",13,UI.Muted);});
            f.Row(Flow.Bt("打开随身道具",()=>Nav("items")),Flow.Bt("布置已购家具",()=>Nav("furniture")));
        }
        void ItemsPage(Flow f)
        {
            Intro(f,"一点补给，慢慢恢复。","道具立即生效，不消耗日程。每轮小店补货两份，不能无限购买。");
            Goods(f,Cat.Goods.Where(g=>g.kind=="item"),(c,g)=>{int k=Cat.GoodIndex(g.id);c.P("<b>"+g.name+"</b>",19,null,false,4);c.P(g.description,15,UI.Muted);c.P("持有 "+S.inventory[k]+" 份",15);
                c.Row(Flow.Bt("使用一份",()=>{var t=S.Clone();Core.Use(t,g.id);Commit(t,M,g.name+"已使用，"+g.description+"。");},Look2.Primary,S.inventory[k]>=1&&!Ended,"Use:"+g.id));});
            f.Row(Flow.Bt("去市场看看",()=>Nav("market")));
        }
        void FurniturePage(Flow f)
        {
            Intro(f,"把出租屋，住成自己的小天地。","已购家具可摆放或收起，摆放后的效果会用于日程结算。主页的房间背景暂时没有家具叠层：摆放后效果照常用于结算，但主页画面不会出现或消失家具。");
            if(S.housing!=null){var e=Cat.HomeOf(S.housing);var t=Core.RestGain(S);var n=Note(f);n.P(e.name+"（"+(e.area??"面积待定")+"）· 家具槽位 "+S.installed.Count+" / "+e.slots+" · 每个休息半日 "+Pace.Delta(-t[0],-t[1])+"（家具加成封顶 精力 6、压力 4）· 已付 12 个月租金 HK$ "+Fmt.Money(S.rentPaid)+"，押金 HK$ "+Fmt.Money(S.deposit)+"。本版本不能中途换房。",15);n.End();}
            Goods(f,Cat.Goods.Where(g=>g.kind=="furniture"),(c,g)=>{int k=Cat.GoodIndex(g.id);bool on=S.installed.Contains(g.id);c.P("<b>"+g.name+"</b>",19,null,false,4);c.P(g.description,15,UI.Muted);c.P(on?"已摆放 · 效果生效":S.inventory[k]>0?"已拥有 · 收纳中":"尚未拥有",15,on?UI.Good:UI.Muted);
                c.Row(Flow.Bt(on?"收起":"摆放",()=>{var t=S.Clone();Core.Toggle(t,g.id);Commit(t,M,on?"已收起家具。":"已摆放，效果已生效。");},Look2.Primary,S.inventory[k]>0&&(on||Core.FreeSlots(S)>=1),"Place:"+g.id));});
            f.Row(Flow.Bt("去二手市场",()=>Nav("market")));
        }
        // ---------- 课程与作业 ----------
        static readonly Dictionary<string,string> TypeNames=new Dictionary<string,string>{{"exam","考试主导型"},{"comprehensive","综合考核型"},{"project","项目实践型"}};
        static string StatusName(Work e){return e.status=="submitted"?(e.late?"已迟交提交":"已按时提交"):e.status=="unreleased"?"尚未发布":e.status=="active"?"进行中":e.status=="complete"?"已完成 · 可打磨":e.status=="overdue"?"补交中":"缺交";}
        static string ProjectTitle(string c){var t=Cat.CourseOf(c);return t.project=="题目待定"?t.name+"小组项目（题目待定）":t.project;}
        static string DueText(string c,string k){return Cal.Date(Core.Due(c,k))+"（第 "+Cat.Load(c,k).dueWeek+" 周末）";}
        static string LetterOf(Work w){var t=Core.WorkLetter(w);return t!=null?Fmt.Letter(t):"—";}
        void CoursesPage(Flow f)
        {
            Intro(f,"先学会，再着手。","每门课各有一份个人作业（第 3 教学周发布）和一个小组项目（第 4 教学周发布）。完成后可以手动提交，也可以继续安排半日打磨；未手动提交的在截止日结束时自动提交。截止日仍未完成的进入 14 天补交期，期内完成立即迟交提交，得分为原始分 × 0.8；补交期结束仍未完成为 0 分。");
            f.P("每门课的成绩分三种：<b>尚未评估</b>（还没有任何分项评分，只看进度）、<b>已评定部分</b>（部分分项已出分，不是总分）、<b>正式成绩</b>（学期结束后公布）。未发布、未到期的作业不按 0 分计；真正缺交、无故缺考仍为 0 分并按原权重计入。",16,UI.Muted);
            if(Details(f,"grading","完整评分规则"))f.P("课程总评按原权重计入：作业（按时＝作品质量，迟交 × 0.8，缺交 0）与考试（10 ＋ 0.70 × 掌握度 ＋ 0.20 × 研究分析；精力低于 40（即疲劳＝100−精力 超过 60）或压力超过 60 的部分各按 0.15 扣分；无故缺考 0 分，病假缺考在第 16 周补考）。学期结束后给出正式成绩；普通课堂出勤低于 70% 强制 F。",15,UI.Muted);
            foreach(int sem in new[]{1,2})
            {
                Step(f,SemesterName(sem));float cw=(f.w-20)/2;int col=0;float rowY=f.y,rowH=0;
                foreach(var c in Cat.Courses.Where(x=>x.semester==sem))
                {
                    var sub=new Flow(f.root,f.x+col*(cw+20),cw,rowY);var card=sub.Card(UI.Hex("FFFCF5"),UI.Line,16,8);var a=S.Attendance(c.id);var l=Cat.LessonOf(c.id);var rate=Core.AttendanceRate(a);
                    card.P("<b>"+c.name+"</b>\n<size=80%><color="+UI.Rich(UI.Muted)+">"+c.teacher+" · 每"+Cal.Weekdays[l.weekday]+Cal.Half(l.half)+" · "+TypeNames[l.type]+"</color></size>",20);
                    card.P("主能力 "+Cat.AbilityName(Cat.CourseAbility[c.id])+" · 掌握度 "+Fmt.F1(S.Mastery(c.id))+"\n已听课 "+a.attended+" 次"+(a.excused>0?" · 病假 "+a.excused+" 次":"")+" · 出勤 "+(rate==null?"—":Mathf.RoundToInt((float)rate.Value*100)+"%"),15,UI.Muted);
                    Standing(card,c.id);foreach(var k in new[]{"individual","group"})Sheet(card,k,c.id);card.End();
                    rowH=Mathf.Max(rowH,sub.y-rowY);col++;if(col==2){col=0;rowY+=rowH;rowH=0;}
                }
                f.y=rowY+rowH;
            }
            f.Row(Flow.Bt("回到行程安排  →",()=>Nav("planner"),Look2.Primary));
        }
        void Standing(Flow f,string course)
        {
            var n=Pace.CourseStanding(S,course);var c=f.Card(UI.Hex("F6EBDA"),UI.Line,12,6);
            c.P("<size=80%>"+(n.status=="official"?"正式成绩":n.status=="partial"?"已评定部分":n.status=="unknown"?"无法评估":"尚未评估")+"</size>  <b>"+n.headline+"</b>",16,n.failed||n.status=="unknown"?UI.Warn:UI.Ink);
            if(n.next!=null)c.P("<b>现在：</b>"+n.next,15);foreach(var r in n.reasons)c.P(r,15,UI.Warn);if(n.attendanceRisk)c.P("出勤率目前低于 70%：学期结束时仍不足将强制 F。",15,UI.Warn);
            if(Details(c,"parts:"+course,"分项与覆盖范围（"+n.parts.Count(p=>p.score!=null)+"/"+n.parts.Count+" 项已评定）"))
            {
                foreach(var p in n.parts)c.P(p.name+" · 权重 "+p.weight+"%：<color="+UI.Rich(UI.Muted)+">"+p.text+"</color>",14);
                if(n.status=="partial")c.P("已评定的分项按权重合计 "+n.counted+" 分（满分为已覆盖的 "+n.covered+" 分），这不是课程总分；其余分项未评分前不计 0 分。",14,UI.Muted);
            }
            c.End();
        }
        void Sheet(Flow f,string kind,string course)
        {
            var o=Cat.CourseOf(course);var s=Core.WorkOf(S,kind,course);int total=Core.Total(kind,course);var l=Cat.Load(course,kind);double d=Core.PolishGain(s);
            f.P(Core.KindName(kind)+" · "+StatusName(s)+" · 截止 "+DueText(course,kind),14,UI.Brick,true,4);
            if(s.status=="unreleased"){f.P("<b>等待老师发布</b>\n第 "+l.releaseWeek+" 教学周的课堂时段发布，缺课也会收到通知。",15,UI.Muted);return;}
            f.P("<b>"+(kind=="individual"?o.assignment:ProjectTitle(course))+"</b>",17,null,false,4);if(kind=="individual")f.P(o.requirement,14,UI.Muted);
            var bar=f.Block(8,4);UI.Bar(bar,0,0,f.w,8,(float)(s.progress/total*100),UI.Sage);
            var p=Core.TeamReady(S.Proj(course))&&kind=="group"?" · 小组：你、"+string.Join("、",S.Proj(course).members.Select(x=>S.Npc(x).name)):"";
            f.P("完成度 "+Fmt.N(s.progress)+" / "+total+(s.polish>0?" · 已打磨 "+s.polish+" 个半日":"")+p,14,UI.Muted);
            if(s.status=="complete")
            {
                f.P("作品质量 <b>"+Fmt.N(s.quality.Value)+"</b> · 作品预计等级 <b>"+LetterOf(s)+"</b>（提交后才计分，不是课程正式成绩）· 已打磨 "+s.polish+" 次 · "+(d>0?"下一次打磨 +"+Fmt.N(d):"已达上限 100，继续打磨不再提高质量"),15);
                f.P("可以现在提交，也可以继续安排半日打磨；未手动提交的会在 "+Cal.Date(Core.Due(course,kind))+" 截止日结束时自动提交。",14,UI.Muted);
                f.Row(Flow.Bt("现在提交（分数锁定，之后不能再打磨）",()=>{var t=S.Clone();var e=Core.SubmitNow(t,kind,course);if(e!=null){Toast(e);Render();return;}Commit(t,M,"已提交「"+(kind=="individual"?o.assignment:ProjectTitle(course))+"」，之后不能再打磨。");},Look2.Primary,!Ended&&!autoRun,"Submit:"+kind+":"+course));
            }
            if(s.status=="overdue")f.P("已过截止日。"+Cal.Date(Core.LateDue(course,kind))+"（补交期最后一天）结束前完成会立即迟交提交，得分为原始分 × 0.8；仍未完成为 0 分。",14,UI.Warn);
            if(s.status=="submitted")f.P(s.score==null?Cal.Date(s.submittedDay.Value)+(s.late?"迟交":"按时")+"提交。旧档作品：提交时还没有作品质量记录，分数无法还原，暂不计入课程总评。":Cal.Date(s.submittedDay.Value)+(s.late?"迟交提交：作品质量 "+Fmt.N(s.quality.Value)+" × 0.8 ＝ 最终得分 "+Fmt.N(s.score.Value):"按时提交：最终得分 "+Fmt.N(s.score.Value))+"（"+LetterOf(s)+"）。",15,s.late?UI.Warn:UI.Good);
            if(s.status=="missing")f.P("补交期结束仍未完成：缺交，得分 0，按原权重计入课程总评。",14,UI.Warn);
            if(kind=="group"&&s.status!="submitted"&&s.status!="missing"&&!Core.TeamReady(S.Proj(course)))f.Row(Flow.Bt("去组队与分工  →",()=>{teamCourse=course;Nav("team");}));
        }
        // ---------- 小组与同学 ----------
        string Skills(Npc n){return string.Join("  ",Cat.Abilities.Select((k,i)=>Cat.AbilityNames[i].Substring(0,2)+" <b>"+Fmt.N(n.abilities[i])+"</b>"));}
        string Member(string course,string npc)
        {
            var r=S.Proj(course);if(r.members.Contains(npc)){var role=Cat.Roles.FirstOrDefault(x=>r.roles[x]==npc);return "本组成员"+(role!=null?" · "+Core.RoleName(course,role):" · 待分工");}return r.Invited(npc)?"本项目已邀请过（未成功）":null;
        }
        void TeamPage(Flow f)
        {
            int sem=Cat.CourseOf(teamCourse).semester;Intro(f,"把各自的长处，放到合适的位置。","每门课各有一个小组项目：你与两位同学组成三人小组。先邀请同学加入——同一项目对同一人只能邀请一次；同学四项能力的平均越高越难请到，好感与你的声望会提高成功率。正式邀请 2 次后仍缺人，可以公开招募普通同学补齐。人齐后再分配三个岗位（会给出推荐分工），开工前可以自由调整；第一次实际进行小组作业后，成员与分工锁定。");
            var n=Note(f);n.P("你的声望 <b>"+S.reputation+"</b> / 100（同学圈对你的合作评价）。小组项目提交时：按时每位组员好感 +5、声望 +2，迟交 −3／−2，并按最终等级再加减声望（A +4 … F −4）；缺交声望 −12、每位实际组员好感 −6（没组队也扣声望）。候选同学的能力分布是暂定的候选设置。",15);n.End();
            var tabs=f.Block(56);float x=0;
            foreach(var c in Cat.Courses.Where(c=>c.semester==sem||S.Proj(c.id).status!="unreleased"))
            {
                var p=S.Proj(c.id);string label=c.name+"\n<size=75%>"+StatusName(p)+(Core.TeamReady(p)?(p.locked?" · 已开工":" · 已组队"):"")+"</size>";float bw=Mathf.Max(150,UI.Width(c.name,16)+40);var cc=c.id;
                UI.Btn(tabs,label,x,0,bw,54,()=>{teamCourse=cc;roleDraft=null;Render();},teamCourse==c.id?Look2.Selected:Look2.Tab,true,"Course:"+c.id,"page",16);x+=bw+8;
            }
            TeamDetail(f,teamCourse);Roster(f,teamCourse);
        }
        void TeamDetail(Flow f,string t)
        {
            var s=S.Proj(t);var c=Cat.CourseOf(t);var lockMsg=Core.TeamLock(S,t);bool full=s.members.Count>=2,ready=Core.TeamReady(s);
            if(s.status=="unreleased")
            {
                var e=Note(f);e.P("<b>"+c.name+"的小组项目尚未发布</b>",19);e.P("在第 "+Cat.Load(t,"group").releaseWeek+" 教学周的课堂时段发布，缺课也会收到通知。截止："+DueText(t,"group")+"。",16);e.Row(Flow.Bt("先去安排行程  →",()=>Nav("planner"),Look2.Primary));e.End();return;
            }
            if(roleDraftFor!=t+":"+string.Join(",",s.members)){roleDraft=new Dictionary<string,string>(s.roles);roleDraftFor=t+":"+string.Join(",",s.members);}
            var brief=f.Card(UI.Hex("FFF8EC"),UI.Hex("D8BF98"));brief.P(c.name+" · "+StatusName(s)+" · 截止 "+DueText(t,"group"),14,UI.Brick,true,2);brief.P("<b>"+ProjectTitle(t)+"</b>",20);
            var bar=brief.Block(8,4);UI.Bar(bar,0,0,brief.w,8,(float)(s.progress/Core.Total("group",t)*100),UI.Sage);
            brief.P("完成度 "+Fmt.N(s.progress)+" / "+Core.Total("group",t)+(s.quality==null?"":" · 作品质量 "+Fmt.N(s.quality.Value)+"（"+(s.status=="submitted"||s.status=="missing"?"":"预计 ")+LetterOf(s)+"）")+(s.polish>0?" · 已打磨 "+s.polish+" 次":"")+(ready?" · 当前分工每个半日协作 +"+Fmt.N(Core.Rate(S,"group",t)):""),14,UI.Muted);brief.End();
            Step(f,"01 · 当前队伍",s.members.Count+" / 2 位组员 · 已正式邀请 "+s.invites.Count+" 次"+(lockMsg!=null?" · "+lockMsg:""));
            if(full){foreach(var role in Cat.Roles){var who=s.roles[role];var npc=who!=null&&who!="self"?S.Npc(who):null;var ab=Core.RoleAbility(t,role);f.P(Core.RoleName(t,role)+" · "+Cat.AbilityName(ab)+"：<b>"+(who=="self"?UI.Safe(S.name)+"（你，"+Fmt.F1(S.Ab(ab))+"）":npc!=null?npc.name+"（"+Fmt.N(npc.Ab(ab))+"）":"未分配")+"</b>",16,null,false,4);}}
            else{f.P("你：<b>"+UI.Safe(S.name)+"</b>",16,null,false,4);for(int i=0;i<2;i++){var npc=i<s.members.Count?S.Npc(s.members[i]):null;f.P("组员 "+(i+1)+"：<b>"+(npc!=null?npc.name+"（综合 "+Fmt.F1(npc.abilities.Average())+"）":"空缺")+"</b>",16,null,false,4);}}
            f.Space(6);
            if(full&&lockMsg==null)
            {
                f.P("开工前可以调整三人的分工（不会重新抽邀请概率）；第一次实际进行这门课的小组作业后锁定。每半日进度按三人各自岗位能力计算（你用实际能力）。",15,UI.Muted);
                var people=new List<string>{"self"};people.AddRange(s.members);
                f.Row(Cat.Roles.Select(role=>{var who=roleDraft[role];var ab=Core.RoleAbility(t,role);string name=who=="self"?UI.Safe(S.name)+"（你）":S.Npc(who)!=null?S.Npc(who).name:"未分配";var rr=role;
                    return Flow.Bt(Core.RoleName(t,role)+"："+name,()=>Pick(Core.RoleName(t,rr),people.Select(x=>new KeyValuePair<string,string>(x,x=="self"?UI.Safe(S.name)+"（你）· "+Fmt.F1(S.Ab(ab)):S.Npc(x).name+" · "+Fmt.N(S.Npc(x).Ab(ab)))).ToList(),v=>roleDraft[rr]=v));}).ToArray());
                bool ok=Core.TeamReady(new Project{members=s.members,roles=roleDraft});bool same=Cat.Roles.All(k=>roleDraft[k]==s.roles[k]);
                f.Row(Flow.Bt("确认分工",()=>{var x=S.Clone();var e=Core.SetRoles(x,t,roleDraft);if(e!=null){Toast(e);Render();return;}Commit(x,M,"分工已更新。");},Look2.Secondary,ok&&!same),
                    Flow.Bt("恢复推荐分工",()=>{roleDraft=Core.BestRoles(S,t);Render();}),Flow.Bt("安排小组作业  →",()=>Nav("planner"),Look2.Primary));
                if(!ok)f.P("三人需要各负责一个不同岗位。",15,UI.Warn);
            }
            if(!full&&lockMsg==null)
            {
                Step(f,"02 · 候选同学","每批 4 张；本项目还可额外刷新 "+(s.refreshed?0:1)+" 次");
                foreach(var id in s.shown)
                {
                    var a=S.Npc(id);var other=Cat.Courses.Where(r=>r.id!=t&&S.Proj(r.id).members.Contains(id)).Select(r=>r.name).ToList();var st=Member(t,id);var card=f.Card(s.members.Contains(id)?UI.Hex("EEF3E6"):UI.Hex("FFFCF5"),UI.Line,14,8);
                    card.P("<b>"+a.name+"</b>  <size=82%>综合能力 "+Fmt.F1(a.abilities.Average())+" · 对你的好感 "+S.affection[id]+(other.Count>0?" · 也在："+string.Join("、",other):"")+"</size>",18);card.P(Skills(a),15,UI.Muted);
                    if(st!=null)card.P(st,15,UI.Muted);else{var nid=id;card.Row(Flow.Bt("邀请加入项目 · 成功率 "+Fmt.N(Core.InviteChance(S,id))+"%",()=>Invite(t,nid),Look2.Primary,true,"Invite:"+id));}card.End();
                }
                if(s.shown.Count==0)f.P("没有可展示的候选同学。",15,UI.Muted);
                f.Row(Flow.Bt("换一批候选",()=>{var x=S.Clone();var e=Core.Refresh(x,t);if(e!=null){Toast(e);Render();return;}Commit(x,M,"换了一批候选同学；本项目的额外刷新已用完。");},Look2.Secondary,!s.refreshed),
                    Core.CanRecruit(S,t)?Flow.Bt("公开招募普通同学（补齐空位，必定成功）",()=>{var x=S.Clone();List<string> add;var e=Core.Recruit(x,t,out add);if(e!=null){Toast(e);Render();return;}Commit(x,M,"公开招募到 "+string.Join("、",add.Select(i=>x.Npc(i).name))+"，队伍已满员，已给出推荐分工，开工前可以调整。");},Look2.Primary):null);
                if(!Core.CanRecruit(S,t))f.P("正式邀请 2 次后仍缺人，可公开招募普通同学补齐。",15,UI.Muted);
            }
            if(ready)f.P(c.name+"小组：你、"+string.Join("、",s.members.Select(i=>S.Npc(i).name))+"。"+(s.locked?"已开工，成员与分工锁定。":""),16,UI.Good);
        }
        float inviteLock;
        void Invite(string course,string npc)
        {
            if(Time.unscaledTime<inviteLock)return;inviteLock=Time.unscaledTime+.3f;var x=S.Clone();bool ok;double chance;var e=Core.Invite(x,course,npc,UnityEngine.Random.value*0.9999999,out ok,out chance);
            if(e!=null){Toast(e);Render();return;}var name=S.Npc(npc).name;Commit(x,M,ok?name+"答应加入（成功率 "+Fmt.N(chance)+"%）。"+(x.Proj(course).members.Count==2?"两位组员到齐，已按能力给出推荐分工，开工前可以调整。":""):name+"这次没有答应（成功率 "+Fmt.N(chance)+"%）。本项目不能再邀请同一个人。");
        }
        void Roster(Flow f,string t)
        {
            var known=S.known.Select(S.Npc).Where(x=>x!=null).ToList();if(known.Count==0)return;var r=S.Proj(t);bool can=Core.TeamLock(S,t)==null&&r.members.Count<2;
            Step(f,"同学录","认识的 "+known.Count+" 位同学；好感跨课程保留");
            foreach(var a in known)
            {
                var with=Cat.Courses.Where(c=>S.Proj(c.id).members.Contains(a.id)).Select(c=>c.name).ToList();var st=Member(t,a.id);var row=f.Card(UI.Hex("FFFCF5"),UI.Line,12,6);
                row.P("<b>"+a.name+"</b>  <size=82%><color="+UI.Rich(UI.Muted)+">"+(a.kind=="pool"?"候选同学":a.kind=="fallback"?"普通同学（公开招募）":"旧版同学")+" · 好感 "+S.affection[a.id]+(with.Count>0?" · 合作："+string.Join("、",with):"")+"</color></size>",17);row.P(Skills(a),14,UI.Muted);
                if(a.kind=="pool"&&can&&st==null&&!r.shown.Contains(a.id)){var nid=a.id;row.Row(Flow.Bt("邀请加入项目 · 成功率 "+Fmt.N(Core.InviteChance(S,a.id))+"%",()=>Invite(t,nid),Look2.Primary,true,"Invite:"+a.id));}else if(st!=null)row.P(st,14,UI.Muted);row.End(8);
            }
        }
        // ---------- 手账与回忆 ----------
        void JournalPage(Flow f)
        {
            Intro(f,"那些普普通通，却属于你的日子。","默认只看本轮重点和需要留意的事；完整记录按半日保留，日期、活动与实际变化都在。");
            var all=S.history.Concat(S.logs).ToList();
            if(all.Count==0){f.P("<b>手账的第一页，还空着。</b>\n安排日程并开始生活，故事会慢慢写下来。",19,null,false,16,TextAlignmentOptions.Center);f.Row(Flow.Bt("打开本轮安排",()=>Nav("lite"),Look2.Primary));return;}
            if(journalFull)
            {
                f.Row(Flow.Bt("返回本轮重点",()=>{journalFull=false;Render();},Look2.Secondary,true,null,"page"));f.P("完整记录 "+all.Count+" 条（新的在前）：日期、活动与实际变化",15,UI.Muted);
                foreach(var l in Enumerable.Reverse(all))
                {
                    var c=f.Card(UI.Hex("FFFCF5"),UI.Line,14,6);c.P(l.date+Cal.Half(l.half)+" · <b>"+l.title+"</b>",16,UI.Brick);var loc=Pace.TripOfLog(l);
                    if(loc!=null&&Pace.HasScenery(loc)){var tex=UI.Tex(Pace.Trips[loc].scenery.src);c.Image(tex,200,()=>Zoom(tex,Pace.Trips[loc].scenery.alt));}
                    c.P(Pace.Clean(l.text),15);c.P("实际变化："+Pace.Changes(l),13,UI.Muted);c.End(8);
                }
                return;
            }
            Step(f,"第 "+(S.round+1)+" 轮重点","最多三条；没有发生的事不写");var hl=Pace.Highlights(S).Take(3).ToList();
            if(hl.Count==0)f.P("这一轮还没有记录。",16,UI.Muted);else foreach(var h in hl){f.P(h.label,15,UI.Muted,true,2);f.P(h.text,17);}
            var att=Pace.Attention(S);if(att.Count>0){var a=Alert(f);a.P("<b>需要留意</b>",18,UI.Hex("8C3A2A"));foreach(var x in att)a.P("· "+x,15);a.End();}
            LastLog(f,all.Last(),"最近一条 · ");f.Row(Flow.Bt("查看完整记录（"+all.Count+" 条）",()=>{journalFull=true;Render();},Look2.Primary,true,null,"page"));
        }
        void MemoriesPage(Flow f)
        {
            Intro(f,"这段时间留下的事。","只记录真正发生过的事件、合作与完成的旅行；重复打开、翻看照片不会改变任何数值。");
            var mem=Pace.Memories(S);var trips=Pace.TripMemories(S,M);
            if(mem.Count==0&&trips.Count==0){f.P("<b>还没有回忆</b>\n出门散步、和同学一起做事、出去玩一整天，都会留下记录；没有发生的事不会出现在这里。",18,null,false,16,TextAlignmentOptions.Center);return;}
            var tabs=f.Block(44);float x=0;foreach(var t in new[]{new[]{"all","全部 "+(mem.Count+trips.Count)},new[]{"life","事件与合作 "+mem.Count},new[]{"travel","旅行 "+trips.Count}}){var k=t[0];float bw=UI.Width(t[1],16)+40;UI.Btn(tabs,t[1],x,0,bw,42,()=>{memoryFilter=k;Render();},memoryFilter==k?Look2.Selected:Look2.Tab,true,"Mem:"+k,"page",16);x+=bw+8;}
            var items=new List<KeyValuePair<int,Action<Flow>>>();
            if(memoryFilter!="travel")foreach(var m in mem){var mm=m;items.Add(new KeyValuePair<int,Action<Flow>>(m.day,ff=>MemoryCard(ff,mm)));}
            if(memoryFilter!="life")foreach(var t in trips){var tt=t;items.Add(new KeyValuePair<int,Action<Flow>>(t.day,ff=>TripCard(ff,tt)));}
            if(items.Count==0)f.P(memoryFilter=="travel"?"还没有完成过全天游玩。只计划、上午出发或因病取消的旅行不会记在这里。":"这一类还没有回忆。",16,UI.Muted);
            foreach(var it in items.OrderBy(i=>i.Key))it.Value(f);
        }
        void MemoryCard(Flow f,Memory m)
        {
            var c=f.Card(UI.Hex("FFFCF5"),UI.Line,14,8);var media=Pace.CgFor(m.cg,Pace.LookOf(M,m.key));if(media!=null&&!failedCg.Contains(m.cg)){var tex=UI.Tex(media.src);if(tex!=null)c.Image(tex,280,()=>Zoom(tex,media.alt));}
            c.P(Cal.Date(m.day),14,UI.Brick,true,2);c.P("<b>"+m.title+"</b>",18,null,false,4);c.P(m.text,16);c.End();
        }
        void TripCard(Flow f,TripMemory t)
        {
            var c=f.Card(UI.Hex("FFFCF5"),UI.Line,14,8);string mode;if(!photoMode.TryGetValue(t.key,out mode))mode="scenery";var tr=Pace.HasScenery(t.loc)?Pace.Trips[t.loc]:null;
            if(t.scenery&&tr!=null){var media=t.selfie&&mode=="selfie"?tr.selfie:tr.scenery;var tex=UI.Tex(media.src);if(tex!=null)c.Image(tex,280,()=>Zoom(tex,media.alt));
                if(t.selfie)c.Row(Flow.Bt("风景",()=>{photoMode[t.key]="scenery";Render();},mode=="scenery"?Look2.Selected:Look2.Tab,true,null,"page"),Flow.Bt("自拍",()=>{photoMode[t.key]="selfie";Render();},mode=="selfie"?Look2.Selected:Look2.Tab,true,null,"page"));}
            c.P(t.date,14,UI.Brick,true,2);c.P("<b>全天游玩 · "+t.name+"</b>",18,null,false,4);c.P(t.text,16);if(t.backfilled)c.P("旧存档里的旅行：按当时的记录补上风景与日期（没有当时的角色记录，不补自拍）。",14,UI.Muted);c.End();
        }
        // ---------- 目标、回顾、消息、状态 ----------
        void GoalsPage(Flow f)
        {
            Intro(f,"四轮样例的目标","每轮一个目标，只按已经发生的事判断；它不是通关条件，也不影响评分。毕业去向等长期路线不在本样例内。");
            foreach(var g in Pace.Goals(S))
            {
                var c=f.Card(g.status=="done"?UI.Hex("EEF3E6"):g.status=="current"?UI.Hex("FFF8EC"):UI.Hex("FFFCF5"),g.status=="current"?UI.Brick:UI.Line,14,8,g.status=="current"?2:1.5f);
                c.P("第 "+(g.round+1)+" 轮  <size=80%><color="+UI.Rich(UI.Muted)+">"+g.range+"</color></size>",15,UI.Brick,true,2);
                c.P("<b>"+g.title+"</b>  <size=80%><color="+(g.status=="done"?"#4F7A43":g.status=="missed"?"#A7432F":"#7D6858")+">"+Pace.GoalLabels[g.status]+"</color></size>",18);
                if(!string.IsNullOrEmpty(g.next))c.P(g.next,16);if(g.note!=null)c.P(g.note,15,UI.Muted);c.End();
            }
            f.P("现在："+Pace.PhaseLabel(S.round)+" · 倾向「"+Pace.FocusName(M.focus)+"」 · 第 "+(S.round+1)+" / 4 轮。",15,UI.Muted);
        }
        void ReviewPage(Flow f)
        {
            Intro(f,"第 "+(S.round+1)+" 轮到目前为止","只整理已经发生的事，不发奖励；这一轮结束时会有完整的两周回顾。");
            var e=Pace.Highlights(S).Take(3).ToList();if(e.Count==0)f.P("这一轮还没有记录。",16,UI.Muted);else foreach(var h in e){f.P(h.label,15,UI.Muted,true,2);f.P(h.text,17);}
            f.Row(Flow.Bt("回看回忆",()=>Nav("memories")),Flow.Bt("完整手账",()=>Nav("journal")));
        }
        void MessagesPage(Flow f)
        {
            var todo=Pace.Todo(S,M);Step(f,"眼下要处理",todo.Count>0?todo.Count+" 件":"没有");
            if(todo.Count>0){foreach(var t in todo)f.P("· "+t,17);f.Row(Flow.Bt("回到主页，按主按钮处理",()=>Nav("room"),Look2.Primary));}else f.P("没有需要你处理的事。",16,UI.Muted);
            var notes=Enumerable.Reverse(M.digest).ToList();Step(f,"已发生的通知","课程发布与系统安排，共 "+notes.Count+" 条；不是同学私信");
            if(notes.Count==0)f.P("还没有通知。",16,UI.Muted);foreach(var n in notes)f.P("<size=80%><color="+UI.Rich(UI.Muted)+">第 "+(n.round+1)+" 轮 · "+Cal.Date(n.day)+"</color></size>\n"+n.text,16);
        }
        void StatusPage(Flow f)
        {
            int res=Core.Reserve(S);f.P("现金与预算",20,Title2,true);
            f.P("可用现金 <b>HK$ "+Fmt.Money(S.money)+"</b>"+(S.housing!=null?"；押金 HK$ "+Fmt.Money(S.deposit)+" 在租期内不可用，不算现金；"+Cat.HomeOf(S.housing).name+"的 12 个月租金 HK$ "+Fmt.Money(S.rentPaid)+" 已在签约时付清":"")+"。",16);
            f.P("基础生活预留 HK$ "+Fmt.Money(res)+"（余下 "+res/Cat.DailyCost+" 天 × HK$75，只作提示）· 预计自由预算 <b><color="+(S.money-res<0?"#A7432F":"#3E2C22")+">HK$ "+Fmt.Money(S.money-res)+"</color></b>",16);
            var last=Pace.LastLog(S);StatBar(f,"精力",Pace.EnergyOf(S.fatigue),Pace.EnergyLevel(S.fatigue),last!=null?Pace.Neg(last.fatigue):(int?)null,true,Pace.SickEnergy);
            StatBar(f,"压力",S.stress,Pace.StressLevel(S.stress),last!=null?last.stress:(int?)null,false,Pace.SickStress);
            f.P("升／降：上个半日的实际变化 · 竖线：病假线",14,UI.Muted);
            if(Details(f,"status-rules","状态说明"))f.P("精力＝100−疲劳：上课、作业、兼职会消耗精力，休息、散步会恢复。若一天开始时精力 ≤ "+Pace.SickEnergy+"（疲劳 ≥ 90）或压力 ≥ "+Pace.SickStress+"，当天安排病假（医疗费 HK$180，有课记为病假）。考试按开考前的状态计分，精力低于 40 或压力高于 60 的部分会扣分。系统预测会太累时，会把复习、兼职、出门等推荐安排换成休息。",15,UI.Muted);
            var r=Cal.Rounds[S.round];int day=Core.Today(S);string half=S.phase=="running"?Cal.SlotOf(S.round,Math.Min(S.cursor,S.plan.Count-1)).half:null;
            f.P(SemesterName(r.semester)+(r.semester>0?" · 第 "+((day-Cal.SemStart(r.semester))/7+1)+" 周":"")+" · "+Cal.Date(day)+Cal.Half(half)+" · 第 "+(S.round+1)+" 轮（四轮样例；完整一年共 "+Cal.Rounds.Length+" 轮）。窗外的夕阳只是画面，不代表游戏里的时刻。",14,UI.Muted);
        }
        void StatBar(Flow f,string name,int value,string[] level,int? change,bool upGood,int line)
        {
            string ch=change==null?"":change==0?"持平":(change>0?"升 ":"降 ")+Math.Abs(change.Value);Color cc=change==null||change==0?UI.Muted:(change>0)==upGood?UI.Good:UI.Warn;
            f.P("<b>"+name+"</b>  <size=85%>"+(level[1]!="ok"?"！":"")+level[0]+"  <color="+UI.Rich(cc)+">"+ch+"</color></size>  <b>"+value+"</b><size=80%> / 100</size>",18,null,false,4);
            var b=f.Block(14);Color c=level[1]=="risk"?UI.Hex("B9473A"):level[1]=="low"?UI.Hex("D99A3C"):UI.Sage;UI.Bar(b,0,2,f.w,10,value,c);UI.Box(b,f.w*line/100f,0,2,14,UI.Brown);
        }
        void GameSettings(Flow f)
        {
            var items=new[]{new[]{"display","显示"},new[]{"sound","声音"},new[]{"save","存档"},new[]{"profile","角色与说明"}};var tabs=f.Block(44);float x=0;
            foreach(var t in items){var k=t[0];float bw=UI.Width(t[1],16)+40;UI.Btn(tabs,t[1],x,0,bw,42,()=>{gameTab=k;Render();},gameTab==k?Look2.Selected:Look2.Tab,true,"Settings:"+k,"page",16);x+=bw+8;}
            if(gameTab=="display")DisplaySettings(f);else if(gameTab=="sound")SoundSettings(f);
            else if(gameTab=="save")
            {
                f.P("存档",21,Title2,true);f.P(saveFailed?"这一步没能写入存档：请先导出存档，否则关闭游戏后进度可能丢失。":"进度自动保存在"+Where+"（只使用轻松版自己的存档，与经典版互不读写）。",16,saveFailed?UI.Warn:UI.Ink);
                f.P("换电脑或换新版试玩包前，请先导出存档（文件名以 HK-Master-Life-LITE-save 开头）；原版的存档文件不能导入轻松版，网页版轻松版导出的存档可以导入。",16);
                f.Row(Flow.Bt("导出存档",Export),Flow.Bt("导入存档",Import),Flow.Bt("读取求职前存档",LoadBeforeJob));
                f.Row(Flow.Bt("返回开始页面",()=>{autoRun=false;dialog=null;win="room";titleLayer=null;screen="title";Render();},Look2.Secondary,true,"BackToTitle","close"));
                f.P("游戏会暂停，进度和没处理完的事都留着，“继续游戏”回到这里。"+ImportHelp(),15,UI.Muted);f.P("版本："+Build,15,UI.Muted);
            }
            else
            {
                f.P("主角档案",21,Title2,true);var draftState=S.Clone();
                ProfileEditor(f,draftState,d=>{S.appearance=d.appearance.Clone();Commit(S,M);});
                f.Row(Flow.Bt("保存名字",()=>{S.name=draftState.name;Commit(S,M,"已保存。");}));
                f.P("试玩说明",21,Title2,true);
                f.P("这是《港硕生活模拟器》的<b>轻松版四轮样例</b>：沿用原版的日历、课程、作业、经济与状态规则；每两轮为一个阶段，阶段开始时选一次倾向（学业成长／香港生活／工作体验），日常排程交给系统，预测会太累时自动换成休息，也可以随时“放轻松一点”。普通通知合并显示，遇到选择和关键结果才停下。样例在第 4 轮结束时结束，不是完整一年，也没有毕业结局。“详细日历”是原版的 28 格计划，可以手动修改。",16);
                f.P("精力＝100−疲劳：工作消耗精力，休息恢复精力。每天日初判断是否生病：精力降到 10 或以下（即疲劳 ≥ 90）或压力达到 90 时，当天上午和下午都改为休养并支付一次医疗费。有课时记为获批病假。课程最终出勤不足 70% 为 F，病假不算无故缺勤。",16);
                f.Row(Flow.Bt("重新开始",()=>{dialog="reset";Render();},Look2.Danger),Flow.Bt("打开经典版（17 轮）",OpenClassic,Look2.Secondary,true,"OpenClassic"));
            }
        }
    }
}
