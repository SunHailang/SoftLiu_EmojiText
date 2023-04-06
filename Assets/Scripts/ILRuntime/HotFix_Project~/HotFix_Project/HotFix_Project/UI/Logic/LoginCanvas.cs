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
            base.Initialization(go);

            m_btnStart = m_curGo.transform.Find("Content/StartButton").GetComponent<Button>();
            m_btnStart.onClick.AddListener(BtnStart_OnClick);
        }

        public override void OnShow(Action callback = null)
        {
            base.OnShow(callback);
        }

        public override void OnClose(Action callback = null)
        {
            base.OnClose(callback);
        }
        #region Event Callback
        private void BtnStart_OnClick()
        {
            UnityEngine.Debug.Log($"[LoginCanvas] Start Click.");
            UIManager.Instance.LoadingScene("Scene_1");
            UIManager.Instance.CloseCanvasUI<LoginCanvas>();
        }

        #endregion
    }
}
