using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace HarbourYear
{
    [Serializable] public class SaveEnvelope {public int schema=2;public YearState state;public List<int> emptySlots=new List<int>();}
    public static class YearSave
    {
        const string Key="HarbourYear.Save.v2",Backup="HarbourYear.Backup.v2";
        public static string LastError="";
        public static bool SuppressWrites;
        public static bool Exists{get{return Read()!=null;}}
        public static bool HasLegacy{get{return PlayerPrefs.HasKey("HarbourYear.Save.v1");}}
        public static string Encode(YearState s)
        {var e=new SaveEnvelope{state=s};for(int i=0;i<s.plan.Count;i++)if(s.plan[i]==null)e.emptySlots.Add(i);try{return JsonUtility.ToJson(e);}finally{foreach(int i in e.emptySlots)s.plan[i]=null;}}
        public static void Write(YearState s)
        {
            if(SuppressWrites)return;
            try{string json=Encode(s);if(Parse(json)==null)throw new Exception("存档状态校验失败，保留上次记录");string old=PlayerPrefs.GetString(Key,"");if(Parse(old)!=null)PlayerPrefs.SetString(Backup,old);PlayerPrefs.SetString(Key,json);PlayerPrefs.Save();LastError="";}
            catch(Exception e){LastError="存档未写入："+e.Message;Debug.LogWarning(LastError);}
        }
        static bool LookValid(Appearance a){return a!=null&&a.gender>=0&&a.gender<2&&a.age>=0&&a.age<2&&a.front>=0&&a.front<4&&a.back>=0&&a.back<4&&a.outfit>=0&&a.outfit<4;}
        static bool Finite(float f){return !float.IsNaN(f)&&!float.IsInfinity(f);}
        public static YearState Parse(string json)
        {
            try
            {
                if(string.IsNullOrEmpty(json)||!json.Contains("\"schema\"")||!json.Contains("\"version\""))return null;
                var e=JsonUtility.FromJson<SaveEnvelope>(json);var s=e.state;if(e.schema!=2||s==null||s.version!=2||s.round<1||s.round>17||s.courses==null||s.courses.Count!=8||s.plan==null||s.flags==null||s.journal==null||s.summary==null||s.applications==null||s.students==null||s.skills==null||s.skills.Length!=5||!LookValid(s.look))return null;
                if(!new[]{"intro","setup","plan","executing","event","summary","ending"}.Contains(s.phase))return null;
                if(s.room<0||s.room>2||s.economy<0||s.economy>2||s.owned==null||s.equipped==null||s.bag==null||s.market==null||e.emptySlots==null)return null;
                bool ready=s.phase!="intro"&&s.phase!="setup";if(ready&&(s.plan.Count!=Rules.Slots(s)||s.students.Count!=6))return null;
                if(s.cursor<0||s.cursor>s.plan.Count)return null;
                foreach(var i in e.emptySlots){if(i<0||i>=s.plan.Count)return null;s.plan[i]=null;}
                if((s.phase=="executing"||s.phase=="summary"||s.phase=="ending"||s.phase=="event"&&s.eventReturn=="executing")&&s.plan.Any(p=>p==null))return null;
                foreach(var c in s.courses)if(c==null||c.assignment==null||c.project==null||c.team==null||c.team.Length!=5||c.team.Any(i=>i<-1||i>=6)||!Finite(c.learning)||!Finite(c.assignment.work)||!Finite(c.project.work)||c.attended+c.approvedLeave+c.absent!=c.scheduled)return null;
                foreach(var p in s.plan)if(p!=null&&((int)p.kind<0||(int)p.kind>=Catalog.Actions.Length||p.course<0||p.course>=8))return null;
                foreach(var n in s.students)if(n==null||!LookValid(n.look)||n.skills==null||n.skills.Length!=5)return null;
                if(s.owned.Any(i=>i<0||i>=5)||s.equipped.Any(i=>!s.owned.Contains(i))||s.bag.Any(i=>i<5||i>=8))return null;
                if(!Finite(s.stress)||!Finite(s.fatigue)||!Finite(s.academic)||!Finite(s.research)||!Finite(s.career)||!Finite(s.adaptation)||s.skills.Any(f=>!Finite(f)))return null;
                Rules.Normalize(s);return s;
            }catch{return null;}
        }
        public static YearState Read(){LastError="";var s=Parse(PlayerPrefs.GetString(Key,""));if(s!=null)return s;s=Parse(PlayerPrefs.GetString(Backup,""));if(s!=null)LastError="主存档异常，已恢复上一次备份。";return s;}
    }
}
