using UnityEngine;

namespace HotFix_Project.UI.UICore
{
    public class UiBase : UiComponent
    {
        public override void BindingGo(GameObject go)
        {
            m_curGo = go;
        }
    }
}
