using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUpdateSlider : MonoBehaviour
{
    [SerializeField] private Slider m_slider = null;
    [SerializeField] private Text m_progressText = null;
    [SerializeField] private Text m_latestVersionText = null;

    public float CurValue => m_slider.value;

    public event Action ComplateCallback = null;
    

    public void SetSliderProgress(float value)
    {
        m_slider.value = value;
        value *= 100;
        m_progressText.text = $"{value:F1}%";
        if (value >= 1)
        {
            ComplateCallback?.Invoke();
        }
    }

    public void SetLatestVersionText(string text)
    {
        m_latestVersionText.text = text;
    }
}