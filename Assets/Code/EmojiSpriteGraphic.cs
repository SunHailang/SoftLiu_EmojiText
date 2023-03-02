using System.Collections.Generic;
using UnityEngine;


namespace Code
{
    [System.Serializable]
    public class EmojiSpriteGraphic
    {
        public int SpriteID = 0;
        public SpriteGraphic SpriteGraphicData = null;
    }

    #region 模型数据信息

    public class MeshInfo
    {
        public List<Vector3> Vertices = null;
        public List<Vector2> UVs = null;
        public List<Color> Colors = null;
        public List<int> Triangles = null;
        public bool visable = true;
        public bool listsInitalized = false;

        public void Reset()
        {
            if (Vertices == null || UVs == null || Colors == null || Triangles == null)
            {
                listsInitalized = false;
            }

            if (!listsInitalized)
            {
                Vertices = Utils.ListPool<Vector3>.Get();
                UVs = Utils.ListPool<Vector2>.Get();
                Colors = Utils.ListPool<Color>.Get();
                Triangles = Utils.ListPool<int>.Get();

                listsInitalized = true;
            }

            this.Vertices.Clear();
            this.UVs.Clear();
            this.Colors.Clear();
            this.Triangles.Clear();
        }

        public void Release()
        {
            if (listsInitalized)
            {
                Utils.ListPool<Vector3>.Release(Vertices);
                Utils.ListPool<Vector2>.Release(UVs);
                Utils.ListPool<Color>.Release(Colors);
                Utils.ListPool<int>.Release(Triangles);
                Utils.Pool<MeshInfo>.Release(this);

                Vertices = null;
                UVs = null;
                Colors = null;
                Triangles = null;

                listsInitalized = false;
            }
        }
    }

    #endregion

    /// <summary>
    /// 超链接信息类
    /// </summary>
    public class HrefInfo
    {
        /// <summary>
        /// 超链接id
        /// </summary>
        public int Id;

        /// <summary>
        /// 顶点开始索引值
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// 顶点结束索引值
        /// </summary>
        public int EndIndex;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 超链接的值
        /// </summary>
        public string HrefValue;

        /// <summary>
        /// 碰撞盒范围
        /// </summary>
        public readonly List<Rect> Boxes = new List<Rect>();
    }

    /// <summary>
    /// 图片的信息
    /// </summary>
    public class SpriteTagInfo
    {
        /// <summary>
        /// 为了兼容unity2019 单行的顶点的索引
        /// </summary>
        public int NewIndex;

        /// <summary>
        /// 图集id
        /// </summary>
        public int Id;

        /// <summary>
        /// 标签标签
        /// </summary>
        public string Tag;

        /// <summary>
        /// 标签大小
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// 表情位置
        /// </summary>
        public Vector3[] Pos = new Vector3[4];

        /// <summary>
        /// uv
        /// </summary>
        public Vector2[] UVs = new Vector2[4];

        public Color ColorData;
    }
}