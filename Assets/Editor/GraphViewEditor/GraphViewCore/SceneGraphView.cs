using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<SceneGraphView, UxmlTraits> { }

    public SceneGraphView()
    {
        Insert(0, new GridBackground());
        
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("66a4157e258c80848b59a10bde0ddaa4"));
        styleSheets.Add(styleSheet);
        
        //graphViewChanged = OnGraphViewChanged;

    }
    
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {

        return graphViewChange;
    }

    public override void HandleEvent(EventBase evt)
    {
        base.HandleEvent(evt);
        if (evt is ValidateCommandEvent commandEvent)
        {
            Debug.Log("Event:");
            Debug.Log(commandEvent.commandName);
            //限制一下0.2s执行一次  不然短时间会多次执行
            if (commandEvent.commandName.Equals("Paste"))
            {
               
            }
        }
    }

    public void AddNode()
    {
        
    }

    /// <summary>
    /// 遍历所有节点，根据当前节点状态修改颜色（Debug）
    /// </summary>
    protected void ChangeTitleColor()
    {
        Color runningColor = new Color(0.37f, 1,1,1f); //浅蓝
        Color compeletedColor = new Color(0.5f,1,0.37f,1f); //浅绿
        Color portColor = new Color(0.41f, 0.72f,0.72f,1f); //灰蓝

        nodes.ForEach(x =>
        {
            // if(x is BaseNodeView node)
            // {
            //     if (node.state?.State == EState.Running || node.state?.State == EState.Enter || node.state?.State == EState.Exit)
            //     {
            //         node.titleContainer.style.backgroundColor = new StyleColor(runningColor);
            //     }
            //     if (node.state?.State == EState.Finish)
            //     {
            //         node.titleContainer.style.backgroundColor = new StyleColor(compeletedColor);
            //     }
            // }
        });
    }
    
}