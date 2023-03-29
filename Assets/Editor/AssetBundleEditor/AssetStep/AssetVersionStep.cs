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