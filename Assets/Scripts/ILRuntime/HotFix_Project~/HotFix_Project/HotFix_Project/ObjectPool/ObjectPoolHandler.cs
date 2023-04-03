using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class ObjectPoolHandler<T> : IDisposable where T : ObjectPool.ObjectPool, new()
    {
        private int m_maxCount = 0;

        private List<T> m_curList = null;

        public ObjectPoolHandler(int maxCount)
        {
            m_maxCount = maxCount;

            m_curList = new List<T>(m_maxCount);
        }

        public T GetPoolObject()
        {
            if (m_curList.Count > 0)
            {
                return m_curList[m_curList.Count - 1];
            }
            return new T();
        }

        public void RecyclePoolObject(T obj)
        {
            if (m_curList.Count >= m_maxCount)
            {
                obj.Dispose();
            }
            else
            {
                m_curList.Add(obj);
            }
        }

        private bool m_IsDispose = false;
        public void Dispose()
        {
            if (m_IsDispose)
            {
                if (m_curList != null && m_curList.Count > 0)
                {
                    for (int i = 0; i < m_curList.Count; i++)
                    {
                        if (m_curList[i] != null)
                        {
                            m_curList[i].Dispose();
                        }
                    }
                }

                m_IsDispose = true;
            }
            System.GC.SuppressFinalize(this);
        }
    }
}
