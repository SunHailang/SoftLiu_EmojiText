using UnityEngine;

namespace HotFix_Project.ResourceLoaderCore
{
    public class EditorLoaderManager : ResourceLoaderManager
    {

        public EditorLoaderManager() : base("EditorLoader")
        {

        }

        public override System.Collections.IEnumerator LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            string bundleName = "ui/logic/gamecontroller";
            yield return LoadAssetAsync<T>(bundleName, assetName, callback);
        }
    }
}
