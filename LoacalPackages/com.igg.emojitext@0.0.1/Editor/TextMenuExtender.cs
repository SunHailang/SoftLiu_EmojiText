using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using igg.EmojiText.Runtime;

namespace igg.EmojiText.Editor
{
    public class TextMenuExtender
    {
        [MenuItem("GameObject/UI/UiEmojiText", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            GameObject go = null;
            string prefab = Path.Combine(Application.dataPath, "../LoacalPackages/com.igg.emojitext@0.0.1/Tests/ResourcesRex/Prefab/UiEmojiText.prefab");
            UiEmojiText text = AssetDatabase.LoadAssetAtPath<UiEmojiText>(prefab);
            bool exists = File.Exists(prefab);
            if (exists)
            {
                go = PrefabUtility.LoadPrefabContents(prefab);
                //go = GameObject.Instantiate(text).gameObject;
            }
            else
            {
                go = new GameObject();
                go.AddComponent<UiEmojiText>();
            }
            go.name = "UiEmojiText";
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null)
            {
                parent = new GameObject("Canvas");
                parent.layer = LayerMask.NameToLayer("UI");
                parent.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                parent.AddComponent<CanvasScaler>();
                parent.AddComponent<GraphicRaycaster>();

                EventSystem _es = GameObject.FindObjectOfType<EventSystem>();
                if (!_es)
                {
                    _es = new GameObject("EventSystem").AddComponent<EventSystem>();
                    _es.gameObject.AddComponent<StandaloneInputModule>();
                }
            }
            GameObjectUtility.SetParentAndAlign(go, parent);
            //注册返回事件
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }

}