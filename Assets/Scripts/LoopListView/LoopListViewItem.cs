using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopListViewItem : MonoBehaviour
{
    // indicates the item’s index in the list
    //if itemTotalCount is set -1, then the mItemIndex can be from –MaxInt to +MaxInt.
    //If itemTotalCount is set a value >=0 , then the mItemIndex can only be from 0 to itemTotalCount -1.
    

    //int mItemIndexInPool = -1;
    public int ItemIndexInPool
    {
        get { return mItemIndex; }
        set { mItemIndex = value; }
    }
    
    int mItemIndex = -1;
    public int ItemIndex
    {
        get
        {
            return mItemIndex;
        }
        set
        {
            mItemIndex = value;
        }
    }
    
    private float mPadding;
    public float Padding
    {
        get { return mPadding; }
        set { mPadding = value; }
    }
    
    //ndicates the item’s id. 
    //This property is set when the item is created or fetched from pool, 
    //and will no longer change until the item is recycled back to pool.
    int mItemId = -1;
    public int ItemId
    {
        get
        {
            return mItemId;
        }
        set
        {
            mItemId = value;
        }
    }
    bool mIsInitHandlerCalled = false;
    public bool IsInitHandlerCalled
    {
        get
        {
            return mIsInitHandlerCalled;
        }
        set
        {
            mIsInitHandlerCalled = value;
        }
    }
    string mItemPrefabName;
    public string ItemPrefabName
    {
        get
        {
            return mItemPrefabName;
        }
        set
        {
            mItemPrefabName = value;
        }
    }
    
    private float mStartPosOffset = 0;
    public float StartPosOffset
    {
        get { return mStartPosOffset; }
        set { mStartPosOffset = value; }
    }
    
    float mDistanceWithViewPortSnapCenter = 0;
    public float DistanceWithViewPortSnapCenter
    {
        get { return mDistanceWithViewPortSnapCenter; }
        set { mDistanceWithViewPortSnapCenter = value; }
    }
    
    private LoopListView mParentListView = null;
    public LoopListView ParentListView
    {
        get
        {
            return mParentListView;
        }
        set
        {
            mParentListView = value;
        }
    }
    private RectTransform mCachedRectTransform;
    public RectTransform CachedRectTransform
    {
        get
        {
            if (mCachedRectTransform == null)
            {
                mCachedRectTransform = gameObject.GetComponent<RectTransform>();
            }
            return mCachedRectTransform;
        }
    }
    public float ItemSize
    {
        get
        {
            if (ParentListView.IsVertList)
            {
                return  CachedRectTransform.rect.height;
            }
            else
            {
                return CachedRectTransform.rect.width;
            }
        }
    }
    
    public float TopY
    {
        get
        {
            ListItemArrangeType arrageType = ParentListView.ArrangeType;
            if (arrageType == ListItemArrangeType.TopToBottom)
            {
                return CachedRectTransform.anchoredPosition3D.y;
            }
            else if(arrageType == ListItemArrangeType.BottomToTop)
            {
                return CachedRectTransform.anchoredPosition3D.y + CachedRectTransform.rect.height;
            }
            return 0;
        }
    }

    public float BottomY
    {
        get
        {
            ListItemArrangeType arrageType = ParentListView.ArrangeType;
            if (arrageType == ListItemArrangeType.TopToBottom)
            {
                return CachedRectTransform.anchoredPosition3D.y - CachedRectTransform.rect.height;
            }
            else if (arrageType == ListItemArrangeType.BottomToTop)
            {
                return CachedRectTransform.anchoredPosition3D.y;
            }
            return 0;
        }
    }
    
    public float LeftX
    {
        get
        {
            ListItemArrangeType arrageType = ParentListView.ArrangeType;
            if (arrageType == ListItemArrangeType.LeftToRight)
            {
                return CachedRectTransform.anchoredPosition3D.x;
            }
            else if (arrageType == ListItemArrangeType.RightToLeft)
            {
                return CachedRectTransform.anchoredPosition3D.x - CachedRectTransform.rect.width;
            }
            return 0;
        }
    }

    public float RightX
    {
        get
        {
            ListItemArrangeType arrageType = ParentListView.ArrangeType;
            if (arrageType == ListItemArrangeType.LeftToRight)
            {
                return CachedRectTransform.anchoredPosition3D.x + CachedRectTransform.rect.width;
            }
            else if (arrageType == ListItemArrangeType.RightToLeft)
            {
                return CachedRectTransform.anchoredPosition3D.x;
            }
            return 0;
        }
    }
    public float ItemSizeWithPadding
    {
        get
        {
            return ItemSize + mPadding;
        }
    }
    
    int mItemCreatedCheckFrameCount = 0;
    public int ItemCreatedCheckFrameCount
    {
        get { return mItemCreatedCheckFrameCount; }
        set { mItemCreatedCheckFrameCount = value; }
    }
    
}