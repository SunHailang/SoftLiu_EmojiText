using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project.ObjectPool
{
    public class ObjectPool : System.IDisposable
    {

        public ObjectPool()
        {

        }

        public virtual void Reset()
        {

        }


        private bool m_isDispose = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(m_isDispose)
            {
                if(disposing)
                {
                    DisposeManagedResources();
                }
                DisposeUnManagedResources();
                m_isDispose = false;
            }
        }
        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            
        }
        /// <summary>
        /// 释放非托管资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {
            
        }
        ~ObjectPool()
        {
            Dispose(false);
        }
    }
}
