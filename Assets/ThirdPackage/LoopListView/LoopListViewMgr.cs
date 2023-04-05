using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListViewMgr : MonoBehaviour
{
    public LoopListView m_listView = null;
    public Button m_btn = null;

    class TestItemData
    {
        public uint ID;
        public string Msg;
    }
    
    private List<TestItemData> m_testItemList = new List<TestItemData>();
    
    private void Start()
    {
        m_listView.InitListView(0, onGetItemByIndex, onCreateItem);
        
        m_btn.onClick.AddListener(Btn_OnClick);
        
    }

    private void SetListViewCount()
    {
        m_listView.SetListItemCount(m_testItemList.Count);
    }

    private uint index = 0;
    private void Btn_OnClick()
    {
        m_testItemList.Add(new TestItemData()
        {
            ID = index, 
            Msg = $"ID:{index}"
        });
        index++;
        SetListViewCount();
    }

    #region Callback

    private LoopListViewItem onGetItemByIndex(LoopListView listView, int index)
    {
        LoopListViewItem viewItem = listView.NewListViewItem("LoopListViewTestItem");

        LoopListViewTestItem item = viewItem.GetComponent<LoopListViewTestItem>();
        item.SetDesc(m_testItemList[index].Msg);
        
        return viewItem;
    }

    private void onCreateItem(string name, LoopListViewItem viewItem)
    {
        
    }

    #endregion
}
