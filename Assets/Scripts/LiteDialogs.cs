using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// 轻松版弹出窗口：半日选择、替换兼职、批量填充、开始确认、重新开始、课程通知、随机事件、结果与旅行回顾。
namespace HarbourYear.Lite
{
    public partial class LiteGame
    {
        void DrawGameOverlays()
        {
            if(dialog=="slot")SlotDialog();else if(dialog=="replace")ReplaceDialog();else if(dialog=="batch")BatchDialog();else if(dialog=="review")StartDialog();else if(dialog=="reset")ResetDialog();
            if(S.notice!=null)NoticeDialog();
            var kind=Pace.PendingKind(S,M);
            if(kind=="event"&&laterKey!=EventDialogKey(S))EventDialog();
            if(kind=="result"){var card=Pace.PendingCard(S,M);if(laterKey!=card.key)ResultDialog(card);}
            if(kind=="trip"){var t=Pace.PendingTrip(M);if(laterKey!=t.key)TripDialog(t);}
        }
        void Close(){dialog=null;Render();}
        // 选项按钮：标题加一行说明，可禁用。
        void Option(Flow f,string mark,string title,string sub,Action click,bool enabled=true,string id=null)
        {
            string text="<b>"+title+"</b>"+(string.IsNullOrEmpty(sub)?"":"\n<size=82%><color="+UI.Rich(enabled?UI.Muted:UI.Hex("A99A8A"))+">"+sub+"</color></size>");
            var t=UI.Text(f.root,text,f.x+64,0,f.w-80,10,18);float h=Mathf.Max(56,UI.Measure(t,f.w-80)+20);Destroy(t.gameObject);
            var b=UI.Btn(f.root,null,f.x,f.y,f.w,h,click,Look2.Secondary,enabled,id);var m=UI.Box(b.transform,12,(h-40)/2,40,40,enabled?UI.Hex("F3E2C8"):UI.Hex("E8E1D6"),20);
            UI.Text(m.transform,enabled?mark:"锁",0,0,40,40,17,UI.Brown2,true,TextAlignmentOptions.Center);UI.Text(b.transform,text,64,10,f.w-80,h-20,18,enabled?UI.Ink:UI.Muted);f.y+=h+8;
        }
        static string WorkHint(GameState e,string kind,string course)
        {
            var r=Core.WorkOf(e,kind,course);int i=Core.Total(kind,course);
            if(r.status=="complete"){double g=Core.PolishGain(r);return "已完成：质量 "+Fmt.N(r.quality.Value)+"（预计 "+LetterOf(r)+"），继续安排即为打磨"+(g>0?"，下一次 +"+Fmt.N(g):"，已达上限 100，不再提高")+"（已打磨 "+r.polish+" 次）。截止 "+Cal.Date(Core.Due(course,kind))+" 自动提交。";}
            return r.status=="overdue"?"补交中：进度 "+Fmt.N(r.progress)+"/"+i+"，"+Cal.Date(Core.LateDue(course,kind))+" 前完成会立即迟交提交（原始分 × 0.8）。":"进度 "+Fmt.N(r.progress)+"/"+i+" · 截止 "+Cal.Date(Core.Due(course,kind))+(kind=="group"?" · 每半日协作 +"+Fmt.N(Core.Rate(e,"group",course)):"");
        }
        void SetSlot(string item)
        {
            var t=Core.SetSlot(S,slotSel,item);if(t.error!=null){Toast(t.error);Render();return;}dialog=null;PlanChange(t.plan,SlotTitle(slotSel)+"："+Core.Label(item));
        }
        void SlotDialog()
        {
            var o=Overlay("Slot");var day=Cal.SlotOf(S.round,slotSel).day;var f=Panel(o,"slot",SlotTitle(slotSel)+" · "+Cal.Weekdays[Cal.Weekday(day)],"港硕生活模拟器",true,1020,0,Close);
            f.P("安排这个半日的主要活动。已有安排："+Core.Label(S.plan[slotSel])+"。",16,UI.Muted);
            var fixedItem=Cal.Fixed(S.round,slotSel);
            if(fixedItem!=null){var t=Cal.SessionAt(S.round,slotSel);Option(f,t.kind=="class"?"课":"考",(t.kind=="class"?"上课":t.kind=="midterm"?"期中考试":"期末考试")+" · "+Cat.CourseName(t.course),(t.moveReason!=null?t.moveReason+"。":"")+(t.kind=="class"?"按课表到课。":"固定时段考试，替换当周课堂。"),()=>SetSlot(fixedItem),true,"ActionFixed");}
            var mk=Core.FixedItem(S,S.round,slotSel);if(mk!=null&&mk.StartsWith("makeup:"))Option(f,"考",Core.Label(mk),"病假缺考后的补考，与正式考试同一公式计分；改成其他安排会记为缺考（0 分）。当天生病也照常进行。",()=>SetSlot(mk),true,"ActionMakeup");
            foreach(var b in Cat.Basics)
            {
                bool cap=S.plan[slotSel]!=b.id&&S.plan.Count(x=>x==b.id)>=b.cap;var g=Core.RestGain(S);var id=b.id;
                string sub=cap?"已达到本轮次数上限。":b.id=="rest"?b.description+" 当前："+Pace.Delta(-g[0],-g[1])+"。":b.id=="review"?b.description+" 研究分析 +0.5，「"+Cat.CourseName(S.reviewFocus)+"」掌握度 +8；"+Pace.Delta(b.fatigue,b.stress,b.money)+"。":b.description;
                Option(f,b.mark,b.name,sub,()=>SetSlot(id),!cap,"Action:"+b.id);
            }
            foreach(var c in Core.SemesterCourses(S))foreach(var k in new[]{"individual","group"}){var item=Core.WorkItem(k,c.id);var block=Core.ItemBlock(S,item);Option(f,Cat.WorkMarks[k=="individual"?0:1],Core.Label(item),block??WorkHint(S,k,c.id),()=>SetSlot(item),block==null,"Action:"+item);}
            if(S.hired.Count==0)Option(f,"工","兼职","需先在“兼职求职”中申请并获录用。",null,false,"ActionJob");
            else foreach(var j in S.hired){var job=Cat.JobOf(j);bool full=Core.JobId(S.plan[slotSel])==null&&Core.JobCount(S.plan)>=6;var item="job:"+j;Option(f,"工","兼职 · "+job.name,full?"所有兼职合计每轮最多 6 个半日。":"半日工资 HK$"+job.pay+"；"+Pace.Delta(job.fatigue,job.stress)+"；"+Cat.AbilityName(job.growth)+" +"+Fmt.N(job.growthPerShift)+"。",()=>SetSlot(item),!full,"Action:"+item);}
            int am=slotSel-slotSel%2;string reason=am<Core.Pos(S)?"上午已经过去，全天游玩要占用同一天两个尚未执行的半日。":Cal.Fixed(S.round,am)!=null||Cal.Fixed(S.round,am+1)!=null?"这一天有课程或考试，不能安排全天游玩。":null;
            Step(f,"全天游玩","占用这一天上午和下午；HK$"+Cat.TripCost+"，"+Pace.Delta(-Cat.TripFatigue,-Cat.TripStress)+"（整天各算一次）");
            foreach(var t in Cat.Trips){var item="trip:"+t.id;Option(f,"游",t.name,reason??t.text,()=>SetSlot(item),reason==null,"Trip:"+t.id);}
            f.Row(Flow.Bt("清空这个半日"+(Core.Trip(S.plan[slotSel])!=null?"（全天游玩会一起清空两格）":""),()=>SetSlot(null),Look2.Text,true,"ClearSlot"));EndPanel(f);
        }
        void ReplaceDialog()
        {
            var o=Overlay("Replace");var target=replaceTarget??S.hired[0];var t=Core.ReplaceJobs(S,target,replaceFrom);var n=Cat.JobOf(target);var f=Panel(o,"replace","替换本轮兼职","港硕生活模拟器",true,980,0,Close);
            f.P("把本轮尚未执行的兼职半日换成另一个已录用岗位：只改岗位，保留日期与半日位置，不改变兼职总格数，不占用求职机会，也不会动课程、作业等其他安排。预览不改动计划，确认后可用“撤销”恢复。",16,UI.Muted);
            var froms=new List<KeyValuePair<string,string>>{new KeyValuePair<string,string>("lower","工资低于「"+n.name+"」的岗位"),new KeyValuePair<string,string>("others","所有其他岗位")};froms.AddRange(S.hired.Where(x=>x!=target).Select(x=>new KeyValuePair<string,string>(x,"只替换「"+Cat.JobOf(x).name+"」")));
            f.Row(Flow.Bt("换成："+n.name+" · 半日 HK$"+n.pay,()=>Pick("换成（已录用岗位）",S.hired.Select(x=>new KeyValuePair<string,string>(x,Cat.JobOf(x).name+" · 半日 HK$"+Cat.JobOf(x).pay)).ToList(),v=>replaceTarget=v)),
                Flow.Bt("替换："+(froms.FirstOrDefault(x=>x.Key==replaceFrom).Value??froms[0].Value),()=>Pick("替换哪些兼职半日",froms,v=>replaceFrom=v)));
            if(t.error!=null)f.P(t.error,16,UI.Warn);
            else{var c=Note(f);c.P("将替换 "+t.count+" 个半日："+string.Join("、",t.slots.Select(SlotTitle))+"。预计工资 "+(t.pay>0?"+":"")+t.pay+" 元；"+(Pace.Delta(t.fatigue,t.stress)==""?"精力与压力不变":Pace.Delta(t.fatigue,t.stress))+"；能力成长变化："+(t.growth.Count>0?string.Join("、",t.growth.Select(g=>Cat.AbilityName(g.Key)+" "+(g.Value>0?"+":"")+Fmt.N(g.Value))):"无")+"。以上只在实际执行后结算。",15);c.End();}
            f.Row(Flow.Bt("确认替换",()=>{dialog=null;PlanChange(t.plan,"已把 "+t.count+" 个兼职半日换成「"+n.name+"」，可撤销。");},Look2.Primary,t.error==null,"ConfirmReplace"),Flow.Bt("取消",Close));EndPanel(f);
        }
        void BatchDialog()
        {
            var o=Overlay("Batch");var f=Panel(o,"batch","批量填充活动","港硕生活模拟器",true,980,0,Close);f.P("只填尚未执行的空白半日，已有安排会保留。",16);
            var opts=Cat.Basics.Select(b=>new KeyValuePair<string,string>(b.id,b.name)).ToList();
            opts.AddRange(Core.SemesterCourses(S).SelectMany(c=>new[]{Core.TaskItem(c.id),Core.ProjectItem(c.id)}).Select(x=>new KeyValuePair<string,string>(x,Core.Label(x)+(Core.ItemBlock(S,x)!=null?" · 暂不可安排":""))));
            opts.AddRange(Cat.Jobs.Select(j=>new KeyValuePair<string,string>("job:"+j.id,"兼职 · "+j.name+(Core.JobUsable(S,j.id)!=null?" · 需先求职录用":""))));
            var ranges=new List<KeyValuePair<string,string>>{new KeyValuePair<string,string>("all","全部未执行日期"),new KeyValuePair<string,string>("first","第一周"),new KeyValuePair<string,string>("second","第二周")};
            var halves=new List<KeyValuePair<string,string>>{new KeyValuePair<string,string>("all","上午和下午"),new KeyValuePair<string,string>("am","只填上午"),new KeyValuePair<string,string>("pm","只填下午")};
            f.Row(Flow.Bt("活动："+Core.Label(batchItem),()=>Pick("选择活动",opts,v=>batchItem=v)),Flow.Bt("范围："+ranges.First(x=>x.Key==batchRange).Value,()=>Pick("填充范围",ranges,v=>batchRange=v)),Flow.Bt("时段："+halves.First(x=>x.Key==batchHalf).Value,()=>Pick("时段",halves,v=>batchHalf=v)));
            var row=f.Block(46);UI.Text(row,"安排半日数",0,10,130,30,17);UI.Btn(row,"−",140,0,46,44,()=>{batchCount=Math.Max(1,batchCount-1);Render();});UI.Text(row,batchCount.ToString(),190,8,60,30,20,UI.Ink,true,TextAlignmentOptions.Center);UI.Btn(row,"+",254,0,46,44,()=>{batchCount=Math.Min(28,batchCount+1);Render();});
            f.Row(Flow.Bt("填入 "+batchCount+" 个半日",()=>Batch(false),Look2.Primary,true,"BatchFill"),Flow.Bt("填满该范围空位",()=>Batch(true),Look2.Secondary,true,"BatchAll"));
            var block=Core.ItemBlock(S,batchItem);if(block!=null)f.P(block,15,UI.Warn);EndPanel(f);
        }
        void Batch(bool all)
        {
            int z=Core.Pos(S);var slots=Enumerable.Range(0,S.plan.Count).Where(e=>e>=z&&(batchRange=="all"||(batchRange=="first"?e<14:e>=14))&&(batchHalf=="all"||Cal.SlotOf(S.round,e).half==batchHalf)).ToList();
            var n=Core.FillMany(S,batchItem,all?slots.Count(e=>S.plan[e]==null):batchCount,slots);if(n.error!=null){Toast(n.error);Render();return;}dialog=null;
            PlanChange(n.plan,"已填入 "+n.added+" 个半日「"+Core.Label(batchItem)+"」"+(n.unfilled>0?"；"+n.unfilled+" 个因空位或次数上限未安排":"，已有安排保持不变")+"。");
        }
        void StartDialog()
        {
            var o=Overlay("Start");int z=Core.Pos(S),empty=S.plan.Skip(z).Count(x=>x==null);var missing=Enumerable.Range(0,S.plan.Count).Where(t=>t>=z&&Core.FixedItem(S,S.round,t)!=null&&S.plan[t]!=Core.FixedItem(S,S.round,t)).ToList();
            var f=Panel(o,"start",S.phase=="planning"?"开始这两周的生活":"继续余下的日程","港硕生活模拟器",true,900,0,Close);
            f.P("你可以随时暂停并修改尚未执行的半日。老师发布作业时，游戏会自动暂停。",17);if(empty>0){var n=Note(f);n.P("还有 "+empty+" 个半日留白，将安排为「在家休息」。",16);n.End();}
            if(missing.Count>0){var a=Alert(f);a.P("<b>"+missing.Count+" 个课堂／考试时段尚未安排</b>",17,UI.Hex("8C3A2A"));a.P(string.Join("、",missing.Select(SlotTitle))+"。如执行其他活动，课堂记为无故缺勤，考试与补考记为缺考（0 分）。",15);a.Row(Flow.Bt("一键填入课表",()=>{dialog=null;FillClasses();}));a.End();}
            f.Wide("▶ 确认并"+(S.phase=="planning"?"开始":"继续"),StartFromPlanner,Look2.Primary,true,"ConfirmStart");EndPanel(f);
        }
        void StartFromPlanner()
        {
            laterKey=null;int z=Core.Pos(S);var e=Core.ApplyPlan(S,S.plan.Select((x,i)=>i>=z?x??"rest":x).ToList());var t=Pace.Customized(S,e,M);dialog=null;undo.Clear();
            if(e.phase=="planning"){var n=Pace.StartRound(e,t);if(n.error!=null){Commit(e,t,n.error);return;}e=n.state;t=n.meta;}
            if(t.conflict!=null){Commit(e,t,"休整和临近截止的作业撞在一起了：请先在“本轮安排”里选择“先休整”或“继续赶进度”。");return;}
            runTimer=0;autoRun=true;Commit(e,t);
        }
        void ResetDialog()
        {
            var o=Overlay("Reset");var f=Panel(o,"reset","重新开启这段生活？","港硕生活模拟器",true,900,0,Close);
            f.P("开始新生活会替换当前进度。会先看一段开场，再到建档；完成建档时才替换（旧进度另存一份备份）。中途返回，当前进度都还在。",18);
            f.Row(Flow.Bt("先导出存档",Export),Flow.Bt("取消",Close,Look2.Secondary,true,null,"close"),Flow.Bt("开始新生活",()=>{dialog=null;autoRun=false;win="room";screen="prologue";Render();},Look2.Danger,true,"ConfirmReset"));EndPanel(f);
        }
        void DismissNotice(string go,string course=null)
        {
            laterKey=null;var e=Pace.Run(S,M,false,0);autoRun=false;S=e.state;M=e.meta;if(course!=null)teamCourse=course;if(go!=null)win=go;Commit(S,M);
        }
        void NoticeDialog()
        {
            var o=Overlay("Notice");var c=Cat.CourseOf(S.notice.course);
            if(S.notice.kind=="makeup")
            {
                var f=Panel(o,"notice",c.name+"：补考已排入日程","病假缺考 · 补考通知",true,960,0,null);var card=Note(f);card.P(c.teacher,14,UI.Brick,true,2);
                card.P("<b>"+string.Join("；",new[]{"midterm","final"}.Select(k=>{var x=Cal.ExamOf(c.id,k);var r=x!=null?S.Exam(x.id):null;if(r==null||r.makeup!="pending")return null;var m=Core.MakeupOf(x.id);return (k=="midterm"?"期中":"期末")+"补考："+Cal.Date(m.day)+Cal.Half(m.half);}).Where(x=>x!=null))+"</b>",19);
                card.P("因病假缺考，这场考试不按 0 分处理。补考占用上面的半日，与正式考试使用同一公式计分；那个半日原来的安排已改为补考（如原本是全天游玩，整天取消，另一个半日请重新安排）。补考当天即使生病也照常进行。",16);card.End();
                f.Row(Flow.Bt("查看日程  →",()=>DismissNotice("planner"),Look2.Primary,true,"NoticePlanner"),Flow.Bt("知道了",()=>DismissNotice(null),Look2.Secondary,true,"NoticeOk"));EndPanel(f);return;
            }
            bool g=S.notice.kind=="group";var p=Panel(o,"notice",c.teacher+(g?"发布了小组项目":"布置了新作业"),"课堂时段 · 作业通知",true,960,0,null);var l=Note(p);l.P(c.name,14,UI.Brick,true,2);
            if(g){l.P("<b>小组项目："+ProjectTitle(c.id)+"</b>",20);l.P("三人一组，完成资料研究、报告撰写与课堂展示。请先邀请两位同学，并为每人分配一个岗位，再开始这门课的小组作业。",16);l.P("<b>截止："+DueText(c.id,"group")+"</b>",17);}
            else{l.P("<b>"+c.assignment+"</b>",20);l.P(c.requirement,16);l.P("<b>截止："+DueText(c.id,"individual")+"</b>",17);}l.End();
            p.P("作业按教学进度发布，与是否出席这节课无关。可调整接下来尚未执行的半日。",15,UI.Muted);
            p.Row(Flow.Bt("查看"+(g?"组队与分工":"课程与作业")+"  →",()=>DismissNotice(g?"team":"courses",g?c.id:null),Look2.Primary,true,"NoticeOpen"),Flow.Bt("回到日程",()=>DismissNotice("planner"),Look2.Secondary,true,"NoticePlanner"));EndPanel(p);
        }
        float chooseLock;
        void EventDialog()
        {
            var o=Overlay("Event");var e=Cat.EventOf(S.ev.id);var npc=S.ev.npc!=null?S.Npc(S.ev.npc):null;string who=npc!=null?npc.name:"组员";Func<string,string> n=s=>Core.Who(s,who);
            var body=Pace.LookOf(M,"event:"+S.round+":"+S.ev.day+":"+S.ev.id)??S.appearance.bodyId;var media=Pace.CgFor(e.cg,body);
            var f=Panel(o,"event",n(e.title),"随机事件 · "+Cal.Date(S.ev.day)+Cal.Half(S.ev.half)+(S.ev.course!=null?" · "+Cat.CourseName(S.ev.course)+"小组":""),true,1000,0,()=>{laterKey=EventDialogKey(S);Render();},"稍后再选");
            if(media!=null&&!failedCg.Contains(e.cg)){var tex=UI.Tex(media.src);if(tex!=null)f.Image(tex,260,()=>Zoom(tex,media.alt));}
            f.P(n(e.text),19);
            for(int i=0;i<e.options.Length;i++)
            {
                var opt=e.options[i];bool poor=opt.money<0&&S.money<-opt.money;int idx=i;string eff=Pace.Delta(opt.fatigue,opt.stress,opt.money,npc!=null?opt.affection:0,opt.reputation,npc!=null?npc.name:"");
                Option(f,(i+1).ToString(),n(opt.label),poor?"余额不足，不能选这个选项。":eff==""?"无数值变化":eff,()=>{
                    if(Time.unscaledTime<chooseLock)return;chooseLock=Time.unscaledTime+.3f;var t=S.Clone();var err=Core.Choose(t,idx);if(err!=null){Toast(err);Render();return;}
                    var m=Pace.RecordLook(S,M);autoRun=false;Commit(t,m,n(opt.result)+"（"+(eff==""?"无数值变化":eff)+"）");},!poor,"EventChoice"+i);
            }
            if(Details(f,"event-detail","选完会暂停，准备好再继续。 · 查看详情"))f.P("选项效果叠加在这个半日原本活动的效果上（费用从现金扣除，余额不足的选项不能选）。“稍后再选”只收起窗口，日程不会前进；主页按钮或任何“继续”都会重新打开这件事，不会重新抽签。",15,UI.Muted);
            EndPanel(f);
        }
        void ResultDialog(Card card)
        {
            var o=Overlay("Result");var f=Panel(o,"result",card.title,card.delta!=null?"这件事的结果":"新的开始",true,1000,0,null);var media=Pace.CgFor(card.cg,Pace.LookOf(M,card.key));
            if(media!=null&&!failedCg.Contains(card.cg)){var tex=UI.Tex(media.src);if(tex!=null)f.Image(tex,250,()=>Zoom(tex,media.alt));}
            foreach(var l in card.lines)f.P(l,18);
            if(card.delta!=null)f.P("这个半日的实际变化（含活动与选项）：现金 "+Pace.Signed(card.delta[0])+"、精力 "+Pace.Signed(Pace.Neg(card.delta[1]))+"、压力 "+Pace.Signed(card.delta[2])+"。现在：精力 "+Pace.EnergyOf(S.fatigue)+"、压力 "+S.stress+"。",15,UI.Muted);
            f.P("♥ 新增回忆：「"+card.memory+"」（可在“回忆”中回看）",16,UI.Brick,true);
            f.Wide("继续",()=>Commit(S,Pace.MarkSeen(M,card.key)),Look2.Primary,true,"ResultContinue");f.Row(Flow.Bt("稍后再看",()=>{laterKey=card.key;Render();},Look2.Text,true,"ResultLater","close"));EndPanel(f);
        }
        void TripDialog(TripRec t)
        {
            var o=Overlay("Trip");var tr=Pace.Trips[t.loc];var name=Cat.TripOf(t.loc).name;var f=Panel(o,"trip","全天游玩 · "+name,"旅行回顾 · "+Cal.Date(t.day),false,1000,820,null);
            var tex=UI.Tex(tr.scenery.src);if(tex!=null)f.Image(tex,420,()=>Zoom(tex,tr.scenery.alt));f.P(tr.line,22,Title2,true,6,TextAlignmentOptions.Center);
            f.P("费用与恢复已经在当天按原规则结算过；收进手账只是记下这一天"+(t.selfie!=null?"，手账里还能翻到这天的自拍":"")+"。",15,UI.Muted);
            f.Wide("收进手账",()=>Commit(S,Pace.KeepTrip(M,t.key),"已收进手账："+Cal.Date(t.day)+" · "+name+"。可在“手账 · 生活回忆 · 旅行”里回看。"),Look2.Primary,true,"TripKeep");
            f.Row(Flow.Bt("稍后再看",()=>{laterKey=t.key;Render();},Look2.Text,true,"TripLater","close"));EndPanel(f);
        }
    }
}
