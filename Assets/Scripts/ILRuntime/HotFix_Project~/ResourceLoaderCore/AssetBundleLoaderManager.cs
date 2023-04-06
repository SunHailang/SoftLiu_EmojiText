
using System;

namespace HotFix_Project.ResourceLoaderCore
{
    public class AssetBundleLoaderManager : ResourceLoaderManager
    {

        public AssetBundleLoaderManager() : base("AssetBundleLoader")
        {

        }

        public override System.Collections.IEnumerator LoadSceneAssetAsync<T>(string sceneName, Action<bool, T> callback)
        {
            string bundleName = $"environments/{sceneName.ToLower()}.unity3d";
            yield return LoadAssetAsync(bundleName, sceneName, callback);
        }


        public override void LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            string bundleName = $"ui/logic/{assetName.ToLower()}";
            UnityEngine.Debug.Log($"[AssetBundleLoaderManager LoadUiAssetAsync] : {bundleName}");
            HotFixMonoBehaviour.Instance.DoCoroutine(LoadAssetAsync<T>(bundleName, assetName, callback));
        }
    }
}
