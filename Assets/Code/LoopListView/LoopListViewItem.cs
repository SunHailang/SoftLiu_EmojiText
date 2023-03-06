using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopListViewItem : MonoBehaviour
{
    // indicates the item’s index in the list
    //if itemTotalCount is set -1, then the mItemIndex can be from –MaxInt to +MaxInt.
    //If itemTotalCount is set a value >=0 , then the mItemIndex can only be from 0 to itemTotalCount -1.
    int mItemIndex = -1;

    public int ItemIndexInPool
    {
        get { return mItemIndex; }
        set { mItemIndex = value; }
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
}