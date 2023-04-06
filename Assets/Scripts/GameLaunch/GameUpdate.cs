using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUpdate : MonoBehaviour
{
    [SerializeField] private Text m_curResVersionText = null;
    [SerializeField] private GameUpdateSlider m_updateSlider = null;


    private IEnumerator Start()
    {
        string persistentDataPath = GameConfigData.GetPlatformResRootPath();

        List<KeyValuePair<string, int>> m_updateFile = new List<KeyValuePair<string, int>>();

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
        // 1. 进度 0~10 校验版本内容
        int resServerInfoCount = resServerData.ResInfoList.Count;
        float perProcess = 10.0f / resServerInfoCount;
        float latestProcess = 0f;
        for (int i = 0; i < resServerInfoCount; i++)
        {
            string dirName = resServerData.ResInfoList[i].Name;
            yield return WebRequestManager.Instance.CreateDownloadFileRequest($"{dirName}/version.bytes", persistentDataPath);
            yield return null;
            string verPath = Path.Combine(persistentDataPath, $"{dirName}/version.bytes");
            AssetVersionData assetServerData = VersionUtilities.ReadVersionData<AssetVersionData>(verPath);

            Dictionary<string, KeyValuePair<string, int>> assetMd5Dict = FileUtilities.GetAssetMD5Data(Path.Combine(persistentDataPath, dirName));
            int assetServerMD5Count = assetServerData.AssetMd5List.Count;
            perProcess /= assetServerMD5Count;
            for (int k = 0; k < assetServerMD5Count; k++)
            {
                AssetInfoData assetInfo = assetServerData.AssetMd5List[k];
                if (assetMd5Dict.TryGetValue(assetInfo.Path, out KeyValuePair<string, int> pair))
                {
                    if (assetInfo.Size != pair.Value || assetInfo.MD5 != pair.Key)
                    {
                        int index = assetInfo.Path.IndexOf(dirName, StringComparison.Ordinal);
                        string file = assetInfo.Path.Substring(index);
                        m_updateFile.Add(new KeyValuePair<string, int>(file, assetInfo.Size));
                        //yield return WebRequestManager.Instance.CreateDownloadFileRequest(file, persistentDataPath);
                        yield return null;
                    }
                    assetMd5Dict.Remove(assetInfo.Path);
                }
                else
                {
                    int index = assetInfo.Path.IndexOf(dirName, StringComparison.Ordinal);
                    string file = assetInfo.Path.Substring(index);
                    m_updateFile.Add(new KeyValuePair<string, int>(file, assetInfo.Size));
                    //yield return WebRequestManager.Instance.CreateDownloadFileRequest(file, persistentDataPath);
                    yield return null;
                }

                latestProcess += assetServerMD5Count;
                m_updateSlider.SetSliderProgress(latestProcess / 100.0f);
            }

            if (assetMd5Dict.Count > 0)
            {
                foreach (var asset in assetMd5Dict)  
                {
                    Debug.Log($"File Path : {asset.Key}");
                    string path = Path.Combine(GameConfigData.GetPlatformResRootPath(), $"../../{asset.Key}");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                
            }
        }

        // 2. 进度 10 ~ 90 下载文件
        int updateFileCount = m_updateFile.Count;
        perProcess = 80.0f / updateFileCount;

        for (int j = 0; j < updateFileCount; j++)
        {
            KeyValuePair<string, int> file = m_updateFile[j];

            yield return WebRequestManager.Instance.CreateDownloadFileRequest(file.Key, persistentDataPath);
            yield return null;

            m_updateSlider.SetSliderProgress((latestProcess + perProcess * j) / 100.0f);
        }

        m_updateSlider.SetLatestVersionText("");
        // 文件对比
        m_curResVersionText.text = $"当前资源版本号：{VersionUtilities.GetResVersion(resServerData.Version)}";
        // 对差异化的文件进行下载
        // 下载完成 
        yield return null;
        GC.Collect();
        // 加载ILRuntime
        yield return HotFixMgr.Instance.LoadHotFixAssembly();

        yield return EndProgress();
    }

    private System.Collections.IEnumerator LoadAssetDependencieAsync(string bundleName, AssetBundleManifest assetBundleManifest)
    {
        string m_rootPath = Path.Combine(GameConfigData.GetPlatformResRootPath(), "AssetBundle");


        // 获取依赖
        string[] dependencies = assetBundleManifest.GetAllDependencies(bundleName);
        for (int i = 0; i < dependencies.Length; i++)
        {
            yield return LoadAssetDependencieAsync(dependencies[i], assetBundleManifest);
        }

        //if (!m_assetbundleLoadDict.TryGetValue(bundleName, out AsyncAssetHandler assetHandler) || assetHandler == null || assetHandler.IsNull())
        {
            string filePath = Path.Combine(m_rootPath, bundleName);
            Debug.Log($"LoadAssetDependencieAsync : {bundleName}, {filePath}");
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
            yield return request;
            var assetData = request.assetBundle;
            if (assetData != null)
            {
            }
            else
            {
                // 加载失败
                Debug.Log($"[LoadAssetDependencieAsync] assetData isNull: {bundleName}");
                yield break;
            }
        }
    }

    private IEnumerator EndProgress()
    {
        float value = m_updateSlider.CurValue;
        while (value < 1f)
        {
            yield return null;
            value += Time.deltaTime * 0.05f;
            m_updateSlider.SetSliderProgress(value);
        }

        m_updateSlider.SetSliderProgress(1.0f);
    }

    public void Btn_OnClick()
    {
        HotFixMonoBehaviour.Instance.Init();
    }
}