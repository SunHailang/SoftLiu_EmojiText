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
            Dictionary<string, KeyValuePair<string, int>> assetMd5Dict = new Dictionary<string, KeyValuePair<string, int>>();

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
                    if (file.Name != "version.bytes")
                    {
                        using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        {
                            MD5 md5 = MD5.Create();
                            byte[] bytes = md5.ComputeHash(fs);
                            string fileMd5 = System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
                            int index = file.FullName.LastIndexOf(AssetBundleUtilityEditor.AssetBundleRootFolder, StringComparison.Ordinal);
                            if (index < 0)
                            {
                                Debug.LogError($"[AssetVersionUpdate] Index:{index}, {file.FullName}");
                                continue;
                            }

                            index += AssetBundleUtilityEditor.AssetBundleRootFolder.Length + 1;
                            if (index >= file.FullName.Length)
                            {
                                Debug.LogError($"[AssetVersionUpdate] Index:{index}, {file.FullName}");
                                continue;
                            }

                            string filePath = file.FullName.Substring(index);
                            filePath = filePath.Replace('\\', '/');
                            assetMd5Dict[filePath] = new KeyValuePair<string, int>(fileMd5, (int) fs.Length);
                        }
                    }
                }
            }

            GetMd5(output);

            // 生成 Version MD5 文件
            // 1. 读取 Version.bytes 文件
            string versionPath = Path.Combine(output, "version.bytes");
            uint abVersion = 0;
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
                var data = LitJson.JsonMapper.ToObject<AssetBundleVersionData>(jsonText);

                data ??= new AssetBundleVersionData();

                data.Version += 1;
                abVersion = data.Version;

                data.AssetBundleMd5List ??= new List<AssetBundleMD5>(assetMd5Dict.Count);

                data.AssetBundleMd5List.Clear();
                foreach (KeyValuePair<string, KeyValuePair<string, int>> pair in assetMd5Dict)
                {
                    data.AssetBundleMd5List.Add(new AssetBundleMD5()
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
                bool existAB = false;
                for (int i = 0; i < data.ResInfoList.Count; i++)
                {
                    if (data.ResInfoList[i].Name == "AssetBundleVersion")
                    {
                        data.ResInfoList[i].Version = abVersion;
                        existAB = true;
                    }
                }

                if (!existAB)
                {
                    data.ResInfoList.Add(new ResInfoData()
                    {
                        Name = "AssetBundleVersion",
                        Version = abVersion
                    });
                }

                jsonText = LitJson.JsonMapper.ToJson(data);
                bytes = System.Text.Encoding.UTF8.GetBytes(jsonText);

                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);

                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }

            AssetLabelStep.AssetLabelUpdate(true);
        }
    }
}