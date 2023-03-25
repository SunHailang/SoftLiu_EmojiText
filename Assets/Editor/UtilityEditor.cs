using System;

public static class UtilityEditor
{
    /// <summary>
    /// 跟据文件全路径 获取Assets文件下的路径
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public static string GetBasePath(string fullPath)
    {
        int index = fullPath.IndexOf(@"Assets\", StringComparison.Ordinal);
        if (index > 0)
        {
            return fullPath.Substring(index, fullPath.Length - index);
        }
        return fullPath;
    }

}
