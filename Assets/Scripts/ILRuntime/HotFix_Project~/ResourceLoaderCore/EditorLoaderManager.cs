using System;
using UnityEngine;

namespace HotFix_Project.ResourceLoaderCore
{
    public class EditorLoaderManager : ResourceLoaderManager
    {

        public EditorLoaderManager() : base("EditorLoader")
        {

        }

        public override System.Collections.IEnumerator LoadSceneAssetAsync<T>(string sceneName, Action<bool, T> callback)
        {
            yield return LoadAssetAsync<T>("", sceneName, callback);
        }

        public override void LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            //string bundleName = "ui/logic/gamecontroller";
            //HotFixMonoBehaviour.Instance.DoCoroutine(LoadAssetAsync<T>(bundleName, assetName, callback));
        }
    }
}
