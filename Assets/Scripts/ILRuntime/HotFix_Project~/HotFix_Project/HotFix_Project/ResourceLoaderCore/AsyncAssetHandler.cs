using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix_Project.ResourceLoaderCore
{
    public class AsyncAssetHandler : ObjectPool.ObjectPool
    {
        public int Count = 0;

        public string AssetBundleName = "";
        public AssetBundle AssetBundleData = null;

        public bool IsNull()
        {
            Debug.Log($"[AsyncAssetHandler] IsNull : {AssetBundleName}");
            return AssetBundleData == null;
        }

        public override void Reset()
        {
            Count = 0;
        }

        protected override void DisposeManagedResources()
        {
            // 释放托管资源
            base.DisposeManagedResources();
        }

        protected override void DisposeUnManagedResources()
        {
            // 释放非托管资源
            base.DisposeUnManagedResources();
        }
    }
}
