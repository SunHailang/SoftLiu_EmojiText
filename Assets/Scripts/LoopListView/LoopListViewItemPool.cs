using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopListViewItemPool
{
    GameObject mPrefabObj;
        string mPrefabName;
        int mInitCreateCount = 1;
        float mPadding = 0;
        float mStartPosOffset = 0;
        List<LoopListViewItem> mTmpPooledItemList = new List<LoopListViewItem>();
        List<LoopListViewItem> mPooledItemList = new List<LoopListViewItem>();
        List<LoopListViewItem> mActivedItemList = new List<LoopListViewItem>();
        static int mCurItemIdCount = 0;
        RectTransform mItemParent = null;
        public System.Action<string,LoopListViewItem> OnCreateItem2 = null;
        
        public LoopListViewItemPool()
        {

        }
        
        public void Init(GameObject prefabObj, float padding, float startPosOffset, int createCount, RectTransform parent, System.Action<string, LoopListViewItem> onCreate)
        {
            mPrefabObj = prefabObj;
            mPrefabName = mPrefabObj.name;
            mInitCreateCount = createCount;
            mPadding = padding;
            mStartPosOffset = startPosOffset;
            mItemParent = parent;
            mPrefabObj.SetActive(false);
            OnCreateItem2 = onCreate;
            for (int i = 0; i < mInitCreateCount; ++i)
            {
                LoopListViewItem tViewItem = CreateItem();
                tViewItem.ItemIndexInPool = i;
                RecycleItemReal(tViewItem);
            }
        }
        public LoopListViewItem GetItem()
        {
            mCurItemIdCount++;
            LoopListViewItem tItem = null;
            if (mTmpPooledItemList.Count > 0)
            {
                int count = mTmpPooledItemList.Count;
                tItem = mTmpPooledItemList[count - 1];
                mTmpPooledItemList.RemoveAt(count - 1);
                tItem.gameObject.SetActive(true);
            }
            else
            {
                int count = mPooledItemList.Count;
                if (count == 0)
                {
                    tItem = CreateItem();
                }
                else
                {
                    tItem = mPooledItemList[count - 1];
                    mPooledItemList.RemoveAt(count - 1);
                    tItem.gameObject.SetActive(true);
                }
            }
            tItem.Padding = mPadding;
            tItem.ItemId = mCurItemIdCount;
            return tItem;
        }

        public LoopListViewItem GetItemByIndex(int index)
        {
            mCurItemIdCount++;
            LoopListViewItem tItem = null;
            if (mTmpPooledItemList.Find(x => x.ItemIndexInPool == index) != null)
            {
                int count = mTmpPooledItemList.Count;
                tItem = mTmpPooledItemList.Find(x => x.ItemIndexInPool == index);
                mTmpPooledItemList.Remove(tItem);
                tItem.gameObject.SetActive(true);
            }
            else if (mPooledItemList.Find(x => x.ItemIndexInPool == index) != null)
            {
                tItem = mPooledItemList.Find(x => x.ItemIndexInPool == index);
                mPooledItemList.Remove(tItem);
                tItem.gameObject.SetActive(true);
            }
            else
            {
                tItem = CreateItem();
                tItem.ItemIndexInPool = index;
            }
            tItem.Padding = mPadding;
            tItem.ItemId = mCurItemIdCount;
            mActivedItemList.Add(tItem);
            return tItem;
        }

        public void ResetAllItemInitState()
        {
            for (int i = 0, count = mTmpPooledItemList.Count;i < count; i++)
            {
                mTmpPooledItemList[i].IsInitHandlerCalled = false;
                RecycleItemReal(mTmpPooledItemList[i]);
            }
            mTmpPooledItemList.Clear();
            for (int i = 0, count = mPooledItemList.Count; i < count; ++i)
            {
                mPooledItemList[i].IsInitHandlerCalled = false;
            }
            for (int i = 0, count = mActivedItemList.Count; i < count; ++i)
            {
                mActivedItemList[i].IsInitHandlerCalled = false;
            }
        }

        public void DestroyAllItem()
        {
            ClearTmpRecycledItem();
            int count = mPooledItemList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameObject.DestroyImmediate(mPooledItemList[i].gameObject);
            }
            mPooledItemList.Clear();
            OnCreateItem2 = null;
        }
        public LoopListViewItem CreateItem()
        {
            GameObject go = GameObject.Instantiate<GameObject>(mPrefabObj, Vector3.zero, Quaternion.identity, mItemParent);
            go.SetActive(true);
            RectTransform rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            LoopListViewItem tViewItem = go.GetComponent<LoopListViewItem>();
            tViewItem.ItemPrefabName = mPrefabName;
            tViewItem.StartPosOffset = mStartPosOffset;
            OnCreateItem2?.Invoke(mPrefabName, tViewItem);
            return tViewItem;
        }
        void RecycleItemReal(LoopListViewItem item)
        {
            item.gameObject.SetActive(false);
            mPooledItemList.Add(item);
            mActivedItemList.Remove(item);
        }
        public void RecycleItem(LoopListViewItem item)
        {
            mTmpPooledItemList.Add(item);
        }
        public void ClearTmpRecycledItem()
        {
            int count = mTmpPooledItemList.Count;
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; ++i)
            {
                RecycleItemReal(mTmpPooledItemList[i]);
            }
            mTmpPooledItemList.Clear();
        }
        
        
}
