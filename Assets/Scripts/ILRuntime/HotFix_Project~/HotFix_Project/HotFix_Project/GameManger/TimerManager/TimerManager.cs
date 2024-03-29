﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class TimerManager : Utils.AutoGeneratedSingleton<TimerManager>
    {
        // 用一颗树记录当前游戏内的所有的Timer对象

        private readonly LinkedList<Timer.Timer> m_timerLinkList = new LinkedList<Timer.Timer>();


        public void Initialization()
        {
            m_timerLinkList.Clear();
        }

        public void Update(float deltaTime)
        {
            IEnumerator<Timer.Timer> timers = m_timerLinkList.GetEnumerator();
            while (timers.MoveNext())
            {
                if (timers.Current != null)
                {
                    timers.Current.Process(deltaTime);
                }
            }
        }


        public void Release()
        {
            this.Dispose();
        }

        protected override void DisposeManagedResources()
        {
            IEnumerator<Timer.Timer> timers = m_timerLinkList.GetEnumerator();
            while (timers.MoveNext())
            {
                if (timers.Current != null)
                {
                    timers.Current.Dispose();
                }
            }
            m_timerLinkList.Clear();
        }

    }
}
