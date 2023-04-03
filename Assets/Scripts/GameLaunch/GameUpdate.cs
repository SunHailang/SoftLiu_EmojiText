using System;
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
        FileUtilities.IsExistsDirectory(persistentDataPath);
        string resVersionPath = Path.Combine(persistentDataPath, "ResVersion.bytes");
        // 显示当前资源版本号
        ResVersionData resData = VersionUtilities.ReadVersionData<ResVersionData>(resVersionPath);
        string curResVersion = VersionUtilities.GetResVersion(resData.Version);
        m_curResVersionText.text = $"当前资源版本号：{curResVersion}";
        // 下载服务器资源版本文件
        yield return WebRequestManager.Instance.CreateDownloadFileRequest("ResVersion.bytes", persistentDataPath);
        ResVersionData resServerData = VersionUtilities.ReadVersionData<ResVersionData>(resVersionPath);
        if (resData.Version != resServerData.Version)
        {
            // 更新最新版本资源
            string resServerVersion = VersionUtilities.GetResVersion(resServerData.Version);
            m_updateSlider.SetLatestVersionText($"最新资源版本号：{resServerVersion}");
        }
        else
        {
            // 比较本地文件和服务器文件
            m_updateSlider.SetLatestVersionText($"配置校验中...");
        }

        m_updateSlider.SetSliderProgress(0);
        for (int i = 0; i < resServerData.ResInfoList.Count; i++)
        {
            m_updateSlider.SetSliderProgress((i + 1 * 10) / 100.0f);
            string dirName = resServerData.ResInfoList[i].Name;
            yield return WebRequestManager.Instance.CreateDownloadFileRequest($"{dirName}/version.bytes", persistentDataPath);
            yield return null;
            string verPath = Path.Combine(persistentDataPath, $"{dirName}/version.bytes");
            AssetVersionData assetServerData = VersionUtilities.ReadVersionData<AssetVersionData>(verPath);

            Dictionary<string, KeyValuePair<string, int>> assetMd5Dict = FileUtilities.GetAssetMD5Data(Path.Combine(persistentDataPath, dirName));
            for (int k = 0; k < assetServerData.AssetMd5List.Count; k++)
            {
                AssetInfoData assetInfo = assetServerData.AssetMd5List[k];
                if (assetMd5Dict.TryGetValue(assetInfo.Path, out KeyValuePair<string, int> pair))
                {
                    if (assetInfo.Size != pair.Value || assetInfo.MD5 != pair.Key)
                    {
                        int index = assetInfo.Path.IndexOf(dirName, StringComparison.Ordinal);
                        string file = assetInfo.Path.Substring(index);
                        yield return WebRequestManager.Instance.CreateDownloadFileRequest(file, persistentDataPath);
                        yield return null;
                    }
                }
                else
                {
                    int index = assetInfo.Path.IndexOf(dirName, StringComparison.Ordinal);
                    string file = assetInfo.Path.Substring(index);
                    yield return WebRequestManager.Instance.CreateDownloadFileRequest(file, persistentDataPath);
                    yield return null;
                }
            }
        }


        // 文件对比

        // 对差异化的文件进行下载
        m_updateSlider.SetSliderProgress(1);
        // 下载完成 
        yield return null;
        GC.Collect();
        // 加载ILRuntime
        yield return HotFixMgr.Instance.LoadHotFixAssembly();
    }


    public void Btn_OnClick()
    {
        HotFixMonoBehaviour.Instance.Init();
    }
}