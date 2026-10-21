using UnityEditor;
using UnityEngine;
// 轻松版素材导入：放大到 2 的幂以便压缩并生成 mipmap（缩小显示更平滑）；显示比例见 Resources/Lite/sizes.txt。
namespace HarbourYear.Editor
{
    public class LiteImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Resources/Lite/"))return;var t=(TextureImporter)assetImporter;
            t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.ToLarger;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.wrapMode=TextureWrapMode.Clamp;
            t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.alphaIsTransparency=true;t.sRGBTexture=true;t.isReadable=false;
        }
        void OnPreprocessAudio()
        {
            if(!assetPath.StartsWith("Assets/Resources/Lite/"))return;var a=(AudioImporter)assetImporter;var s=a.defaultSampleSettings;
            bool music=assetPath.Contains("bgm");s.loadType=music?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;s.compressionFormat=AudioCompressionFormat.Vorbis;s.quality=music?0.6f:0.8f;a.defaultSampleSettings=s;a.loadInBackground=music;
        }
    }
}
