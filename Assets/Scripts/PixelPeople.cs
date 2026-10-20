using System;
using System.Collections.Generic;
using UnityEngine;
using static HarbourYear.NotebookUI;
namespace HarbourYear
{
    // Original code-defined pixel layers. All bodies share a garment/hair anchor; no face/body stats coupling.
    public static class PixelPeople
    {
        static readonly Dictionary<string,Texture2D> cache=new Dictionary<string,Texture2D>();
        public static void Draw(Transform p,Appearance a,float x,float y,float w,float h)
        {
            string key=a.gender+":"+a.age+":"+a.front+":"+a.back+":"+a.outfit;Texture2D t;
            if(!cache.TryGetValue(key,out t)){t=Make(a);cache[key]=t;}
            var im=Picture(p,t,x,y,w,h);im.uvRect=new Rect(0,0,1,1);im.name="Avatar "+key;
        }
        static Texture2D Make(Appearance a)
        {
            const int W=64,H=128;var pixels=new Color32[W*H];
            Action<int,int,int,int,string> rect=(x,y,w,h,c)=>{Color32 col=Hex(c);for(int yy=Math.Max(0,y);yy<Math.Min(H,y+h);yy++)for(int xx=Math.Max(0,x);xx<Math.Min(W,x+w);xx++)pixels[(H-1-yy)*W+xx]=col;};
            Action<int[],string> poly=(points,c)=>{Color32 col=Hex(c);for(int yy=0;yy<H;yy++)for(int xx=0;xx<W;xx++){bool inside=false;for(int i=0,j=points.Length-2;i<points.Length;j=i,i+=2){float xi=points[i],yi=points[i+1],xj=points[j],yj=points[j+1];if((yi>yy)!=(yj>yy)&&xx<(xj-xi)*(yy-yi)/(yj-yi)+xi)inside=!inside;}if(inside)pixels[(H-1-yy)*W+xx]=col;}};
            string skin="D6A07B",light="E7B58D",shade="B7785E",hair=a.gender==0?"353038":"493329",shine=a.age==1?"93887C":"705043";int broad=a.age==1?2:0;
            // Back hair has four distinct designs per gender, underneath body and clothes.
            if(a.gender==0){poly(new[]{24,9,40,9,43,15,43,28+a.back*2,39,34+a.back,24,32,21,20},hair);if(a.back==2)rect(18,18,6,18,hair);if(a.back==3){rect(40,26,6,15,hair);rect(22,28,3,10,shine);}}
            else{int length=new[]{29,44,55,37}[a.back];poly(new[]{23,9,39,9,44,15,45,length,39,length+4,20,length+2,19,19},hair);if(a.back==2){rect(44,27,7,33,hair);rect(45,38,5,13,shine);}if(a.back==3){rect(18,26,6,24,hair);rect(19,36,4,14,shine);}}
            // Four body silhouettes: gender alters shoulders/hips; age changes build and facial pixel detail.
            poly(new[]{24,13,39,13,42,19,41,29,37,35,27,35,22,29,21,20},skin);rect(25,16,13,13,light);rect(29,33,8,9,skin);
            int left=a.gender==0?18-broad:20-broad,right=a.gender==0?46+broad:44+broad;
            poly(new[]{left,40,27,37,38,37,right,42,right+4,69,right,82,right-5,79,right-6,61,21,61,left+4,80,left-1,80,left-4,66},skin);
            // Legs and shoes, covered selectively by each outfit.
            poly(new[]{22-broad,72,42+broad,72,42,93,38,119,33,119,32,88,30,119,24,119,21,94},shade);
            rect(23,116,8,6,"2A303B");rect(33,116,9,6,"2A303B");rect(21,121,11,3,"CFBDA5");rect(33,121,12,3,"CFBDA5");
            string[] male={"425D69","B86D43","303F53","78978D"},female={"DDC5A2","B57A4C","535364","73978C"};string coat=(a.gender==0?male:female)[a.outfit];
            string trousers=a.outfit==0?"A18C6D":a.outfit==1?"526970":"374555";
            poly(new[]{22-broad,72,42+broad,72,42,94,38,117,33,117,32,88,30,117,24,117,21,94},trousers);rect(24,83,2,30,"718080");
            poly(new[]{left,41,26,37,39,37,right,42,right+3,58,right-3,61,right-5,54,right-2,77,left+2,77,left+5,54,left-1,61,left-5,57},coat);
            if(a.outfit==0){poly(new[]{28,39,36,39,38,75,26,75},"EBE2CE");rect(31,42,2,32,"C3B494");rect(left+3,55,6,2,"B9AD8C");if(a.gender==1){poly(new[]{22-broad,73,42+broad,73,47,98,18,98},"3D5263");rect(24,78,2,18,"667788");rect(39,79,2,17,"667788");}}
            if(a.outfit==1){poly(new[]{25,38,38,38,40,44,35,47,27,46,22,42},"DFC096");rect(28,48,1,9,"DBC5A8");rect(36,48,1,9,"DBC5A8");rect(25,64,14,7,"8E573E");if(a.gender==1)rect(22,78,5,34,"677F6E");}
            if(a.outfit==2){poly(new[]{27,38,38,38,32,58},"E4D9CA");poly(new[]{24,39,30,45,28,50,32,58,26,65,21,43},"72828B");poly(new[]{40,39,34,45,36,50,32,58,38,65,43,43},"72828B");rect(31,59,2,15,"BBA174");if(a.gender==0){poly(new[]{31,41,34,41,35,51,32,56,30,51},"A2644E");}}
            if(a.outfit==3){rect(left-2,55,7,4,"E0D7C6");rect(right-4,55,7,4,"E0D7C6");rect(31,41,2,35,"D8D1BC");rect(36,47,5,6,"506E68");rect(29,46,1,30,"ADC2B3");if(a.gender==1){poly(new[]{23-broad,75,42+broad,75,44,99,20,99},"394C58");rect(24,81,2,16,"617584");}}
            // Front hair: independently selectable, gender-specific silhouettes.
            if(a.gender==0){switch(a.front){case 0:poly(new[]{21,20,22,12,27,7,39,9,43,15,42,21,38,17,29,16,24,23},hair);break;case 1:poly(new[]{21,19,22,10,39,9,44,16,40,23,36,18,33,22,29,18,24,23},hair);break;case 2:poly(new[]{21,18,25,5,34,6,42,12,43,20,35,15,27,15},hair);break;case 3:poly(new[]{20,18,22,11,26,8,30,10,35,7,40,10,44,17,40,23,37,17,32,19,28,15,24,24},hair);break;}rect(24,12,8,2,shine);}
            else{switch(a.front){case 0:poly(new[]{20,20,21,13,25,9,39,9,44,17,43,26,39,26,39,19,24,19,24,27,20,27},hair);break;case 1:poly(new[]{20,20,22,12,37,8,43,14,44,22,40,20,34,15,27,22,22,30,20,28},hair);break;case 2:poly(new[]{20,25,21,13,29,8,40,10,45,19,44,30,40,25,37,16,33,13,29,17,25,27,22,30},hair);break;case 3:poly(new[]{20,23,21,12,28,7,39,8,44,15,42,21,38,18,30,16,25,20,22,27},hair);rect(17,15,5,6,hair);break;}rect(25,12,8,2,shine);}
            rect(26,24,3,1,"3A3030");rect(36,24,3,1,"3A3030");rect(32,26,1,3,shade);rect(30,31,5,1,"9D6557");if(a.age==1){rect(24,27,3,1,shade);rect(38,27,3,1,shade);rect(23,18,2,5,shine);}
            var tex=new Texture2D(W,H,TextureFormat.RGBA32,false);tex.name="Layered student";tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;tex.SetPixels32(pixels);tex.Apply();return tex;
        }
    }
}
