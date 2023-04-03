

namespace HotFix_Project.Timer
{
    public abstract class Timer : System.IDisposable
    {
        protected string m_name = "";
        public string Name => m_name;

        protected bool m_isPause = false;
        public bool IsPause => m_isPause;

        public System.Action onCallback;

        public Timer(string _name)
        {
            m_name = _name;
        }

        public abstract void Process(float deltaTime);

        public abstract void Start();

        public abstract void Pause();

        #region 资源释放
        private bool m_isDisposed = false;
        public void Dispose()
        {
            m_name = "";
            m_isPause = true;
            onCallback = null;
            Dispose(true);
            // 调用SuppressFinalize()方法就意味着垃圾会后期认为这个对象根本没有析构函数
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (!m_isDisposed)
            {
                if (isDisposing)
                {
                    // 清理托管资源对象
                    DisposeManagedResources();
                }
                // 清理非托管资源对象
                DisposeUnManagedResources();
                m_isDisposed = true;
            }
        }
        /// <summary>
        /// 清理托管资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {

        }
        /// <summary>
        /// 清理非托管资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {

        }

        ~Timer()
        {
            Dispose(false);
        }

        #endregion
    }
}
