using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class TimerUpdate : Timer
    {
        public TimerUpdate(string _name) : base(_name)
        {

        }


        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);
        }

    }
}
