public static class VersionUtilities
{
    /// <summary>
    /// 32位 0000 . 00000000 . 00000000 . 000000000000
    /// </summary>
    /// <param name="value">版本号值</param>
    /// <returns></returns>
    public static string GetVersion(int value)
    {
        if (value <= 0)
        {
            value = 1;
        }

        uint major = (uint) (value >> 28);
        uint minor = (uint) ((value >> 20) & 0xFF);
        uint patch = (uint) ((value >> 12) & 0xFF);
        uint desc = (uint) (value & 0xFFF);

        return $"{major}.{minor}.{patch}.{desc:D4}";
    }
}