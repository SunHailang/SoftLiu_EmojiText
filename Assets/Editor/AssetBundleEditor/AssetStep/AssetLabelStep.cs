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
        // private static string m_assetbundlePackageFolder = "AssetBundlePackage";
        private static string m_assetbundlePackageRootPath = Path.Combine(Application.dataPath, "AssetBundlePackage/");


        public void Execute(BuildTarget buildTarget, string output)
        {
            HashSet<string> assetLabelList = new HashSet<string>();
            AssetLabelUpdate(false, assetLabelList, output);
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Pre;
        }

        [MenuItem("HotFix/AssetBundle/AssetLabel/Set AssetLabels")]
        public static void AssetLabelsMenu()
        {
            HashSet<string> assetLabelList = new HashSet<string>();
            string output = "";
            AssetLabelUpdate(false, assetLabelList, output);
            Debug.Log($"AssetBundle/AssetLabel Complete");
        }

        [MenuItem("HotFix/AssetBundle/AssetLabel/Clean AssetLabels")]
        public static void AssetLabelsClean()
        {
            AssetLabelUpdate(true, null, "");
        }

        /// <summary>
        /// 更新 AssetBundlePackage 文件夹下的资源文件的 assetlabel 值
        /// </summary>
        /// <param name="isClear"></param>
        public static void AssetLabelUpdate(bool isClear, HashSet<string> assetLabelList = null, string output = null)
        {
            // 设置场景 Shaders
            SetShadersAssetLabel(isClear, assetLabelList);
            // 设置UI逻辑资源
            SetUiLogicAssetLabel(isClear, assetLabelList);
            // 设置字体
            SetFontAssetLabe(isClear, assetLabelList);
            // 设置场景 Environments
            SetEnvironmentsAssetLabel(isClear, assetLabelList);

            // 清空无用的AssetBundle标记
            AssetDatabase.RemoveUnusedAssetBundleNames();
            if (!isClear && assetLabelList != null && !string.IsNullOrEmpty(output))
            {
                // 清除多余的 AB 包
                ClearAssetBundle(assetLabelList, output);
            }

            AssetDatabase.Refresh();
        }

        private static void SetShadersAssetLabel(bool isClear, HashSet<string> assetLabelList)
        {
            DirectoryInfo shaderswDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/Shaders");
            FileInfo[] shaders = shaderswDir.GetFiles("*", SearchOption.AllDirectories);
            string shadersAssetName = isClear ? "" : "shaders";
            SetAssetLabel(shaders, shadersAssetName, assetLabelList);
        }

        /// <summary>
        /// 设置 Environments 路径下的所有需要打 AB 包的的资源
        /// </summary>
        /// <param name="isClear"></param>
        /// <param name="assetLabelList"></param>
        private static void SetEnvironmentsAssetLabel(bool isClear, HashSet<string> assetLabelList)
        {
            DirectoryInfo uiLogicDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/Environments");
            DirectoryInfo[] dirInfos = uiLogicDir.GetDirectories();
            for (int index = 0; index < dirInfos.Length; index++)
            {
                DirectoryInfo dirInfo = dirInfos[index];
                string dirName = dirInfo.Name;

                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                string uiLogicAssetName = isClear ? "" : $"environments/{dirName.ToLower()}";
                SetAssetLabel(files, uiLogicAssetName, assetLabelList);
            }
        }

        /// <summary>
        /// 设置 UI/Fonts 路径下字体 AB 包
        /// </summary>
        /// <param name="isClear"></param>
        /// <param name="assetLabelList"></param>
        public static void SetFontAssetLabe(bool isClear, HashSet<string> assetLabelList)
        {
            DirectoryInfo fontDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/UI/Fonts");
            FileInfo[] fonts = fontDir.GetFiles("*", SearchOption.AllDirectories);
            string fontsAssetName = isClear ? "" : "ui/fonts";
            SetAssetLabel(fonts, fontsAssetName, assetLabelList);
        }

        /// <summary>
        /// 设置 AssetBundlePackage/UI/Logic 路径下的所有需要打 AB包的资源
        /// </summary>
        /// <param name="isClear"></param>
        private static void SetUiLogicAssetLabel(bool isClear, HashSet<string> assetLabelList)
        {
            DirectoryInfo uiLogicDir = new DirectoryInfo($"{m_assetbundlePackageRootPath}/UI/Logic");
            DirectoryInfo[] dirInfos = uiLogicDir.GetDirectories();
            List<string> uiPathList = new List<string>()
            {
                "Prefabs", "SpriteAtlas", "Textures/BigImg"
            };
            foreach (DirectoryInfo dirInfo in dirInfos)
            {
                string dirName = dirInfo.Name;
                for (int i = 0; i < uiPathList.Count; i++)
                {
                    string uiDirPath = $"{m_assetbundlePackageRootPath}/UI/Logic/{dirName}/{uiPathList[i]}";
                    if (!Directory.Exists(uiDirPath)) continue;
                    DirectoryInfo prefabDir = new DirectoryInfo(uiDirPath);
                    FileInfo[] files = prefabDir.GetFiles("*", SearchOption.AllDirectories);
                    string uiLogicAssetName = isClear ? "" : $"ui/logic/{dirName}";
                    SetAssetLabel(files, uiLogicAssetName, assetLabelList);
                }
            }
        }

        private static void SetAssetLabel(FileInfo[] fileList, string assetName, HashSet<string> assetLabelList)
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
                        if (fileInfo.Extension == ".unity" && !string.IsNullOrEmpty(assetName))
                        {
                            assetName += ".unity3d";
                        }
                        assetImporter.assetBundleName = assetName;
                    }
                }
            }

            if (!string.IsNullOrEmpty(assetName) && assetLabelList != null) assetLabelList.Add(assetName);
        }

        private static void ClearAssetBundle(HashSet<string> assetLabelList, string output)
        {
            if (assetLabelList == null || string.IsNullOrEmpty(output)) return;
            // 1. 读取已存在的版本文件 version.bytes
            string versionPath = Path.Combine(output, "version.bytes");
            // 2. 解析版本文件 版本号 +1
            using (FileStream fs = new FileStream(versionPath, FileMode.OpenOrCreate))
            {
                int totalCount = (int) fs.Length;
                byte[] buffer = new byte[fs.Length];
                int offset = 0;
                while (totalCount > 0)
                {
                    int count = fs.Read(buffer, offset, totalCount);
                    if (count <= 0) break;
                    offset += count;
                    totalCount -= count;
                }

                var jsonText = System.Text.Encoding.UTF8.GetString(buffer);
                var data = LitJson.JsonMapper.ToObject<AssetVersionData>(jsonText);
                if (data == null || data.AssetMd5List == null) return;
                string root = AssetBundleUtilityEditor.AssetBundleRootFolder;
                int outIndex = output.IndexOf(root, StringComparison.Ordinal);
                root = output.Substring(outIndex + root.Length);
                for (int i = 0; i < data.AssetMd5List.Count; i++)
                {
                    if (data.AssetMd5List[i].Path.EndsWith(".manifest")) continue;
                    // int index = data.AssetBundleMd5List[i].Path.IndexOf(root, StringComparison.Ordinal);
                    if (root.Length >= data.AssetMd5List[i].Path.Length) continue;
                    string assetLabel = data.AssetMd5List[i].Path.Substring(root.Length);
                    if (!assetLabelList.Contains(assetLabel))
                    {
                        // 删除这个文件
                        string filePath = Path.Combine(output, assetLabel);
                        if (File.Exists(filePath)) File.Delete(filePath);
                        string fileManifestPath = Path.Combine(output, $"{assetLabel}.manifest");
                        if (File.Exists(fileManifestPath)) File.Delete(fileManifestPath);
                    }
                }
            }
        }
    }
}