using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class VersionUtilities
{
    /// <summary>
    /// 获取包版本号
    /// </summary>
    /// <returns></returns>
    public static string GetGameVersion()
    {
        return Application.version;
    }


    /// <summary>
    /// 32位 4|0000 . 6|000000 . 8|00000000 . 14|00000000000000
    /// </summary>
    /// <param name="value">版本号值</param>
    /// <returns></returns>
    public static string GetResVersion(uint value)
    {
        if (value <= 0)
        {
            value = 1;
        }

        uint major = (uint) (value >> 28) & 0xF;
        uint minor = (uint) ((value >> 22) & 0x3F);
        uint patch = (uint) ((value >> 12) & 0xFF);
        uint desc = (uint) (value & 0x3FFF);

        return $"{major}.{minor}.{patch}.{desc:D4}";
    }

    /// <summary>
    /// 读取资源文件的版本内容转换成 T类型
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ReadVersionData<T>(string path) where T : class, new()
    {
        T data = default;
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            string jsonText = System.Text.Encoding.UTF8.GetString(bytes);
            data = LitJson.JsonMapper.ToObject<T>(jsonText);
        }
        data ??= new T();
        return data;
    }
}

public class AssetBundleMD5
{
    public string Path = "";
    public string MD5 = "";
    public int Size = 0;
}

public class AssetBundleVersionData
{
    public uint Version = 0;
    public List<AssetBundleMD5> AssetBundleMd5List = null;
}

public class ResInfoData
{
    public string Name = "";
    public uint Version = 0;
}

public class ResVersionData
{
    public uint Version = 0;
    public List<ResInfoData> ResInfoList = null;
}

public class DatasVersionData
{
}