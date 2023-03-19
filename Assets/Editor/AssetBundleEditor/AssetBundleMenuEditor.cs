using UnityEngine;
using UnityEditor;
using System.IO;


public static class AssetBundleMenu
{
    [MenuItem("HotFix/AssetBundle/AssetLabel/Clean AssetLabels")]
    public static void AssetLabelsClean()
    {
        string assetBundlePath = "Assets/AssetBundlePackage";
        DirectoryInfo rootDir = new DirectoryInfo(assetBundlePath);


        static void Clean(DirectoryInfo dirInfo)
        {
            foreach (DirectoryInfo directoryInfo in dirInfo.GetDirectories())
            {
                Clean(directoryInfo);
            }
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                if (fileInfo.Extension == ".meta") continue;

                string basePath = UtiliityEditor.GetBasePath(fileInfo.FullName);
                AssetImporter assetImporter = AssetImporter.GetAtPath(basePath);
                if (assetImporter != null && assetImporter.assetBundleName != "")
                {
                    assetImporter.assetBundleName = "";
                }
            }
        }
        
        Clean(rootDir);
    }
    [MenuItem("HotFix/AssetBundle/AssetLabel/Set AssetLabels")]
    public static void AssetLabelsMenu()
    {
        Debug.Log($"AssetBundle/AssetLabel Start");
        AssetLabelDLL();
        
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

    private static void AssetLabelDLL()
    {
        string dllPath = "Assets/AssetBundlePackage/HotFixDLL";
        DirectoryInfo dllDir = new DirectoryInfo(dllPath);
        foreach (FileInfo fileInfo in dllDir.GetFiles())
        {
            if (fileInfo.Extension == ".meta") continue;

            string basePath = UtiliityEditor.GetBasePath(fileInfo.FullName);
            AssetImporter assetImporter = AssetImporter.GetAtPath(basePath);
            if (assetImporter != null && assetImporter.assetBundleName != "HotFixDLL/dll")
            {
                assetImporter.assetBundleName = $"HotFixDLL/dll";
            }
        }
    }


    [MenuItem("HotFix/AssetBundle/Build PC")]
    public static void AssetBundleBuild_PC()
    {
        string outputPath = Path.Combine($"{Application.dataPath}", "../AssetBundles/PC/");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        //BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
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