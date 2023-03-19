using UnityEngine;
using UnityEditor;
using System.IO;


public static class AssetBundleMenu
{
    [MenuItem("HotFix/AssetBundle/AssetLabels")]
    public static void AssetLabelsMenu()
    {
        Debug.Log($"AssetBundle/AssetLabel");
        string assetBundlePath = "Assets/AssetBundlePackage";

        DirectoryInfo rootDir = new DirectoryInfo(assetBundlePath);

        DirectoryInfo uiDir = new DirectoryInfo($"{assetBundlePath}/UI");

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
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = $"ui/logic/{dirName}";
                }
            }
        }

        AssetDatabase.Refresh();
    }


    [MenuItem("HotFix/AssetBundle/Build PC")]
    public static void AssetBundleBuild_PC()
    {
        string outputPath = Path.Combine($"{Application.dataPath}", "../AssetBundle/PC/");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }

    [MenuItem("HotFix/AssetBundle/Build Android")]
    public static void AssetBundleBuild_Android()
    {
    }

    [MenuItem("HotFix/AssetBundle/Build iOS")]
    public static void AssetBundleBuild_iOS()
    {
    }
}