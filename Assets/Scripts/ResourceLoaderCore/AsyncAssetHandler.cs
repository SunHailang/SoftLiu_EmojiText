using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceLoaderCore
{
    public class AsyncAssetHandler
    {
        public int Count = 0;

        public string AssetBundleName = "";
        public AssetBundle AssetBundleData = null;

        public bool IsNull()
        {
            Debug.Log($"[AsyncAssetHandler] IsNull : {AssetBundleName}, {AssetBundleData == null}");
            return AssetBundleData == null;
        }

        public void Reset()
        {
            Count = 0;
        }

        protected void DisposeManagedResources()
        {
            // 释放托管资源
        }

        protected void DisposeUnManagedResources()
        {
            // 释放非托管资源
        }
    }
}
