using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoopListView : MonoBehaviour
{
    [SerializeField] private List<ItemPrefabConfData> m_itemPrefabList = new List<ItemPrefabConfData>();


    #region 私有属性

    private ScrollRect m_ScrollRect = null;
    private RectTransform m_ScrollContentRect = null;
    private RectTransform m_ScrollViewportRect = null;
    private ScrollRect.MovementType mRawMovementType = ScrollRect.MovementType.Unrestricted;

    private Dictionary<string, LoopListViewItemPool> m_ItemPoolDict = new Dictionary<string, LoopListViewItemPool>();
    #endregion
    
    private System.Func<LoopListView, int, LoopListViewItem> m_onGetItemByIndex;
    private System.Action<string, LoopListViewItem> m_onCreateItem;

    private void InitPool()
    {
        m_ItemPoolDict.Clear();
        foreach (ItemPrefabConfData data in m_itemPrefabList)
        {
            if(data.mItemPrefab == null) continue;
            string prefabName = data.mItemPrefab.name;
            if (m_ItemPoolDict.TryGetValue(prefabName, out LoopListViewItemPool itemPool) && itemPool != null)
            {
                continue;
            }
            RectTransform rtf = data.mItemPrefab.GetComponent<RectTransform>();
            if (rtf == null)
            {
                Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                continue;
            }
            LoopListViewItem tItem = data.mItemPrefab.GetComponent<LoopListViewItem>();
            if (tItem == null)
            {
                tItem = data.mItemPrefab.AddComponent<LoopListViewItem>();
            }
            
            LoopListViewItemPool pool = new LoopListViewItemPool();
            pool.Init(data.mItemPrefab, data.mPadding, data.mStartPosOffset, data.mInitCreateCount, m_ScrollContentRect, m_onCreateItem);
            m_ItemPoolDict[prefabName] = pool;
        }
    }
    

    public void InitListView(int count, System.Func<LoopListView, int, LoopListViewItem> onGetItemByIndex, Action<string, LoopListViewItem> onCreateItem)
    {
        m_ScrollRect = gameObject.GetComponent<ScrollRect>();
        if (m_ScrollRect == null)
        {
            Debug.LogError($"ListView Init Failed! ScrollRect Componet Not Found!");
            return;
        }

        m_ScrollContentRect = m_ScrollRect.content;
        m_ScrollViewportRect = m_ScrollRect.viewport;
        mRawMovementType = m_ScrollRect.movementType;

        m_onGetItemByIndex = onGetItemByIndex;
        m_onCreateItem = onCreateItem;

        InitPool();
    }

    public LoopListViewItem NewListViewItem(string prefabName)
    {

        return null;
    }
    
}

[System.Serializable]
public class ItemPrefabConfData
{
    public GameObject mItemPrefab = null;
    public float mPadding = 0;
    public int mInitCreateCount = 0;
    public float mStartPosOffset = 0;
}

