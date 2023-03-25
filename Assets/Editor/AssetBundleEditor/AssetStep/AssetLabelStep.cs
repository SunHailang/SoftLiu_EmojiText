using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{
    public class AssetLabelStep : IAssetStep
    {
        private static string m_assetbundlePackageFolder = "AssetBundlePackage";
        private static string m_assetbundlePackageRootPath = Path.Combine(Application.dataPath, "AssetBundlePackage/");


        public void Execute(BuildTarget buildTarget, string output)
        {
            AssetLabelUpdate(false);
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Pre;
        }

        [MenuItem("HotFix/AssetBundle/AssetLabel/Set AssetLabels")]
        public static void AssetLabelsMenu()
        {
            AssetLabelUpdate(false);
            Debug.Log($"AssetBundle/AssetLabel Complete");
        }

        [MenuItem("HotFix/AssetBundle/AssetLabel/Clean AssetLabels")]
        public static void AssetLabelsClean()
        {
            AssetLabelUpdate(true);
            // 清空无用的AssetBundle标记
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 更新 AssetBundlePackage 文件夹下的资源文件的 assetlabel 值
        /// </summary>
        /// <param name="isClear"></param>
        public static void AssetLabelUpdate(bool isClear)
        {
            int index = m_assetbundlePackageRootPath.IndexOf(m_assetbundlePackageFolder, StringComparison.Ordinal);

            DirectoryInfo uiLogicDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/UI/Logic");
            DirectoryInfo[] dirInfos = uiLogicDir.GetDirectories();
            foreach (DirectoryInfo dirInfo in dirInfos)
            {
                string dirName = dirInfo.Name;
                DirectoryInfo prefabDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/UI/Logic/{dirName}/Prefabs");
                FileInfo[] files = prefabDir.GetFiles("*", SearchOption.AllDirectories);
                string uiLogicAssetName = isClear ? "" : $"ui/logic/{dirName}";
                SetAssetLabel(files, uiLogicAssetName);
            }

            DirectoryInfo fontDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/UI/Fonts");
            FileInfo[] fonts = fontDir.GetFiles("*", SearchOption.AllDirectories);
            string fontsAssetName = isClear ? "" : "ui/fonts";
            SetAssetLabel(fonts, fontsAssetName);

            AssetDatabase.Refresh();
        }

        private static void SetAssetLabel(FileInfo[] fileList, string assetName)
        {
            if (fileList == null || fileList.Length <= 0)
            {
                return;
            }

            foreach (FileInfo fileInfo in fileList)
            {
                if (fileInfo.Extension == ".meta") continue;

                string basePath = UtilityEditor.GetBasePath(fileInfo.FullName);
                AssetImporter assetImporter = AssetImporter.GetAtPath(basePath);
                if (assetImporter != null)
                {
                    if (assetImporter.assetBundleName != assetName)
                    {
                        assetImporter.assetBundleName = assetName;
                    }
                }
            }
        }
    }
}