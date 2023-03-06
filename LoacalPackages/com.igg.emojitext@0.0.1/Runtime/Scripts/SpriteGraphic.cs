using System;
using UnityEngine;
using UnityEngine.UI;

namespace igg.EmojiText.Runtime
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    public class SpriteGraphic : MaskableGraphic
    {
        private const string _defaultShader = "Hidden/UI/Emoji";

        private Material _defaultMater = null;

        public SpriteAsset m_spriteAsset;

        //顶点缓存数据
        readonly UIVertex[] _tempVerts = new UIVertex[4];

        //模型数据
        private MeshInfo _meshInfo = null;

        public MeshInfo MeshInfo
        {
            get { return _meshInfo; }
            set
            {
                if (value == null && _meshInfo != null)
                {
                    _meshInfo.Reset();
                }
                else
                    _meshInfo = value;
                SetAllDirty();
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (m_spriteAsset != null && m_spriteAsset.TexSource != null)
                {
                    return m_spriteAsset.TexSource;
                }

                return base.mainTexture;
            }
        }

        public override Material material
        {
            get
            {
                if (_defaultMater == null && m_spriteAsset != null)
                {
                    _defaultMater = new Material(Shader.Find(_defaultShader));
                    //是否开启动画
                    if (m_spriteAsset.IsStatic)
                        _defaultMater.DisableKeyword("EMOJI_ANIMATION");
                    else
                    {
                        _defaultMater.EnableKeyword("EMOJI_ANIMATION");
                        _defaultMater.SetFloat("_CellAmount", m_spriteAsset.Column);
                        _defaultMater.SetFloat("_Speed", m_spriteAsset.Speed);
                    }
                }
        
                return _defaultMater;
            }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            //在这里可以做一个数据判断，如果数据一样 就不再刷新
            if (_meshInfo != null)
            {
                for (int i = 0; i < _meshInfo.Vertices.Count; i++)
                {
                    int tempVertsIndex = i & 3;
                    _tempVerts[tempVertsIndex].position = _meshInfo.Vertices[i]; // Utility.TransformWorld2Point(transform, _meshInfo.Vertices[i]);
                    _tempVerts[tempVertsIndex].uv0 = _meshInfo.UVs[i];
                    // 每一个Mesh有4个顶点
                    int colorIndex = i / 4;
                    _tempVerts[tempVertsIndex].color = _meshInfo.Colors[colorIndex];
                    if (tempVertsIndex == 3)
                        vh.AddUIVertexQuad(_tempVerts);
                }
            }
        }
    }
}