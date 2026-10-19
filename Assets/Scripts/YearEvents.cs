using System;
using System.Collections.Generic;
namespace HarbourYear
{
    public class EventChoice
    {
        public string title, effect, unavailable; public Action<YearState> apply;
        public EventChoice(string t,string e,Action<YearState> a,string locked=""){title=t;effect=e;apply=a;unavailable=locked;}
    }
    public class LifeEvent
    {
        public string title, location, body; public EventChoice[] choices;
        public LifeEvent(string t,string l,string b,params EventChoice[] c){title=t;location=l;body=b;choices=c;}
    }
    public static class Events
    {
        static EventChoice C(string t,string e,Action<YearState> a,string locked=""){return new EventChoice(t,e,a,locked);}
        static void Work(YearState s,int c,bool p,float value){var t=p?s.courses[c].project:s.courses[c].assignment;if(!t.submitted)t.work=Rules.Clamp(t.work+value);}
        public static string ForRound(YearState s)
        {
            if(s.round==12||s.round==13||s.round==14||s.round==24||s.round==26)return "r"+s.round;
            if(s.money<650&&!s.Has("budgetEvent"))return "budget";
            if(s.stress>=85&&!s.Has("burnoutEvent"))return "burnout";
            return "r"+s.round;
        }
        public static LifeEvent Get(YearState s)
        {
            switch(s.eventId)
            {
                case "arrival":return new LifeEvent("你好，香港。","九月 · 新生注册处","拖着行李走出地铁，导航说还有八分钟，没说是上坡八分钟。学生证拿到了，这一年的四门课、机会和生活，都从这里开始。第一天下午，你想先做什么？",
                    C("先找教室，再办借书证","学业 +4 · 压力 -3",x=>{x.academic+=4;x.stress-=3;}),
                    C("跟着同学去吃一碗云吞面","适应 +6 · 花费 $60 · 认识同学阿晴",x=>{x.adaptation+=6;x.money-=60;x.Flag("friend");}),
                    C("听一场研究组的迎新分享","科研 +4 · 建立导师联系 · 压力 +3",x=>{x.research+=4;x.stress+=3;x.Flag("mentor");}));
                case "r1":return new LifeEvent("书桌终于收拾好了","宿舍 · 晚上十点","插线板找到了，课表也贴到了墙上。室友把一张二手电饭煲转让帖发给你：一年很短，但总不能顿顿靠便利店。",
                    C("合购一个电饭煲","花费 $100 · 适应 +4 · 压力 -8",x=>{x.money-=100;x.adaptation+=4;x.stress-=8;x.Flag("rice");}),
                    C("先研究学校饭堂路线","适应 +3 · 压力 -4",x=>{x.adaptation+=3;x.stress-=4;}));
                case "r2":return new LifeEvent("找不到的参考书","图书馆 · 三楼","指定阅读只剩电子版。你和旁边的同学同时对着下载按钮叹气：原来研究生的共同语言是「VPN 连上了吗」。",
                    C("交换阅读笔记","研究方法学习 +12 · 科研 +2",x=>{x.courses[0].learning=Rules.Clamp(x.courses[0].learning+12);x.research+=2;}),
                    C("参加馆员的数据库小课堂","学业 +3 · 压力 -5",x=>{x.academic+=3;x.stress-=5;}));
                case "r3":return new LifeEvent("小组文件：谁动了我的表格","教学楼 · 讨论室","四个人，五个版本，六种文件命名方式。阿晴提议先把分工写清楚。另一位同学说自己可以通宵合并，只需要大家相信奇迹。",
                    C("约好分工和每周十分钟碰头","作业一进度 +20 · 职业 +3 · 留下协作回响",x=>{Work(x,0,false,20);x.career+=3;x.Flag("team");}),
                    C("接下最难的部分，换取安静写作","作业一进度 +30 · 科研 +3 · 压力 +10",x=>{Work(x,0,false,30);x.research+=3;x.stress+=10;x.Flag("solo");}),
                    C("请助教帮忙确认任务边界","作业一进度 +12 · 压力 -6",x=>{Work(x,0,false,12);x.stress-=6;}));
                case "r4":return new LifeEvent("八达通里还剩多少钱","街市 · 傍晚","你开始记得哪一家的菜便宜、哪一站转车不用绕远。小摊老板认出了你，说今天的番茄很适合煮面。",
                    C("买菜回去做一顿","花费 $40 · 压力 -12 · 适应 +3",x=>{x.money-=40;x.stress-=12;x.adaptation+=3;}),
                    C("绕去海边，看看这座城","花费 $80 · 压力 -8 · 适应 +6",x=>{x.money-=80;x.stress-=8;x.adaptation+=6;}));
                case "r5":return new LifeEvent("导师的门开着","办公室 · Office Hour","你带着一个还不成熟的问题去见老师。她没有嫌问题小，反而拿出一篇文献：「先从能复现的部分开始。我们需要一个认真写实验记录的人。」",
                    C("加入读书会，从复现开始","建立导师联系 · 科研 +5 · 科研行动需学业 24",x=>{x.Flag("mentor");x.research+=5;}),
                    C("请老师帮我梳理课程论文","研究方法学习 +15 · 学业 +3",x=>{x.courses[0].learning=Rules.Clamp(x.courses[0].learning+15);x.academic+=3;}),
                    C("坦诚最近太忙，留下联系方式","建立导师联系 · 压力 -5",x=>{x.Flag("mentor");x.stress-=5;}));
                case "r6":return new LifeEvent("提交按钮前的最后五分钟","图书馆 · 作业截止日","论文标题已经改了三次。你发现参考文献里有一条年份不对；旁边的同学说「先交上去再说」，但也愿意帮你看一眼。",
                    C("互查引用后提交","研究方法作业进度 +15 · 压力 +3",x=>{Work(x,0,false,15);x.stress+=3;}),
                    C("按现有版本提交，今晚早点睡","压力 -12 · 截止日按现有质量提交",x=>x.stress-=12));
                case "r7":return new LifeEvent("排班与讨论课撞车了","校园助理值班台","临时多出一个下午班，可偏偏和项目讨论撞了。负责人说可以换班，只是需要你自己把事情协调好。",
                    C("和同学换班，欠一杯奶茶","收入 $250 · 压力 +4 · 职业 +3",x=>{x.money+=250;x.stress+=4;x.career+=3;}),
                    C("谢绝加班，守住自己的课表","数据分析学习 +10 · 压力 -5",x=>{x.courses[1].learning=Rules.Clamp(x.courses[1].learning+10);x.stress-=5;}),
                    C("接下加班，稍后补课","收入 $500 · 压力 +12 · 数据分析学习 -5",x=>{x.money+=500;x.stress+=12;x.courses[1].learning=Rules.Clamp(x.courses[1].learning-5);}));
                case "r8":return new LifeEvent("表格终于对上了","小组群 · 晚上十一点",s.Has("team")?"还记得那份分工表吗？现在每个人都按约交来了内容，阿晴甚至补好了图注。你们终于能把会议提前结束。":"合并文件比写文件还难。阿晴问你要不要把后面的合作方式改一下，至少让下一个项目少一点凌晨消息。",
                    C("整理成一份共享模板",s.Has("team")?"数据作业 +25 · 职业 +4 · 协作回响":"数据作业 +12 · 职业 +2",x=>{Work(x,1,false,x.Has("team")?25:12);x.career+=x.Has("team")?4:2;x.Flag("team");}),
                    C("今晚结束工作，去吃宵夜","花费 $80 · 压力 -16 · 适应 +3",x=>{x.money-=80;x.stress-=16;x.adaptation+=3;}));
                case "r9":return new LifeEvent("招聘会上的长队","体育馆 · 招聘会","你排到展位时，招聘同事问：「可以用一分钟讲一下你做过的项目吗？」脑内论文摘要开始播放，语速不小心调到了两倍。",
                    C("用一个具体问题讲清自己的贡献","职业 +6 · 压力 +4",x=>{x.career+=6;x.stress+=4;}),
                    C("先听岗位需求，再记下要补的技能","职业 +3 · 求职准备经历 +1",x=>{x.career+=3;x.careerCount++;}));
                case "r10":return new LifeEvent("一场不太标准的实习面试","线上会议 · 下午三点","面试官的猫从镜头前走过。气氛松了下来，你终于能把注意力从「我有没有说错」转向「这份工作到底做什么」。",
                    C("展示作品，并约定可投入的时间",s.career>=28?"面试通过 · 解锁实习 · 压力 +4":"职业不足 28，获得反馈 · 职业 +5",x=>{if(x.career>=28){x.Flag("interview");x.stress+=4;Rules.Experience(x,"通过实习面试，谈好了不与上课冲突的排班。");}else x.career+=5;}),
                    C("询问工作内容，先把课业完成","职业 +3 · 压力 -8",x=>{x.career+=3;x.stress-=8;}));
                case "r11":return new LifeEvent("期末周的互助桌","图书馆 · 靠窗长桌","桌上摆着四台电脑和一袋橘子。没有人能替你写完项目，但有人愿意帮你确认：这张图，真的看得懂吗？",
                    C("交换一次项目反馈","两门上学期项目进度各 +15 · 压力 +5",x=>{Work(x,0,true,15);Work(x,1,true,15);x.stress+=5;}),
                    C("负责补给，让大家停下来吃点东西","花费 $90 · 压力 -18 · 适应 +3",x=>{x.money-=90;x.stress-=18;x.adaptation+=3;}));
                case "r12":return new LifeEvent("第一份期末项目","教学楼 · 学期考核","研究方法课程走到了最后一页。老师提醒：题目不必改变世界，但证据要能支撑结论。你回头看入学时的提纲，发现自己确实会问问题了。",
                    C("做最后一次论证检查","研究方法项目 +12 · 压力 +4",x=>{Work(x,0,true,12);x.stress+=4;}),
                    C("写下这一学期的收获","学业 +3 · 压力 -10",x=>{x.academic+=3;x.stress-=10;}));
                case "r13":return new LifeEvent("学期收尾，不是人生结算","校园 · 冬日午后","数据项目的提交窗口今晚关闭。邮箱里有人晒实习，有人发旅行照片。你把页面关掉，先看看自己这一学期真正完成了什么。",
                    C("复核图表，让结果可复现","数据项目 +15 · 科研 +3 · 压力 +4",x=>{Work(x,1,true,15);x.research+=3;x.stress+=4;}),
                    C("给自己放半天假","压力 -20 · 适应 +3",x=>{x.stress-=20;x.adaptation+=3;}));
                case "r14":return new LifeEvent("新学期的第一张空白页","校园 · 第二学期","城市观察和综合项目开放了。你在课表旁写下几个候选方向：继续研究、找工作，或先留点时间想清楚。空白不一定意味着落后。",
                    C("把研究问题带进新的课程","科研 +5 · 学业 +3",x=>{x.research+=5;x.academic+=3;}),
                    C("把课程项目整理成作品集","职业 +5 · 适应 +3",x=>{x.career+=5;x.adaptation+=3;}),
                    C("重新安排节奏，留出休息日","压力 -18",x=>x.stress-=18));
                case "r15":return new LifeEvent("街区里的田野课","深水埗 · 下午","采访对象问：「你们写的报告，最后会有人看吗？」你愣了一下，决定至少让里面的每一句话都对得起今天的对话。",
                    C("整理访谈，并回访确认","城市观察学习 +15 · 作业 +15 · 压力 +5",x=>{x.courses[2].learning=Rules.Clamp(x.courses[2].learning+15);Work(x,2,false,15);x.stress+=5;}),
                    C("沿着街区慢慢走，补一张观察地图","适应 +7 · 压力 -8",x=>{x.adaptation+=7;x.stress-=8;}));
                case "r16":return new LifeEvent("申请季，开始发送","宿舍 · 邮箱草稿箱","草稿箱里的开头改了又改。职业中心提醒，合适的经历需要写出来，也需要真的投出去。去目标页可以提交工作或博士申请，第 24 回合截止。",
                    C("和职业顾问演练一次表达","职业 +5 · 求职准备经历 +1",x=>{x.career+=5;x.careerCount++;}),
                    C("给潜在导师写一封具体的邮件","导师联系 · 科研 +4",x=>{x.Flag("mentor");x.research+=4;}));
                case "r17":return new LifeEvent("雨太大了，计划小一点","校园巴士站 · 黄色雨衣","雨声大到听不见耳机。群里有人说今天不适合立宏大目标，适合把袜子晾干。你忽然觉得这个建议很科学。",
                    C("回去煮面，整理明天要做的事","压力 -15 · 学业 +2",x=>{x.stress-=15;x.academic+=2;}),
                    C("在咖啡店等雨，和同学聊近况","花费 $65 · 适应 +5 · 压力 -10",x=>{x.money-=65;x.adaptation+=5;x.stress-=10;}));
                case "r18":return new LifeEvent("一段访谈的两种写法","城市观察课 · 作业截止日","老师圈出一句话：「这里是事实，还是你的解释？」删掉一个漂亮但站不住脚的判断，有时候比多写一页更难。",
                    C("补充证据，把推断写清楚","城市观察作业 +20 · 科研 +3",x=>{Work(x,2,false,20);x.research+=3;}),
                    C("保留朴素版本，按时交付","职业 +3 · 压力 -10",x=>{x.career+=3;x.stress-=10;}));
                case "r19":return new LifeEvent("从那封邮件说起","导师办公室 · 茶水间",s.Has("mentor")?"老师记得你之前提过的问题，问最近做到了哪一步。原来那些短短的交流，真的会在后来接上。":"你终于鼓起勇气敲门。老师说：先讲讲你感兴趣的具体问题，不用从「不好意思打扰您」开始。",
                    C("带上记录，讨论下一步",s.Has("mentor")?"科研 +7 · 导师交流回响":"建立导师联系 · 科研 +3",x=>{x.research+=x.Has("mentor")?7:3;x.Flag("mentor");}),
                    C("请教如何把研究经历讲给雇主听","职业 +5 · 压力 -5",x=>{x.career+=5;x.stress-=5;}));
                case "r20":return new LifeEvent("项目提案，终于落地","综合项目课 · 白板前","白板上的箭头越来越多，老师问：「剩下的时间里，哪一个功能必须完成？」你第一次感受到，缩小范围也需要判断力。",
                    C("缩小题目，做出能交付的版本","综合项目作业 +20 · 职业 +4",x=>{Work(x,3,false,20);x.career+=4;}),
                    C("保留难题，请助教帮忙拆解","综合项目学习 +15 · 科研 +4 · 压力 +5",x=>{x.courses[3].learning=Rules.Clamp(x.courses[3].learning+15);x.research+=4;x.stress+=5;}));
                case "r21":return new LifeEvent("有经验的人，也会犹豫","海边 · 傍晚",s.internshipCount>=2?"实习同事问你毕业后会不会留下。你发现自己已经能说出工作的好处，也能说出它不适合自己的地方。":"看着渡轮靠岸，你开始认真想象毕业后的日常。在哪里生活，做什么工作，这两个问题不一定有同一个答案。",
                    C("把自己的优先级写下来","职业 +4 · 适应 +4 · 压力 -6",x=>{x.career+=4;x.adaptation+=4;x.stress-=6;}),
                    C("先给家里打个电话","压力 -16",x=>x.stress-=16));
                case "r22":return new LifeEvent("毕业申请系统已开放","教务处 · 自动邮件","邮件里没有鸡汤，只有清单：课程、作业、申请表。你松了口气，至少这些都可以一项项确认。申请也可以在目标页办理。",
                    C("现在提交毕业申请","记录毕业申请 · 压力 -5",x=>{x.graduationApplied=true;x.stress-=5;}),
                    C("先检查材料，稍后在目标页申请","学业 +2 · 仍需提交毕业申请",x=>x.academic+=2));
                case "r23":return new LifeEvent("最后一次互相改稿","讨论室 · 周五",s.Has("team")?"阿晴把你当初做的共享模板翻了出来。里面已经添了好多人的批注。一个小小的好习惯，陪你们走了这么远。":"同学们约了最后一次改稿会。不需要比较谁的计划更漂亮，只需要帮彼此找出一处能改得更清楚的地方。",
                    C("互相做一次模拟答辩",s.Has("team")?"两门下学期项目各 +20 · 压力 +5":"两门下学期项目各 +12 · 压力 +5",x=>{Work(x,2,true,x.Has("team")?20:12);Work(x,3,true,x.Has("team")?20:12);x.stress+=5;}),
                    C("拍张合照，先把这个晚上留下","压力 -18 · 适应 +5",x=>{x.stress-=18;x.adaptation+=5;Rules.Experience(x,"在讨论室留下了一张有些模糊、但人人都在笑的合照。");}));
                case "r24":return new LifeEvent("最后的申请窗口","职业中心 · 日历上的圈","本年度的工作与博士申请窗口即将关闭。已经投递的材料会按时回复；没发出的草稿，不会自己变成 offer。毕业申请也请再核对一次。",
                    C("复核毕业手续与城市报告","提交毕业申请 · 城市项目 +12",x=>{x.graduationApplied=true;Work(x,2,true,12);}),
                    C("整理这一年的成果清单","职业 +3 · 科研 +3 · 压力 -8",x=>{x.career+=3;x.research+=3;x.stress-=8;}));
                case "r25":return new LifeEvent("答辩之后的走廊","综合项目课 · 最后一场展示","老师问完最后一个问题，你才发现手里的水已经凉了。离开教室时，走廊还是那条走廊，但你已经能清楚地解释自己做了什么。",
                    C("把反馈补进最终报告","综合项目 +15 · 学业 +3",x=>{Work(x,3,true,15);x.academic+=3;}),
                    C("收好材料，和同学去庆祝","花费 $120 · 压力 -20 · 适应 +3",x=>{x.money-=120;x.stress-=20;x.adaptation+=3;}));
                case "r26":return new LifeEvent("手账的最后一页","教务处 · 毕业审核","该交的材料，会在今天做最后核对。去向不止一个时，你可以自己决定；还有未完成的课程，也会得到明确的补修清单。",
                    C("确认毕业申请，收好这一年的记录","提交毕业申请 · 压力 -10",x=>{x.graduationApplied=true;x.stress-=10;}),
                    C("暂不申请毕业，准备延期计划","撤回毕业申请 · 本年进入延期结局",x=>{x.graduationApplied=false;x.Flag("voluntaryDelay");}));
                case "budget":return new LifeEvent("余额比想象中更诚实","宿舍 · 记账本","账单一项项加起来，才发现奶茶并不是唯一的嫌疑人。学生事务处有无息应急金，也有人愿意帮你找一班临时助理工作。",
                    C("接一班临时助理工作","收入 $700 · 压力 +12 · 职业 +3",x=>{x.money+=700;x.stress+=12;x.career+=3;x.Flag("budgetEvent");}),
                    C("申请无息应急生活金","现金 +$1000 · 待还 +$1000 · 压力 -10",x=>{x.money+=1000;x.debt+=1000;x.stress-=10;x.Flag("budgetEvent");}),
                    C("和室友一起调整生活预算","现金 +$150 · 压力 -5",x=>{x.money+=150;x.stress-=5;x.Flag("budgetEvent");}));
                case "burnout":return new LifeEvent("身体先按下了暂停","校园健康中心","你盯着同一段文字看了十分钟。咨询老师说，先睡好和求助，并不是对目标不认真。你决定给接下来的两周腾一点空间。",
                    C("预约支持服务，今天早点回去","压力 -30 · 花费 $80",x=>{x.stress-=30;x.money-=80;x.Flag("burnoutEvent");}),
                    C("请朋友陪着散步，重新排日程","压力 -22 · 适应 +3",x=>{x.stress-=22;x.adaptation+=3;x.Flag("burnoutEvent");}));
                default:return new LifeEvent("平常的一天","校园 · 午后","没有特别的消息。食堂的汤正好温热，待办清单也终于短了一点。",C("记住这个普通的下午","压力 -8",x=>x.stress-=8),C("整理笔记再回去","学业 +2",x=>x.academic+=2));
            }
        }
        public static bool Choose(YearState s,int option)
        {
            if(s.phase!="event")return false;var e=Get(s);if(option<0||option>=e.choices.Length||e.choices[option].unavailable!="")return false;
            var c=e.choices[option];c.apply(s);Rules.Normalize(s);Rules.Experience(s,e.title+" / "+c.title);Rules.Log(s,"事件「"+e.title+"」："+c.effect);
            if(s.eventReturn=="settle")Rules.FinishRound(s);else{s.phase="plan";s.eventId="";}return true;
        }
    }
}
