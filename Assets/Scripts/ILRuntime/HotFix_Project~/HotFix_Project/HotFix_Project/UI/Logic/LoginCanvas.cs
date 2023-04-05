using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project.UI
{
    public class LoginCanvas : UICore.UiBase
    {
        private Button m_btnStart = null;

        public override void Initialization(GameObject go)
        {
            m_curGo = go;
            m_btnStart = m_curGo.transform.Find("Content/StartButton").GetComponent<Button>();

        }

        public override void OnShow(Action callback = null)
        {
            base.OnShow(callback);
        }

        public override void OnClose(Action callback = null)
        {
            base.OnClose(callback);
        }
    }
}
