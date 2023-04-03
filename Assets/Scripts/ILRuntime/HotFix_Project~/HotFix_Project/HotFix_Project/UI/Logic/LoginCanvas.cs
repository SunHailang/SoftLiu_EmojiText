using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project.UI
{
    public class LoginCanvas : UICore.UiBase
    {
        public override void BindingGo(GameObject go)
        {
            m_curGo = go;
        }
    }
}
