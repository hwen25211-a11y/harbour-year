using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HarbourYear.Lite;
// 轻松版界面冒烟测试：用真实按钮从开始页面玩到四轮结束，沿途截图并检查文字溢出与异常。
namespace HarbourYear.Editor
{
    [InitializeOnLoad] public static class LiteSmoke
    {
        static LiteSmoke(){EditorApplication.update+=Tick;}
        public static void Run(){SessionState.SetBool("LiteSmoke",true);SessionState.SetFloat("LiteSmokeStart",(float)EditorApplication.timeSinceStartup);EditorSceneManager.OpenScene(YearBuild.Scene);EditorApplication.isPlaying=true;}
        static void Tick()
        {
            if(!SessionState.GetBool("LiteSmoke",false))return;
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat("LiteSmokeStart",0)>600){File.WriteAllText("docs/lite-smoke-failure.txt","Timed out");SessionState.SetBool("LiteSmoke",false);EditorApplication.Exit(1);return;}
            if(EditorApplication.isPlaying&&LiteGame.Instance!=null&&UnityEngine.Object.FindObjectOfType<LiteSmokeRunner>()==null)new GameObject("Lite smoke runner").AddComponent<LiteSmokeRunner>();
        }
    }
    public class LiteSmokeRunner:MonoBehaviour
    {
        readonly List<string> report=new List<string>();readonly HashSet<string> shots=new HashSet<string>();readonly List<string> errors=new List<string>();LiteGame game;int shotIndex;
        readonly string[] keys={LiteSave.Key,LiteSave.BeforeJob,LiteSave.BeforeNewGame,LiteSave.BeforeImport,LiteSave.Unreadable,"HarbourYear.Lite.Audio","HarbourYear.Lite.BigText"};readonly Dictionary<string,string> saved=new Dictionary<string,string>();
        void OnEnable(){Application.logMessageReceived+=Log;}void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string m,string st,LogType t){if(t==LogType.Exception||t==LogType.Error)errors.Add(m+"\n"+st);}
        void Check(bool ok,string message){if(!ok)throw new Exception(message);report.Add("PASS "+message);}
        Button Find(string name){return UnityEngine.Object.FindObjectsOfType<Button>().Where(x=>x.name==name&&x.gameObject.activeInHierarchy).OrderByDescending(x=>x.transform.GetSiblingIndex()).LastOrDefault();}
        bool Has(string name){return Find(name)!=null;}
        string First(string prefix){var b=UnityEngine.Object.FindObjectsOfType<Button>().Where(x=>x.name.StartsWith(prefix)&&x.gameObject.activeInHierarchy&&x.interactable).OrderBy(x=>x.name).FirstOrDefault();return b!=null?b.name:null;}
        void Click(string name)
        {
            var b=Find(name);Check(b!=null&&b.interactable,"button "+name+" available");Canvas.ForceUpdateCanvases();
            var sr=b.GetComponentInParent<ScrollRect>();
            if(sr!=null){var view=sr.viewport;var rt=(RectTransform)b.transform;var c=view.InverseTransformPoint(rt.TransformPoint(rt.rect.center));float top=-c.y;float vh=view.rect.height;
                if(top<0||top>vh){var pos=sr.content.anchoredPosition;pos.y=Mathf.Clamp(pos.y+top-vh/2,0,Mathf.Max(0,sr.content.rect.height-vh));sr.content.anchoredPosition=pos;Canvas.ForceUpdateCanvases();}}
            var r=(RectTransform)b.transform;var point=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=point};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);var target=hits.Count>0?hits[0].gameObject.GetComponentInParent<Button>():null;
            Check(target==b,"raycast reaches "+name+(target!=null?"":" (hit "+(hits.Count>0?hits[0].gameObject.name:"nothing")+")"));ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
        IEnumerator Shot(string label)
        {
            yield return null;yield return new WaitForEndOfFrame();int overflow=0;
            foreach(var t in UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>()){if(!t.gameObject.activeInHierarchy||t.overflowMode!=TextOverflowModes.Ellipsis)continue;t.ForceMeshUpdate();if(t.isTextTruncated||t.isTextOverflowing){overflow++;report.Add("TEXT_TRUNCATED "+label+" | "+t.text.Replace('\n',' '));}}
            report.Add("UI "+label+" truncated="+overflow);if(shots.Add(label))ScreenCapture.CaptureScreenshot(Path.GetFullPath("docs/screenshots/v3-"+(++shotIndex).ToString("00")+"-"+label+".png"));yield return null;
        }
        IEnumerator Start()
        {
            foreach(var k in keys)saved[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;foreach(var k in keys)PlayerPrefs.DeleteKey(k);PlayerPrefs.Save();
            Directory.CreateDirectory("docs/screenshots");foreach(var old in Directory.GetFiles("docs/screenshots","v3-*.*"))File.Delete(old);
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);UnityEngine.Random.InitState(20261021);for(int i=0;i<10;i++)yield return null;game=LiteGame.Instance;Destroy(game.gameObject);yield return null;yield return null;LiteGame.Launch();for(int i=0;i<4;i++)yield return null;game=LiteGame.Instance;
            var routine=Probe();
            while(true)
            {
                object value=null;bool more=false;
                try{more=routine.MoveNext();if(more)value=routine.Current;if(errors.Count>0)throw new Exception("runtime error: "+errors[0]);}
                catch(Exception e){report.Add("FAILED "+e);Finish(1);yield break;}
                if(!more)break;yield return value;
            }
            report.Add("LITE_SMOKE_OK");Finish(0);
        }
        void Finish(int code)
        {
            File.WriteAllLines("docs/lite-smoke-results.txt",report.ToArray());LiteSave.SuppressWrites=true;
            foreach(var kv in saved){if(kv.Value==null)PlayerPrefs.DeleteKey(kv.Key);else PlayerPrefs.SetString(kv.Key,kv.Value);}PlayerPrefs.Save();SessionState.SetBool("LiteSmoke",false);EditorApplication.Exit(code);
        }
        IEnumerator Probe()
        {
            yield return Shot("title");Check(!Find("Continue").interactable,"continue disabled without save");Click("StartSound");yield return null;Check(LiteAudio.Instance.prefs.enabled,"sound can be enabled");
            Click("Settings");yield return Shot("title-settings");Click("Back:返回");yield return null;Click("Gallery");yield return Shot("gallery-empty");Click("GalleryBack");yield return null;
            Click("NewGame");yield return Shot("prologue");Click("PrologueStart");yield return Shot("setup-quick");Click("Look:male");yield return null;Click("Advanced");yield return Shot("setup-profile");Click("ToEconomy");yield return Shot("setup-economy");
            Click("ToHousing");yield return Shot("setup-housing");Click("Back:返回开始页面");yield return null;Check(game.screen=="title","setup back keeps title");Click("NewGame");yield return null;Click("PrologueSkip");yield return null;
            Click("Look:female");yield return null;Click("StartLife");yield return Shot("home");Check(game.screen=="game"&&game.S.plan.All(x=>x!=null),"new life planned");Check(game.S.money==27500,"default budget");
            Click("PeriodMore");yield return Shot("plan-round1");
            Click("Details:specials");yield return null;Click("SpecialSlot");yield return Shot("picker");Click(First("Pick:"));yield return null;Click("SpecialItem");yield return null;Click("Pick:review");yield return null;
            int before=game.M.free.Count;Click("AddSpecial");yield return null;Check(game.M.free.Count==before+1,"special arrangement added");
            Click("Relax");yield return null;Check(game.M.relaxed.Count>0||game.M.free.Count>0,"relax handled");
            Click("Tab:jobs");yield return null;Click("Job:fast_food");yield return null;Check(game.S.lastAttempt!=null&&game.S.lastAttempt.job=="fast_food","job attempt recorded");
            Click("Tab:planner");yield return null;Click("Speed");yield return null;Click("StartSchedule");yield return Shot("start-confirm");Click("ConfirmStart");
            for(int i=0;i<150&&game.S.cursor<3;i++)yield return null;Check(game.S.phase=="running"&&game.S.cursor>=1,"planner auto-run advances");
            if(Has("Pause"))Click("Pause");yield return null;
            for(int i=0;i<6;i++){if(Has("EventChoice0")){yield return Shot("event-autorun");Click(Find("EventChoice0").interactable?"EventChoice0":"EventChoice1");yield return null;}else if(Has("ResultContinue")){Click("ResultContinue");yield return null;}else if(Has("TripKeep")){Click("TripKeep");yield return null;}}
            if(Has("Back:返回"))Click("Back:返回");yield return null;
            Click("Nav:schedule");yield return null;Click("Tab:planner");yield return Shot("planner");Click(First("Slot"));yield return Shot("slot-picker");Click("Back:返回");yield return null;Click("Tab:jobs");yield return Shot("jobs");Click("Back:返回");yield return null;
            Click("Nav:bag");yield return Shot("items");Click("Tab:market");yield return Shot("market");Click("Buy:bun");yield return null;Check(game.S.inventory[1]==1,"bought bun");Click("Tab:furniture");yield return Shot("furniture");Click("Back:返回");yield return null;
            Click("Nav:studies");yield return Shot("courses");Click("Back:返回");yield return null;Click("Nav:goals");yield return Shot("goals");Click("Back:返回");yield return null;
            Click("IconSettings");yield return Shot("settings");Click("Settings:profile");yield return Shot("settings-profile");Click("Back:返回");yield return null;Click("HudDate");yield return Shot("status");Click("Back:返回");yield return null;
            int guard=0;string lastKind="";bool manualTried=false;
            while(!(game.S.round==3&&(game.S.phase=="summary"||game.S.phase=="finished")&&LiteGame.Instance.win=="lite"&&Has("Back:返回")&&!Has("NextRound")&&Pace.PendingKind(game.S,game.M)==null))
            {
                if(++guard>400)throw new Exception("play-through stuck at round "+game.S.round+" "+game.S.phase+" "+lastKind);yield return null;
                if(Has("EventChoice0")){yield return Shot("event");Click(Find("EventChoice0").interactable?"EventChoice0":"EventChoice1");lastKind="event";continue;}
                if(Has("ResultContinue")){yield return Shot("result");Click("ResultContinue");lastKind="result";continue;}
                if(Has("TripKeep")){yield return Shot("trip");Click("TripKeep");lastKind="trip";continue;}
                if(Has("NoticeOk")||Has("NoticeOpen")){yield return Shot("notice-"+game.S.notice.kind);Click(Has("NoticeOk")?"NoticeOk":"NoticePlanner");yield return null;if(game.win!="room")Click("Back:返回");lastKind="notice";continue;}
                if(Has("NextRound")){yield return Shot("round-review");Click("NextRound");yield return null;lastKind="next";continue;}
                if(game.win!="room"&&Has("AcceptTeams")&&!manualTried){manualTried=true;Click("Back:返回");yield return null;Click("Nav:classmates");yield return null;Click("Course:"+Pace.FirstProjectCourse(game.S));yield return null;var inv=First("Invite:");if(inv!=null){Click(inv);yield return Shot("invite");}Click("Back:返回");yield return null;continue;}
                if(game.win!="room"&&Has("AcceptTeams")){yield return Shot("teams");Click("AcceptTeams");lastKind="teams";continue;}
                if(game.win!="room"&&Has("ConflictRest")){yield return Shot("conflict");Click("ConflictRest");lastKind="conflict";continue;}
                if(game.win=="lite"&&game.S.phase=="planning"&&Has("Focus:life")){yield return Shot("focus");Click(game.S.round==2?"Focus:academic":"Focus:life");yield return null;Click("StartRound");lastKind="start";continue;}
                if(game.win!="room"){Click("Back:返回");lastKind="close";continue;}
                Click("MainButton");lastKind="main:"+game.S.phase;
            }
            yield return Shot("sample-end");Check(Pace.SampleEnded(game.S),"four-round sample reaches its end");Check(game.S.history.Count+game.S.logs.Count==112,"112 half-days recorded");
            Click("Back:返回");yield return null;Click("Nav:journal");yield return Shot("journal");Click("Tab:memories");yield return Shot("memories");Click("Back:返回");yield return null;Click("Nav:classmates");yield return Shot("team");Click("Back:返回");yield return null;
            Click("IconMessages");yield return Shot("messages");Click("Back:返回");yield return null;
            var exported=LiteSave.Encode(game.S,game.M);string err;Check(LiteSave.Decode(exported,out err)!=null,"export decodes");Check(LiteSave.Decode("{}",out err)==null&&LiteSave.Decode("{broken",out err)==null,"bad saves rejected");
            var old=(JObj)Json.Parse(exported);old["schemaVersion"]=1.0;var l0=old.O("lite");var v1=new JObj();foreach(var k in new[]{"round","free","auto","customized","seen","teamPrompted","defaultTeams","digest"})v1[k]=l0[k];v1["focus"]="city";old["lite"]=v1;
            var up=LiteSave.Decode(Json.Write(old),out err);Check(up!=null&&up.upgraded&&up.lite.focus=="life"&&up.lite.conflicts.Count==0,"schemaVersion 1 save upgrades (city→香港生活) "+err);
            var over=Core.New(5);Core.QuickStart(over,"兼职",Look.Default("female"));var nr=Pace.NewRound(over,Pace.NewMeta());over=nr.state;over.hired.Add("fast_food");var autos=nr.meta.auto.Take(7).ToList();foreach(var i in autos)over.plan[i]="job:fast_food";
            var fixedSave=LiteSave.Decode(LiteSave.Encode(over,nr.meta),out err);Check(fixedSave!=null&&fixedSave.repaired&&Core.JobCount(fixedSave.state.plan)==6&&fixedSave.repairedSlots.SequenceEqual(new[]{autos.Last()}),"job overflow repaired from the latest recommended shift "+err);
            Click("IconSettings");yield return null;Click("Settings:save");yield return null;Click("BackToTitle");yield return Shot("title-continue");Check(Find("Continue").interactable,"continue enabled");
            Click("Gallery");yield return Shot("gallery");Click(First("Gallery:"));yield return null;Check(EventSystem.current.currentSelectedGameObject!=null&&EventSystem.current.currentSelectedGameObject.name=="CloseZoom"&&Find("CloseZoom").navigation.mode==Navigation.Mode.None,"zoom keeps keyboard focus on 关闭大图");yield return Shot("zoom");Click("CloseZoom");yield return null;Click("Back:返回");yield return null;Click("Continue");yield return null;Check(game.screen=="game","continue returns home");
            Click("IconSettings");yield return null;Click("Settings:profile");yield return null;Click("OpenClassic");for(int i=0;i<6;i++)yield return null;Check(UnityEngine.Object.FindObjectOfType<YearGame>()!=null,"classic version opens");yield return Shot("classic-menu");
            Click("BackToLite");for(int i=0;i<6;i++)yield return null;Check(LiteGame.Instance!=null&&UnityEngine.Object.FindObjectOfType<YearGame>()==null,"back to lite");game=LiteGame.Instance;yield return Shot("back-to-lite");
        }
    }
}
