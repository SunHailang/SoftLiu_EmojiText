using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ResourceLoaderCore
{
    public abstract class ResourceLoaderManager
    {
        public abstract T LoadAssetAsync<T>(string path, string name) where T : UnityEngine.Object;
    }
}
