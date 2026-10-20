using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    public partial class YearGame
    {
        void DrawSetup()
        {
            Text(root,"先认识一下，今年的你。",60,48,1440,62,39,Navy,true);Text(root,"外观只影响形象，不决定能力。开局选择可以慢慢比较。",62,123,1390,37,21,Muted);
            var preview=Box(root,60,197,390,645,Hex("E6DDC8")).transform;PixelPeople.Draw(preview,State.look,75,33,240,468);
            Text(preview,State.name,30,527,330,46,31,Navy,true,TextAlignmentOptions.Center);Text(preview,"前后发独立 · 整套衣装",30,590,330,29,18,Muted,false,TextAlignmentOptions.Center);
            Text(root,"姓名",491,199,340,30,20,Navy,true);var field=InputBox(root,State.name,491,241,452,50);field.onEndEdit.AddListener(v=>{State.name=v;YearSave.Write(State);});
            ToggleRow("性别",new[]{"男","女"},State.look.gender,491,321,i=>{State.look.gender=i;SaveAndRender();});
            ToggleRow("基础素体",new[]{"青年","中年"},State.look.age,491,425,i=>{State.look.age=i;SaveAndRender();});
            Cycle("前发",State.look.front,491,540,()=>{State.look.front=(State.look.front+1)%4;SaveAndRender();});
            Cycle("后发",State.look.back,723,540,()=>{State.look.back=(State.look.back+1)%4;SaveAndRender();});
            Text(root,"整套服装",491,652,456,31,20,Navy,true);
            for(int i=0;i<4;i++){int id=i;Button(root,Catalog.Outfits[i],491+(i%2)*231,698+(i/2)*65,217,50,()=>{State.look.outfit=id;SaveAndRender();},State.look.outfit==i?Navy:Pale,State.look.outfit==i?White:Navy,21);}
            Text(root,"经济条件",1000,199,539,33,23,Navy,true);
            for(int i=0;i<3;i++){int id=i;Button(root,Balance.EconomyNames[i]+"  $"+Balance.StartingMoney[i].ToString("N0"),1000,252+i*63,532,50,()=>{State.economy=id;SaveAndRender();},State.economy==i?Orange:White,State.economy==i?White:Navy,21);}
            Text(root,"租住房屋",1000,467,539,33,23,Navy,true);
            for(int i=0;i<3;i++){int id=i;var room=Catalog.Rooms[i];Button(root,room.name+" / "+room.slots+" 槽 / 双周 $"+room.rent,1000,520+i*67,532,54,()=>{State.room=id;SaveAndRender();},State.room==i?Navy:White,State.room==i?White:Navy,19);}
            Text(root,"学费已支付；房租与生活费按轮扣除。\n当前房型每双周总开销 $"+(Catalog.Rooms[State.room].rent+Balance.LivingCost)+"，寒假按三周计。",1000,745,532,88,20,Muted);
            Button(root,"就从这里开始  →",60,886,1472,62,()=>{Rules.Start(State);SaveAndRender();ShowGuide();},Orange,White,25,true,"StartYear");
        }
        TMP_InputField InputBox(Transform parent,string value,float x,float y,float w,float h)
        {
            var bg=Box(parent,x,y,w,h,White);bg.raycastTarget=true;var input=bg.gameObject.AddComponent<TMP_InputField>();var txt=Text(bg.transform,value,12,5,w-24,h-10,24,Navy);txt.richText=false;input.textViewport=bg.rectTransform;input.textComponent=txt;input.fontAsset=NotebookUI.Font;input.characterLimit=12;input.text=value;return input;
        }
        void ToggleRow(string title,string[] names,int current,float x,float y,Action<int> choose)
        {Text(root,title,x,y,450,29,20,Navy,true);for(int i=0;i<names.Length;i++){int id=i;Button(root,names[i],x+i*231,y+40,217,49,()=>choose(id),current==i?Navy:Pale,current==i?White:Navy,22);}}
        void Cycle(string title,int value,float x,float y,Action next){Text(root,title,x,y,215,29,20,Navy,true);Button(root,"款式 "+(value+1)+" / 4  →",x,y+41,217,50,next,Pale,Navy,21);}
    }
}
