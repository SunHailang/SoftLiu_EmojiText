using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ResourceLoaderCore
{
    public class AssetBundleLoaderManager : ResourceLoaderManager
    {
        public override T LoadAssetAsync<T>(string path, string name)
        {
            return null;
        }
    }
}
