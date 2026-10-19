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
namespace HarbourYear.Editor
{
    [InitializeOnLoad] public static class YearSmoke
    {
        static YearSmoke(){EditorApplication.update+=Tick;}
        public static void Run(){SessionState.SetBool("HarbourSmoke",true);SessionState.SetFloat("HarbourStart",(float)EditorApplication.timeSinceStartup);EditorSceneManager.OpenScene(YearBuild.Scene);EditorApplication.isPlaying=true;}
        static void Tick(){if(!SessionState.GetBool("HarbourSmoke",false))return;if(EditorApplication.timeSinceStartup-SessionState.GetFloat("HarbourStart",0)>210){File.WriteAllText("docs/smoke-failure.txt","Timed out");SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(1);return;}if(EditorApplication.isPlaying&&UnityEngine.Object.FindObjectOfType<YearGame>()!=null&&UnityEngine.Object.FindObjectOfType<YearSmokeRunner>()==null)new GameObject("Smoke runner").AddComponent<YearSmokeRunner>();}
    }
    public class YearSmokeRunner:MonoBehaviour
    {
        List<string> report=new List<string>();YearGame game;string previousSave,previousBackup;int previousAudio;
        void Check(bool ok,string message){if(!ok)throw new Exception(message);report.Add("PASS "+message);}
        void Click(string name)
        {
            var b=UnityEngine.Object.FindObjectsOfType<Button>().FirstOrDefault(x=>x.name==name&&x.gameObject.activeInHierarchy);Check(b!=null&&b.interactable,"button "+name+" available");Canvas.ForceUpdateCanvases();var rt=(RectTransform)b.transform;var point=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center));var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=point};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0&&hits[0].gameObject==b.gameObject,"raycast reaches "+name);ExecuteEvents.Execute(hits[0].gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
        void Audit(string label)
        {
            int overflow=0;foreach(var t in UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>()){t.ForceMeshUpdate();if(t.isTextOverflowing){overflow++;report.Add("TEXT_OVERFLOW "+label+" | "+t.text.Replace('\n',' '));}}
            report.Add("UI "+label+" overflow="+overflow);ScreenCapture.CaptureScreenshot(Path.GetFullPath("docs/screenshots/v2-"+label+".png"));
        }
        void Restore(){YearSave.SuppressWrites=true;if(previousSave==null)PlayerPrefs.DeleteKey("HarbourYear.Save.v2");else PlayerPrefs.SetString("HarbourYear.Save.v2",previousSave);if(previousBackup==null)PlayerPrefs.DeleteKey("HarbourYear.Backup.v2");else PlayerPrefs.SetString("HarbourYear.Backup.v2",previousBackup);PlayerPrefs.SetInt("HarbourYear.AudioMuted",previousAudio);PlayerPrefs.Save();}
        IEnumerator Start()
        {
            previousSave=PlayerPrefs.HasKey("HarbourYear.Save.v2")?PlayerPrefs.GetString("HarbourYear.Save.v2"):null;previousBackup=PlayerPrefs.HasKey("HarbourYear.Backup.v2")?PlayerPrefs.GetString("HarbourYear.Backup.v2"):null;previousAudio=PlayerPrefs.GetInt("HarbourYear.AudioMuted",0);PlayerPrefs.SetInt("HarbourYear.AudioMuted",1);
            Directory.CreateDirectory("docs/screenshots");Screen.SetResolution(1600,1000,FullScreenMode.Windowed);game=UnityEngine.Object.FindObjectOfType<YearGame>();var routine=Probe();
            while(true){object value=null;bool more=false;try{more=routine.MoveNext();if(more)value=routine.Current;}catch(Exception e){report.Add("FAILED "+e);File.WriteAllLines("docs/smoke-results.txt",report.ToArray());Restore();SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(1);yield break;}if(!more)break;yield return value;}
            report.Add("SMOKE_OK");File.WriteAllLines("docs/smoke-results.txt",report.ToArray());Restore();SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(0);
        }
        IEnumerator Probe()
        {
            for(int i=0;i<8;i++)yield return null;Audit("01-title");yield return null;Click("NewGame");yield return null;if(game.MenuVisible){Click("ConfirmNewGame");yield return null;}
            Check(game.State.phase=="intro","intro shown");Audit("02-intro");yield return null;Click("IntroNext");yield return null;Audit("03-setup");yield return null;
            game.State.name="陈星澜";game.State.look.gender=1;game.State.look.age=1;game.State.look.front=2;game.State.look.back=3;game.State.look.outfit=2;game.SaveAndRender();yield return null;Audit("04-avatar");yield return null;
            Click("StartYear");yield return null;Audit("05-guide");yield return null;Click("CloseGuide");yield return null;Click("EventChoice2");yield return null;
            Check(game.State.phase=="plan"&&game.State.plan.Count==28,"new schedule has 28 slots");Check(YearSave.Read().plan.All(p=>p==null),"empty slots survive JSON");Audit("06-planner-empty");yield return null;
            Click("Slot1");yield return null;Audit("07-picker-locked");yield return null;Check(!UnityEngine.Object.FindObjectsOfType<Button>().First(x=>x.name=="ActionLecture").interactable,"lecture grey outside timetable");Check(!UnityEngine.Object.FindObjectsOfType<Button>().First(x=>x.name=="ActionInternship").interactable,"locked internship disabled");Click("返回课表");yield return null;
            Click("FillClasses");yield return null;Click("FillRest");yield return null;Audit("08-planner-filled");yield return null;
            Click("Tab课程");yield return null;Audit("09-courses");yield return null;Click("Course0");yield return null;Audit("10-course-detail");yield return null;Click("收好课程档案");yield return null;
            Click("Tab小组");yield return null;Audit("11-teams");yield return null;Click("AssignRole0");yield return null;Audit("12-classmates");yield return null;Click("返回");yield return null;
            Click("Tab生活");yield return null;Audit("13-housing");yield return null;Click("商店");yield return null;Audit("14-shop");yield return null;Click("二手群");yield return null;Audit("15-market");yield return null;
            Click("Tab目标");yield return null;Audit("16-goals");yield return null;Click("Tab日程");yield return null;Click("ExecuteTurn");yield return null;Audit("17-confirm");yield return null;Click("ConfirmExecute");yield return null;
            Check(game.State.phase=="executing","execution started");Rules.Step(game.State);game.SaveAndRender();int cursor=game.State.cursor;game.ContinueGame();yield return null;Check(game.State.cursor==cursor&&game.State.phase=="executing","mid-execution resume");
            HarbourYear.Tests.YearChecks.Drain(game.State);game.SaveAndRender();yield return null;Audit("18-summary");yield return null;Click("NextRound");yield return null;Check(game.State.round==2&&game.State.phase=="plan","round advances");
            var winter=HarbourYear.Tests.YearChecks.New();winter.round=9;Rules.BlankPlan(winter);YearSave.Write(winter);game.ContinueGame();yield return null;Audit("19-winter");yield return null;Click("Slot0");yield return null;Audit("20-winter-picker");yield return null;Click("返回课表");yield return null;
            Check(YearSave.Parse("{}") == null&&YearSave.Parse("{broken")==null,"bad saves rejected");
            var end=HarbourYear.Tests.YearChecks.Play("all");YearSave.Write(end);game.ContinueGame();yield return null;Check(game.State.phase=="ending"&&game.State.ending=="phd","explicit ending page");Audit("21-ending");yield return null;Click("ChooseEnding");yield return null;Audit("22-choose-ending");yield return null;Click("Endinghk");yield return null;Check(game.State.ending=="hk","multiple offers choose through UI");
            var sick=HarbourYear.Tests.YearChecks.New();Rules.FillClasses(sick);Rules.FillEmpty(sick,new PlanItem(ActionKind.Rest));sick.fatigue=99;Rules.Begin(sick);Rules.Step(sick);YearSave.Write(sick);game.ContinueGame();yield return null;Audit("23-medical");yield return null;Click("EventChoice0");yield return null;Check(game.State.phase=="executing"&&game.State.recoverySlots==4,"medical resumes schedule");
            for(int i=0;i<6;i++)yield return null;Check(Resources.Load<Texture2D>("Art/RoomV2")!=null,"pixel room art present");Check(Resources.Load<TMP_FontAsset>("Fonts/Notebook SDF")!=null,"Chinese font loaded");
        }
    }
}
