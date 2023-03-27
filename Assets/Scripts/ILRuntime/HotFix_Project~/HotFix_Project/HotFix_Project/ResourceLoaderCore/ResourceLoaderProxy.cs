using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ResourceLoaderCore
{
    public class ResourceLoaderProxy
    {
        private Dictionary<string, string> m_assetbundleLoadDict = new Dictionary<string, string>();

        static ResourceLoaderProxy()
        {

        }
        public static ResourceLoaderManager GetInstance()
        {
#if UNITY_EDITOR
            return new EditorLoaderManager();
#else
            return new AssetBundleLoaderManager();
#endif
        }
    }


    public class AssetInfo
    {

    }
}
