using UnityEngine;

namespace HotFix_Project.UI.UICore
{
    public class UiBase : UiComponent
    {
        private UnityEngine.Animation m_animation = null;

        public override void Initialization(GameObject go)
        {
            m_curGo = go;
            m_animation = m_curGo.GetComponent<Animation>();
        }


        public virtual void OnShow(System.Action callback = null)
        {

        }

        public virtual void OnClose(System.Action callback = null)
        {

        }

        public virtual void Release()
        {
            this.Dispose();
        }

    }
}
