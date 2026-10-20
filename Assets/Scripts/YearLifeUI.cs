using System;
using System.Linq;
using UnityEngine;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame
    {
        string lifeTab="住房";
        void DrawLife()
        {
            Text(root,"安顿好日常。",320,130,1200,52,35,Navy,true);Text(root,"本轮账单 $"+Life.Bill(State)+" · 已摆放 "+Life.UsedSlots(State)+" / "+Catalog.Rooms[State.room].slots+" 槽 · 每轮最多购买 6 件商品",320,197,1240,36,20,Muted);
            string[] tabs={"住房","商店","二手群","背包"};for(int i=0;i<4;i++){string t=tabs[i];Button(root,t,320+i*312,253,298,48,()=>{lifeTab=t;Render();},lifeTab==t?Navy:White,lifeTab==t?White:Navy,21);}
            if(lifeTab=="住房")DrawHousing();else if(lifeTab=="背包")DrawBag();else DrawShop(lifeTab=="二手群");
        }
        void DrawHousing()
        {
            for(int i=0;i<3;i++){int id=i;var room=Catalog.Rooms[i];var p=Box(root,320+i*418,332,402,177,White).transform;Text(p,room.name,20,17,362,37,24,Navy,true);Text(p,"双周租金 $"+room.rent+" · "+room.slots+" 槽\n休息额外恢复疲劳 "+room.rest,20,69,363,59,19,Muted);string locked=Life.RoomLock(State,id);Button(p,locked==""?"搬入 · 搬家费 $600":locked,20,132,363,31,()=>{Life.Move(State,id);SaveAndRender();},Pale,Navy,17,State.phase=="plan"&&locked=="");}
            Text(root,"我的家具 · 点击摆放或收起",320,545,1228,37,25,Navy,true);
            if(State.owned.Count==0){Text(root,"房间还是空的。到商店或二手群看看，先挑一件真正用得上的。",320,614,1214,54,23,Muted);Picture(root,roomArt,320,691,1234,202);return;}
            var list=Scroll(root,320,609,1237,289,Math.Max(289,(State.owned.Count+2)/3*160));
            for(int j=0;j<State.owned.Count;j++){int id=State.owned[j];var item=Catalog.Items[id];bool on=State.equipped.Contains(id);var p=Box(list,(j%3)*411,(j/3)*160,393,143,White).transform;Text(p,item.name+" · "+item.slots+" 槽",17,14,359,31,22,Navy,true);Text(p,item.note,17,55,359,44,18,Muted);Button(p,on?"已摆放 · 收起来":"摆进房间",17,106,359,29,()=>{if(!Life.Equip(State,id))Notice("家具槽位不足，请先收起其他家具或换房。");else SaveAndRender();},on?Sage:Pale,on?White:Navy,17,State.phase=="plan");}
        }
        void DrawShop(bool market)
        {
            Text(root,market?"毕业转让群 · 每轮刷新一次 · 低价抢手品有 25% 概率已售，不成功不扣款":"校园生活商店 · 食物放入背包，家具购买后需要摆放",320,331,1226,47,21,Muted);
            int count=market?State.market.Count:Catalog.Items.Length;var list=Scroll(root,320,401,1239,493,Math.Max(493,(count+2)/3*207));
            for(int j=0;j<count;j++)
            {
                int listing=market?j:-1,id=market?State.market[j].item:j;var item=Catalog.Items[id];int price=market?State.market[j].price:item.price;
                var p=Box(list,(j%3)*411,(j/3)*207,393,189,White).transform;Text(p,item.name+(market&&State.market[j].rare?" · 抢手":""),17,17,357,35,24,Navy,true);Text(p,item.note,17,62,357,61,19,Muted);
                string locked=market&&State.market[j].sold?"已售出":Life.BuyLock(State,id,price);Button(p,locked==""?"购买 · $"+price:locked,17,139,357,34,()=>{string result=Life.Buy(State,id,listing);YearSave.Write(State);Notice(result);},locked==""?Orange:Pale,locked==""?White:Muted,19,locked=="");
            }
        }
        void DrawBag()
        {
            Text(root,"食物可以自己吃，也可以送给同学；家具留在住房页管理。",320,334,1231,44,21,Muted);
            if(State.bag.Count==0){Text(root,"背包里还没有食物。\n到商店或二手群买一点，忙的时候也要吃饭。",320,439,1200,113,28,Navy);return;}
            var list=Scroll(root,320,407,1238,487,Math.Max(487,State.bag.Count*110));for(int i=0;i<State.bag.Count;i++){int idx=i;var item=Catalog.Items[State.bag[i]];var p=Box(list,0,i*110,1218,94,White).transform;Text(p,item.name,20,19,300,34,24,Navy,true);Text(p,"疲劳 -"+item.foodFatigue+" · 压力 -"+item.foodStress,347,24,407,31,20,Muted);Button(p,"自己吃",797,20,176,51,()=>{Life.Consume(State,idx);SaveAndRender();},Sage,White,21,State.phase=="plan");Button(p,"送给同学",990,20,207,51,()=>GiftDialog(idx),Pale,Navy,21,State.phase=="plan");}
        }
        void GiftDialog(int index)
        {
            var p=Modal("把一份好心情分出去。","只赠送食物 · 默契影响小组匹配",880,740);for(int i=0;i<State.students.Count;i++){int id=i;var n=State.students[i];Button(p,n.name+" · 默契 "+n.goodwill,32,148+i*71,816,55,()=>{Life.Consume(State,index,id);SaveAndRender();},Pale,Navy,23);}
            Button(p,"先放回包里",32,646,816,56,CloseModal,Navy,White,23);
        }
    }
}
