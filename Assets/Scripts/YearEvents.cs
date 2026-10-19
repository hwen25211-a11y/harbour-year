using System;
using System.Linq;
namespace HarbourYear
{
    public class EventChoice
    {
        public string title,effect,unavailable;public Action<YearState> apply;
        public EventChoice(string t,string e,Action<YearState> a,string l=""){title=t;effect=e;apply=a;unavailable=l;}
    }
    public class LifeEvent
    {
        public string title,location,body;public EventChoice[] choices;
        public LifeEvent(string t,string l,string b,params EventChoice[] c){title=t;location=l;body=b;choices=c;}
    }
    public static class Events
    {
        static EventChoice C(string t,string e,Action<YearState> a,string l=""){return new EventChoice(t,e,a,l);}
        public static string ForRound(YearState s){if(s.round==8||s.round==9||s.round==10||s.round==15||s.round==17)return "r"+s.round;if(s.money<Life.Bill(s)&&!s.Has("budget"))return "budget";return "r"+s.round;}
        static void Project(YearState s,float amount){int start=Rules.SemesterStart(s);for(int i=start;i<start+4;i++)if(!s.courses[i].project.submitted)s.courses[i].project.work=Rules.Clamp(s.courses[i].project.work+amount);}
        public static LifeEvent Get(YearState s)
        {
            switch(s.eventId)
            {
                case "arrival":return new LifeEvent("你好，香港。","九月 · 新生注册处","导航说还有八分钟，没说是上坡八分钟。学生证领到了，租房合同收好了。四门新课和一张空课表，等你把这一年慢慢填满。第一天下午，先去哪？",
                    C("找教室，办借书证","学业 +4 · 压力 -3",x=>{x.academic+=4;x.stress-=3;}),C("跟同学去吃云吞面","适应 +6 · 花费 $60",x=>{x.adaptation+=6;x.money-=60;x.Flag("friend");}),C("听研究组迎新分享","科研 +4 · 建立导师联系",x=>{x.research+=4;x.Flag("mentor");}));
                case "homeCG":return new LifeEvent("有一碗汤，专门给你留着。","寒假 · 家里的饭桌","有人问香港的日子好不好过，你先讲起饭堂，再讲起小组作业。离家时多了一袋水果，和一句听过很多次、这次还是认真听了的「记得吃饭」。",
                    C("给家人讲讲已经习惯的小事","适应 +2 · 压力 -3",x=>{x.adaptation+=2;x.stress-=3;}),C("帮忙收拾饭桌，再坐一会儿","疲劳 -5",x=>x.fatigue-=5));
                case "travelCG":return new LifeEvent("天地宽广。","寒假 · 渡轮甲板","海风吹过来，待办清单暂时很远。你没有忽然解决所有问题，只是记起生活比成绩单和邮箱大一点。第 10 轮，日程新增的压力与疲劳会减半。",
                    C("记下一个新的观察","科研 +2 · 适应 +2",x=>{x.research+=2;x.adaptation+=2;}),C("什么也不赶，看一会儿海","压力 -6",x=>x.stress-=6));
                case "medical":return new LifeEvent("身体发来一封置顶邮件","校医室 · 上午","昨晚的咖啡没能代替睡眠。校医建议先休养两天，可以帮你登记病假。接下来四个半日的计划会取消；相关课程记获批病假，作业和收入不会凭空完成。",
                    C("登记病假，买药回家休养","花费 $150 · 接下来 4 格休养 · 压力 -8",x=>{x.money-=Balance.MedicalCost;x.recoverySlots=4;x.stress-=8;}),C("申请学生支援，安排校内休养","应急金待还 +$150 · 接下来 4 格休养 · 压力 -5",x=>{x.debt+=Balance.MedicalCost;x.recoverySlots=4;x.stress-=5;}));
                case "budget":return new LifeEvent("余额比待办事项短","房间 · 记账时间","你算了一遍，又算一遍。下张账单确实比余额长。学生支援处可以提供无息应急生活金；也可以先节省一笔本轮开销，再重新安排兼职。",
                    C("申请一笔生活周转金","现金 +$1500 · 待还 +$1500 · 压力 -5",x=>{x.Flag("budget");x.money+=1500;x.debt+=1500;x.stress-=5;}),C("退掉还没使用的交通套票","现金 +$200 · 适应 +2",x=>{x.Flag("budget");x.money+=200;x.adaptation+=2;}));
                case "r1":return new LifeEvent("小组文件夹的第一条消息","教学楼 · 讨论室","五个岗位，六位候选同学。有人擅长检索，有人讲起话来很有条理。先看公开的能力，再分配任务，通常比按谁先回复群聊更靠谱。",
                    C("约定分工和每周十分钟碰头","所有未交项目进度 +8 · 留下协作回响",x=>{Project(x,8);x.Flag("team");}),C("先自己做一份提纲供大家参考","逻辑表达 +6 · 疲劳 +6",x=>{x.skills[1]+=6;x.fatigue+=6;}));
                case "r2":return new LifeEvent("饭堂的靠窗位置","校园 · 午后","认识的同学招呼你坐下，聊到有人在研究组帮忙，也有人正在改简历。半小时里交换的不只有菜价，还有下次见面的理由。",
                    C("问问研究组正在做什么","导师联系 · 科研 +4",x=>{x.Flag("mentor");x.research+=4;}),C("互相看一眼简历","职业 +5 · 演讲 +3",x=>{x.career+=5;x.skills[4]+=3;}),C("这一顿先认真吃饭","疲劳 -10 · 压力 -8",x=>{x.fatigue-=10;x.stress-=8;}));
                case "r3":return new LifeEvent("很便宜的台灯","手机 · 二手交易群","毕业生把台灯拍得像博物馆展品，配文却只有「明天搬走」。你已经开始分得清什么是便宜，什么是买了也没地方放。",
                    C("收藏帖子，先量量书桌","适应 +4 · 进入二手市场可购买家具",x=>x.adaptation+=4),C("帮同学搬一趟家具","现金 +$100 · 疲劳 +6 · 压力 -4",x=>{x.money+=100;x.fatigue+=6;x.stress-=4;}));
                case "r4":return new LifeEvent("不是所有文件都叫最终版","小组群 · 考核周",s.Has("team")?"之前约好的十分钟碰头派上了用场。每个人知道去哪里找资料，你们终于不用在群聊里搜索三个不同的最终版。":"群里出现了三份最终版。每份都差一点，合在一起也不会自动变成完整作业。",
                    C("统一文档和引用格式",s.Has("team")?"项目进度 +14 · 压力 -5":"项目进度 +6 · 疲劳 +5",x=>{Project(x,x.Has("team")?14:6);if(x.Has("team"))x.stress-=5;else x.fatigue+=5;}),C("请助教核对方向","学业 +4 · 逻辑表达 +4",x=>{x.academic+=4;x.skills[1]+=4;}));
                case "r5":return new LifeEvent("排班和课堂撞在一起","兼职群 · 傍晚","负责人问你能不能多顶一个时段。那天正好有课。课表可以改，考勤记录却会留下来；你决定先谈清楚实际能给出的时间。",
                    C("换到不冲突的半日","职业 +3 · 压力 +3",x=>{x.career+=3;x.stress+=3;}),C("这次不接，发出自己的可用时段","沟通练习：逻辑表达 +4 · 压力 -4",x=>{x.skills[1]+=4;x.stress-=4;}));
                case "r6":return new LifeEvent("有人终于在截止日前说了实话","小组群 · 晚上","负责排版的同学说电脑坏了。早一点知道，总比提交前五分钟知道好。你们打开分工表，重新安排还能完成的部分。",
                    C("临时帮忙整合","项目进度 +12 · 疲劳 +10",x=>{Project(x,12);x.fatigue+=10;}),C("约好把可编辑文件交出来","项目进度 +6 · 逻辑表达 +4",x=>{Project(x,6);x.skills[1]+=4;}));
                case "r7":return new LifeEvent("台上三分钟","教室 · 项目汇报","投影亮起。你发现真正有用的不是把每页讲完，而是让听众带走一个清楚的结论。",
                    C("和组员再走一遍关键问题","演讲 +6 · 项目进度 +5",x=>{x.skills[4]+=6;Project(x,5);}),C("按时收工，留个清醒的早上","疲劳 -12 · 压力 -8",x=>{x.fatigue-=12;x.stress-=8;}));
                case "r8":return new LifeEvent("第一学期，交卷。","校园 · 十二月","课程成绩将在本轮结束时结算。作业、小组项目、考试和出勤各有一份记录。寒假没有常规课程，可以安排两次探亲、旅游或寻找实习。",
                    C("把最值得改进的地方写下来","学业 +3 · 压力 -8",x=>{x.academic+=3;x.stress-=8;}),C("先给家里打个电话","压力 -12 · 疲劳 -8",x=>{x.stress-=12;x.fatigue-=8;}));
                case "r9":return new LifeEvent("把日历翻到新的一年","寒假 · 行李旁",s.Has("travelCG")?"旅途里有一刻，你没有想绩点，也没有想简历。天地宽广不是忽然没有烦恼，而是知道它们之外还有很多事情。":s.Has("homeCG")?"饭桌上有人问你在香港吃得习不习惯。你讲了很多小事，最后发现自己已经把那座城叫成了「回去的地方」。":"岗位列表还在更新。你留了几封认真的申请，也给自己留了几个不用设闹钟的早晨。",
                    C("给下学期留一个具体目标","职业 +4 · 压力 -5",x=>{x.career+=4;x.stress-=5;}),C("把假期好好过完","疲劳 -15 · 压力 -10",x=>{x.fatigue-=15;x.stress-=10;}));
                case "r10":return new LifeEvent("新课表，旧书包","第二学期 · 教学楼","四门新课已经开放。曾经要查地图的教室，现在一眼就知道从哪个楼梯上。未来的方向也该写进提醒里了。",
                    C("检查志愿和申请窗口","职业 +3 · 第 11—15 轮可投递",x=>x.career+=3),C("和导师约一次讨论","建立导师联系 · 科研 +4",x=>{x.Flag("mentor");x.research+=4;}));
                case "r11":return new LifeEvent("实习面试的最后一问","线上会议 · 下午","对方问：如果需求不够明确，你会先做什么？你终于有了课堂之外的例子，也知道不是每个问题都要立刻装作有答案。",
                    C("用项目举例，先澄清需求",s.career>=28?"实习面试通过 · 职业 +3":"获得面试反馈 · 职业 +5",x=>{if(x.career>=28){x.Flag("interview");x.career+=3;}else x.career+=5;}),C("主动说明还需要补足的经验","求职准备 +1 · 演讲 +4",x=>{x.careerCount++;x.skills[4]+=4;}));
                case "r12":return new LifeEvent("论点终于站住了","图书馆 · 靠窗桌","一段写了又删的文字，终于找到合适的证据。老师留下一句「这一版清楚多了」，你截图保存了。",
                    C("顺着问题补一份研究记录","科研 +5 · 检索 +3",x=>{x.research+=5;x.skills[0]+=3;}),C("把表达方法用进求职材料","职业 +5 · 写作 +3",x=>{x.career+=5;x.skills[2]+=3;}));
                case "r13":return new LifeEvent("雨很大，群聊也很热闹","房间 · 暴雨傍晚","窗外的雨声把世界缩小了一点。小组讨论改到线上，大家打开摄像头，背景里是各自的床、书和没收的衣服。",
                    C("一起把框架讲清楚","项目进度 +8 · 逻辑表达 +3",x=>{Project(x,8);x.skills[1]+=3;}),C("提前结束，明天再看一遍","压力 -10 · 疲劳 -8",x=>{x.stress-=10;x.fatigue-=8;}));
                case "r14":return new LifeEvent("邮件已发送","电脑前 · 求职季","写完邮件的最后一句，按下发送。申请不保证有回复，但高属性也不会替你投递。第 15 轮是本学年的最后申请窗口。",
                    C("回目标页核对准备情况","职业 +4 · 压力 +2",x=>{x.career+=4;x.stress+=2;}),C("联系研究方向合适的导师","导师联系 · 科研 +4",x=>{x.Flag("mentor");x.research+=4;}));
                case "r15":return new LifeEvent("毕业申请提醒","教务处 · 系统通知","系统发来毕业手续清单。游戏的毕业标准是八门课全部通过、CGPA 至少 2.00；手续在此确认，忘记点按钮不会单独让一个已达学业标准的人延毕。",
                    C("核对姓名与课程记录，提交手续","毕业手续完成 · 压力 -4",x=>{x.graduationApplied=true;x.stress-=4;}),C("请教务一起核对学分","毕业手续完成 · 学业 +2",x=>{x.graduationApplied=true;x.academic+=2;}));
                case "r16":return new LifeEvent("毕业照之前","校园 · 黄昏","有人带着 offer，有人还在等消息。大家约好拍照，不需要先给每个人的下一年打分。",
                    C("帮大家整理一份共享相册","适应 +4 · 压力 -8",x=>{x.adaptation+=4;x.stress-=8;}),C("和同学散一次步","疲劳 -10 · 压力 -12",x=>{x.fatigue-=10;x.stress-=12;}));
                default:return new LifeEvent("这一年，快写完了","五月 · 校园出口","最后的考核将在本轮结算。先看是否毕业，再看你选择的方向。工作或录取要有真实申请；没有 offer，也可以按自己的意愿继续寻找。",
                    C("把想走的方向留在目标页","压力 -10 · 学年结束后进入结局页",x=>x.stress-=10),C("感谢这一年的自己","疲劳 -10 · 学年结束后进入结局页",x=>x.fatigue-=10));
            }
        }
        public static bool Choose(YearState s,int n)
        {
            if(s.phase!="event")return false;var e=Get(s);if(n<0||n>=e.choices.Length||e.choices[n].unavailable!="")return false;
            var c=e.choices[n];c.apply(s);Rules.Normalize(s);Rules.Experience(s,e.title+"："+c.title);Rules.Log(s,"生活事件："+c.title+"（"+c.effect+"）");s.eventId="";s.phase=s.eventReturn;return true;
        }
    }
}
