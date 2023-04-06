
using System;
using UnityEngine;
using UnityEngine.U2D;

namespace ResourceLoaderCore
{
    public class AssetBundleLoaderManager : ResourceLoaderManager
    {
        public AssetBundleLoaderManager() : base("AssetBundleLoader")
        {

        }

        public override System.Collections.IEnumerator LoadSceneAssetAsync(string sceneName, Action<bool> callback)
        {
            string bundleName = $"environments/{sceneName.ToLower()}.unity3d";
            yield return LoadAssetAsync(bundleName, callback);
        }
        
        public override void LoadUiAssetAsync(string assetName, Action<bool, SpriteAtlas> callback)
        {
            string bundleName = $"ui/logic/{assetName.ToLower()}";
            HotFixMonoBehaviour.Instance.StartCoroutine(LoadAssetAsync(bundleName, assetName, callback));
        }


        public override void LoadUiAssetAsync(string assetName, System.Action<bool, GameObject> callback)
        {
            string bundleName = $"ui/logic/{assetName.ToLower()}";
            UnityEngine.Debug.Log($"[AssetBundleLoaderManager LoadUiAssetAsync] : {bundleName}");
            HotFixMonoBehaviour.Instance.StartCoroutine(LoadAssetAsync<GameObject>(bundleName, assetName, callback));
        }
    }
}
