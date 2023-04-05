
using System;

namespace HotFix_Project.ResourceLoaderCore
{
    public class AssetBundleLoaderManager : ResourceLoaderManager
    {

        public AssetBundleLoaderManager() : base("AssetBundleLoader")
        {

        }

        public override void LoadSceneAssetAsync(string sceneName, Action<bool> callback)
        {
           
        }

        public override void LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            string bundleName = $"ui/logic/{assetName.ToLower()}";
            UnityEngine.Debug.Log($"[AssetBundleLoaderManager LoadUiAssetAsync] : {bundleName}");
            HotFixMonoBehaviour.Instance.DoCoroutine(LoadAssetAsync<T>(bundleName, assetName, callback));
        }
    }
}
