using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class Timer
    {
        private string m_name = "";
        public string Name => m_name;

        private bool m_isPause = false;
        public bool IsPause => m_isPause;

        public Timer(string _name)
        {
            m_name = _name;
        }

        public virtual void Process(float deltaTime)
        {
            if (IsPause) return;
        }

        public virtual void Start()
        {
            m_isPause = false;
        }

        public virtual void Pause()
        {
            m_isPause = true;
        }
    }
}
