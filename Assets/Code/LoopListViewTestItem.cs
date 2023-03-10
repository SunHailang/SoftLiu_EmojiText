using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListViewTestItem : MonoBehaviour
{
    public Text m_descText = null;
    
    public void SetID(uint id)
    {
        
    }

    public void SetDesc(string desc)
    {
        m_descText.text = desc;
    }
}
