using UnityEngine;


namespace HotFix_Project.UI
{
    public class GameEntry : UICore.UiComponent
    {
        public override void BindingGo(GameObject go)
        {
            m_curGo = go;
        }
    }
}
