using System.Collections.Generic;

namespace HotFix_Project.UI.UICore
{
    public abstract class UiComponent : System.IDisposable
    {
        protected UnityEngine.GameObject m_curGo = null;

        private Dictionary<int, System.Type> m_curComponent = new Dictionary<int, System.Type>();

        public abstract void Initialization(UnityEngine.GameObject go);

        public virtual bool TryGetComponent<T>(out T outType) where T : UiComponent
        {
            int hashValue = typeof(T).GetHashCode();
            if (m_curComponent.TryGetValue(hashValue, out System.Type type))
            {
                if (type is T)
                {
                    outType = type as T;
                    return true;
                }
            }
            outType = null;
            return false;
        }

        public virtual T GetOrAddComponent<T>() where T : UiComponent
        {
            int hashValue = typeof(T).GetHashCode();
            if (m_curComponent.TryGetValue(hashValue, out System.Type type))
            {
                if (type is T)
                {
                    return type as T;
                }
            }
            type = typeof(T);
            m_curComponent[hashValue] = type;
            return type as T;
        }

        #region 资源回收
        private bool m_isDispose = false;
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!m_isDispose)
            {
                if (disposing)
                {
                    // 托管资源回收
                    DisposeManagedResources();
                }
                // 非托管资源会收
                DisposeUnManagedResources();
                m_isDispose = true;
                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 清理托管资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            if (m_curGo != null)
            {
                UnityEngine.GameObject.Destroy(m_curGo);
            }
            m_curGo = null;
        }
        /// <summary>
        /// 清理非托管资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {

        }

        ~UiComponent()
        {
            Dispose(false);
        }
        #endregion
    }
}
