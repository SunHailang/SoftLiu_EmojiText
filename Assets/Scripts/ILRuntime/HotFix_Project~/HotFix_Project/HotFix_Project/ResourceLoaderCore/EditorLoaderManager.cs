using System;
using UnityEngine;

namespace HotFix_Project.ResourceLoaderCore
{
    public class EditorLoaderManager : ResourceLoaderManager
    {

        public EditorLoaderManager() : base("EditorLoader")
        {

        }

        public override void LoadSceneAssetAsync(string sceneName, Action<bool> callback)
        {
           
        }

        public override void LoadUiAssetAsync<T>(string assetName, System.Action<bool, T> callback)
        {
            //string bundleName = "ui/logic/gamecontroller";
            //HotFixMonoBehaviour.Instance.DoCoroutine(LoadAssetAsync<T>(bundleName, assetName, callback));
        }
    }
}
