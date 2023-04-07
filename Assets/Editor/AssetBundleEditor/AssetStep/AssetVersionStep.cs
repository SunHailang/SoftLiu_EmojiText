/**
 * @Author 
 * @FileName AssetVersionStep.cs
 * @Data 2023年3月25日
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{
    public class AssetVersionStep : IAssetStep
    {
        public void Execute(BuildTarget buildTarget, string output)
        {
            AssetVersionUpdate(output);
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Post;
        }

        
        [MenuItem("HotFix/AssetBundle/Create Version/PC")]
        private static void AssetBundleCreateVersion_PC()
        {
            string output = GetOutputPath(BuildTarget.StandaloneWindows);
            AssetVersionUpdate(output);
        }
        [MenuItem("HotFix/AssetBundle/Create Version/Android")]
        private static void AssetBundleCreateVersion_Android()
        {
            string output = GetOutputPath(BuildTarget.Android);
            AssetVersionUpdate(output);
        }
        [MenuItem("HotFix/AssetBundle/Create Version/iOS")]
        private static void AssetBundleCreateVersion_iOS()
        {
            string output = GetOutputPath(BuildTarget.iOS);
            AssetVersionUpdate(output);
        }

        private static string GetOutputPath(BuildTarget target)
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
            return outDir.FullName.Replace('\\', '/');
        }

        private static void AssetVersionUpdate(string output)
        {
            Dictionary<string, KeyValuePair<string, int>> assetMd5Dict = FileUtilities.GetAssetMD5Data(output);

            bool hasChange = AssetBundleUtilityEditor.UpdateAssetFile(output, assetMd5Dict);
            if (hasChange)
            {
                AssetBundleUtilityEditor.UpdateAssetDataVersion(output, assetMd5Dict, out uint abVersion);
                AssetBundleUtilityEditor.UpdateAssetResVersion(output, "AssetBundle", abVersion);
            }

            AssetLabelStep.AssetLabelUpdate(true);
        }
    }
}