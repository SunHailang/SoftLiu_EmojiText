using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class ResourceLoaderProxy
    {
        private static ResourceLoaderCore.ResourceLoaderManager m_manager = null;
        private readonly static object _lock = new object();

        static ResourceLoaderProxy()
        {

        }

        public static ResourceLoaderCore.ResourceLoaderManager GetInstance()
        {
            lock (_lock)
            {
                if (m_manager == null)
                {
#if UNITY_EDITOR
                    if (!GameConfigData.UseAssetBundleLoader)
                    {
                        m_manager = new ResourceLoaderCore.EditorLoaderManager();
                    }
                    else
                    {
                        m_manager = new ResourceLoaderCore.AssetBundleLoaderManager();
                    }
#else
                    m_manager = new ResourceLoaderCore.AssetBundleLoaderManager();
#endif
                }
                return m_manager;
            }
        }
    }


    public class AssetInfo
    {

    }
}
