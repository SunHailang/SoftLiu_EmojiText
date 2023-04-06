using System;
using UnityEngine;
using UnityEngine.U2D;

namespace ResourceLoaderCore
{
    public class EditorLoaderManager : ResourceLoaderManager
    {

        public EditorLoaderManager() : base("EditorLoader")
        {

        }

        public override System.Collections.IEnumerator LoadSceneAssetAsync(string sceneName, Action<bool> callback)
        {
            yield return null;
        }
        
        public override void LoadUiAssetAsync(string assetName, Action<bool, SpriteAtlas> callback)
        {
            
        }


        public override void LoadUiAssetAsync(string assetName, System.Action<bool, GameObject> callback)
        {
            
        }
    }
}
