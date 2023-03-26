using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace AssetBundleEditor
{
    public static class AssetBundleMenuEditor
    {
        #region AssetMenu
        
        [MenuItem("HotFix/AssetBundle/Build PC")]
        public static void AssetBundleBuild_PC()
        {
            BuildAssetBundles(BuildTarget.StandaloneWindows);
        }

        [MenuItem("HotFix/AssetBundle/Build Android")]
        public static void AssetBundleBuild_Android()
        {
            BuildAssetBundles(BuildTarget.Android);
        }

        [MenuItem("HotFix/AssetBundle/Build iOS")]
        public static void AssetBundleBuild_iOS()
        {
            BuildAssetBundles(BuildTarget.iOS);
        }

        #endregion
        
        
        private static void BuildAssetBundles(BuildTarget target)
        {
            string output = AssetBundleUtilityEditor.GetBuildAssetBundlePath(target, AssetBundleUtilityEditor.AssetBundleRootPath);
            DirectoryInfo outDir = null;
            if (!Directory.Exists(output))
            {
                outDir = Directory.CreateDirectory(output);
            }
            else
            {
                outDir = new DirectoryInfo(output);
            }
            output = outDir.FullName.Replace('\\', '/');
            AssetBundleProcess.Excute(output, target);
        }
       
    }
}