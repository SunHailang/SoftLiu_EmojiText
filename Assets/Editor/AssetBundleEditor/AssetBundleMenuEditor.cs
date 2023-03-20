using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;


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
        string output = Path.Combine($"{Application.dataPath}", "../");
        BuildAssetBundles(output, @"AssetBundles\PC\", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }

    [MenuItem("HotFix/AssetBundle/Build Android")]
    public static void AssetBundleBuild_Android()
    {
        string output = Path.Combine($"{Application.dataPath}", "../");
        BuildAssetBundles(output, @"AssetBundles\Android\", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

    [MenuItem("HotFix/AssetBundle/Build iOS")]
    public static void AssetBundleBuild_iOS()
    {
        string output = Path.Combine($"{Application.dataPath}", "../");
        BuildAssetBundles(output, @"AssetBundles\iOS\", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
    }

    private static void BuildAssetBundles(string dataPath, string assetPath, BuildAssetBundleOptions options, BuildTarget target)
    {
        string output = Path.Combine(dataPath, assetPath);
        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }

        BuildPipeline.BuildAssetBundles(output, options, target);

        Debug.Log("BuildAssetBundles Complete!!!!!");
        Caching.ClearCache();

        Dictionary<string, string> assetMd5Dict = new Dictionary<string, string>();

        void GetMd5(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                GetMd5(directory.FullName);
            }

            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".manifest")
                {
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                    {
                        MD5 md5 = MD5.Create();
                        byte[] bytes = md5.ComputeHash(fs);
                        string fileMd5 = System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
                        int index = file.FullName.LastIndexOf(assetPath, StringComparison.Ordinal);
                        string filePath = file.FullName.Substring(index, file.FullName.Length - index);
                        filePath = filePath.Replace('\\', '/');
                        assetMd5Dict[filePath] = fileMd5;
                    }
                }
            }
        }

        GetMd5(output);

        // 生成 Version MD5 文件
        // 1. 读取 Version.bytes 文件
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

            string jsonText = System.Text.Encoding.UTF8.GetString(buffer);
            VersionData data = LitJson.JsonMapper.ToObject<VersionData>(jsonText);
            if (data == null)
            {
                data = new VersionData();
            }

            data.version += 1;

            if (data.assetBundleMd5List == null)
            {
                data.assetBundleMd5List = new List<AssetBundleMD5>(assetMd5Dict.Count);
            }

            data.assetBundleMd5List.Clear();
            foreach (KeyValuePair<string, string> pair in assetMd5Dict)
            {
                data.assetBundleMd5List.Add(new AssetBundleMD5()
                {
                    path = pair.Key, MD5 = pair.Value
                });
            }

            jsonText = LitJson.JsonMapper.ToJson(data);
            buffer = System.Text.Encoding.UTF8.GetBytes(jsonText);

            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(0);

            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
    }
}

public class AssetBundleMD5
{
    public string path = "";
    public string MD5 = "";
}

public class VersionData
{
    public uint version;
    public List<AssetBundleMD5> assetBundleMd5List = null;
}