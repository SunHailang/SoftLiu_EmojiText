using System;
using System.Collections.Generic;

namespace HotFix_Project
{
    public class ObjectPoolHandler<T> : IDisposable where T : ObjectPool.ObjectPool, new()
    {
        private int m_maxCount = 0;

        private Stack<T> m_curList = null;

        public ObjectPoolHandler(int maxCount)
        {
            m_maxCount = maxCount;
            
            m_curList = new Stack<T>(m_maxCount);
        }

        public T GetPoolObject() 
        {
            if (m_curList.Count > 0)
            {
                return m_curList.Pop();
            }
            return null;
        }

        public void RecyclePoolObject(T obj)
        {
            if (m_curList.Count >= m_maxCount)
            {
                obj.Dispose();
            }
            else
            {
                m_curList.Push(obj);
            }
        }

        private bool m_IsDispose = false;
        public void Dispose()
        {
            if (m_IsDispose)
            {
                if (m_curList != null)
                {
                    while (m_curList.Count > 0)
                    {
                        T obj = m_curList.Pop();
                        if (obj != null)
                        {
                            obj.Dispose();
                        }
                    }
                }
                m_IsDispose = true;
            }
            System.GC.SuppressFinalize(this);
        }
    }
}
