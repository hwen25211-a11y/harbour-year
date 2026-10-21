using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
// 轻松版存档：只用自己的键，与经典版 17 轮存档互不读写。导出文件与网页版轻松版相同，可互相导入。
namespace HarbourYear.Lite
{
    public class SaveData{public GameState state;public LiteMeta lite;public bool upgraded,repaired;public List<int> repairedSlots=new List<int>();}
    public static class LiteSave
    {
        public const string Key="HarbourYear.Lite.Save",Unreadable="HarbourYear.Lite.Unreadable",BeforeJob="HarbourYear.Lite.BeforeJob",BeforeNewGame="HarbourYear.Lite.BeforeNewGame",BeforeImport="HarbourYear.Lite.BeforeImport";
        public const string Product="hk-master-life-lite",FilePrefix="HK-Master-Life-LITE-save";
        public static bool SuppressWrites;
        public static string Encode(GameState s,LiteMeta m,bool pretty=false){var o=new JObj();o["productId"]=Product;o["schemaVersion"]=2.0;o["state"]=s.ToJ();o["lite"]=m.ToJ();return Json.Write(o,pretty);}
        // 结构检查：载入失败或数值异常时整体拒绝，不读取半份存档。
        static string Check(GameState s,LiteMeta m){return CheckState(s)??CheckMeta(m);}
        static string CheckMeta(LiteMeta m){return m.v!=2||(m.focus!="balanced"&&Cat.Foci.All(f=>f.id!=m.focus))?"轻松版记录无效。":null;}
        // 网页版 308afee 包可能整轮多排一班兼职；只在这一种情况下修正：从最晚的尚未执行、系统推荐的兼职半日起改成休息，已执行的不动。
        static GameState RepairJobs(GameState s,List<int> auto)
        {
            if(Core.JobCount(s.plan)<=6)return null;var t=s.Clone();int start=Core.Pos(t);var future=Enumerable.Range(0,t.plan.Count).Where(i=>i>=start&&Core.JobId(t.plan[i])!=null).ToList();
            foreach(var i in future.Where(auto.Contains).Reverse().Concat(future.Where(i=>!auto.Contains(i)).Reverse())){if(Core.JobCount(t.plan)<=6)break;t.plan[i]="rest";}
            return CheckState(t)==null?t:null;
        }
        // 第一批／P01 的轻松版记录（schemaVersion 1）：游戏进度原样保留，只升级轻松版记录。
        static LiteMeta FromV1(JObj o)
        {
            string f=o.S("focus");if(f!="study"&&f!="balanced"&&f!="city")throw new FormatException("旧倾向无效");
            var m=new LiteMeta{v=2,focus=f=="study"?"academic":f=="city"?"life":"balanced",round=o.I("round"),free=J.Ints(o.A("free")),auto=J.Ints(o.A("auto")),customized=o.B("customized"),seen=J.Strs(o.A("seen")),teamPrompted=J.Strs(o.A("teamPrompted")),defaultTeams=J.Strs(o.A("defaultTeams"))};
            m.digest=o.A("digest").Cast<JObj>().Select(d=>new Digest{round=d.I("round"),day=d.I("day"),text=d.S("text")}).ToList();m.focusPhase=Pace.PhaseIndex(Math.Max(0,m.round));m.dayCheck=o.S("dayCheck");return m;
        }
        static string CheckState(GameState s)
        {
            if(s.version!=9)return "存档版本不符。";if(s.round<0||s.round>Cal.Last)return "轮次超出范围。";
            if(!new[]{"planning","running","summary","finished"}.Contains(s.phase))return "阶段无效。";
            if(Cat.Economies.All(e=>e.id!=s.economy)||Cat.HomeOf(s.housing)==null)return "经济条件或住房无效。";
            int n=Cal.Slots(s.round);if(s.plan.Count!=Core.Blank(s.round).Count||s.cursor<0||s.cursor>n||(s.phase=="planning"&&s.cursor!=0)||(s.phase=="running"&&s.cursor>=n)||((s.phase=="summary"||s.phase=="finished")&&s.cursor!=n))return "日程进度无效。";
            if(s.logs.Count!=s.cursor)return "本轮记录与进度不一致。";if(s.plan.Take(s.cursor).Any(x=>x==null))return "已执行的半日缺少安排。";
            if(Core.JobCount(s.plan)>6)return "本轮兼职超过每轮 6 个半日的上限。";
            if(s.npcs.Count!=26||s.npcs.Select(x=>x.id).Distinct().Count()!=26||s.npcs.Any(x=>!s.affection.ContainsKey(x.id)))return "同学名单不完整。";
            if(!s.appearance.Valid()||string.IsNullOrEmpty(s.name)||s.name.Length>20)return "角色信息无效。";
            if(Core.CheckAbilities(s.initialAbilities)!=null||s.abilities.Any(a=>a<0||a>100||double.IsNaN(a)))return "能力数值无效。";
            if(s.fatigue<0||s.fatigue>100||s.stress<0||s.stress>100)return "精力或压力无效。";
            if(s.plan.Any(x=>x!=null&&Cat.BasicOf(x)==null&&!x.Contains(":")))return "日程里有无法识别的安排。";
            return null;
        }
        public static SaveData Decode(string text,out string error)
        {
            error=null;object raw;try{raw=Json.Parse(text);}catch{error="文件不是有效的 JSON。";return null;}
            var o=raw as JObj;if(o==null){error="文件不是存档。";return null;}
            if(o.S("productId")!=Product){error=o["version"] is double&&o.Has("plan")?"这是原版《港硕生活模拟器》的存档，轻松版不能读取。原版存档不会被改动，请用原版试玩包继续。":"这不是轻松版的存档文件（缺少轻松版标识）。";return null;}
            var v=o["schemaVersion"];if(!(v is double)||((double)v!=2&&(double)v!=1)){error="存档版本 "+(v==null?"undefined":Json.Write(v))+" 与当前轻松版（2）不一致，暂不能读取。";return null;}
            try
            {
                var s=GameState.From(o.O("state"));var lite=o.O("lite");var save=new SaveData();var bad=CheckState(s);
                if(bad!=null)
                {
                    var fixedState=RepairJobs(s,lite["auto"] is List<object>?J.Ints(lite.A("auto")):new List<int>());if(fixedState==null){error="存档里的游戏进度已损坏或不完整，未读取（"+bad+"）";return null;}
                    save.repairedSlots=Enumerable.Range(0,s.plan.Count).Where(i=>fixedState.plan[i]!=s.plan[i]).ToList();s=fixedState;save.repaired=true;
                }
                LiteMeta m;try{m=(double)v==1?FromV1(lite):LiteMeta.From(lite);}catch{error="存档里的轻松版记录已损坏，未读取。";return null;}
                if(CheckMeta(m)!=null){error="存档里的轻松版记录已损坏，未读取。";return null;}
                save.state=s;save.lite=m;save.upgraded=(double)v==1;return save;
            }
            catch(Exception e){error="存档里的游戏进度已损坏或不完整，未读取。";Debug.LogWarning(e);return null;}
        }
        public static bool HasRaw{get{return PlayerPrefs.HasKey(Key);}}
        public static SaveData Load(out bool unreadable)
        {
            unreadable=false;if(!PlayerPrefs.HasKey(Key))return null;var raw=PlayerPrefs.GetString(Key,"");string err;var s=Decode(raw,out err);
            if(s==null){unreadable=true;if(!PlayerPrefs.HasKey(Unreadable)){PlayerPrefs.SetString(Unreadable,raw);PlayerPrefs.Save();}}return s;
        }
        public static string LastError="";
        public static bool Write(GameState s,LiteMeta m)
        {
            if(SuppressWrites)return true;
            try{var text=Encode(s,m);string err;if(Decode(text,out err)==null)throw new Exception(err);PlayerPrefs.SetString(Key,text);PlayerPrefs.Save();LastError="";return true;}
            catch(Exception e){LastError=e.Message;Debug.LogWarning("轻松版存档未写入："+e.Message);return false;}
        }
        // 替换当前进度前，把旧进度另存一份；备份失败时不替换。
        public static string Replace(SaveData next,SaveData current,string backupKey)
        {
            string old=current!=null?Encode(current.state,current.lite):PlayerPrefs.HasKey(Key)?PlayerPrefs.GetString(Key):null;
            if(old!=null){PlayerPrefs.SetString(backupKey,old);if(PlayerPrefs.GetString(backupKey)!=old)return "没能保存旧进度的备份，所以没有替换：旧进度仍然保留。";}
            if(!Write(next.state,next.lite)){if(old!=null)PlayerPrefs.SetString(Key,old);return "新进度没能写入，旧进度仍然保留（另有一份备份）。";}
            return null;
        }
        public static void SaveBeforeJob(GameState s,LiteMeta m){if(!SuppressWrites){PlayerPrefs.SetString(BeforeJob,Encode(s,m));PlayerPrefs.Save();}}
        public static SaveData LoadBeforeJob(){if(!PlayerPrefs.HasKey(BeforeJob))return null;string e;return Decode(PlayerPrefs.GetString(BeforeJob),out e);}
        public static string FileName(GameState s){return FilePrefix+"_round"+(s.round+1).ToString("00")+"_"+DateTime.Now.ToString("yyyy-MM-dd")+".json";}
#if UNITY_WEBGL&&!UNITY_EDITOR
        [DllImport("__Internal")] static extern void LiteDownload(string name,string text);
        [DllImport("__Internal")] static extern void LitePickFile(string obj,string method);
#endif
        // 导出：浏览器版下载文件；电脑版写到桌面（找不到桌面则写到“下载”）。
        public static string Export(string name,string text)
        {
#if UNITY_WEBGL&&!UNITY_EDITOR
            LiteDownload(name,text);return "已开始下载 "+name;
#else
            var dir=ExportFolder();var path=Path.Combine(dir,name);File.WriteAllText(path,text);return path;
#endif
        }
        public static string ExportFolder()
        {
            var desk=Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);if(!string.IsNullOrEmpty(desk)&&Directory.Exists(desk))return desk;
            var down=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");if(Directory.Exists(down))return down;return Application.persistentDataPath;
        }
        // 导入：浏览器版弹出选择文件；电脑版读取桌面或“下载”里最新的轻松版存档。
        public static string PickImport(string obj,string method,out string text)
        {
            text=null;
#if UNITY_WEBGL&&!UNITY_EDITOR
            LitePickFile(obj,method);return null;
#else
            var dirs=new[]{ExportFolder(),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads")}.Distinct().Where(Directory.Exists);
            var file=dirs.SelectMany(d=>Directory.GetFiles(d,FilePrefix+"*.json")).OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
            if(file==null)return "在桌面和“下载”文件夹里没有找到以 "+FilePrefix+" 开头的存档文件。";
            if(new FileInfo(file).Length>2000000)return "文件太大，不是存档。";text=File.ReadAllText(file);return "file:"+file;
#endif
        }
    }
}
