#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        public override void LoadUiAssetAsync(string assetName, Action<bool, SpriteAtlas> callback)
        {
#if UNITY_EDITOR
            string path = $"AssetBundlePackage/UI/Logic/{assetName}/SpriteAtlas/{assetName}.spriteatlas";
            if (File.Exists(path))
            {
                SpriteAtlas go = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                callback(true, go);
            }
            else
            {
                callback(false, null);
            }
#endif
        }


        public override void LoadUiAssetAsync(string assetName, System.Action<bool, GameObject> callback)
        {
#if UNITY_EDITOR
            string path = $"Assets/AssetBundlePackage/UI/Logic/{assetName}/Prefabs/{assetName}.prefab";
            if (File.Exists(path))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                callback(true, go);
            }
            else
            {
                callback(false, null);
            }
#endif
        }
    }
}
#endif