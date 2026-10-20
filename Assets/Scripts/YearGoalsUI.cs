using System;
using System.Linq;
using UnityEngine;
using TMPro;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame
    {
        string GoalProgress(string route)
        {
            if(route=="hk")return "职业 "+State.career.ToString("0")+" / "+Balance.HkCareer+" · 适应 "+State.adaptation.ToString("0")+" / "+Balance.HkAdaptation+"\n实习 "+State.internshipCount+" / "+Balance.HkInternships+" 次 · CGPA ≥"+Balance.HkGpa.ToString("0.00");
            if(route=="mainland")return "职业 "+State.career.ToString("0")+" / "+Balance.MainlandCareer+"\n求职准备 "+State.careerCount+" / "+Balance.MainlandPrep+" 次 · CGPA ≥"+Balance.MainlandGpa.ToString("0.00");
            if(route=="phd")return "科研 "+State.research.ToString("0")+" / "+Balance.PhdResearch+" · 学业 "+State.academic.ToString("0")+" / "+Balance.PhdAcademic+"\n科研 "+State.researchCount+" / "+Balance.PhdSessions+" 次 · "+(State.Has("mentor")?"已联系导师":"待联系导师")+"\nCGPA ≥"+Balance.PhdGpa.ToString("0.00");
            return "完成八门课，给自己留一段探索时间。";
        }
        void DrawGoals()
        {
            Text(root,"这一年，想走向哪里？",320,130,1200,55,35,Navy,true);Text(root,"先选志愿，再准备与投递。没有工作 offer 也可以沿所选方向继续求职。",320,197,1234,37,20,Muted);
            var list=Scroll(root,320,252,1240,652,1100);string[] routes={"hk","mainland","phd","explore"};
            for(int i=0;i<4;i++)
            {
                string route=routes[i];float y=i*207;var p=Box(list,0,y,1219,190,White).transform;Text(p,Catalog.RouteName(route),23,19,380,39,28,Navy,true);Text(p,GoalProgress(route),23,75,670,91,21,Muted);
                Button(p,State.aspiration==route?"当前人生志愿":"设为我的志愿",740,19,453,43,()=>{State.aspiration=route;SaveAndRender();},State.aspiration==route?Sage:Pale,State.aspiration==route?White:Navy,20,State.phase=="plan");
                if(route!="explore")
                {
                    var app=State.applications.FirstOrDefault(a=>a.route==route);string locked=Rules.ApplicationLock(State,route);string msg=app==null?(locked==""?"条件已满足，申请费 $150":locked):app.offer?"已收到有条件 offer":app.resolved?"申请未通过":"第 "+app.replyRound+" 轮回复";
                    Text(p,msg,740,78,454,51,18,app!=null&&app.offer?Sage:Orange);
                    Button(p,app==null?"实际投递申请  →":"查看申请邮件",740,139,453,35,()=>{if(app==null){Rules.Apply(State,route);SaveAndRender();}else ApplicationDialog(app);},Navy,White,18,State.phase=="plan"&&(app!=null||locked==""),"Apply"+route);
                }
                else Text(p,"只要满足毕业标准就可以选择。\n不会因没有 offer 而延迟毕业。",740,88,454,69,20,Sage);
            }
            var h=Box(list,0,835,1219,237,Hex("E4EAD9")).transform;Text(h,"毕业与可叠加荣誉",23,17,1167,40,26,Navy,true);
            Text(h,"毕业：8 门全部通过、CGPA ≥2.00；当前预计 "+Rules.GPA(State,true).ToString("0.00")+"。\n奖学金：主动申请、毕业、CGPA ≥3.60、学业 ≥65。\n优秀毕业表现：CGPA ≥3.60、八课各 ≥80、科研 ≥12 次或实习 ≥8 次。",23,73,771,127,20,Ink);
            Button(h,State.scholarshipApplied?"奖学金已申请":"申请学业奖学金",824,135,370,55,()=>{State.scholarshipApplied=true;Rules.Experience(State,"提交学业奖学金申请。");SaveAndRender();},Orange,White,19,State.phase=="plan"&&!State.scholarshipApplied);
        }
        void ApplicationDialog(ApplicationState app)
        {var p=Modal(Catalog.RouteName(app.route),"第 "+app.submittedRound+" 轮投递 / 第 "+app.replyRound+" 轮回复",900,521);Text(p,app.reason,32,158,835,193,25,Ink);Button(p,"收好这封邮件",32,424,835,58,CloseModal,Navy,White,23);}
        void DrawEnding()
        {
            string route=State.ending;if(string.IsNullOrEmpty(route)){State.ending=Rules.PreferredEnding(State);route=State.ending;YearSave.Write(State);}
            Text(root,"学年终章 · "+Catalog.RouteName(route),320,127,1236,55,37,Navy,true);
            Text(root,Rules.CanGraduate(State)?"顺利毕业 / 你的志愿与实际申请，一起写下这一页。":"延迟毕业 / 本局学年已经结束，后续可补交、补考或重修。",320,197,1237,38,21,Muted);
            var list=Scroll(root,320,256,1240,583,1320+State.journal.Count*49);
            Picture(list,roomArt,0,0,1215,180);var badge=Box(list,23,22,461,138,new Color(.98f,.96f,.9f,.97f)).transform;Text(badge,"最终 CGPA",19,15,418,31,20,Muted);Text(badge,Rules.GPA(State).ToString("0.00")+" / 4.00",19,62,418,61,42,Navy,true);
            Text(list,Rules.EndingStory(route),0,211,1193,118,24,Ink);Text(list,"为什么走到这里",0,356,1198,39,27,Navy,true);
            string reason=Rules.GraduationReason(State)+"\n原志愿："+Catalog.RouteName(State.aspiration)+"。";
            if(route=="phd"||route=="hk"||route=="mainland")reason+="已实际投递并收到相应 offer。";
            else if(route.EndsWith("-search"))reason+="尚未获聘，按志愿保留继续求职的方向。";
            if(route!=Rules.PreferredEnding(State))reason+="你在毕业时选择了另一个可达方向。";
            else if(State.aspiration=="phd"&&route!="phd"&&route!="delay")reason+="博士方向尚无录取，匹配已获得的工作机会；无工作 offer 时保留探索。";
            Text(list,reason,0,407,1193,104,21,Muted);
            Text(list,"八门课的成绩单",0,547,1189,39,27,Navy,true);
            for(int i=0;i<8;i++){float y=607+i*52;var c=State.courses[i];Text(list,Catalog.Courses[i].title,0,y,402,37,21,Navy);Text(list,"出勤 "+Rules.Attendance(c).ToString("0")+"% · 病假 "+c.approvedLeave,431,y,440,37,19,Muted);Text(list,Rules.Score(State,i).ToString("0.0")+" / "+Rules.Letter(Rules.Score(State,i)),930,y,260,37,22,Rules.Score(State,i)>=50?Sage:Red,true,TextAlignmentOptions.TopRight);}
            var honors=Rules.Honors(State);Text(list,"荣誉 / "+(honors.Count==0?"完成这一程，本身值得被记住。":string.Join(" · ",honors.ToArray())),0,1050,1200,48,24,Orange,true);
            Text(list,"科研 "+State.researchCount+" 次 · 实习 "+State.internshipCount+" 次 · 求职准备 "+State.careerCount+" 次\n余额 $"+State.money+" · 应急金待还 $"+State.debt+" · 最终能力见左侧",0,1120,1200,82,21,Muted);
            Text(list,"重要经历",0,1230,1200,41,27,Navy,true);for(int i=0;i<State.journal.Count;i++)Text(list,State.journal[i],0,1295+i*49,1196,44,18,Ink);
            Button(root,"查看其他可达去向",320,867,405,50,ChooseEnding,Pale,Navy,20,Rules.CanGraduate(State),"ChooseEnding");Button(root,"重新开始一年",743,867,387,50,ConfirmRestart,Orange,White,21);Button(root,"返回扉页",1147,867,412,50,()=>{menu=true;SaveAndRender();},Navy,White,21);
        }
        void ChooseEnding()
        {
            var routes=Rules.Endings(State);var p=Modal("这些方向，都可以选择。","已满足毕业条件 / 尊重你的最后决定",1030,820);for(int i=0;i<routes.Count;i++){string route=routes[i];Button(p,Catalog.RouteName(route)+(route==State.ending?" · 当前选择":""),32,145+i*91,966,71,()=>{State.ending=route;Rules.Experience(State,"毕业时选择："+Catalog.RouteName(route));SaveAndRender();},route==State.ending?Sage:Pale,route==State.ending?White:Navy,25,true,"Ending"+route);}Button(p,"返回成绩单",32,741,966,49,CloseModal,Navy,White,22);
        }
    }
}
