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
    [InitializeOnLoad]
    public static class YearSmoke
    {
        static YearSmoke(){EditorApplication.update+=Tick;}
        public static void Run(){SessionState.SetBool("HarbourSmoke",true);SessionState.SetFloat("HarbourStart",(float)EditorApplication.timeSinceStartup);EditorSceneManager.OpenScene(YearBuild.Scene);EditorApplication.isPlaying=true;}
        static void Tick()
        {
            if(!SessionState.GetBool("HarbourSmoke",false))return;
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat("HarbourStart",0)>150){File.WriteAllText("docs/smoke-failure.txt","Timed out");SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(1);return;}
            if(EditorApplication.isPlaying&&UnityEngine.Object.FindObjectOfType<YearGame>()!=null&&UnityEngine.Object.FindObjectOfType<YearSmokeRunner>()==null)new GameObject("Smoke runner").AddComponent<YearSmokeRunner>();
        }
    }
    public class YearSmokeRunner:MonoBehaviour
    {
        List<string> report=new List<string>();YearGame game;
        void Check(bool ok,string message){if(!ok)throw new Exception(message);report.Add("PASS "+message);}
        void Click(string name){var b=UnityEngine.Object.FindObjectsOfType<Button>().FirstOrDefault(x=>x.name==name&&x.gameObject.activeInHierarchy);Check(b!=null&&b.interactable,"button "+name+" available");Canvas.ForceUpdateCanvases();var rt=(RectTransform)b.transform;var point=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center));var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=point};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0&&hits[0].gameObject==b.gameObject,"raycast reaches "+name);ExecuteEvents.Execute(hits[0].gameObject,pointer,ExecuteEvents.pointerClickHandler);}
        void Audit(string label)
        {
            int overflow=0;foreach(var t in UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>()){t.ForceMeshUpdate();if(t.isTextOverflowing){overflow++;report.Add("TEXT_OVERFLOW "+label+" | "+t.text.Replace('\n',' '));}}
            report.Add("UI "+label+" activeTexts="+UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>().Length+" overflow="+overflow);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("docs/screenshots/"+label+".png"));
        }
        IEnumerator Start()
        {
            Directory.CreateDirectory("docs/screenshots");Screen.SetResolution(1600,1000,FullScreenMode.Windowed);game=UnityEngine.Object.FindObjectOfType<YearGame>();
            var routine=Probe();while(true){object value=null;bool more=false;try{more=routine.MoveNext();if(more)value=routine.Current;}catch(Exception e){report.Add("FAILED "+e);File.WriteAllLines("docs/smoke-results.txt",report.ToArray());SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(1);yield break;}if(!more)break;yield return value;}
            report.Add("SMOKE_OK");File.WriteAllLines("docs/smoke-results.txt",report.ToArray());SessionState.SetBool("HarbourSmoke",false);EditorApplication.Exit(0);
        }
        IEnumerator Probe()
        {
            for(int i=0;i<6;i++)yield return null;Check(game.MenuVisible,"title screen opened");Audit("01-title");for(int i=0;i<4;i++)yield return null;
            Click("NewGame");yield return null;if(game.MenuVisible){Click("ConfirmNewGame");yield return null;}Click("CloseGuide");yield return null;Check(game.State.phase=="event","intro survives guide close");Audit("02-arrival");yield return null;
            Click("EventChoice2");yield return null;Check(game.State.phase=="plan"&&game.State.Has("mentor"),"intro choice applied");Audit("03-planner");for(int i=0;i<4;i++)yield return null;
            Click("Tab课程");yield return null;Audit("04-courses");yield return null;Click("Tab目标");yield return null;Audit("05-goals");yield return null;Click("Tab日程");yield return null;
            game.State.plan.Add(new PlanItem(ActionKind.Lecture));game.State.plan.Add(new PlanItem(ActionKind.Study,1));game.State.plan.Add(new PlanItem(ActionKind.Career));game.State.plan.Add(new PlanItem(ActionKind.Rest));game.SaveAndRender();yield return null;
            Audit("06-schedule");yield return null;Click("ExecuteTurn");yield return null;Click("ConfirmExecute");yield return null;Check(game.State.phase=="event","execution reaches event");
            var saved=YearSave.Read();Check(saved!=null&&saved.eventId==game.State.eventId&&saved.phase=="event","save preserves pending event");
            game.ContinueGame();yield return null;Click("EventChoice0");yield return null;Check(game.State.phase=="summary","event settles once");Audit("07-summary");yield return null;
            saved=YearSave.Read();Check(saved!=null&&saved.summary.Count==game.State.summary.Count&&saved.phase=="summary","summary saved");Click("NextRound");yield return null;Check(game.State.round==2&&game.State.phase=="plan","next round via UI");
            var json=JsonUtility.ToJson(game.State);var roundtrip=YearSave.Parse(json);Check(roundtrip!=null&&roundtrip.round==2&&roundtrip.courses.Count==4&&roundtrip.courses[0].learning>0,"JSON save roundtrip preserves course progress");Check(YearSave.Parse("{broken") == null && YearSave.Parse("{}") == null,"invalid saves safely rejected");
            var original=YearSave.Read();var ending=HarbourYear.Tests.YearChecks.Play("all");YearSave.Write(ending);game.ContinueGame();yield return null;Audit("08-choose-ending");yield return null;Check(Rules.Endings(game.State).Count==4,"multiple endings selectable");
            Click("读博深造  →");yield return null;Check(game.State.ending=="phd","ending selected through button");Audit("09-ending");for(int i=0;i<5;i++)yield return null;YearSave.Write(original);game.ContinueGame();
            Check(Resources.Load<TMP_FontAsset>("Fonts/Notebook SDF")!=null,"bundled TMP Chinese font present");Check(Resources.Load<Texture2D>("Art/Harbour")!=null,"illustration loaded");
        }
    }
}
