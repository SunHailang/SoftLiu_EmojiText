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
        public override System.Collections.IEnumerator LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            string bundleName = "ui/logic/gamecontroller";
            UnityEngine.Debug.Log($"[AssetBundleLoaderManager LoadUiAssetAsync] : {bundleName}");
            yield return LoadAssetAsync<T>(bundleName, assetName, callback);
        }
    }
}
