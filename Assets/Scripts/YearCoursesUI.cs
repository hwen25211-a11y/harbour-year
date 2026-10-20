using System;
using System.Linq;
using TMPro;
using UnityEngine;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame
    {
        void DrawCourses()
        {
            Text(root,"课程与考勤",320,130,1100,55,35,Navy,true);Text(root,"毕业：8 门课全部 ≥50，CGPA ≥2.00。单课合规出勤低于 70% 直接判 F。",320,197,1235,39,20,Muted);
            var list=Scroll(root,320,254,1240,651,1220);
            for(int i=0;i<8;i++)
            {
                int id=i;var d=Catalog.Courses[i];var c=State.courses[i];bool open=State.round>=d.open;float x=(i%2)*615,y=(i/2)*303;var p=Box(list,x,y,599,287,White).transform;
                Text(p,d.code+" / "+(i<4?"第一学期":"第二学期"),19,14,360,27,15,Orange,true);Text(p,open?Rules.Score(State,i,true).ToString("0.0")+" 分":"未开课",432,14,147,31,21,open?Sage:Muted,true,TextAlignmentOptions.TopRight);
                Text(p,d.title,19,52,553,37,27,Navy,true);
                float attend=Rules.Attendance(c);Text(p,"合规出勤 "+attend.ToString("0.0")+"%  ·  到课 "+c.attended+" / 病假 "+c.approvedLeave+" / 旷课 "+c.absent,19,102,555,30,17,attend<70?Red:Ink);
                Text(p,"下次缺课后 "+(100f*(c.attended+c.approvedLeave)/(c.scheduled+1)).ToString("0.0")+"%  ·  学习 "+c.learning.ToString("0")+"%",19,140,555,28,16,Muted);
                Text(p,TaskLine(c.assignment,"个人作业",d.homeworkDue),19,177,555,28,17,Ink);Text(p,TaskLine(c.project,"小组项目",d.projectDue),19,214,555,28,17,Ink);
                Button(p,"考核详情 / 提交任务  →",19,251,559,28,()=>CourseDetail(id),Pale,Navy,16,open,"Course"+i);
            }
        }
        string TaskLine(CourseTask t,string name,int due){return name+" "+(t.submitted?(t.late?"已补交":"已提交")+" · "+t.quality.ToString("0")+"分":t.work.ToString("0")+"% · "+(State.round>due?"逾期":"待交"))+" / 第 "+due+" 轮截止";}
        void CourseDetail(int id)
        {
            var d=Catalog.Courses[id];var c=State.courses[id];var w=Balance.Weights[(int)d.type];var p=Modal(d.title,d.code+" / "+d.note,1020,842);
            Text(p,"到课 "+c.attended+" + 获批病假 "+c.approvedLeave+" / 应到 "+c.scheduled+" = 合规出勤 "+Rules.Attendance(c).ToString("0.0")+"%\n病假只保护考勤，不增加学习、作业或考试成绩。",32,147,955,79,23,Ink);
            Text(p,"考核权重：作业 "+(w[0]*100)+"% · 小组项目 "+(w[1]*100)+"%"+(w[2]>0?" · 期中 "+(w[2]*100)+"%":"")+(w[3]>0?" · 期末 "+(w[3]*100)+"%":""),32,248,955,40,20,Sage,true);
            Text(p,"本课学习 "+c.learning.ToString("0")+"% · "+(d.type==Assessment.Project?"无考试":(d.type==Assessment.Exam?"期中第 "+(d.open+3)+" 轮 / "+(c.midtermDone?c.midterm.ToString("0")+" 分":"待考")+"；":"")+"期末第 "+d.end+" 轮 / "+(c.finalDone?c.finalExam.ToString("0")+" 分":"待考")),32,301,955,46,20,Muted);
            for(int i=0;i<2;i++){bool project=i==1;var t=project?c.project:c.assignment;float y=382+i*125;Text(p,TaskLine(t,project?"小组项目":"个人作业",project?d.projectDue:d.homeworkDue),32,y,683,58,21,Navy);Bar(p,32,y+69,671,t.work,Sage,6);Button(p,t.submitted?"已提交":"确认提交",741,y,246,60,()=>{Rules.Submit(State,id,project);SaveAndRender();CourseDetail(id);},Orange,White,21,State.phase=="plan"&&!t.submitted&&t.work>0&&State.round<=d.end);}
            Text(p,"截止轮按现有成果自动提交；本学期可补交，质量 ×75%。提交后锁定。\n项目质量还取决于分工匹配；考试根据本课实际学习进度结算。",32,659,955,69,19,Muted);Button(p,"收好课程档案",32,757,955,52,CloseModal,Navy,White,22);
        }
        void DrawTeams()
        {
            if(teamCourse<Rules.SemesterStart(State)||teamCourse>=Rules.SemesterStart(State)+4)teamCourse=Rules.SemesterStart(State);
            Text(root,"和谁一起，把事情做好？",320,130,1200,53,35,Navy,true);Text(root,"每门课单独分工；同学一人负责一个岗位，换人会交换原岗位。外观与能力独立随机。",320,197,1238,37,19,Muted);
            for(int i=0;i<4;i++){int id=Rules.SemesterStart(State)+i;Button(root,Catalog.Courses[id].title,320+i*313,254,299,53,()=>{teamCourse=id;Render();},teamCourse==id?Navy:White,teamCourse==id?White:Navy,18);}
            Text(root,"分工匹配 "+Life.TeamFit(State,teamCourse).ToString("0.0")+" / 100",320,337,640,40,25,Sage,true);Text(root,"提高项目质量，不能替代实际推进项目。",932,343,620,33,18,Muted,false,TextAlignmentOptions.TopRight);
            for(int r=0;r<5;r++)
            {
                int role=r;int n=State.courses[teamCourse].team[r];var student=n>=0?State.students[n]:null;float y=406+r*95;var p=Box(root,320,y,1235,81,White).transform;
                Text(p,Catalog.Roles[r],20,17,180,38,24,Navy,true);Text(p,student==null?State.name+"（自己兼任）":student.name,218,15,277,33,22,Ink);
                Text(p,Catalog.Skills[r]+" "+(student==null?State.skills[r].ToString("0"):student.skills[r].ToString())+(student==null?" / 兼任效率 65%":" / 可靠度 "+student.reliability+" / 默契 "+student.goodwill),508,20,490,45,19,Muted);
                Button(p,"查看同学 / 分配",1010,15,205,51,()=>AssignDialog(role),Pale,Navy,18,State.phase=="plan"&&!State.courses[teamCourse].project.submitted,"AssignRole"+r);
            }
        }
        void AssignDialog(int role)
        {
            var p=Modal("谁来负责「"+Catalog.Roles[role]+"」？","查看公开能力后再选择",1130,855);var list=Scroll(p,32,143,1066,586,6*147);
            for(int i=0;i<State.students.Count;i++)
            {
                int id=i;var n=State.students[i];float y=i*147;var card=Box(list,0,y,1045,134,Pale).transform;PixelPeople.Draw(card,n.look,8,6,60,117);
                Text(card,n.name+" · "+n.habit,89,12,730,31,21,Navy,true);Text(card,string.Join("  ",Enumerable.Range(0,5).Select(k=>Catalog.Skills[k]+" "+n.skills[k]).ToArray()),89,53,740,48,17,Ink);
                Text(card,"可靠度 "+n.reliability+" · 默契 "+n.goodwill+" · 当前岗位能力 "+n.skills[role],89,103,732,24,16,Sage);
                Button(card,"由这位负责",850,42,177,48,()=>{Life.Assign(State,teamCourse,role,id);SaveAndRender();},Navy,White,19);
            }
            Button(p,"返回",32,766,310,53,CloseModal,Pale,Navy,21);Button(p,"这个岗位由我兼任",359,766,739,53,()=>{Life.Assign(State,teamCourse,role,-1);SaveAndRender();},Navy,White,22);
        }
    }
}
