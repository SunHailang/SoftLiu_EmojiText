using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

class SnapData
{
    public SnapStatus mSnapStatus = SnapStatus.NoTargetSet;
    public int mSnapTargetIndex = 0;
    public float mTargetSnapVal = 0;
    public float mCurSnapVal = 0;
    public bool mIsForceSnapTo = false;
    public bool mIsTempTarget = false;
    public int mTempTargetIndex = -1;
    public float mMoveMaxAbsVec = -1;

    public void Clear()
    {
        mSnapStatus = SnapStatus.NoTargetSet;
        mTempTargetIndex = -1;
        mIsForceSnapTo = false;
        mMoveMaxAbsVec = -1;
    }
}

public class LoopListViewInitParam
{
    // all the default values
    public float mDistanceForRecycle0 = 300; //mDistanceForRecycle0 should be larger than mDistanceForNew0
    public float mDistanceForNew0 = 200;
    public float mDistanceForRecycle1 = 300; //mDistanceForRecycle1 should be larger than mDistanceForNew1
    public float mDistanceForNew1 = 200;
    public float mSmoothDumpRate = 0.3f;
    public float mSnapFinishThreshold = 0.01f;
    public float mSnapVecThreshold = 145;
    public float mItemDefaultWithPaddingSize = 20; //item's default size (with padding)
    public float mLastItemPaddingRight = 0;

    public static LoopListViewInitParam CopyDefaultInitParam()
    {
        return new LoopListViewInitParam();
    }
}

