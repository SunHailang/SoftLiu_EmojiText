using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{

    public class AssetLabelStep : IAssetStep
    {
        public void Execute()
        {
            
        }

        [MenuItem("HotFix/AssetBundle/AssetLabel/Set AssetLabels")]
        public static void AssetLabelsMenu()
        {
            string assetBundlePath = "Assets/AssetBundlePackage";

            // DirectoryInfo rootDir = new DirectoryInfo(assetBundlePath);
            //
            // DirectoryInfo uiDir = new DirectoryInfo($"{assetBundlePath}/UI");

            DirectoryInfo uiLogicDir = new DirectoryInfo($"{assetBundlePath}/UI/Logic");

            DirectoryInfo[] dirInfos = uiLogicDir.GetDirectories();
            foreach (DirectoryInfo dirInfo in dirInfos)
            {
                string dirName = dirInfo.Name;
                DirectoryInfo prefabDir = new DirectoryInfo($"{assetBundlePath}/UI/Logic/{dirName}/Prefabs");
                FileInfo[] files = prefabDir.GetFiles();
                foreach (FileInfo fileInfo in files)
                {
                    if (fileInfo.Extension == ".meta") continue;

                    string basePath = UtiliityEditor.GetBasePath(fileInfo.FullName);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(basePath);
                    string bundleName = $"ui/logic/{dirName}";
                    if (assetImporter != null && assetImporter.assetBundleName != bundleName)
                    {
                        assetImporter.assetBundleName = bundleName;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"AssetBundle/AssetLabel Complete");
        }

        private static void AddAssetLabel()
        {
            
        }
    }
}