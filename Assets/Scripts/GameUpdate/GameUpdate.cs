using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameUpdate : MonoBehaviour
{
    [SerializeField] private GameUpdateSlider m_updateSlider = null;
    [SerializeField] private Text m_pathText = null;
    private  IEnumerator Start()
    {
        string persistentDataPath = Application.persistentDataPath;
        m_pathText.text = $"[PersistentDataPath]:{persistentDataPath}";

        string TargetName = "PC";
        
        
        // 读取本地版本文件
        string datasPath = Path.Combine(persistentDataPath, "Res/Datas/");
        
        m_updateSlider.SetSliderProgress(0);
        // 下载服务器版本文件
        
        // 文件对比
        
        // 对差异化的文件进行下载
        
        // 下载完成 
        yield return null;
        // 加载ILRuntime
        HotFixMgr.Instance.Init();
    }
    
}
