using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class FileUtilities
{
    /// <summary>
    /// 判断文件夹是否存在
    /// </summary>
    /// <param name="dirPath">文件夹路径</param>
    /// <param name="create">不存在是否新建： True 新建， False 不新建</param>
    /// <returns>存在：True， 其他：False</returns>
    public static bool IsExistsDirectory(string dirPath, bool create = true)
    {
        if (!Directory.Exists(dirPath))
        {
            if (create)
            {
                Directory.CreateDirectory(dirPath);
                Debug.Log($"[FileUtilities] IsExistsDirectory Create:{dirPath}");
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 拷贝文件夹下的所有（包括文件、子文件夹）
    /// </summary>
    /// <param name="sourcePath">源文件夹路径</param>
    /// <param name="destPath">目标文件夹路径</param>
    public static void CopyDirectory(string sourcePath, string destPath)
    {
        string floderName = Path.GetFileName(sourcePath);
        string dirPath = Path.Combine(destPath, floderName);
        bool isExist = IsExistsDirectory(dirPath, true);
        DirectoryInfo di = new DirectoryInfo(dirPath);

        string[] files = Directory.GetFileSystemEntries(sourcePath);

        foreach (string file in files)
        {
            if (Directory.Exists(file))
            {
                CopyDirectory(file, di.FullName);
            }
            else
            {
                File.Copy(file, Path.Combine(di.FullName, Path.GetFileName(file)), true);
            }
        }
    }

    public static Dictionary<string, KeyValuePair<string, int>> GetAssetMD5Data(string output)
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
                        string rootFolder = "HotFixRes";
                        int index = file.FullName.LastIndexOf(rootFolder, StringComparison.Ordinal);
                        if (index < 0)
                        {
                            Debug.LogError($"[AssetVersionUpdate] Index:{index}, {file.FullName}");
                            continue;
                        }

                        index += rootFolder.Length + 1;
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

        return assetMd5Dict;
    }
}