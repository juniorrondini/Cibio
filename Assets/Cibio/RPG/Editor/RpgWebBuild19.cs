using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
    public static class RpgWebBuild19
    {
        public static void Build()
        {
            string output=null;var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-cibio-build")output=args[i+1];
            if(string.IsNullOrEmpty(output))throw new Exception("Especifique -cibio-build <pasta Web>");
            RpgBuilder.ValidateSaved();var snapshot=new Dictionary<string,byte[]>();
            foreach(var file in Directory.GetFiles("ProjectSettings","*.asset"))snapshot[file]=File.ReadAllBytes(file);
            var textures=AssetDatabase.FindAssets("t:Texture2D",new[]{RpgBuilder.Root+"/Resources/RpgArt"});
            foreach(var guid in textures){string path=AssetDatabase.GUIDToAssetPath(guid);snapshot[path+".meta"]=File.ReadAllBytes(path+".meta");}
            try
            {
                // Web-only import overrides reduce download and GPU memory. Restore every source meta afterwards.
                foreach(var guid in textures)
                {
                    var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;if(!importer)continue;
                    var platform=importer.GetPlatformTextureSettings("WebGL");platform.name="WebGL";platform.overridden=true;
                    platform.format=TextureImporterFormat.DXT5;platform.textureCompression=TextureImporterCompression.Compressed;platform.crunchedCompression=false;
                    importer.SetPlatformTextureSettings(platform);importer.SaveAndReimport();
                }
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;
                PlayerSettings.WebGL.decompressionFallback=false;
                Directory.CreateDirectory(output);var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{RpgBuilder.Scene},target=BuildTarget.WebGL,locationPathName=output,options=BuildOptions.None});
                if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build Web falhou: "+result.summary.result);
                Debug.Log("CIBIO_20_WEB_BUILD_OK "+output);
            }
            finally
            {
                foreach(var file in snapshot)File.WriteAllBytes(file.Key,file.Value);
                Debug.Log("CIBIO_20_WEB_SOURCE_SETTINGS_RESTORED");
            }
        }
    }
}
