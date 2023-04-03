using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ResourceLoaderCore
{
    public class AssetBundleLoaderManager : ResourceLoaderManager
    {

        public AssetBundleLoaderManager() : base("AssetBundleLoader")
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
