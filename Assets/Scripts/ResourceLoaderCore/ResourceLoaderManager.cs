using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

namespace ResourceLoaderCore
{
    public abstract class ResourceLoaderManager : IDisposable
    {
        private readonly string m_curKeyID = "";

        private readonly string m_rootPath = "";

        private readonly string m_assetBundleMainName = "AssetBundle";
        private AssetBundleManifest m_assetBundleManifest = null;

        private readonly Dictionary<string, AsyncAssetHandler> m_assetbundleLoadDict = new Dictionary<string, AsyncAssetHandler>();

        /// <summary>
        /// 校验 加载、卸载 AssetBundle时候可能会存在循环依赖
        /// </summary>
        private readonly HashSet<string> m_checkBundleCycleDependencies = new HashSet<string>();

        public ResourceLoaderManager(string KeyID)
        {
            m_curKeyID = KeyID;
            // 初始化对象池
            //ObjectPoolManager.Instance.InitPool<AsyncAssetHandler>(20);
            Debug.Log("1");
            m_rootPath = Path.Combine(GameConfigData.GetPlatformResRootPath(), "AssetBundle");
            Debug.Log($"2: {m_rootPath}");
            // 加载Main
            string mainPath = Path.Combine(m_rootPath, m_assetBundleMainName);
            Debug.Log($"3: {mainPath}");
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(mainPath));
            Debug.Log($"4: {assetBundle == null}");
            m_assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            Debug.Log($"5: {m_assetBundleManifest == null}");
            AsyncAssetHandler handle = new AsyncAssetHandler(); // ObjectPoolManager.Instance.GetPoolObject<AsyncAssetHandler>() as AsyncAssetHandler;
            Debug.Log($"6: {handle == null}");
            handle.Reset();
            handle.Count++;
            handle.AssetBundleName = m_assetBundleMainName;
            handle.AssetBundleData = assetBundle;
            m_assetbundleLoadDict[handle.AssetBundleName] = handle;
            
            // string shaderPath = Path.Combine(m_rootPath, "shaders");
            // AssetBundle shaderBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(shaderPath));
            // AsyncAssetHandler shaderHandle = new AsyncAssetHandler(); // ObjectPoolManager.Instance.GetPoolObject<AsyncAssetHandler>() as AsyncAssetHandler;
            // Debug.Log($"6: {shaderHandle == null}");
            // shaderHandle.Reset();
            // shaderHandle.Count++;
            // shaderHandle.AssetBundleName = "shaders";
            // shaderHandle.AssetBundleData = shaderBundle;
            // m_assetbundleLoadDict[shaderHandle.AssetBundleName] = shaderHandle;
        }

        public string GetResourceKeyID()
        {
            return m_curKeyID;
        }

