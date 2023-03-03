using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    public class EmojiManager : MonoBehaviour
    {
        public static readonly Dictionary<int, Dictionary<string, SpriteInforGroup>> IndexSpriteInfo = new Dictionary<int, Dictionary<string, SpriteInforGroup>>();

        [SerializeField] private List<SpriteGraphic> _spriteList = new List<SpriteGraphic>();  //图集
        private Dictionary<int, SpriteGraphic> _spriteGraphicDict = new Dictionary<int, SpriteGraphic>();

        public Dictionary<int, SpriteGraphic> SpriteGraphicDict => _spriteGraphicDict;
        //渲染列表
        private Dictionary<int, MeshInfo> _renderMesh = new Dictionary<int, MeshInfo>();

        private void Awake()
        {
            Initialize();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            Initialize();
        }
#endif


        void Initialize()
        {
            SpriteGraphic[] _spriteGraphics = _spriteList.ToArray(); // gameObject.GetComponentsInChildren<SpriteGraphic>();
            if (_spriteGraphics == null || _spriteGraphics.Length <= 0) return;
            foreach (SpriteGraphic graphic in _spriteGraphics)
            {
                if (graphic.m_spriteAsset == null) continue;
                _spriteGraphicDict[graphic.m_spriteAsset.Id] = graphic;
            }

            foreach (KeyValuePair<int, SpriteGraphic> spriteGraphic in _spriteGraphicDict)
            {
                SpriteAsset mSpriteAsset = spriteGraphic.Value.m_spriteAsset;
                if (!IndexSpriteInfo.TryGetValue(mSpriteAsset.Id, out Dictionary<string, SpriteInforGroup> spriteGroup) || spriteGroup == null)
                {
                    spriteGroup = new Dictionary<string, SpriteInforGroup>();
                    foreach (SpriteInforGroup item in mSpriteAsset.ListSpriteGroup)
                    {
                        if (item == null) continue;
                        if ((!spriteGroup.TryGetValue(item.Tag, out SpriteInforGroup spriteInfoGroup) || spriteInfoGroup == null)
                            && item.ListSpriteInfor != null && item.ListSpriteInfor.Count > 0)
                        {
                            spriteGroup[item.Tag] = item;
                        }
                    }

                    IndexSpriteInfo[mSpriteAsset.Id] = spriteGroup;
                }
            }
        }

        private void Update()
        {
            foreach (KeyValuePair<int, MeshInfo> meshData in _renderMesh)
            {
                if (!_spriteGraphicDict.TryGetValue(meshData.Key, out SpriteGraphic spriteGraphic))
                {
                    continue;
                }
        
                if (spriteGraphic == null)
                {
                    _spriteGraphicDict.Remove(meshData.Key);
                    return;
                }

                if (spriteGraphic.MeshInfo != null)
                {
                    Utils.Pool<MeshInfo>.Release(spriteGraphic.MeshInfo);
                }
                if (meshData.Value.visable)
                {
                    spriteGraphic.MeshInfo = meshData.Value;
                }
            }
            //清掉渲染索引
            _renderMesh.Clear();
        }

        //更新Text文本信息
        public void UpdateTextInfo(int id, List<SpriteTagInfo> value, bool visable)
        {
            if (value == null)
            {
                if (_spriteGraphicDict.TryGetValue(id, out SpriteGraphic textMeshInfo) && textMeshInfo != null && textMeshInfo.MeshInfo != null)
                {
                    textMeshInfo.MeshInfo.Release();
                }
            }
            else
            {
                if (!_spriteGraphicDict.TryGetValue(id, out SpriteGraphic spriteGraphic))
                {
                    return;
                }

                if (spriteGraphic == null)
                {
                    _spriteGraphicDict.Remove(id);
                    return;
                }
                
                MeshInfo meshInfo = Utils.Pool<MeshInfo>.Get();
                meshInfo.Reset();
                meshInfo.visable = visable;
                for (int i = 0; i < value.Count; i++)
                {
                    for (int j = 0; j < value[i].Pos.Length; j++)
                    {
                        //世界转本地坐标->避免位置变换的错位
                        meshInfo.Vertices.Add(Utils.Utility.TransformWorld2Point(spriteGraphic.transform, value[i].Pos[j]));
                    }
                    meshInfo.UVs.AddRange(value[i].UVs);
                    meshInfo.Colors.Add(value[i].ColorData);
                }
                _renderMesh[id] = meshInfo;
            }
        }
    }
}