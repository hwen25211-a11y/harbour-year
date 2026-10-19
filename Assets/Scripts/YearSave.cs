using System;
using UnityEngine;
namespace HarbourYear
{
    public static class YearSave
    {
        const string Key="HarbourYear.Save.v1", Backup="HarbourYear.Backup.v1";
        public static string LastError="";
        public static bool Exists {get{return Read()!=null;}}
        public static void Write(YearState state)
        {
            try{string json=JsonUtility.ToJson(state);var old=PlayerPrefs.GetString(Key,"");if(IsValid(old))PlayerPrefs.SetString(Backup,old);PlayerPrefs.SetString(Key,json);PlayerPrefs.Save();LastError="";}
            catch(Exception e){LastError="存档未写入："+e.Message;Debug.LogWarning(LastError);}
        }
        static bool IsValid(string json){return Parse(json)!=null;}
        public static YearState Parse(string json)
        {
            try{if(string.IsNullOrEmpty(json)||!json.Contains("\"version\"")||!json.Contains("\"courses\""))return null;var s=JsonUtility.FromJson<YearState>(json);if(s==null||s.version!=1||s.round<1||s.round>26||s.courses==null||s.courses.Count!=4||s.plan==null||s.plan.Count>4||s.flags==null||s.journal==null||s.summary==null||s.applications==null)return null;
                if(s.phase!="plan"&&s.phase!="event"&&s.phase!="summary"&&s.phase!="ending")return null;
                foreach(var c in s.courses)if(c==null||c.assignment==null||c.project==null)return null;
                foreach(var p in s.plan)if(p==null||p.course<0||p.course>3||(int)p.kind<0||(int)p.kind>9)return null;
                if(float.IsNaN(s.stress)||float.IsNaN(s.academic))return null;Rules.Normalize(s);return s;
            }catch{return null;}
        }
        public static YearState Read(){var s=Parse(PlayerPrefs.GetString(Key,""));if(s!=null)return s;s=Parse(PlayerPrefs.GetString(Backup,""));if(s!=null)LastError="主存档异常，已恢复上一次备份。";return s;}
    }
}
