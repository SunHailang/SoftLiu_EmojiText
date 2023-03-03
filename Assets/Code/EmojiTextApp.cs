using System;
using System.Collections;
using System.Collections.Generic;
using Code;
using UnityEngine;
using UnityEngine.UI;

public class EmojiTextApp : MonoBehaviour
{
    private const string m_textStr = @"Hello World [0#emoji_19#emoji][1#emoji_2#emoji]1111456456456456[1#emoji_3#emoji]4564[-1#我是超链接1号-点我#https:\\www.baidu.com]564564564[0#emoji_35#emoji]5645645645[-2#我是超链接2号-点我#https:\\www.igg.com]6456456[0#emoji_38#emoji]456456456";
    
    [SerializeField] private UiEmojiText m_textPrefab = null;
    [SerializeField] private Transform m_parentTrans = null;

    [SerializeField] private Button m_btn = null;

    private void Start()
    {
        m_btn.onClick.AddListener(Btn_OnClick);
        for (int i = 0; i < 3; i++)
        {
            InstantianteText();
        }
        
    }

    private void InstantianteText()
    {
        UiEmojiText text = Instantiate(m_textPrefab, m_parentTrans);
        text.transform.localScale = Vector3.one;
        text.transform.localRotation = Quaternion.identity;
        text.transform.localPosition = Vector3.zero;
        text.text = m_textStr;

        float preferredWidth = text.GetPreferredWidth();
        preferredWidth = preferredWidth > 710 ? 710 : preferredWidth;
        float preferredHeight = text.GetPreferredHeight(preferredWidth);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
        
        text.OnHrefClick.AddListener(Href_OnClick);
    }

    private void Href_OnClick(string hrefValue, int id)
    {
        Debug.Log($"点击了超链接：ID:{id}, HrefValue:{hrefValue}");
    }

    private void Btn_OnClick()
    {
        InstantianteText();
    }
}
