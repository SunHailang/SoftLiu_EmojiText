using System.Collections;
using UnityEngine;

public class GameUpdate : MonoBehaviour
{
    [SerializeField] private GameUpdateSlider m_updateSlider = null;
    
    // Start is called before the first frame update
    private  IEnumerator Start()
    {
        // 读取本地版本文件
        m_updateSlider.SetSliderProgress(0);
        // 下载服务器版本文件
        
        // 文件对比
        
        // 对差异化的文件进行下载
        
        // 下载完成 进入游戏
        yield return null;
    }
    
}