public class LoopListView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private List<ItemPrefabConfData> m_itemPrefabList = new List<ItemPrefabConfData>();
    [SerializeField] private ListItemArrangeType m_ArrangeType = ListItemArrangeType.TopToBottom;

    [SerializeField] private bool m_SupportScrollBar = true;

    [SerializeField] private bool m_ItemSnapEnable = false;

    [SerializeField] private Vector2 m_ViewPortSnapPivot = Vector2.zero;
    [SerializeField] private Vector2 m_ItemSnapPivot = Vector2.zero;

    #region 私有属性

    public ListItemArrangeType ArrangeType
    {
        get { return m_ArrangeType; }
        set { m_ArrangeType = value; }
    }

    private bool m_IsDraging = false;
    private bool m_IsScrollingDown = false;
    private PointerEventData m_PointerEventData = null;
    
    public System.Action<PointerEventData> m_OnBeginDragAction = null;
    public System.Action<PointerEventData> m_OnDragingAction = null;
    public System.Action<PointerEventData> m_OnEndDragAction = null;

    private RectTransform m_ContainerTrans;
    private ScrollRect m_ScrollRect = null;
    private RectTransform m_ScrollRectTransform = null;
    public RectTransform ScrollRectTransform => m_ScrollRectTransform;

    private RectTransform m_ViewPortRectTransform = null;
    private Image m_ScrollImage = null;
    private RectTransform m_ScrollContentRect = null;
    private RectTransform m_ScrollViewportRect = null;
    private ScrollRect.MovementType m_RawMovementType = ScrollRect.MovementType.Unrestricted;
    private bool m_ViewPortResizeToContentSize = false;
    private Vector2 m_ViewPortRawSize = Vector2.zero;

    private Vector3[] m_ItemWorldCorners = new Vector3[4];
    private Vector3[] m_ViewPortRectLocalCorners = new Vector3[4];
    private Vector3 m_LastFrameContainerPos = Vector3.zero;
    public System.Action<LoopListView, LoopListViewItem> m_OnSnapItemFinished = null;
    public System.Action<LoopListView, LoopListViewItem> m_OnSnapNearestChanged = null;

    private ClickEventListener m_ScrollBarClickEventListener = null;
    private SnapData m_CurSnapData = new SnapData();
    private Vector3 m_LastSnapCheckPos = Vector3.zero;
    public Action OnUpdateContentSize = null;

    private int m_CurSnapNearestItemIndex = -1;
    private Vector2 m_AdjustedVec;
    private bool m_NeedAdjustVec = false;
    private int m_LeftSnapUpdateExtraCount = 1;

    private float m_SmoothDumpVel = 0;
    private float m_SmoothDumpRate = 0.3f;
    private float m_SnapFinishThreshold = 0.1f;
    private float m_SnapVecThreshold = 145;
    private float m_SnapMoveDefaultMaxAbsVec = 3400f;
    
    private float m_ItemDefaultWithPaddingSize = 20;

    private bool m_ListViewInited = false;
    private int m_ListUpdateCheckFrameCount = 0;

    private Dictionary<string, LoopListViewItemPool> m_ItemPoolDict = new Dictionary<string, LoopListViewItemPool>();
    private List<LoopListViewItemPool> m_ItemPoolList = new List<LoopListViewItemPool>();


    private List<LoopListViewItem> m_ItemList = new List<LoopListViewItem>();

    private int m_ItemTotalCount = 0;
    private int m_CurReadyMinItemIndex = 0;
    private int m_CurReadyMaxItemIndex = 0;
    private bool m_NeedCheckNextMinItem = true;
    private bool m_NeedCheckNextMaxItem = true;
    private ItemPosMgr m_ItemPosMgr = null;
    private float m_DistanceForRecycle0 = 300;
    private float m_DistanceForNew0 = 200;
    private float m_DistanceForRecycle1 = 300;
    private float m_DistanceForNew1 = 200;
    private int m_LastItemIndex = 0;

    private float m_LastItemPadding = 0;
    private float m_LastItemPaddingRight = 0;

    private bool m_IsVertList = false;
    public bool IsVertList => m_IsVertList;

    public float ViewPortSize => m_IsVertList ? ViewPortHeight : ViewPortWidth;
    public float ViewPortWidth => m_ViewPortRectTransform.rect.width;
    public float ViewPortHeight => m_ViewPortRectTransform.rect.height;

    #endregion

    private System.Func<LoopListView, int, LoopListViewItem> m_onGetItemByIndex;
    private System.Action<string, LoopListViewItem> m_onCreateItem;

    private Action<LoopListView> onLoopListViewAnimation;

    private void InitItemPool()
    {
        m_ItemPoolDict.Clear();
        foreach (ItemPrefabConfData data in m_itemPrefabList)
        {
            if (data.mItemPrefab == null) continue;
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

    void AdjustPivot(RectTransform rtf)
    {
        Vector2 pivot = rtf.pivot;

        if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            pivot.y = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            pivot.y = 1;
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            pivot.x = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            pivot.x = 1;
        }

        rtf.pivot = pivot;
    }

    void AdjustAnchor(RectTransform rtf)
    {
        Vector2 anchorMin = rtf.anchorMin;
        Vector2 anchorMax = rtf.anchorMax;
        if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            anchorMin.y = 0;
            anchorMax.y = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            anchorMin.y = 1;
            anchorMax.y = 1;
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            anchorMin.x = 0;
            anchorMax.x = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            anchorMin.x = 1;
            anchorMax.x = 1;
        }

        rtf.anchorMin = anchorMin;
        rtf.anchorMax = anchorMax;
    }

    void AdjustContainerPivot(RectTransform rtf)
    {
        Vector2 pivot = rtf.pivot;
        if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            pivot.y = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            pivot.y = 1;
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            pivot.x = 0;
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            pivot.x = 1;
        }

        rtf.pivot = pivot;
    }

    public void ResetListView(bool resetPos = true)
    {
        m_ViewPortRectTransform.GetLocalCorners(m_ViewPortRectLocalCorners);
        if (resetPos)
        {
            m_ContainerTrans.anchoredPosition3D = Vector3.zero;
        }

        ForceSnapUpdateCheck();
    }

    public void InitListView(int count, System.Func<LoopListView, int, LoopListViewItem> onGetItemByIndex, Action<string, LoopListViewItem> onCreateItem,
        LoopListViewInitParam initParam = null,
        Action<LoopListView> animationAction = null)
    {
        if (initParam != null)
        {
            m_DistanceForRecycle0 = initParam.mDistanceForRecycle0;
            m_DistanceForNew0 = initParam.mDistanceForNew0;
            m_DistanceForRecycle1 = initParam.mDistanceForRecycle1;
            m_DistanceForNew1 = initParam.mDistanceForNew1;
            m_SmoothDumpRate = initParam.mSmoothDumpRate;
            m_SnapFinishThreshold = initParam.mSnapFinishThreshold;
            m_SnapVecThreshold = initParam.mSnapVecThreshold;
            m_ItemDefaultWithPaddingSize = initParam.mItemDefaultWithPaddingSize;
            m_LastItemPaddingRight = initParam.mLastItemPaddingRight;
        }

        m_ScrollRect = gameObject.GetComponent<ScrollRect>();
        if (m_ScrollRect == null)
        {
            Debug.LogError("ListView Init Failed! ScrollRect component not found!");
            return;
        }

        if (m_DistanceForRecycle0 <= m_DistanceForNew0)
        {
            Debug.LogError("mDistanceForRecycle0 should be bigger than mDistanceForNew0");
        }

        if (m_DistanceForRecycle1 <= m_DistanceForNew1)
        {
            Debug.LogError("mDistanceForRecycle1 should be bigger than mDistanceForNew1");
        }

        m_CurSnapData.Clear();
        m_ItemPosMgr = new ItemPosMgr(m_ItemDefaultWithPaddingSize);
        m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();
        m_ContainerTrans = m_ScrollRect.content;
        m_ViewPortRectTransform = m_ScrollRect.viewport;
        if (m_ViewPortRectTransform == null)
        {
            m_ViewPortRectTransform = m_ScrollRectTransform;
        }

        if (m_ScrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && m_ScrollRect.horizontalScrollbar != null)
        {
            Debug.LogError("ScrollRect.horizontalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
        }

        if (m_ScrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && m_ScrollRect.verticalScrollbar != null)
        {
            Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
        }

        m_ScrollContentRect = m_ScrollRect.content;
        m_ScrollViewportRect = m_ScrollRect.viewport;
        m_RawMovementType = m_ScrollRect.movementType;

        m_IsVertList = (m_ArrangeType == ListItemArrangeType.TopToBottom || m_ArrangeType == ListItemArrangeType.BottomToTop);
        m_ScrollRect.horizontal = !m_IsVertList;
        m_ScrollRect.vertical = m_IsVertList;
        SetScrollbarListener();
        // AdjustPivot(m_ViewPortRectTransform);
        // AdjustAnchor(m_ContainerTrans);
        // AdjustContainerPivot(m_ContainerTrans);

        m_onCreateItem = onCreateItem;

        InitItemPool();
        m_onGetItemByIndex = onGetItemByIndex;

        // REXFIX:By Sun.Lx, 17D12M2020Y
        // tableViewCell动画
        onLoopListViewAnimation = animationAction;

        if (m_ListViewInited == true)
        {
            Debug.LogError("LoopListView2.InitListView method can be called only once.");
        }

        m_ListViewInited = true;
        ResetListView();
        //SetListItemCount(itemTotalCount, true);
        m_CurSnapData.Clear();
        m_ItemTotalCount = count;
        if (m_ItemTotalCount < 0)
        {
            m_SupportScrollBar = false;
        }

        if (m_SupportScrollBar)
        {
            m_ItemPosMgr.SetItemMaxCount(m_ItemTotalCount);
        }
        else
        {
            m_ItemPosMgr.SetItemMaxCount(0);
        }

        m_CurReadyMaxItemIndex = 0;
        m_CurReadyMinItemIndex = 0;
        m_LeftSnapUpdateExtraCount = 1;
        m_NeedCheckNextMaxItem = true;
        m_NeedCheckNextMinItem = true;
        UpdateContentSize();
    }

    private void SetScrollbarListener()
    {
        m_ScrollBarClickEventListener = null;
        Scrollbar curScrollBar = null;
        if (m_IsVertList && m_ScrollRect.verticalScrollbar != null)
        {
            curScrollBar = m_ScrollRect.verticalScrollbar;
        }

        if (!m_IsVertList && m_ScrollRect.horizontalScrollbar != null)
        {
            curScrollBar = m_ScrollRect.horizontalScrollbar;
        }

        if (curScrollBar == null)
        {
            return;
        }

        ClickEventListener listener = ClickEventListener.Get(curScrollBar.gameObject);
        m_ScrollBarClickEventListener = listener;
        listener.SetPointerUpHandler(OnPointerUpInScrollBar);
        listener.SetPointerDownHandler(OnPointerDownInScrollBar);
    }

    void OnPointerDownInScrollBar(GameObject obj)
    {
        m_CurSnapData.Clear();
    }

    void OnPointerUpInScrollBar(GameObject obj)
    {
        ForceSnapUpdateCheck();
    }

    public LoopListViewItem NewListViewItem(string itemPrefabName)
    {
        if (m_ItemPoolDict.TryGetValue(itemPrefabName, out LoopListViewItemPool pool) == false)
        {
            return null;
        }

        LoopListViewItem item = pool.GetItem();
        RectTransform rf = item.GetComponent<RectTransform>();
        rf.SetParent(m_ContainerTrans);
        rf.localScale = Vector3.one;
        rf.anchoredPosition3D = Vector3.zero;
        rf.localEulerAngles = Vector3.zero;
        item.ParentListView = this;
        return item;
    }

    public void SetListItemCount(int itemCount, bool resetPos = false)
    {
        m_ItemTotalCount = itemCount;
        if (m_ItemTotalCount < 0)
        {
            m_SupportScrollBar = false;
        }

        if (m_SupportScrollBar)
        {
            m_ItemPosMgr.SetItemMaxCount(m_ItemTotalCount);
        }
        else
        {
            m_ItemPosMgr.SetItemMaxCount(0);
        }

        if (m_ItemTotalCount == 0)
        {
            m_CurReadyMaxItemIndex = 0;
            m_CurReadyMinItemIndex = 0;
            m_NeedCheckNextMaxItem = false;
            m_NeedCheckNextMinItem = false;
            RecycleAllItem();
            ClearAllTmpRecycledItem();
            UpdateListViewBegin(true);
            UpdateContentSize();
            return;
        }

        if (m_CurReadyMaxItemIndex >= m_ItemTotalCount)
        {
            m_CurReadyMaxItemIndex = m_ItemTotalCount - 1;
        }

        m_LeftSnapUpdateExtraCount = 1;
        m_NeedCheckNextMaxItem = true;
        m_NeedCheckNextMinItem = true;
        if (resetPos)
        {
            MovePanelToItemIndex(0, 0);
            return;
        }

        if (m_ItemList.Count == 0)
        {
            MovePanelToItemIndex(0, 0);
            return;
        }

        int maxItemIndex = m_ItemTotalCount - 1;
        int lastItemIndex = m_ItemList[m_ItemList.Count - 1].ItemIndex;
        if (lastItemIndex <= maxItemIndex)
        {
            UpdateListViewBegin(true);
            UpdateContentSize();
            UpdateAllShownItemsPos();
            return;
        }

        MovePanelToItemIndex(maxItemIndex, 0);
    }

    private LoopListViewItem GetNewItemByIndex(int index)
    {
        if (m_SupportScrollBar && index < 0)
        {
            return null;
        }

        if (m_ItemTotalCount > 0 && index >= m_ItemTotalCount)
        {
            return null;
        }

        LoopListViewItem newItem = m_onGetItemByIndex(this, index);
        if (newItem == null)
        {
            return null;
        }

        newItem.ItemIndex = index;
        newItem.ItemCreatedCheckFrameCount = m_ListUpdateCheckFrameCount;
        return newItem;
    }

    public void MovePanelToItemIndex(int itemIndex, float offset)
    {
        m_ScrollRect.StopMovement();
        m_CurSnapData.Clear();
        if (m_ItemTotalCount == 0)
        {
            return;
        }

        if (itemIndex < 0 && m_ItemTotalCount > 0)
        {
            return;
        }

        if (m_ItemTotalCount > 0 && itemIndex >= m_ItemTotalCount)
        {
            itemIndex = m_ItemTotalCount - 1;
        }

        if (offset < 0)
        {
            offset = 0;
        }

        Vector3 pos = Vector3.zero;
        float viewPortSize = ViewPortSize;
        if (offset > viewPortSize)
        {
            offset = viewPortSize;
        }

        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            float containerPos = m_ContainerTrans.anchoredPosition3D.y;
            if (containerPos < 0)
            {
                containerPos = 0;
            }

            pos.y = -containerPos - offset;
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            float containerPos = m_ContainerTrans.anchoredPosition3D.y;
            if (containerPos > 0)
            {
                containerPos = 0;
            }

            pos.y = -containerPos + offset;
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            float containerPos = m_ContainerTrans.anchoredPosition3D.x;
            if (containerPos > 0)
            {
                containerPos = 0;
            }

            pos.x = -containerPos + offset;
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            float containerPos = m_ContainerTrans.anchoredPosition3D.x;
            if (containerPos < 0)
            {
                containerPos = 0;
            }

            pos.x = -containerPos - offset;
        }

        RecycleAllItem();
        LoopListViewItem newItem = GetNewItemByIndex(itemIndex);
        if (newItem == null)
        {
            ClearAllTmpRecycledItem();
            return;
        }

        if (m_IsVertList)
        {
            pos.x = newItem.StartPosOffset;
        }
        else
        {
            pos.y = newItem.StartPosOffset;
        }

        newItem.CachedRectTransform.anchoredPosition3D = pos;
        if (m_SupportScrollBar)
        {
            if (m_IsVertList)
            {
                SetItemSize(itemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
            }
            else
            {
                SetItemSize(itemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
            }
        }

        m_ItemList.Add(newItem);
        UpdateContentSize();
        UpdateListView(viewPortSize + 100, viewPortSize + 100, viewPortSize, viewPortSize);
        AdjustPanelPos();
        ClearAllTmpRecycledItem();
        ForceSnapUpdateCheck();
        UpdateSnapMove(false, true);
        onLoopListViewAnimation?.Invoke(this);
    }

    void UpdateSnapMove(bool immediate = false, bool forceSendEvent = false)
    {
        if (m_ItemSnapEnable == false)
        {
            return;
        }

        if (m_IsVertList)
        {
            UpdateSnapVertical(immediate, forceSendEvent);
        }
        else
        {
            UpdateSnapHorizontal(immediate, forceSendEvent);
        }
    }

    void UpdateSnapHorizontal(bool immediate = false, bool forceSendEvent = false)
    {
        if (m_ItemSnapEnable == false)
        {
            return;
        }

        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        Vector3 pos = m_ContainerTrans.anchoredPosition3D;
        bool needCheck = (pos.x != m_LastSnapCheckPos.x);
        m_LastSnapCheckPos = pos;
        if (!needCheck)
        {
            if (m_LeftSnapUpdateExtraCount > 0)
            {
                m_LeftSnapUpdateExtraCount--;
                needCheck = true;
            }
        }

        if (needCheck)
        {
            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            int curIndex = -1;
            float start = 0;
            float end = 0;
            float itemSnapCenter = 0;
            float curMinDist = float.MaxValue;
            float curDist = 0;
            float curDistAbs = 0;
            float snapCenter = 0;
            if (m_ArrangeType == ListItemArrangeType.RightToLeft)
            {
                snapCenter = -(1 - m_ViewPortSnapPivot.x) * m_ViewPortRectTransform.rect.width;
                Vector3 rightPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
                start = rightPos1.x;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - m_ItemSnapPivot.x);
                for (int i = 0; i < count; ++i)
                {
                    curDist = snapCenter - itemSnapCenter;
                    curDistAbs = Mathf.Abs(curDist);
                    if (curDistAbs < curMinDist)
                    {
                        curMinDist = curDistAbs;
                        curIndex = i;
                    }
                    else
                    {
                        break;
                    }

                    if ((i + 1) < count)
                    {
                        start = end;
                        end -= m_ItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - m_ItemList[i + 1].ItemSize * (1 - m_ItemSnapPivot.x);
                    }
                }
            }
            else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
            {
                snapCenter = m_ViewPortSnapPivot.x * m_ViewPortRectTransform.rect.width;
                Vector3 leftPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
                start = leftPos1.x;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * m_ItemSnapPivot.x;
                for (int i = 0; i < count; ++i)
                {
                    curDist = snapCenter - itemSnapCenter;
                    curDistAbs = Mathf.Abs(curDist);
                    if (curDistAbs < curMinDist)
                    {
                        curMinDist = curDistAbs;
                        curIndex = i;
                    }
                    else
                    {
                        break;
                    }

                    if ((i + 1) < count)
                    {
                        start = end;
                        end += m_ItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + m_ItemList[i + 1].ItemSize * m_ItemSnapPivot.x;
                    }
                }
            }


            if (curIndex >= 0)
            {
                int oldNearestItemIndex = m_CurSnapNearestItemIndex;
                m_CurSnapNearestItemIndex = m_ItemList[curIndex].ItemIndex;
                if (forceSendEvent || m_ItemList[curIndex].ItemIndex != oldNearestItemIndex)
                {
                    if (m_OnSnapNearestChanged != null)
                    {
                        m_OnSnapNearestChanged(this, m_ItemList[curIndex]);
                    }
                }
            }
            else
            {
                m_CurSnapNearestItemIndex = -1;
            }
        }

        if (CanSnap() == false)
        {
            ClearSnapData();
            return;
        }

        float v = Mathf.Abs(m_ScrollRect.velocity.x);
        UpdateCurSnapData();
        if (m_CurSnapData.mSnapStatus != SnapStatus.SnapMoving)
        {
            return;
        }

        if (v > 0)
        {
            m_ScrollRect.StopMovement();
        }

        float old = m_CurSnapData.mCurSnapVal;
        if (m_CurSnapData.mIsTempTarget == false)
        {
            if (m_SmoothDumpVel * m_CurSnapData.mTargetSnapVal < 0)
            {
                m_SmoothDumpVel = 0;
            }

            m_CurSnapData.mCurSnapVal = Mathf.SmoothDamp(m_CurSnapData.mCurSnapVal, m_CurSnapData.mTargetSnapVal, ref m_SmoothDumpVel, m_SmoothDumpRate);
        }
        else
        {
            float maxAbsVec = m_CurSnapData.mMoveMaxAbsVec;
            if (maxAbsVec <= 0)
            {
                maxAbsVec = m_SnapMoveDefaultMaxAbsVec;
            }

            m_SmoothDumpVel = maxAbsVec * Mathf.Sign(m_CurSnapData.mTargetSnapVal);
            m_CurSnapData.mCurSnapVal = Mathf.MoveTowards(m_CurSnapData.mCurSnapVal, m_CurSnapData.mTargetSnapVal, maxAbsVec * Time.deltaTime);
        }

        float dt = m_CurSnapData.mCurSnapVal - old;

        if (immediate || Mathf.Abs(m_CurSnapData.mTargetSnapVal - m_CurSnapData.mCurSnapVal) < m_SnapFinishThreshold)
        {
            pos.x = pos.x + m_CurSnapData.mTargetSnapVal - old;
            m_CurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
            if (m_OnSnapItemFinished != null)
            {
                LoopListViewItem targetItem = GetShownItemByItemIndex(m_CurSnapNearestItemIndex);
                if (targetItem != null)
                {
                    m_OnSnapItemFinished(this, targetItem);
                }
            }
        }
        else
        {
            pos.x = pos.x + dt;
        }

        if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            float minX = m_ViewPortRectLocalCorners[2].x - m_ContainerTrans.rect.width;
            pos.x = Mathf.Clamp(pos.x, minX, 0);
            m_ContainerTrans.anchoredPosition3D = pos;
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            float maxX = m_ViewPortRectLocalCorners[1].x + m_ContainerTrans.rect.width;
            pos.x = Mathf.Clamp(pos.x, 0, maxX);
            m_ContainerTrans.anchoredPosition3D = pos;
        }
    }

    public LoopListViewItem GetShownItemByItemIndex(int itemIndex)
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            return null;
        }

        if (itemIndex < m_ItemList[0].ItemIndex || itemIndex > m_ItemList[count - 1].ItemIndex)
        {
            return null;
        }

        int i = itemIndex - m_ItemList[0].ItemIndex;
        return m_ItemList[i];
    }

    void UpdateCurSnapData()
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            m_CurSnapData.Clear();
            return;
        }

        if (m_CurSnapData.mSnapStatus == SnapStatus.SnapMoveFinish)
        {
            if (m_CurSnapData.mSnapTargetIndex == m_CurSnapNearestItemIndex)
            {
                return;
            }

            m_CurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
        }

        if (m_CurSnapData.mSnapStatus == SnapStatus.SnapMoving)
        {
            if (m_CurSnapData.mIsForceSnapTo)
            {
                if (m_CurSnapData.mIsTempTarget == true)
                {
                    LoopListViewItem targetItem = GetShownItemNearestItemIndex(m_CurSnapData.mSnapTargetIndex);
                    if (targetItem == null)
                    {
                        m_CurSnapData.Clear();
                        return;
                    }

                    if (targetItem.ItemIndex == m_CurSnapData.mSnapTargetIndex)
                    {
                        UpdateAllShownItemSnapData();
                        m_CurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                        m_CurSnapData.mCurSnapVal = 0;
                        m_CurSnapData.mIsTempTarget = false;
                        m_CurSnapData.mSnapStatus = SnapStatus.SnapMoving;
                        return;
                    }

                    if (m_CurSnapData.mTempTargetIndex != targetItem.ItemIndex)
                    {
                        UpdateAllShownItemSnapData();
                        m_CurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                        m_CurSnapData.mCurSnapVal = 0;
                        m_CurSnapData.mSnapStatus = SnapStatus.SnapMoving;
                        m_CurSnapData.mIsTempTarget = true;
                        m_CurSnapData.mTempTargetIndex = targetItem.ItemIndex;
                        return;
                    }
                }

                return;
            }

            if ((m_CurSnapData.mSnapTargetIndex == m_CurSnapNearestItemIndex))
            {
                return;
            }

            m_CurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
        }

        if (m_CurSnapData.mSnapStatus == SnapStatus.NoTargetSet)
        {
            LoopListViewItem nearestItem = GetShownItemByItemIndex(m_CurSnapNearestItemIndex);
            if (nearestItem == null)
            {
                return;
            }

            m_CurSnapData.mSnapTargetIndex = m_CurSnapNearestItemIndex;
            m_CurSnapData.mSnapStatus = SnapStatus.TargetHasSet;
            m_CurSnapData.mIsForceSnapTo = false;
        }

        if (m_CurSnapData.mSnapStatus == SnapStatus.TargetHasSet)
        {
            LoopListViewItem targetItem = GetShownItemNearestItemIndex(m_CurSnapData.mSnapTargetIndex);
            if (targetItem == null)
            {
                m_CurSnapData.Clear();
                return;
            }

            if (targetItem.ItemIndex == m_CurSnapData.mSnapTargetIndex)
            {
                UpdateAllShownItemSnapData();
                m_CurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                m_CurSnapData.mCurSnapVal = 0;
                m_CurSnapData.mIsTempTarget = false;
                m_CurSnapData.mSnapStatus = SnapStatus.SnapMoving;
            }
            else
            {
                UpdateAllShownItemSnapData();
                m_CurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                m_CurSnapData.mCurSnapVal = 0;
                m_CurSnapData.mSnapStatus = SnapStatus.SnapMoving;
                m_CurSnapData.mIsTempTarget = true;
                m_CurSnapData.mTempTargetIndex = targetItem.ItemIndex;
            }
        }
    }

    public void UpdateAllShownItemSnapData()
    {
        if (m_ItemSnapEnable == false)
        {
            return;
        }

        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        Vector3 pos = m_ContainerTrans.anchoredPosition3D;
        LoopListViewItem tViewItem0 = m_ItemList[0];
        tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
        float start = 0;
        float end = 0;
        float itemSnapCenter = 0;
        float snapCenter = 0;
        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            snapCenter = -(1 - m_ViewPortSnapPivot.y) * m_ViewPortRectTransform.rect.height;
            Vector3 topPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            start = topPos1.y;
            end = start - tViewItem0.ItemSizeWithPadding;
            itemSnapCenter = start - tViewItem0.ItemSize * (1 - m_ItemSnapPivot.y);
            for (int i = 0; i < count; ++i)
            {
                m_ItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                if ((i + 1) < count)
                {
                    start = end;
                    end = end - m_ItemList[i + 1].ItemSizeWithPadding;
                    itemSnapCenter = start - m_ItemList[i + 1].ItemSize * (1 - m_ItemSnapPivot.y);
                }
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            snapCenter = m_ViewPortSnapPivot.y * m_ViewPortRectTransform.rect.height;
            Vector3 bottomPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
            start = bottomPos1.y;
            end = start + tViewItem0.ItemSizeWithPadding;
            itemSnapCenter = start + tViewItem0.ItemSize * m_ItemSnapPivot.y;
            for (int i = 0; i < count; ++i)
            {
                m_ItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                if ((i + 1) < count)
                {
                    start = end;
                    end = end + m_ItemList[i + 1].ItemSizeWithPadding;
                    itemSnapCenter = start + m_ItemList[i + 1].ItemSize * m_ItemSnapPivot.y;
                }
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            snapCenter = -(1 - m_ViewPortSnapPivot.x) * m_ViewPortRectTransform.rect.width;
            Vector3 rightPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
            start = rightPos1.x;
            end = start - tViewItem0.ItemSizeWithPadding;
            itemSnapCenter = start - tViewItem0.ItemSize * (1 - m_ItemSnapPivot.x);
            for (int i = 0; i < count; ++i)
            {
                m_ItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                if ((i + 1) < count)
                {
                    start = end;
                    end = end - m_ItemList[i + 1].ItemSizeWithPadding;
                    itemSnapCenter = start - m_ItemList[i + 1].ItemSize * (1 - m_ItemSnapPivot.x);
                }
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            snapCenter = m_ViewPortSnapPivot.x * m_ViewPortRectTransform.rect.width;
            Vector3 leftPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            start = leftPos1.x;
            end = start + tViewItem0.ItemSizeWithPadding;
            itemSnapCenter = start + tViewItem0.ItemSize * m_ItemSnapPivot.x;
            for (int i = 0; i < count; ++i)
            {
                m_ItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                if ((i + 1) < count)
                {
                    start = end;
                    end = end + m_ItemList[i + 1].ItemSizeWithPadding;
                    itemSnapCenter = start + m_ItemList[i + 1].ItemSize * m_ItemSnapPivot.x;
                }
            }
        }
    }


    public LoopListViewItem GetShownItemNearestItemIndex(int itemIndex)
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            return null;
        }

        if (itemIndex < m_ItemList[0].ItemIndex)
        {
            return m_ItemList[0];
        }

        if (itemIndex > m_ItemList[count - 1].ItemIndex)
        {
            return m_ItemList[count - 1];
        }

        int i = itemIndex - m_ItemList[0].ItemIndex;
        return m_ItemList[i];
    }

    public void ClearSnapData()
    {
        m_CurSnapData.Clear();
    }

    public bool CanSnap()
    {
        if (m_IsDraging)
        {
            return false;
        }

        if (m_ScrollBarClickEventListener != null)
        {
            if (m_ScrollBarClickEventListener.IsPressd)
            {
                return false;
            }
        }

        if (m_IsVertList)
        {
            if (m_ContainerTrans.rect.height <= ViewPortHeight)
            {
                return false;
            }
        }
        else
        {
            if (m_ContainerTrans.rect.width <= ViewPortWidth)
            {
                return false;
            }
        }

        float v = 0;
        if (m_IsVertList)
        {
            v = Mathf.Abs(m_ScrollRect.velocity.y);
        }
        else
        {
            v = Mathf.Abs(m_ScrollRect.velocity.x);
        }

        if (v > m_SnapVecThreshold)
        {
            return false;
        }

        float diff = 3;
        Vector3 pos = m_ContainerTrans.anchoredPosition3D;
        if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            float minX = m_ViewPortRectLocalCorners[2].x - m_ContainerTrans.rect.width;
            if (pos.x < (minX - diff) || pos.x > diff)
            {
                return false;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            float maxX = m_ViewPortRectLocalCorners[1].x + m_ContainerTrans.rect.width;
            if (pos.x > (maxX + diff) || pos.x < -diff)
            {
                return false;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            float maxY = m_ViewPortRectLocalCorners[0].y + m_ContainerTrans.rect.height;
            if (pos.y > (maxY + diff) || pos.y < -diff)
            {
                return false;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            float minY = m_ViewPortRectLocalCorners[1].y - m_ContainerTrans.rect.height;
            if (pos.y < (minY - diff) || pos.y > diff)
            {
                return false;
            }
        }

        return true;
    }

    void UpdateSnapVertical(bool immediate = false, bool forceSendEvent = false)
    {
        if (m_ItemSnapEnable == false)
        {
            return;
        }

        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        Vector3 pos = m_ContainerTrans.anchoredPosition3D;
        bool needCheck = (pos.y != m_LastSnapCheckPos.y);
        m_LastSnapCheckPos = pos;
        if (!needCheck)
        {
            if (m_LeftSnapUpdateExtraCount > 0)
            {
                m_LeftSnapUpdateExtraCount--;
                needCheck = true;
            }
        }

        if (needCheck)
        {
            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            int curIndex = -1;
            float start = 0;
            float end = 0;
            float itemSnapCenter = 0;
            float curMinDist = float.MaxValue;
            float curDist = 0;
            float curDistAbs = 0;
            float snapCenter = 0;
            if (m_ArrangeType == ListItemArrangeType.TopToBottom)
            {
                snapCenter = -(1 - m_ViewPortSnapPivot.y) * m_ViewPortRectTransform.rect.height;
                Vector3 topPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
                start = topPos1.y;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - m_ItemSnapPivot.y);
                for (int i = 0; i < count; ++i)
                {
                    curDist = snapCenter - itemSnapCenter;
                    curDistAbs = Mathf.Abs(curDist);
                    if (curDistAbs < curMinDist)
                    {
                        curMinDist = curDistAbs;
                        curIndex = i;
                    }
                    else
                    {
                        break;
                    }

                    if ((i + 1) < count)
                    {
                        start = end;
                        end -= m_ItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - m_ItemList[i + 1].ItemSize * (1 - m_ItemSnapPivot.y);
                    }
                }
            }
            else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
            {
                snapCenter = m_ViewPortSnapPivot.y * m_ViewPortRectTransform.rect.height;
                Vector3 bottomPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
                start = bottomPos1.y;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * m_ItemSnapPivot.y;
                for (int i = 0; i < count; ++i)
                {
                    curDist = snapCenter - itemSnapCenter;
                    curDistAbs = Mathf.Abs(curDist);
                    if (curDistAbs < curMinDist)
                    {
                        curMinDist = curDistAbs;
                        curIndex = i;
                    }
                    else
                    {
                        break;
                    }

                    if ((i + 1) < count)
                    {
                        start = end;
                        end += m_ItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + m_ItemList[i + 1].ItemSize * m_ItemSnapPivot.y;
                    }
                }
            }

            if (curIndex >= 0)
            {
                int oldNearestItemIndex = m_CurSnapNearestItemIndex;
                m_CurSnapNearestItemIndex = m_ItemList[curIndex].ItemIndex;
                if (forceSendEvent || m_ItemList[curIndex].ItemIndex != oldNearestItemIndex)
                {
                    if (m_OnSnapNearestChanged != null)
                    {
                        m_OnSnapNearestChanged(this, m_ItemList[curIndex]);
                    }
                }
            }
            else
            {
                m_CurSnapNearestItemIndex = -1;
            }
        }

        if (CanSnap() == false)
        {
            ClearSnapData();
            return;
        }

        float v = Mathf.Abs(m_ScrollRect.velocity.y);
        UpdateCurSnapData();
        if (m_CurSnapData.mSnapStatus != SnapStatus.SnapMoving)
        {
            return;
        }

        if (v > 0)
        {
            m_ScrollRect.StopMovement();
        }

        float old = m_CurSnapData.mCurSnapVal;
        if (m_CurSnapData.mIsTempTarget == false)
        {
            if (m_SmoothDumpVel * m_CurSnapData.mTargetSnapVal < 0)
            {
                m_SmoothDumpVel = 0;
            }

            m_CurSnapData.mCurSnapVal = Mathf.SmoothDamp(m_CurSnapData.mCurSnapVal, m_CurSnapData.mTargetSnapVal, ref m_SmoothDumpVel, m_SmoothDumpRate);
        }
        else
        {
            float maxAbsVec = m_CurSnapData.mMoveMaxAbsVec;
            if (maxAbsVec <= 0)
            {
                maxAbsVec = m_SnapMoveDefaultMaxAbsVec;
            }

            m_SmoothDumpVel = maxAbsVec * Mathf.Sign(m_CurSnapData.mTargetSnapVal);
            m_CurSnapData.mCurSnapVal = Mathf.MoveTowards(m_CurSnapData.mCurSnapVal, m_CurSnapData.mTargetSnapVal, maxAbsVec * Time.deltaTime);
        }

        float dt = m_CurSnapData.mCurSnapVal - old;

        if (immediate || Mathf.Abs(m_CurSnapData.mTargetSnapVal - m_CurSnapData.mCurSnapVal) < m_SnapFinishThreshold)
        {
            pos.y = pos.y + m_CurSnapData.mTargetSnapVal - old;
            m_CurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
            if (m_OnSnapItemFinished != null)
            {
                LoopListViewItem targetItem = GetShownItemByItemIndex(m_CurSnapNearestItemIndex);
                if (targetItem != null)
                {
                    m_OnSnapItemFinished(this, targetItem);
                }
            }
        }
        else
        {
            pos.y = pos.y + dt;
        }

        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            float maxY = m_ViewPortRectLocalCorners[0].y + m_ContainerTrans.rect.height;
            pos.y = Mathf.Clamp(pos.y, 0, maxY);
            m_ContainerTrans.anchoredPosition3D = pos;
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            float minY = m_ViewPortRectLocalCorners[1].y - m_ContainerTrans.rect.height;
            pos.y = Mathf.Clamp(pos.y, minY, 0);
            m_ContainerTrans.anchoredPosition3D = pos;
        }
    }


    public void ForceSnapUpdateCheck()
    {
        if (m_LeftSnapUpdateExtraCount <= 0)
        {
            m_LeftSnapUpdateExtraCount = 1;
        }
    }

    private void AdjustPanelPos()
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        UpdateAllShownItemsPos();
        float viewPortSize = ViewPortSize;
        float contentSize = GetContentPanelSize();
        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            if (contentSize <= viewPortSize)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.y = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(m_ItemList[0].StartPosOffset, 0, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            if (topPos0.y < m_ViewPortRectLocalCorners[1].y)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.y = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(m_ItemList[0].StartPosOffset, 0, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 downPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
            float d = downPos1.y - m_ViewPortRectLocalCorners[0].y;
            if (d > 0)
            {
                Vector3 pos = m_ItemList[0].CachedRectTransform.anchoredPosition3D;
                pos.y = pos.y - d;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            if (contentSize <= viewPortSize)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.y = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(m_ItemList[0].StartPosOffset, 0, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 downPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
            if (downPos0.y > m_ViewPortRectLocalCorners[0].y)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.y = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(m_ItemList[0].StartPosOffset, 0, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            float d = m_ViewPortRectLocalCorners[1].y - topPos1.y;
            if (d > 0)
            {
                Vector3 pos = m_ItemList[0].CachedRectTransform.anchoredPosition3D;
                pos.y = pos.y + d;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            if (contentSize <= viewPortSize)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.x = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, m_ItemList[0].StartPosOffset, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            if (leftPos0.x > m_ViewPortRectLocalCorners[1].x)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.x = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, m_ItemList[0].StartPosOffset, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 rightPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
            float d = m_ViewPortRectLocalCorners[2].x - rightPos1.x;
            if (d > 0)
            {
                Vector3 pos = m_ItemList[0].CachedRectTransform.anchoredPosition3D;
                pos.x = pos.x + d;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            if (contentSize <= viewPortSize)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.x = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, m_ItemList[0].StartPosOffset, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 rightPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
            if (rightPos0.x < m_ViewPortRectLocalCorners[2].x)
            {
                Vector3 pos = m_ContainerTrans.anchoredPosition3D;
                pos.x = 0;
                m_ContainerTrans.anchoredPosition3D = pos;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, m_ItemList[0].StartPosOffset, 0);
                UpdateAllShownItemsPos();
                return;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            float d = leftPos1.x - m_ViewPortRectLocalCorners[1].x;
            if (d > 0)
            {
                Vector3 pos = m_ItemList[0].CachedRectTransform.anchoredPosition3D;
                pos.x = pos.x - d;
                m_ItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                UpdateAllShownItemsPos();
                return;
            }
        }
    }

    public void UpdateListView(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
    {
        m_ListUpdateCheckFrameCount++;
        bool isNeedCheck = false;
        bool isFirstCheck = true;
        if (m_IsVertList)
        {
            bool needContinueCheck = true;
            int checkCount = 0;
            int maxCount = 9999;
            while (needContinueCheck)
            {
                checkCount++;
                if (checkCount >= maxCount)
                {
                    //Debug.LogError("UpdateListView Vertical while loop " + checkCount + " times! something is wrong!");
                    Debug.LogError($"UpdateListView Vertical while loop {checkCount} times! something is wrong!");
                    break;
                }

                needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1, isFirstCheck);
                isFirstCheck = false;
                if (!isNeedCheck && needContinueCheck)
                {
                    isNeedCheck = true;
                }
            }

            if (isNeedCheck)
            {
                UpdateListViewEnd();
            }
        }
        else
        {
            bool needContinueCheck = true;
            int checkCount = 0;
            int maxCount = 9999;
            while (needContinueCheck)
            {
                checkCount++;
                if (checkCount >= maxCount)
                {
                    //Debug.LogError("UpdateListView  Horizontal while loop " + checkCount + " times! something is wrong!");
                    Debug.LogError($"UpdateListView  Horizontal while loop {checkCount} times! something is wrong!");
                    break;
                }

                needContinueCheck = UpdateForHorizontalList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1, isFirstCheck);
                isFirstCheck = false;
                if (!isNeedCheck && needContinueCheck)
                {
                    isNeedCheck = true;
                }
            }

            if (isNeedCheck)
            {
                UpdateListViewEnd();
            }
        }
    }

    bool UpdateForVertList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1, bool isFirstCheck)
    {
        if (m_ItemTotalCount == 0)
        {
            if (m_ItemList.Count > 0)
            {
                RecycleAllItem();
            }

            return false;
        }

        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            int itemListCount = m_ItemList.Count;
            if (itemListCount == 0)
            {
                float curY = m_ContainerTrans.anchoredPosition3D.y;
                if (curY < 0)
                {
                    curY = 0;
                }

                int index = 0;
                float pos = -curY;
                if (m_SupportScrollBar)
                {
                    if (GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos) == false)
                    {
                        return false;
                    }

                    pos = -pos;
                }

                LoopListViewItem newItem = GetNewItemByIndex(index);
                if (newItem == null)
                {
                    return false;
                }

                if (m_SupportScrollBar)
                {
                    SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }

                m_ItemList.Add(newItem);
                newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                UpdateListViewBegin(isFirstCheck);
                UpdateContentSize();
                return true;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 downPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);

            if (!m_IsDraging && tViewItem0.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && downPos0.y - m_ViewPortRectLocalCorners[1].y > distanceForRecycle0)
            {
                m_ItemList.RemoveAt(0);
                RecycleItemTmp(tViewItem0);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 downPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
            if (!m_IsDraging && tViewItem1.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && m_ViewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
            {
                m_ItemList.RemoveAt(m_ItemList.Count - 1);
                RecycleItemTmp(tViewItem1);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }


            if (m_ViewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
            {
                //往下拉
                m_IsScrollingDown = true;

                if (tViewItem1.ItemIndex > m_CurReadyMaxItemIndex)
                {
                    m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                    m_NeedCheckNextMaxItem = true;
                }

                int nIndex = tViewItem1.ItemIndex + 1;
                if (nIndex <= m_CurReadyMaxItemIndex || m_NeedCheckNextMaxItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        m_NeedCheckNextMaxItem = false;
                        CheckIfNeedUpdataItemPos();
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                        }

                        m_ItemList.Add(newItem);
                        float y = tViewItem1.CachedRectTransform.anchoredPosition3D.y - tViewItem1.CachedRectTransform.rect.height - tViewItem1.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();

                        if (nIndex > m_CurReadyMaxItemIndex)
                        {
                            m_CurReadyMaxItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }

            if (topPos0.y - m_ViewPortRectLocalCorners[1].y < distanceForNew0)
            {
                //往上拉
                m_IsScrollingDown = false;

                if (tViewItem0.ItemIndex < m_CurReadyMinItemIndex)
                {
                    m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                    m_NeedCheckNextMinItem = true;
                }

                int nIndex = tViewItem0.ItemIndex - 1;
                if (nIndex >= m_CurReadyMinItemIndex || m_NeedCheckNextMinItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                        m_NeedCheckNextMinItem = false;
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                        }

                        m_ItemList.Insert(0, newItem);
                        float y = tViewItem0.CachedRectTransform.anchoredPosition3D.y + newItem.CachedRectTransform.rect.height + newItem.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex < m_CurReadyMinItemIndex)
                        {
                            m_CurReadyMinItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }
        }
        else
        {
            if (m_ItemList.Count == 0)
            {
                float curY = m_ContainerTrans.anchoredPosition3D.y;
                if (curY > 0)
                {
                    curY = 0;
                }

                int index = 0;
                float pos = -curY;
                if (m_SupportScrollBar)
                {
                    if (GetPlusItemIndexAndPosAtGivenPos(-curY, ref index, ref pos) == false)
                    {
                        return false;
                    }
                }

                LoopListViewItem newItem = GetNewItemByIndex(index);
                if (newItem == null)
                {
                    return false;
                }

                if (m_SupportScrollBar)
                {
                    SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }

                m_ItemList.Add(newItem);
                newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                UpdateContentSize();
                return true;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 downPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);

            if (!m_IsDraging && tViewItem0.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && m_ViewPortRectLocalCorners[0].y - topPos0.y > distanceForRecycle0)
            {
                m_ItemList.RemoveAt(0);
                RecycleItemTmp(tViewItem0);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 topPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 downPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[0]);
            if (!m_IsDraging && tViewItem1.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && downPos1.y - m_ViewPortRectLocalCorners[1].y > distanceForRecycle1)
            {
                m_ItemList.RemoveAt(m_ItemList.Count - 1);
                RecycleItemTmp(tViewItem1);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }

            if (topPos1.y - m_ViewPortRectLocalCorners[1].y < distanceForNew1)
            {
                if (tViewItem1.ItemIndex > m_CurReadyMaxItemIndex)
                {
                    m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                    m_NeedCheckNextMaxItem = true;
                }

                int nIndex = tViewItem1.ItemIndex + 1;
                if (nIndex <= m_CurReadyMaxItemIndex || m_NeedCheckNextMaxItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_NeedCheckNextMaxItem = false;
                        CheckIfNeedUpdataItemPos();
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                        }

                        m_ItemList.Add(newItem);
                        float y = tViewItem1.CachedRectTransform.anchoredPosition3D.y + tViewItem1.CachedRectTransform.rect.height + tViewItem1.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex > m_CurReadyMaxItemIndex)
                        {
                            m_CurReadyMaxItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }


            if (m_ViewPortRectLocalCorners[0].y - downPos0.y < distanceForNew0)
            {
                if (tViewItem0.ItemIndex < m_CurReadyMinItemIndex)
                {
                    m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                    m_NeedCheckNextMinItem = true;
                }

                int nIndex = tViewItem0.ItemIndex - 1;
                if (nIndex >= m_CurReadyMinItemIndex || m_NeedCheckNextMinItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_NeedCheckNextMinItem = false;
                        return false;
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                        }

                        m_ItemList.Insert(0, newItem);
                        float y = tViewItem0.CachedRectTransform.anchoredPosition3D.y - newItem.CachedRectTransform.rect.height - newItem.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex < m_CurReadyMinItemIndex)
                        {
                            m_CurReadyMinItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool GetPlusItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
    {
        return m_ItemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
    }


    private bool UpdateForHorizontalList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1, bool isFirstCheck)
    {
        if (m_ItemTotalCount == 0)
        {
            if (m_ItemList.Count > 0)
            {
                RecycleAllItem();
            }

            return false;
        }

        if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            if (m_ItemList.Count == 0)
            {
                float curX = m_ContainerTrans.anchoredPosition3D.x;
                if (curX > 0)
                {
                    curX = 0;
                }

                int index = 0;
                float pos = -curX;
                if (m_SupportScrollBar)
                {
                    if (GetPlusItemIndexAndPosAtGivenPos(-curX, ref index, ref pos) == false)
                    {
                        return false;
                    }
                }

                LoopListViewItem newItem = GetNewItemByIndex(index);
                if (newItem == null)
                {
                    return false;
                }

                if (m_SupportScrollBar)
                {
                    SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }

                m_ItemList.Add(newItem);
                newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                UpdateListViewBegin(isFirstCheck);
                UpdateContentSize();
                return true;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 rightPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);

            if (!m_IsDraging && tViewItem0.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && m_ViewPortRectLocalCorners[1].x - rightPos0.x > distanceForRecycle0)
            {
                m_ItemList.RemoveAt(0);
                RecycleItemTmp(tViewItem0);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 rightPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
            if (!m_IsDraging && tViewItem1.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && leftPos1.x - m_ViewPortRectLocalCorners[2].x > distanceForRecycle1)
            {
                m_ItemList.RemoveAt(m_ItemList.Count - 1);
                RecycleItemTmp(tViewItem1);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }


            if (rightPos1.x - m_ViewPortRectLocalCorners[2].x < distanceForNew1)
            {
                if (tViewItem1.ItemIndex > m_CurReadyMaxItemIndex)
                {
                    m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                    m_NeedCheckNextMaxItem = true;
                }

                int nIndex = tViewItem1.ItemIndex + 1;
                if (nIndex <= m_CurReadyMaxItemIndex || m_NeedCheckNextMaxItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        m_NeedCheckNextMaxItem = false;
                        CheckIfNeedUpdataItemPos();
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                        }

                        m_ItemList.Add(newItem);
                        float x = tViewItem1.CachedRectTransform.anchoredPosition3D.x + tViewItem1.CachedRectTransform.rect.width + tViewItem1.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();

                        if (nIndex > m_CurReadyMaxItemIndex)
                        {
                            m_CurReadyMaxItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }

            if (m_ViewPortRectLocalCorners[1].x - leftPos0.x < distanceForNew0)
            {
                if (tViewItem0.ItemIndex < m_CurReadyMinItemIndex)
                {
                    m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                    m_NeedCheckNextMinItem = true;
                }

                int nIndex = tViewItem0.ItemIndex - 1;
                if (nIndex >= m_CurReadyMinItemIndex || m_NeedCheckNextMinItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                        m_NeedCheckNextMinItem = false;
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                        }

                        m_ItemList.Insert(0, newItem);
                        float x = tViewItem0.CachedRectTransform.anchoredPosition3D.x - newItem.CachedRectTransform.rect.width - newItem.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex < m_CurReadyMinItemIndex)
                        {
                            m_CurReadyMinItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }
        }
        else
        {
            if (m_ItemList.Count == 0)
            {
                float curX = m_ContainerTrans.anchoredPosition3D.x;
                if (curX < 0)
                {
                    curX = 0;
                }

                int index = 0;
                float pos = -curX;
                if (m_SupportScrollBar)
                {
                    if (GetPlusItemIndexAndPosAtGivenPos(curX, ref index, ref pos) == false)
                    {
                        return false;
                    }

                    pos = -pos;
                }

                LoopListViewItem newItem = GetNewItemByIndex(index);
                if (newItem == null)
                {
                    return false;
                }

                if (m_SupportScrollBar)
                {
                    SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }

                m_ItemList.Add(newItem);
                newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                UpdateListViewBegin(isFirstCheck);
                UpdateContentSize();
                return true;
            }

            LoopListViewItem tViewItem0 = m_ItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 rightPos0 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);

            if (!m_IsDraging && tViewItem0.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && leftPos0.x - m_ViewPortRectLocalCorners[2].x > distanceForRecycle0)
            {
                m_ItemList.RemoveAt(0);
                RecycleItemTmp(tViewItem0);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }

            LoopListViewItem tViewItem1 = m_ItemList[m_ItemList.Count - 1];
            tViewItem1.CachedRectTransform.GetWorldCorners(m_ItemWorldCorners);
            Vector3 leftPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[1]);
            Vector3 rightPos1 = m_ViewPortRectTransform.InverseTransformPoint(m_ItemWorldCorners[2]);
            if (!m_IsDraging && tViewItem1.ItemCreatedCheckFrameCount != m_ListUpdateCheckFrameCount
                             && m_ViewPortRectLocalCorners[1].x - rightPos1.x > distanceForRecycle1)
            {
                m_ItemList.RemoveAt(m_ItemList.Count - 1);
                RecycleItemTmp(tViewItem1);
                if (!m_SupportScrollBar)
                {
                    UpdateListViewBegin(isFirstCheck);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                }

                return true;
            }


            if (m_ViewPortRectLocalCorners[1].x - leftPos1.x < distanceForNew1)
            {
                if (tViewItem1.ItemIndex > m_CurReadyMaxItemIndex)
                {
                    m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                    m_NeedCheckNextMaxItem = true;
                }

                int nIndex = tViewItem1.ItemIndex + 1;
                if (nIndex <= m_CurReadyMaxItemIndex || m_NeedCheckNextMaxItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        m_NeedCheckNextMaxItem = false;
                        CheckIfNeedUpdataItemPos();
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                        }

                        m_ItemList.Add(newItem);
                        float x = tViewItem1.CachedRectTransform.anchoredPosition3D.x - tViewItem1.CachedRectTransform.rect.width - tViewItem1.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();

                        if (nIndex > m_CurReadyMaxItemIndex)
                        {
                            m_CurReadyMaxItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }

            if (rightPos0.x - m_ViewPortRectLocalCorners[2].x < distanceForNew0)
            {
                if (tViewItem0.ItemIndex < m_CurReadyMinItemIndex)
                {
                    m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                    m_NeedCheckNextMinItem = true;
                }

                int nIndex = tViewItem0.ItemIndex - 1;
                if (nIndex >= m_CurReadyMinItemIndex || m_NeedCheckNextMinItem)
                {
                    LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                    if (newItem == null)
                    {
                        m_CurReadyMinItemIndex = tViewItem0.ItemIndex;
                        m_NeedCheckNextMinItem = false;
                    }
                    else
                    {
                        if (m_SupportScrollBar)
                        {
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                        }

                        m_ItemList.Insert(0, newItem);
                        float x = tViewItem0.CachedRectTransform.anchoredPosition3D.x + newItem.CachedRectTransform.rect.width + newItem.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                        UpdateListViewBegin(isFirstCheck);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex < m_CurReadyMinItemIndex)
                        {
                            m_CurReadyMinItemIndex = nIndex;
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    void CheckIfNeedUpdataItemPos()
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            LoopListViewItem firstItem = m_ItemList[0];
            LoopListViewItem lastItem = m_ItemList[m_ItemList.Count - 1];
            float viewMaxY = GetContentPanelSize();
            if (firstItem.TopY > 0 || (firstItem.ItemIndex == m_CurReadyMinItemIndex && firstItem.TopY != 0))
            {
                UpdateAllShownItemsPos();
                return;
            }

            if ((-lastItem.BottomY) > viewMaxY || (lastItem.ItemIndex == m_CurReadyMaxItemIndex && (-lastItem.BottomY) != viewMaxY))
            {
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            LoopListViewItem firstItem = m_ItemList[0];
            LoopListViewItem lastItem = m_ItemList[m_ItemList.Count - 1];
            float viewMaxY = GetContentPanelSize();
            if (firstItem.BottomY < 0 || (firstItem.ItemIndex == m_CurReadyMinItemIndex && firstItem.BottomY != 0))
            {
                UpdateAllShownItemsPos();
                return;
            }

            if (lastItem.TopY > viewMaxY || (lastItem.ItemIndex == m_CurReadyMaxItemIndex && lastItem.TopY != viewMaxY))
            {
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            LoopListViewItem firstItem = m_ItemList[0];
            LoopListViewItem lastItem = m_ItemList[m_ItemList.Count - 1];
            float viewMaxX = GetContentPanelSize();
            if (firstItem.LeftX < 0 || (firstItem.ItemIndex == m_CurReadyMinItemIndex && firstItem.LeftX != 0))
            {
                UpdateAllShownItemsPos();
                return;
            }

            if ((lastItem.RightX) > viewMaxX || (lastItem.ItemIndex == m_CurReadyMaxItemIndex && lastItem.RightX != viewMaxX))
            {
                UpdateAllShownItemsPos();
                return;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            LoopListViewItem firstItem = m_ItemList[0];
            LoopListViewItem lastItem = m_ItemList[m_ItemList.Count - 1];
            float viewMaxX = GetContentPanelSize();
            if (firstItem.RightX > 0 || (firstItem.ItemIndex == m_CurReadyMinItemIndex && firstItem.RightX != 0))
            {
                UpdateAllShownItemsPos();
                return;
            }

            if ((-lastItem.LeftX) > viewMaxX || (lastItem.ItemIndex == m_CurReadyMaxItemIndex && (-lastItem.LeftX) != viewMaxX))
            {
                UpdateAllShownItemsPos();
                return;
            }
        }
    }


    void SetItemSize(int itemIndex, float itemSize, float padding)
    {
        m_ItemPosMgr.SetItemSize(itemIndex, itemSize + padding);
        if (itemIndex >= m_LastItemIndex)
        {
            m_LastItemIndex = itemIndex;
            m_LastItemPadding = padding;
        }
    }

    private void RecycleItemTmp(LoopListViewItem item)
    {
        if (item == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(item.ItemPrefabName))
        {
            return;
        }

        if (m_ItemPoolDict.TryGetValue(item.ItemPrefabName, out LoopListViewItemPool pool) == false)
        {
            return;
        }

        pool.RecycleItem(item);
    }

    private void RecycleAllItem()
    {
        foreach (LoopListViewItem item in m_ItemList)
        {
            RecycleItemTmp(item);
        }

        m_ItemList.Clear();
    }

    private void ClearAllTmpRecycledItem()
    {
        int count = m_ItemPoolList.Count;
        for (int i = 0; i < count; ++i)
        {
            m_ItemPoolList[i].ClearTmpRecycledItem();
        }
    }

    private float GetContentPanelSize(bool recalculate = false)
    {
        if (m_SupportScrollBar && !recalculate)
        {
            float tTotalSize = m_ItemPosMgr.mTotalSize > 0 ? (m_ItemPosMgr.mTotalSize - m_LastItemPadding + m_LastItemPaddingRight) : 0;
            if (tTotalSize < 0)
            {
                tTotalSize = 0;
            }

            return tTotalSize;
        }

        int count = m_ItemList.Count;
        if (count == 0)
        {
            return 0;
        }

        if (count == 1)
        {
            return m_ItemList[0].ItemSize;
        }

        if (count == 2)
        {
            return m_ItemList[0].ItemSizeWithPadding + m_ItemList[1].ItemSize;
        }

        float s = 0;
        for (int i = 0; i < count - 1; ++i)
        {
            s += m_ItemList[i].ItemSizeWithPadding;
        }

        s += m_ItemList[count - 1].ItemSize;
        return s;
    }

    private float GetItemPos(int itemIndex)
    {
        return m_ItemPosMgr.GetItemPos(itemIndex);
    }

    void UpdateAllShownItemsPos()
    {
        int count = m_ItemList.Count;
        if (count == 0)
        {
            return;
        }

        m_AdjustedVec = (m_ContainerTrans.anchoredPosition3D - m_LastFrameContainerPos) / Time.deltaTime;

        if (m_ArrangeType == ListItemArrangeType.TopToBottom)
        {
            float pos = 0;
            if (m_SupportScrollBar)
            {
                pos = -GetItemPos(m_ItemList[0].ItemIndex);
            }

            float pos1 = m_ItemList[0].CachedRectTransform.anchoredPosition3D.y;
            float d = pos - pos1;
            float curY = pos;
            for (int i = 0; i < count; ++i)
            {
                LoopListViewItem item = m_ItemList[i];
                item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                curY = curY - item.CachedRectTransform.rect.height - item.Padding;
            }

            if (d != 0)
            {
                Vector2 p = m_ContainerTrans.anchoredPosition3D;
                p.y = p.y - d;
                m_ContainerTrans.anchoredPosition3D = p;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.BottomToTop)
        {
            float pos = 0;
            if (m_SupportScrollBar)
            {
                pos = GetItemPos(m_ItemList[0].ItemIndex);
            }

            float pos1 = m_ItemList[0].CachedRectTransform.anchoredPosition3D.y;
            float d = pos - pos1;
            float curY = pos;
            for (int i = 0; i < count; ++i)
            {
                LoopListViewItem item = m_ItemList[i];
                item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                curY = curY + item.CachedRectTransform.rect.height + item.Padding;
            }

            if (d != 0)
            {
                Vector3 p = m_ContainerTrans.anchoredPosition3D;
                p.y = p.y - d;
                m_ContainerTrans.anchoredPosition3D = p;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.LeftToRight)
        {
            float pos = 0;
            if (m_SupportScrollBar)
            {
                pos = GetItemPos(m_ItemList[0].ItemIndex);
            }

            float pos1 = m_ItemList[0].CachedRectTransform.anchoredPosition3D.x;
            float d = pos - pos1;
            float curX = pos;
            for (int i = 0; i < count; ++i)
            {
                LoopListViewItem item = m_ItemList[i];
                item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                curX = curX + item.CachedRectTransform.rect.width + item.Padding;
            }

            if (d != 0)
            {
                Vector3 p = m_ContainerTrans.anchoredPosition3D;
                p.x = p.x - d;
                m_ContainerTrans.anchoredPosition3D = p;
            }
        }
        else if (m_ArrangeType == ListItemArrangeType.RightToLeft)
        {
            float pos = 0;
            if (m_SupportScrollBar)
            {
                pos = -GetItemPos(m_ItemList[0].ItemIndex);
            }

            float pos1 = m_ItemList[0].CachedRectTransform.anchoredPosition3D.x;
            float d = pos - pos1;
            float curX = pos;
            for (int i = 0; i < count; ++i)
            {
                LoopListViewItem item = m_ItemList[i];
                item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                curX = curX - item.CachedRectTransform.rect.width - item.Padding;
            }

            if (d != 0)
            {
                Vector3 p = m_ContainerTrans.anchoredPosition3D;
                p.x = p.x - d;
                m_ContainerTrans.anchoredPosition3D = p;
            }
        }

        if (m_IsDraging)
        {
            m_ScrollRect.OnBeginDrag(m_PointerEventData);
            m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
            m_ScrollRect.velocity = m_AdjustedVec;
            m_NeedAdjustVec = true;
        }
    }

    void UpdateContentSize(bool recalculate = false)
    {
        float size = GetContentPanelSize(recalculate);
        if (m_IsVertList)
        {
            if (m_ContainerTrans.rect.height != size)
            {
                m_ContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
        }
        else
        {
            if (m_ContainerTrans.rect.width != size)
            {
                m_ContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            }
        }

        OnUpdateContentSize?.Invoke();
    }

    void UpdateListViewBegin(bool isFirstCheck)
    {
        if (!this.m_ViewPortResizeToContentSize || !isFirstCheck)
            return;

        if (m_ViewPortRawSize.x != this.ViewPortWidth || m_ViewPortRawSize.y != this.ViewPortHeight)
        {
            if (m_IsVertList)
            {
                m_ScrollRect.movementType = m_RawMovementType;
                m_ViewPortRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, m_ViewPortRawSize.y);
            }
            else
            {
                m_ScrollRect.movementType = m_RawMovementType;
                m_ViewPortRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, m_ViewPortRawSize.x);
            }
        }
    }

    void UpdateListViewEnd()
    {
        if (!this.m_ViewPortResizeToContentSize)
            return;
        if (m_IsVertList)
        {
            float contentSize = m_ContainerTrans.rect.height;
            if (m_ViewPortRawSize.y > contentSize)
            {
                float inset = (m_ViewPortRawSize.y - contentSize) / 2;
                m_ViewPortRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, inset, contentSize);
                m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
            }
        }
        else
        {
            float contentSize = m_ContainerTrans.rect.width;
            if (m_ViewPortRawSize.x > contentSize)
            {
                float inset = (m_ViewPortRawSize.x - contentSize) / 2;
                m_ViewPortRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset, contentSize);
                m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
            }
        }
    }

    public void AdjustCell(int cellNum)
    {
        bool isOver = m_ItemTotalCount >= cellNum;
        m_ScrollRect.horizontal = isOver;
        if (m_ScrollImage == null)
        {
            m_ScrollImage = m_ViewPortRectTransform.GetComponent<Image>();
        }

        m_ScrollImage.raycastTarget = isOver;
        if (!isOver && m_ItemTotalCount != 0)
        {
            LoopListViewItem item = m_ItemList[0];
            float midPos = m_ScrollImage.rectTransform.rect.size.y / 2f;
            float itemWidth = item.ItemSize;
            float itemPadding = item.Padding;
            float itemSizeWithPadding = item.ItemSizeWithPadding;
            float startPos = midPos - (m_ItemTotalCount / 2f * itemWidth + (m_ItemTotalCount - 1) / 2f * itemPadding);

            for (int i = 0; i < m_ItemList.Count; i++)
            {
                item = m_ItemList[i];
                float x = startPos + (i * itemSizeWithPadding);
                item.transform.localPosition = new Vector3(x, item.transform.localPosition.y, 0);
            }
        }
    }

    #region Mono Method

    private void Update()
    {
        if (m_ListViewInited == false)
        {
            return;
        }
        if (m_NeedAdjustVec)
        {
            m_NeedAdjustVec = false;
            if (m_IsVertList)
            {
                if (m_ScrollRect.velocity.y * m_AdjustedVec.y > 0)
                {
                    m_ScrollRect.velocity = m_AdjustedVec;
                }
            }
            else
            {
                if (m_ScrollRect.velocity.x * m_AdjustedVec.x > 0)
                {
                    m_ScrollRect.velocity = m_AdjustedVec;
                }
            }

        }
        if (m_SupportScrollBar)
        {
            m_ItemPosMgr.Update(false);
        }
        UpdateSnapMove();
        UpdateListView(m_DistanceForRecycle0, m_DistanceForRecycle1, m_DistanceForNew0, m_DistanceForNew1);
        ClearAllTmpRecycledItem();
        m_LastFrameContainerPos = m_ContainerTrans.anchoredPosition3D;
    }

    #endregion
    
    #region Drag Callback

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        m_IsDraging = true;
        CacheDragPointerEventData(eventData);
        m_CurSnapData.Clear();
        if (m_OnBeginDragAction != null)
        {
            m_OnBeginDragAction(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        m_IsDraging = false;
        m_PointerEventData = null;
        if (m_OnEndDragAction != null)
        {
            m_OnEndDragAction(eventData);
        }
        ForceSnapUpdateCheck();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        CacheDragPointerEventData(eventData);
        if (m_OnDragingAction != null)
        {
            m_OnDragingAction(eventData);
        }
    }
    
    void CacheDragPointerEventData(PointerEventData eventData)
    {
        if (m_PointerEventData == null)
        {
            m_PointerEventData = new PointerEventData(EventSystem.current);
        }
        m_PointerEventData.button = eventData.button;
        m_PointerEventData.position = eventData.position;
        m_PointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
        m_PointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
    }

    #endregion
}

[System.Serializable]
public class ItemPrefabConfData
{
    public GameObject mItemPrefab = null;
    public float mPadding = 0;
    public int mInitCreateCount = 0;
    public float mStartPosOffset = 0;
}