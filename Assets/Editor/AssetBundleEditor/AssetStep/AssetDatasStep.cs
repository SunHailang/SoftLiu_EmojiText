using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{
    public class AssetDatasStep : IAssetStep
    {
        public void Execute(BuildTarget buildTarget, string output)
        {
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Post;
        }

        [MenuItem("HotFix/AssetBundle/Copy Datas/PC")]
        public static void CopyDatas_PC()
        {
            string output = GetTargetOutput(BuildTarget.StandaloneWindows);
            CopyDatasToRes(BuildTarget.Android, output);
        }

        [MenuItem("HotFix/AssetBundle/Copy Datas/Android")]
        public static void CopyDatas_Android()
        {
            string output = GetTargetOutput(BuildTarget.Android);
            CopyDatasToRes(BuildTarget.Android, output);
        }

        [MenuItem("HotFix/AssetBundle/Copy Datas/iOS")]
        public static void CopyDatas_iOS()
        {
            string output = GetTargetOutput(BuildTarget.iOS);
            CopyDatasToRes(BuildTarget.Android, output);
        }

        private static string GetTargetOutput(BuildTarget target)
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

            return output;
        }

        private static void CopyDatasToRes(BuildTarget target, string output)
        {
            string originPath = Path.Combine(Application.dataPath, "../Datas/");
            if (!FileUtilities.IsExistsDirectory(originPath, false))
            {
                Debug.Log($"[CopyDatasToRes] {originPath} is not exist.");
                return;
            }
            output = Path.Combine(output, "../Datas");
            FileUtilities.CopyDirectory(originPath, output);
            // 1. 获取资源的MD5信息
            Dictionary<string, KeyValuePair<string, int>> assetMd5Dict = FileUtilities.GetAssetMD5Data(output);

            // 2. 判断文件是否变化
            bool hasChange = AssetBundleUtilityEditor.UpdateAssetFile(output, assetMd5Dict);
            // 3. 拷贝资源、更新Res版本号
            if (hasChange)
            {
                // 更新 version.bytes 版本号
                AssetBundleUtilityEditor.UpdateAssetDataVersion(output, assetMd5Dict, out uint datasVersion);
                // 更新 ResVersion.bytes 资源版本号
                AssetBundleUtilityEditor.UpdateAssetResVersion(output, "Datas", datasVersion);
            }
            Debug.Log($"[CopyDatasToRes] Origin:{originPath} To Target:{output} Complete.");
        }
    }
}