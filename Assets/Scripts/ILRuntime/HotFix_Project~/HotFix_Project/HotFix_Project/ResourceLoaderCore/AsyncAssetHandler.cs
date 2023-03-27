using System;
using System.Collections.Generic;

namespace HotFix_Project.ResourceLoaderCore
{
    public interface IAsyncAssetHandler
    {
        bool IsCompleted();
        void Begin();
        bool IsNull();
    }

    public class AsyncAssetHandler<T> : IAsyncAssetHandler where T : UnityEngine.Object
    {
        public void Begin()
        {
            
        }

        public bool IsCompleted()
        {
            return true;
        }

        public bool IsNull()
        {
            return false;
        }
    }
}
