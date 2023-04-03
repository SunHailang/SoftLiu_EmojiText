using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameDebugCanvas : MonoBehaviour
{
    [SerializeField] private GameObject m_curScrollViewGo = null;
    [SerializeField] private RectTransform m_curScrollViewRect = null;
    [SerializeField] private RectTransform m_curTextRect = null;
    [SerializeField] private Text m_curText = null;
    [SerializeField] private RectTransform m_curBtnLogRect = null;
    [SerializeField] private Button m_curBtnLog = null;
    [SerializeField] private Button m_closeBtnLog = null;
    private StringBuilder m_curLog = new StringBuilder();

    private bool m_isShowLog = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Application.logMessageReceived += logCallback;
        m_curBtnLog.onClick.AddListener(btnLog_OnClick);
        m_closeBtnLog.onClick.AddListener(Close_OnClick);
        btnLog_OnClick();
    }
    
    
    private void logCallback(string condition, string stackTrace, LogType type)
    {
        if (string.IsNullOrEmpty(condition)) return;
        string color = "#000000";
        switch (type)
        {
            case LogType.Error:
                color = "#FF0000";
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                color = "#FFFF00";
                break;
            case LogType.Exception:
                break;
        }
        m_curLog.Append($"<color={color}>{condition}</color>\n");
        m_curText.text = m_curLog.ToString();
    }

    private void Close_OnClick()
    {
        if (!m_isShowLog)
        {
            btnLog_OnClick();
        }
    }
    
    private void btnLog_OnClick()
    {
        m_curScrollViewGo.SetActive(m_isShowLog);
        m_curTextRect.sizeDelta = new Vector2(Screen.width - 40, m_curTextRect.rect.height);

        m_curBtnLogRect.anchoredPosition = new Vector2(10, m_isShowLog ? m_curScrollViewRect.rect.height + 10 : 10);

        m_closeBtnLog.interactable = m_isShowLog;
        m_closeBtnLog.targetGraphic.raycastTarget = m_isShowLog;
        
        m_isShowLog = !m_isShowLog;
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= logCallback;
        m_curLog.Clear();
    }
}
