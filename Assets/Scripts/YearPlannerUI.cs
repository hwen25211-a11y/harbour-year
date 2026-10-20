using System;
using System.Linq;
using UnityEngine;
using TMPro;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame
    {
        void DrawPlanner()
        {
            bool planning=State.phase=="plan";bool holiday=Rules.Holiday(State);
            Text(root,holiday?"寒假 · 留两段空白给生活":"这两周，怎么过？",320,129,870,52,34,Navy,true);
            Text(root,holiday?"约三周，2 项活动，可重复；不安排上课与课程任务。":"半天一格 · 固定上课时段已标注 · 点击格子选择日程",320,193,877,34,18,Muted);
            if(holiday)
            {
                Picture(root,roomArt,320,255,875,233);
                for(int i=0;i<2;i++){int id=i;var item=State.plan[i];var b=Button(root,"",320+i*448,514,428,214,()=>OpenPicker(id),White,Navy,20,planning,"Slot"+i);Box(b.transform,0,0,428,4,Orange);Text(b.transform,"寒假安排 "+(i+1),21,21,385,30,18,Orange,true);Text(b.transform,item==null?"＋ 选择一段假期":Catalog.Actions[(int)item.kind].name,21,74,385,48,29,Navy,true);Text(b.transform,item==null?"探亲 / 旅游 / 寻找实习":Catalog.Actions[(int)item.kind].yields,21,142,384,53,18,Muted);}
                Text(root,"旅行的「天地宽广」在第 10 轮生效；重复旅游不会叠加倍率。",320,764,872,56,19,Sage);
            }
            else
            {
                string[] day={"周一","周二","周三","周四","周五","周六","周日"};for(int d=0;d<7;d++)Text(root,day[d],367+d*119,260,112,29,17,Muted,false,TextAlignmentOptions.Center);
                for(int row=0;row<4;row++)
                {
                    float y=303+row*119;Text(root,(row<2?"一\n周":"二\n周")+"\n"+(row%2==0?"上":"下"),320,y+17,36,76,16,Muted);
                    for(int d=0;d<7;d++)
                    {
                        int index=(row/2)*14+d*2+row%2;var item=State.plan[index];int fixedCourse=Rules.ScheduledCourse(State,index);bool active=State.phase=="executing"&&index==State.cursor;
                        var b=Button(root,"",365+d*119,y,111,105,()=>CellClick(index),active?Hex("F5D5A9"):swapFirst==index?Hex("CEDDCD"):White,Navy,18,planning,"Slot"+index);
                        if(fixedCourse>=0)Box(b.transform,0,0,111,4,item!=null&&item.kind==ActionKind.Lecture?Sage:Orange);
                        Text(b.transform,item==null?"＋ 安排":Catalog.Actions[(int)item.kind].name,7,14,97,29,17,Navy,true);
                        string detail=item==null?(fixedCourse>=0?Catalog.Courses[fixedCourse].code+" 有课":"自由半天"):Rules.IsCourse(item.kind)?Catalog.Courses[item.course].code:item.kind==ActionKind.Training?Catalog.Skills[item.course%5]:Catalog.Actions[(int)item.kind].tag;
                        Text(b.transform,detail,7,49,98,25,14,Muted);if(fixedCourse>=0&&(item==null||item.kind!=ActionKind.Lecture))Text(b.transform,"此时缺课计旷课",7,79,98,19,11,Red);else if(index<State.cursor)Text(b.transform,"已完成",7,79,98,19,12,Sage);
                    }
                }
                Button(root,"填入固定课程",320,803,212,43,()=>{Rules.FillClasses(State);SaveAndRender();},Sage,White,18,planning,"FillClasses");
                Button(root,"空格全部休息",545,803,212,43,()=>{Rules.FillEmpty(State,new PlanItem(ActionKind.Rest));SaveAndRender();},Pale,Navy,18,planning,"FillRest");
                Button(root,swapMode?"交换模式：开":"交换两个格子",770,803,212,43,()=>{swapMode=!swapMode;swapFirst=-1;Render();},swapMode?Orange:Pale,swapMode?White:Navy,18,planning);
                Button(root,"清空日程",995,803,200,43,()=>{Rules.BlankPlan(State);SaveAndRender();},Pale,Navy,18,planning);
            }
            if(State.phase=="executing")
            {
                Text(root,"正在度过："+(State.cursor>=Rules.Slots(State)?"本轮结算":Rules.SlotName(State,State.cursor)),320,863,430,40,20,Navy,true);
                Button(root,paused?"继续":"暂停",752,862,106,45,()=>{paused=!paused;Render();},Navy,White,18);
                for(int i=1;i<=3;i++){int v=i;Button(root,i+"×",871+(i-1)*108,862,99,45,()=>{speed=v;Render();},speed==i?Orange:Pale,speed==i?White:Navy,18);}
            }
            else Button(root,"确认日程，开始执行  →",320,866,875,47,ConfirmTurn,Orange,White,22,planning&&State.plan.All(p=>p!=null),"ExecuteTurn");
            DrawDeadlines();
        }
        void CellClick(int slot)
        {
            if(!swapMode){OpenPicker(slot);return;}if(swapFirst<0){swapFirst=slot;Render();return;}
            bool ok=Rules.MovePlan(State,swapFirst,slot);swapFirst=-1;SaveAndRender();if(!ok)Notice("上课只能在对应课程的固定时段，无法交换到这个格子。");
        }
        void DrawDeadlines()
        {
            var p=Box(root,1220,125,340,786,Hex("ECE6D7")).transform;Text(p,"别忘了这些事",20,19,302,41,26,Navy,true);
            Text(p,"我的方向 · "+Catalog.RouteName(State.aspiration),20,81,300,56,20,Orange,true);
            var app=State.applications.FirstOrDefault(a=>a.route==State.aspiration);
            string goal=State.aspiration=="explore"?"先完成 8 门课，CGPA 达到 2.00。":app!=null?(app.offer?"已收到 offer，接下来保住毕业条件。":app.resolved?"本次未获 offer；毕业后可按此方向继续求职。":"已投递，第 "+app.replyRound+" 轮回复。"):(State.round>15?"申请已截止；未获聘仍可继续求职。":(State.round<11?"第 11—15 轮开放申请。\n":"申请窗口正在开放。\n")+GoalProgress(State.aspiration));
            Text(p,goal,20,146,299,137,18,Ink);Button(p,"查看目标与投递  →",20,295,300,43,()=>{tab="目标";Render();},Navy,White,18);
            Box(p,20,360,300,1,Line);Text(p,"近期课业",20,381,290,32,21,Navy,true);
            var tasks=Enumerable.Range(0,8).Where(i=>State.round>=Catalog.Courses[i].open&&State.round<=Catalog.Courses[i].end).SelectMany(i=>new[]{Tuple.Create(Catalog.Courses[i].homeworkDue,i,false),Tuple.Create(Catalog.Courses[i].projectDue,i,true)}).Where(t=>!(t.Item3?State.courses[t.Item2].project:State.courses[t.Item2].assignment).submitted).OrderBy(t=>t.Item1).Take(3).ToArray();
            for(int j=0;j<tasks.Length;j++){var t=tasks[j];var task=t.Item3?State.courses[t.Item2].project:State.courses[t.Item2].assignment;float y=430+j*89;Text(p,Catalog.Courses[t.Item2].code+" · "+(t.Item3?"小组项目":"个人作业"),20,y,300,28,18,Navy,true);Text(p,"第 "+t.Item1+" 轮截止 · "+task.work.ToString("0")+"%"+(t.Item1<State.round?" · 已逾期":""),20,y+34,300,30,16,t.Item1<State.round?Red:Muted);}
            if(tasks.Length==0)Text(p,Rules.Holiday(State)?"寒假没有常规课程任务。\n好好休息，下一轮再出发。":"当前学期任务已提交。\n别忘了考试和出勤。",20,435,300,93,19,Sage);
            Button(p,"打开课程档案  →",20,719,300,43,()=>{tab="课程";Render();},White,Navy,18);
        }
        void OpenPicker(int slot)
        {
            if(State.phase!="plan")return;var p=Modal(Rules.SlotName(State,slot)+"，安排什么？","日程目录 / 暂不能选的活动灰显并排在最后",1180,884);
            int scheduled=Rules.ScheduledCourse(State,slot);if(selectedCourse<Rules.SemesterStart(State)||selectedCourse>=Rules.SemesterStart(State)+4)selectedCourse=Rules.SemesterStart(State);
            var actions=Catalog.Actions.Where(a=>Rules.Holiday(State)?(int)a.kind>=13:(int)a.kind<13).Select(a=>new{info=a,item=PickerItem(a.kind,scheduled,slot)}).Select(x=>new{info=x.info,item=x.item,locked=Rules.ActionLock(State,x.item,slot)}).OrderBy(x=>x.locked!="").ToArray();
            for(int i=0;i<actions.Length;i++)
            {
                var a=actions[i];bool enabled=a.locked=="";float x=32+(i%3)*375,y=143+(i/3)*128;
                var b=Button(p,"",x,y,359,114,()=>{if(Rules.IsCourse(a.item.kind)&&a.item.kind!=ActionKind.Lecture||a.item.kind==ActionKind.Training)ChooseCourseAction(slot,a.item.kind);else ActionDetail(slot,a.item);},enabled?Pale:Hex("DADBD5"),enabled?Navy:Muted,20,enabled,"Action"+a.info.kind);
                Text(b.transform,a.info.name+(enabled?"":" · 未开放"),14,10,328,28,21,enabled?Navy:Muted,true);
                Text(b.transform,enabled?a.info.yields:a.locked,14,44,328,37,16,enabled?Ink:Muted);
                Text(b.transform,"现金 "+Catalog.Signed(a.info.money)+" · 压力 "+Catalog.Signed(a.info.stress)+" · 疲劳 "+Catalog.Signed(a.info.fatigue),14,87,328,22,13,Muted);
            }
            Button(p,"返回课表",32,813,538,43,CloseModal,Navy,White,20);Button(p,"清空这个半日",587,813,559,43,()=>{State.plan[slot]=null;SaveAndRender();},Pale,Navy,20);
        }
        PlanItem PickerItem(ActionKind kind,int scheduled,int slot)
        {
            if(kind==ActionKind.Lecture)return new PlanItem(kind,Math.Max(0,scheduled));
            if(Rules.IsCourse(kind)){int id=Enumerable.Range(Rules.SemesterStart(State),4).FirstOrDefault(i=>Rules.ActionLock(State,new PlanItem(kind,i),slot)=="");return new PlanItem(kind,id);}
            return new PlanItem(kind,selectedCourse);
        }
        void ChooseCourseAction(int slot,ActionKind kind)
        {
            var p=Modal(kind==ActionKind.Training?"练习哪一种能力？":"选择对应课程","日程 / "+Catalog.Actions[(int)kind].name,940,710);int n=kind==ActionKind.Training?5:4;
            for(int i=0;i<n;i++){int id=kind==ActionKind.Training?i:Rules.SemesterStart(State)+i;var item=new PlanItem(kind,id);string locked=Rules.ActionLock(State,item,slot);Button(p,(kind==ActionKind.Training?Catalog.Skills[id]:Catalog.Courses[id].title)+(locked==""?"":" · 已提交"),32,153+i*84,876,66,()=>ActionDetail(slot,item),locked==""?Pale:Line,Navy,24,locked=="");}
            Button(p,"返回行动目录",32,622,876,48,()=>OpenPicker(slot),Navy,White,21);
        }
        void ActionDetail(int slot,PlanItem item)
        {
            var d=Catalog.Actions[(int)item.kind];var p=Modal(Rules.PlanName(item),"行动详情",980,652);Text(p,d.detail,32,149,913,94,24,Ink);
            Text(p,d.yields+"\n现金 "+Catalog.Signed(d.money)+" · 压力 "+Catalog.Signed(d.stress)+" · 疲劳 "+Catalog.Signed(d.fatigue),32,273,913,84,23,Sage,true);
            Text(p,"收益按执行时的疲劳、压力与重复次数计算。每天夜间自动恢复少量状态。\n上课只计固定时段；提前休息能提高后续行动效率。",32,396,914,77,20,Muted);
            Button(p,"返回",32,548,205,60,()=>OpenPicker(slot),Pale,Navy,21);Button(p,"安排这个半日",254,548,408,60,()=>{Rules.SetPlan(State,slot,item);SaveAndRender();},Orange,White,22,true,"AddPlan");
            Button(p,"填入其余空格",678,548,270,60,()=>{Rules.SetPlan(State,slot,item);Rules.FillEmpty(State,item);SaveAndRender();},Navy,White,20,item.kind!=ActionKind.Lecture);
        }
        void ConfirmTurn()
        {
            var p=Modal("这次，就这样安排。","确认日程",960,640);int skipped=Enumerable.Range(0,Rules.Slots(State)).Count(i=>Rules.ScheduledCourse(State,i)>=0&&(State.plan[i].kind!=ActionKind.Lecture||State.plan[i].course!=Rules.ScheduledCourse(State,i)));
            Text(p,"共 "+Rules.Slots(State)+" 项安排 · 按时间顺序执行\n本轮房租与生活费：$"+Life.Bill(State)+"\n"+(skipped>0?"有 "+skipped+" 次固定课程未安排，将按单科记录旷课。":"固定课程已安排妥当。"),32,156,896,151,25,skipped>0?Red:Ink);
            Text(p,"执行中可暂停和切换 1 / 2 / 3 倍速；生活事件出现时自动暂停。\n过度疲劳可能触发病假，病假会取消原计划并留下考勤记录。",32,360,896,91,21,Muted);
            Button(p,"回去调整",32,530,329,62,CloseModal,Pale,Navy,23);Button(p,"确认执行  →",378,530,550,62,()=>{Rules.Begin(State);paused=false;tab="日程";SaveAndRender();},Orange,White,24,true,"ConfirmExecute");
        }
    }
}
