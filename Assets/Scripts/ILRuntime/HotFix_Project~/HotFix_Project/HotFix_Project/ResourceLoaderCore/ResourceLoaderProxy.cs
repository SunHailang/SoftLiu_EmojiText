using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ResourceLoaderCore
{
    public class ResourceLoaderProxy
    {
        private static ResourceLoaderManager m_manager = null;
        private static object _lock = new object();

        static ResourceLoaderProxy()
        {

        }

        public static ResourceLoaderManager GetInstance()
        {
            lock (_lock)
            {
                if (m_manager == null)
                {
#if UNITY_EDITOR
                    m_manager = new EditorLoaderManager();
#else
                    m_manager = new AssetBundleLoaderManager();
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
