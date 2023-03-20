using System;
using System.Collections;
using System.Collections.Generic;
using igg.EmojiText.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLaunch : MonoBehaviour
{
    [SerializeField] private Transform m_text = null;

    public AnimationCurve curve = null;

    private float m_startTime = 0;
    
    private IEnumerator Start()
    {
        m_text.localScale = Vector3.zero;
        // 加载 Update 场景
        AsyncOperation scene = SceneManager.LoadSceneAsync("GameUpdate");
        scene.allowSceneActivation = false;
        
        m_startTime = Time.time;
        StartCoroutine(AnimText(scene));
        
        yield return scene;
    }

    private IEnumerator AnimText(AsyncOperation scene)
    {
        while (m_startTime + 1.5f > Time.time)
        {
            m_text.localScale = Vector3.one * curve.Evaluate(Time.time);
            yield return null;
        }
        scene.allowSceneActivation = true;
    }
}
