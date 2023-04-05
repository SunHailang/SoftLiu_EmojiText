using UnityEngine;


namespace HotFix_Project.UI
{
    public class GameEntry : UICore.UiComponent
    {
        public override void Initialization(GameObject go)
        {
            m_curGo = go;
        }
    }
}