        public abstract void LoadUiAssetAsync(string assetName, System.Action<bool, GameObject> callback);
        public abstract void LoadUiAssetAsync(string assetName, System.Action<bool, SpriteAtlas> callback);
        public abstract System.Collections.IEnumerator LoadSceneAssetAsync(string sceneName, System.Action<bool> callback);
        
        
        protected System.Collections.IEnumerator LoadAssetAsync(string bundleName, System.Action<bool> callback)
        {
            Debug.Log($"[LoadAssetAsync] : {bundleName}");
            m_checkBundleCycleDependencies.Clear();
            yield return LoadAssetDependencieAsync(bundleName);
            m_checkBundleCycleDependencies.Clear();

            if (m_assetbundleLoadDict.TryGetValue(bundleName, out AsyncAssetHandler assetHandler) && assetHandler != null && !assetHandler.IsNull())
            {
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        protected System.Collections.IEnumerator LoadAssetAsync<T>(string bundleName, string name, System.Action<bool, T> callback) where T : UnityEngine.Object
        {
            Debug.Log($"[LoadAssetAsync] : {bundleName}, {name}");
            m_checkBundleCycleDependencies.Clear();
            yield return LoadAssetDependencieAsync(bundleName);
            m_checkBundleCycleDependencies.Clear();

            if (m_assetbundleLoadDict.TryGetValue(bundleName, out AsyncAssetHandler assetHandler) && assetHandler != null && !assetHandler.IsNull())
            {
                // 加载 Bundle内的GameObject
                AssetBundleRequest assetRequest = assetHandler.AssetBundleData.LoadAssetAsync<T>(name);
                yield return assetRequest;
                var assetObj = assetRequest.asset;
                T obj = assetObj as T;

                if (obj != null)
                {
                    callback?.Invoke(true, obj);
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            }
            else
            {
                callback?.Invoke(false, null);
            }
        }


        private System.Collections.IEnumerator LoadAssetDependencieAsync(string bundleName)
        {
            if (!m_checkBundleCycleDependencies.Add(bundleName))
            {
                // AB包存在循环引用
                Debug.Log($"[LoadAssetDependencieAsync] AB包存在循环引用 : {bundleName}");
                yield break;
            }

            // 获取依赖
            string[] dependencies = m_assetBundleManifest.GetAllDependencies(bundleName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                yield return LoadAssetDependencieAsync(dependencies[i]);
            }

            if (!m_assetbundleLoadDict.TryGetValue(bundleName, out AsyncAssetHandler assetHandler) || assetHandler == null || assetHandler.IsNull())
            {
                string filePath = Path.Combine(m_rootPath, bundleName);
                Debug.Log($"LoadAssetDependencieAsync : {bundleName}, {filePath}");
                string ex = Path.GetExtension(filePath);
                AssetBundle assetData = null;
                if (ex == ".unity3d")
                {
                    string url = "";
                    #if UNITY_EDITOR
                    url = $"file://{filePath}";
                    #elif UNITY_ANDROID
                    url = $"jar:file:///{filePath}";
                    #endif
                    UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
                    yield return request.SendWebRequest();
                    assetData = DownloadHandlerAssetBundle.GetContent(request);
                }
                else
                {
                    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
                    yield return request;
                    assetData = request.assetBundle;
                }

                if (assetData != null)
                {
                    if (assetHandler == null)
                    {
                        assetHandler = new AsyncAssetHandler(); //ObjectPoolManager.Instance.GetPoolObject<AsyncAssetHandler>() as AsyncAssetHandler;
                    }

                    assetHandler.Reset();
                    assetHandler.Count++;
                    assetHandler.AssetBundleName = bundleName;
                    assetHandler.AssetBundleData = assetData;
                    Debug.Log($"[LoadAssetDependencieAsync] assetHandler : {assetHandler.AssetBundleName}");
                    m_assetbundleLoadDict[assetHandler.AssetBundleName] = assetHandler;
                }
                else
                {
                    // 加载失败
                    Debug.Log($"[LoadAssetDependencieAsync] assetData isNull: {bundleName}");
                    yield break;
                }
            }
            else
            {
                assetHandler.Count++;
            }
        }

        public System.Collections.IEnumerator UnloadAssetAsync(string bundleName)
        {
            // 卸载 自己 和 依赖
            m_checkBundleCycleDependencies.Clear();
            yield return UnloadAssetDependencieAsync(bundleName);
            m_checkBundleCycleDependencies.Clear();
        }

        private System.Collections.IEnumerator UnloadAssetDependencieAsync(string bundleName)
        {
            if (!m_checkBundleCycleDependencies.Add(bundleName))
            {
                // AB包存在循环引用
                yield break;
            }

            if (m_assetbundleLoadDict.TryGetValue(bundleName, out AsyncAssetHandler assetHandler))
            {
                if (assetHandler != null)
                {
                    if (assetHandler.IsNull())
                    {
                        //ObjectPoolManager.Instance.RecycleObjectPool<AsyncAssetHandler>(assetHandler);
                    }
                    else
                    {
                        assetHandler.Count--;
                        //if (assetHandler.Count <= 0)
                        {
                            // 卸载当前的 assetbundle
                            yield return assetHandler.AssetBundleData.UnloadAsync(true);
                            //assetHandler.Dispose();
                            //ObjectPoolManager.Instance.RecycleObjectPool<AsyncAssetHandler>(assetHandler);
                            m_assetbundleLoadDict.Remove(bundleName);
                        }
                    }
                }
                else
                {
                    m_assetbundleLoadDict.Remove(bundleName);
                }
            }

            // 获取依赖
            string[] dependencies = m_assetBundleManifest.GetAllDependencies(bundleName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                yield return UnloadAssetDependencieAsync(dependencies[i]);
            }
        }


        public System.Collections.IEnumerator UnloadAsset()
        {
            m_assetbundleLoadDict.Clear();
            m_assetBundleManifest = null;
            AssetBundle.UnloadAllAssetBundles(true);
            yield return null;
        }

        public void Dispose()
        {
        }
    }
}