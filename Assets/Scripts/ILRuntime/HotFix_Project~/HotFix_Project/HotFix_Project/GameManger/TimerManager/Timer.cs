

namespace HotFix_Project
{
    public class Timer : System.IDisposable
    {
        protected string m_name = "";
        public string Name => m_name;

        protected bool m_isPause = false;
        public bool IsPause => m_isPause;

        public event System.Action onCallback;

        public Timer(string _name)
        {
            m_name = _name;
        }

        public virtual void Process(float deltaTime)
        {
            if (IsPause) return;
            onCallback?.Invoke();
        }

        public virtual void Start()
        {
            m_isPause = false;
        }

        public virtual void Pause()
        {
            m_isPause = true;
        }
        #region 资源释放
        private bool m_isDisposed = false;
        public void Dispose()
        {
            m_name = "";
            m_isPause = true;
            onCallback = null;
            this.Dispose(true);
            // 调用SuppressFinalize()方法就意味着垃圾会后期认为这个对象根本没有析构函数
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!m_isDisposed)
            {
                if(disposing)
                {
                    // 清理托管资源对象
                }
                // 清理非托管资源对象

                m_isDisposed = true;
            }
        }
        #endregion
    }
}
