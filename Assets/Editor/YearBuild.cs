using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;
namespace HarbourYear.Editor
{
    public static class YearBuild
    {
        public const string Scene="Assets/Scenes/HarbourYear.unity";
        [MenuItem("港硕这一年/准备场景与字体")]
        public static void Prepare()
        {
            string essential="Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
            if(!Directory.Exists("Assets/TextMesh Pro/Resources")){AssetDatabase.ImportPackage(essential,false);AssetDatabase.Refresh();}
            const string fp="Assets/Resources/Fonts/Notebook SDF.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fp)==null)
            {
                var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/NotebookSans.ttf");if(font==null)throw new Exception("Bundled Chinese font missing");
                var asset=TMP_FontAsset.CreateFontAsset(font,36,5,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);asset.name="Notebook SDF";asset.isMultiAtlasTexturesEnabled=true;
                AssetDatabase.CreateAsset(asset,fp);AssetDatabase.AddObjectToAsset(asset.material,asset);foreach(var atlas in asset.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,asset);
                string all=string.Concat(Directory.GetFiles("Assets/Scripts","*.cs").Select(File.ReadAllText));string chars=new string(all.Where(c=>c>=32).Distinct().ToArray());string missing;
                if(!asset.TryAddCharacters(chars,out missing))Debug.LogWarning("Font missing glyphs: "+missing);
                foreach(var atlas in asset.atlasTextures)if(!AssetDatabase.Contains(atlas))AssetDatabase.AddObjectToAsset(atlas,asset);
                EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();
            }
            var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fp);
            if(existing!=null){existing.ClearFontAssetData(true);string chars=new string(string.Concat(Directory.GetFiles("Assets/Scripts","*.cs").Select(File.ReadAllText)).Where(c=>c>=32).Distinct().ToArray());string missing;existing.TryAddCharacters(chars,out missing);if(!string.IsNullOrEmpty(missing))Debug.LogWarning("Missing glyphs: "+missing);foreach(var atlas in existing.atlasTextures)if(!AssetDatabase.Contains(atlas))AssetDatabase.AddObjectToAsset(atlas,existing);EditorUtility.SetDirty(existing);}
            foreach(string path in Directory.GetFiles("Assets/Resources/Art","*.png")){var art=AssetImporter.GetAtPath(path) as TextureImporter;if(art!=null){art.textureType=TextureImporterType.Default;art.maxTextureSize=2048;art.mipmapEnabled=false;art.filterMode=FilterMode.Point;art.textureCompression=TextureImporterCompression.CompressedHQ;art.SaveAndReimport();}}
            var ti=AssetImporter.GetAtPath("Assets/Resources/Art/Harbour.png") as TextureImporter;
            if(ti!=null){ti.textureType=TextureImporterType.Default;ti.maxTextureSize=2048;ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.mipmapEnabled=false;ti.SaveAndReimport();}
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("港硕这一年 · Runtime Entry");EditorSceneManager.SaveScene(scene,Scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
            PlayerSettings.companyName="Harbour Notebook Studio";PlayerSettings.productName="港硕这一年";PlayerSettings.bundleVersion="2.0.0";PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Gamma;
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=true;PlayerSettings.WebGL.template="PROJECT:Notebook";PlayerSettings.WebGL.memorySize=256;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);
            QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;PlayerSettings.stripEngineCode=true;
            AssetDatabase.SaveAssets();Debug.Log("HARBOUR_PREPARE_OK");
        }
        [MenuItem("港硕这一年/构建 macOS")]
        public static void BuildMac(){Prepare();Build(BuildTarget.StandaloneOSX,"Build/macOS/港硕这一年.app");}
        [MenuItem("港硕这一年/构建 WebGL")]
        public static void BuildWebGL(){Prepare();Build(BuildTarget.WebGL,"Build/WebGL");}
        static void Build(BuildTarget target,string location)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(location));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=location,target=target,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("HARBOUR_BUILD_OK "+target+" "+report.summary.totalSize);
        }
    }
}
