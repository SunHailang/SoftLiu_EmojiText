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
                    if (file.Name != "version.bytes")
                    {
                        using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        {
                            MD5 md5 = MD5.Create();
                            byte[] bytes = md5.ComputeHash(fs);
                            string fileMd5 = System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
                            int index = file.FullName.LastIndexOf(AssetBundleUtilityEditor.AssetBundleRootFolder, StringComparison.Ordinal);
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

                var jsonText = System.Text.Encoding.UTF8.GetString(buffer);
                var data = LitJson.JsonMapper.ToObject<VersionData>(jsonText);

                data ??= new VersionData();

                data.Version += 1;
                data.AssetBundleMd5List ??= new List<AssetBundleMD5>(assetMd5Dict.Count);

                data.AssetBundleMd5List.Clear();
                foreach (KeyValuePair<string, string> pair in assetMd5Dict)
                {
                    data.AssetBundleMd5List.Add(new AssetBundleMD5()
                    {
                        Path = pair.Key, MD5 = pair.Value
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
        public string Path = "";
        public string MD5 = "";
    }

    public class VersionData
    {
        public uint Version = 0;
        public List<AssetBundleMD5> AssetBundleMd5List = null;
    }
}