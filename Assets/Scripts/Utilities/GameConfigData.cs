using System.IO;
using UnityEngine;

public static class GameConfigData
{
    public static bool UseAssetBundleLoader
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.Menu.GetChecked("HotFix/Editor Use AssetBundle Loader");
#else
            return false;
#endif
        }
    }


    /// <summary>
    /// 获取当前平台的名称
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return "Android";
#elif UNITY_IPHONE && !UNITY_EDITOR
        return "iOS";
#else
        return "PC";
#endif
    }

    /// <summary>
    /// 获取当前平台的沙盒资源根目录
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformResRootPath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Path.Combine(Application.persistentDataPath, $"Res/{GetPlatformName()}");
#elif UNITY_IPHONE && !UNITY_EDITOR
        return Path.Combine(Application.persistentDataPath, $"Res/{GetPlatformName()}");
#else
        // 在Editor模式下 可以支持 本地资源加载 和 AssetBundle资源加载
        return Path.Combine(Application.dataPath, $"../HotFixRes/Res/{GetPlatformName()}");
#endif
    }
}