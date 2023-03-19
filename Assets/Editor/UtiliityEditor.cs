using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class UtiliityEditor
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
