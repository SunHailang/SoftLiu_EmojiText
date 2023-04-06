using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/**
 * @Author 
 * @FileName AssetBundleUtilityEditor.cs
 * @Data 2023年3月25日
**/
public class AssetBundleUtilityEditor
{
    public static readonly string AssetBundleRootPath = Path.Combine(Application.dataPath, "../../SoftLiu_ServerIOCSharp/Out/Resources/HotFixRes/Res");
    public static readonly string AssetBundleRootFolder = "HotFixRes/";


    /// <summary>
    /// 获取AssetBundle Path
    /// </summary>
    /// <param name="buildTarget"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetBuildAssetBundlePath(BuildTarget buildTarget, string path)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                return Path.Combine(path, "Android/AssetBundle/");
            case BuildTarget.iOS:
                return Path.Combine(path, "iOS/AssetBundle/");
        }

        return Path.Combine(path, "PC/AssetBundle/");
    }


   
    public static void UpdateAssetDataVersion(string output, Dictionary<string, KeyValuePair<string, int>> assetMd5Dict, out uint versionValue)
    {
        if (assetMd5Dict == null || assetMd5Dict.Count <= 0)
        {
            assetMd5Dict = FileUtilities.GetAssetMD5Data(output);
        }

        // 生成 Version MD5 文件
        // 1. 读取 Version.bytes 文件
        string versionPath = Path.Combine(output, "version.bytes");
        versionValue = 0;
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

            data ??= new AssetVersionData();

            data.Version += 1;
            versionValue = data.Version;

            data.AssetMd5List ??= new List<AssetInfoData>(assetMd5Dict.Count);

            data.AssetMd5List.Clear();
            foreach (KeyValuePair<string, KeyValuePair<string, int>> pair in assetMd5Dict)
            {
                data.AssetMd5List.Add(new AssetInfoData()
                {
                    Path = pair.Key, MD5 = pair.Value.Key, Size = pair.Value.Value
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

    public static bool UpdateAssetFile(string output, Dictionary<string, KeyValuePair<string, int>> assetMd5Dict)
    {
        if (string.IsNullOrEmpty(output) || assetMd5Dict == null)
        {
            return false;
        }
        string versionPath = Path.Combine(output, "version.bytes");
        bool hasChange = false;
        using (FileStream fs = new FileStream(versionPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            string jsonText = System.Text.Encoding.UTF8.GetString(bytes);
            var data = LitJson.JsonMapper.ToObject<AssetVersionData>(jsonText);
            data ??= new AssetVersionData();
            data.AssetMd5List ??= new List<AssetInfoData>();
            hasChange = data.AssetMd5List.Count <= 0;
            // 因为需要删除不需要的文件 需要便利整个历史文件记录
            for (int i = 0; i < data.AssetMd5List.Count; i++)
            {
                AssetInfoData assetInfo = data.AssetMd5List[i];
                if (assetMd5Dict.TryGetValue(assetInfo.Path, out KeyValuePair<string, int> pair))
                {
                    if (assetInfo.Size != pair.Value || assetInfo.MD5 != pair.Key)
                    {
                        hasChange = true;
                    }
                }
                else
                {
                    // 已经不包含这个文件了 可以删掉
                    string filePath = Path.Combine(output, $"../../../{assetInfo.Path}");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    hasChange = true;
                }
            }
        }
        return hasChange;
    }
    
    /// <summary>
    /// 更新总资源表的版本号
    /// </summary>
    /// <param name="output">输出路径</param>
    /// <param name="resName">资源名， AssetBundle / Datas / ...</param>
    /// <param name="versionValue">版本号</param>
    public static void UpdateAssetResVersion(string output, string resName, uint versionValue)
    {
        // 生成Res 资源版本
        string resPath = Path.Combine(output, "../ResVersion.bytes");
        using (FileStream fs = new FileStream(resPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            string jsonText = System.Text.Encoding.UTF8.GetString(bytes);
            var data = LitJson.JsonMapper.ToObject<ResVersionData>(jsonText);
            data ??= new ResVersionData();

            data.Version += 1;
            data.ResInfoList ??= new List<ResInfoData>();
            bool existRes = false;
            for (int i = 0; i < data.ResInfoList.Count; i++)
            {
                string resinfo = data.ResInfoList[i].Name;
                if (resinfo.Length == resName.Length && resinfo == resName)
                {
                    data.ResInfoList[i].Version = versionValue;
                    existRes = true;
                }
            }

            if (!existRes)
            {
                data.ResInfoList.Add(new ResInfoData()
                {
                    Name = resName,
                    Version = versionValue
                });
            }

            jsonText = LitJson.JsonMapper.ToJson(data);
            bytes = System.Text.Encoding.UTF8.GetBytes(jsonText);

            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(0);

            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
        }
    }
}