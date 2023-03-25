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
    public static string GetAssetBundleVersion(uint value)
    {
        if (value <= 0)
        {
            value = 1;
        }

        uint major = (uint) (value >> 28);
        uint minor = (uint) ((value >> 22) & 0x3F);
        uint patch = (uint) ((value >> 12) & 0xFF);
        uint desc = (uint) (value & 0x3FFF);

        return $"{major}.{minor}.{patch}.{desc:D4}";
    }
}