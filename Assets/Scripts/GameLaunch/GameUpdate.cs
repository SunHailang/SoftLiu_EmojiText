using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUpdate : MonoBehaviour
{
    [SerializeField] private Text m_curResVersionText = null;
    [SerializeField] private GameUpdateSlider m_updateSlider = null;
    

    private IEnumerator Start()
    {
        string persistentDataPath = GameConfigData.GetPlatformResRootPath();
        List<string> m_updateFile = null;

        // 读取本地版本文件
        string resVersionPath = Path.Combine(persistentDataPath, "ResVersion.bytes");
        if (!File.Exists(resVersionPath))
        {
            // 所有的文件都要下载
            
        }
        else
        {
            m_updateFile = new List<string>();
        }

        // 显示当前资源版本号
        ResVersionData resData = VersionUtilities.ReadVersionData<ResVersionData>(resVersionPath);
        string curResVersion = VersionUtilities.GetResVersion(resData.Version);
        m_curResVersionText.text = $"当前资源版本号：{curResVersion}";
        
        // 读取 AssetBundle 资源版本号
        string assetVersionPath = Path.Combine(persistentDataPath, "AssetBundle/version.bytes");
        AssetBundleVersionData assetVersion = VersionUtilities.ReadVersionData<AssetBundleVersionData>(assetVersionPath);
        
        
        // 读取 Datas 资源版本号
        string datasVersionPath = Path.Combine(persistentDataPath, "Datas/version.bytes");
        
        m_updateSlider.SetSliderProgress(0);
        // 下载服务器资源版本文件
        //UnityWebRequest
        // 文件对比

        // 对差异化的文件进行下载

        // 下载完成 
        yield return null;
        // 加载ILRuntime
        HotFixMgr.Instance.Init();
    }
    
}