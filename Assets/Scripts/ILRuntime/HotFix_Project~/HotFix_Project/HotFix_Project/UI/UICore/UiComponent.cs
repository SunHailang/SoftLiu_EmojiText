using UnityEngine;
using System.Collections.Generic;

namespace HotFix_Project.UI.UICore
{
    public abstract class UiComponent : System.IDisposable
    {
        protected GameObject m_curGo = null;

        private Dictionary<int, System.Type> m_curComponent = new Dictionary<int, System.Type>();

        public abstract void BindingGo(GameObject go);

        public virtual T GetComponent<T>() where T : UiComponent
        {
            return null;
        }

        public virtual T AddComponent<T>() where T : UiComponent
        {
            return null;
        }

        #region 资源回收
        private bool m_isDispose = false;
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(!m_isDispose)
            {
                if(disposing)
                {
                    // 托管资源回收
                }
                // 非托管资源会收
                m_isDispose = true;
                System.GC.SuppressFinalize(this);
            }
        }

        ~UiComponent()
        {
            Dispose(false);
        }
        #endregion
    }
}
