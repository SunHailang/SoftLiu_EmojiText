using System.IO;
using UnityEditor;
using UnityEngine;

/**
 * @Author 
 * @FileName AssetBundleUtilityEditor.cs
 * @Data 2023年3月25日
**/
public static class AssetBundleUtilityEditor
{
    public static readonly string AssetBundleRootPath = Path.Combine(Application.dataPath, "../../SoftLiu_ServerIOCSharp/Out/Resources/HotFixRes/Res");
    public static readonly string AssetBundleRootFolder = "HotFixRes";
    
    
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
}