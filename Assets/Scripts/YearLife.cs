using System;
using System.Linq;
namespace HarbourYear
{
    public static class Life
    {
        public static void CreateStudents(YearState s)
        {
            string[] names={"陈知晴","周一鸣","林予安","许沐","郑嘉宁","宋澄"};string[] habits={"喜欢提前列清单","习惯边走边想","把版本号写得很认真","常带一杯冻柠茶","汇报前会练习计时","总能找到空讨论室"};
            s.students.Clear();for(int i=0;i<6;i++){var p=new Student{name=names[i],habit=habits[i],reliability=Rules.Next(s,52,96),look=new Appearance{gender=Rules.Next(s,0,2),age=Rules.Next(s,0,2),front=Rules.Next(s,0,4),back=Rules.Next(s,0,4),outfit=Rules.Next(s,0,4)}};for(int j=0;j<5;j++)p.skills[j]=Rules.Next(s,30,91);s.students.Add(p);}
        }
        public static float TeamFit(YearState s,int course)
        {
            float total=0;for(int r=0;r<5;r++){int n=s.courses[course].team[r];if(n<0||n>=s.students.Count)total+=s.skills[r]*.65f;else{var p=s.students[n];total+=p.skills[r]*(.6f+.4f*p.reliability/100f)+p.goodwill*.1f;}}return Rules.Clamp(total/5);
        }
        public static bool Assign(YearState s,int course,int role,int student)
        {
            if(s.phase!="plan"||course<0||course>=8||role<0||role>4||student<-1||student>=s.students.Count||s.courses[course].project.submitted)return false;
            // A classmate can own one role in a course. Swapping keeps the previous role assigned.
            int old=s.courses[course].team[role];if(student>=0){int other=Array.IndexOf(s.courses[course].team,student);if(other>=0)s.courses[course].team[other]=old;}
            s.courses[course].team[role]=student;return true;
        }
        public static int Bill(YearState s){return (int)((Catalog.Rooms[s.room].rent+Balance.LivingCost)*(Rules.Holiday(s)?1.5f:1));}
        public static int UsedSlots(YearState s){return s.equipped.Sum(i=>Catalog.Items[i].slots);}
        public static int Bonus(YearState s,int type){return s.equipped.Sum(i=>type==0?Catalog.Items[i].rest:type==1?Catalog.Items[i].calm:Catalog.Items[i].study);}
        public static bool Equip(YearState s,int item)
        {if(s.phase!="plan"||!s.owned.Contains(item)||Catalog.Items[item].Food)return false;if(s.equipped.Contains(item)){s.equipped.Remove(item);return true;}if(UsedSlots(s)+Catalog.Items[item].slots>Catalog.Rooms[s.room].slots)return false;s.equipped.Add(item);return true;}
        public static string RoomLock(YearState s,int room){if(room==s.room)return "正在租住";if(UsedSlots(s)>Catalog.Rooms[room].slots)return "先收起部分家具";if(s.money<Balance.MoveCost)return "搬家费需要 $600";return "";}
        public static bool Move(YearState s,int room){if(s.phase!="plan"||room<0||room>2||RoomLock(s,room)!="")return false;s.money-=Balance.MoveCost;s.room=room;Rules.Experience(s,"搬进"+Catalog.Rooms[room].name+"，从本轮起按新租金结算。");return true;}
        public static void RefreshMarket(YearState s)
        {s.market.Clear();for(int i=0;i<5;i++){int id=Rules.Next(s,0,Catalog.Items.Length);bool rare=Rules.Next(s,0,4)==0;s.market.Add(new MarketListing{item=id,price=(int)(Catalog.Items[id].price*(rare?.35f:Rules.Next(s,55,81)/100f)),rare=rare});}}
        public static string BuyLock(YearState s,int item,int price)
        {if(s.phase!="plan")return "请先结束当前日程";if(s.money<price)return "余额不足";if(s.shopBought>=6)return "本轮已购买 6 件，下一轮补货";if(!Catalog.Items[item].Food&&s.owned.Contains(item))return "已经拥有";return "";}
        public static string Buy(YearState s,int item,int listing=-1)
        {
            if(item<0||item>=Catalog.Items.Length)return "商品不存在";
            MarketListing offer=listing>=0&&listing<s.market.Count?s.market[listing]:null;
            if(listing>=0&&(offer==null||offer.sold||offer.item!=item))return "这件已经售出";
            int price=offer==null?Catalog.Items[item].price:offer.price;string locked=BuyLock(s,item,price);if(locked!="")return locked;
            if(offer!=null){offer.sold=true;if(offer.rare&&Rules.Next(s,0,4)==0)return "慢了一步，卖家已经答应另一位同学了；没有扣款。";}
            s.money-=price;s.shopBought++;if(Catalog.Items[item].Food)s.bag.Add(item);else s.owned.Add(item);
            Rules.Experience(s,"花 $"+price+" 买到"+Catalog.Items[item].name+"。");return "买到了："+Catalog.Items[item].name+(Catalog.Items[item].Food?"，在背包中使用或赠送。":"，可在住房页摆放。");
        }
        public static bool Consume(YearState s,int bagIndex,int student=-1)
        {
            if(s.phase!="plan"||bagIndex<0||bagIndex>=s.bag.Count||student>=s.students.Count)return false;
            int id=s.bag[bagIndex];s.bag.RemoveAt(bagIndex);var item=Catalog.Items[id];
            if(student<0){s.fatigue-=item.foodFatigue;s.stress-=item.foodStress;}else{s.students[student].goodwill=Math.Min(30,s.students[student].goodwill+6);s.stress-=3;Rules.Experience(s,"把"+item.name+"送给"+s.students[student].name+"，对方说下次讨论带点心来。");}Rules.Normalize(s);return true;
        }
    }
}
