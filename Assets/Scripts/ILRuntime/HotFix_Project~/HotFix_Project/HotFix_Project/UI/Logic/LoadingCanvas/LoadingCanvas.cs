using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HotFix_Project.UI
{
    public class LoadingCanvas : UICore.UiBase
    {
        [SerializeField] private Slider m_processSlider = null;
        [SerializeField] private Text m_processText = null;

        private string m_curLoadingScene = "";

        public override void Initialization(GameObject go)
        {
            base.Initialization(go);
            m_processSlider = m_curGo.transform.Find("Current/ProcessSlider").GetComponent<Slider>();
            m_processText = m_curGo.transform.Find("Current/ProcessText").GetComponent<Text>();
        }

        public void SetLoadingScene(string sceneName)
        {
            m_curLoadingScene = sceneName;
            SetSliderValue(0);
        }

        public override void OnShow(Action callback = null)
        {
            base.OnShow(callback);

            HotFixMonoBehaviour.Instance.DoCoroutine(LoadingAssetScene());
        }

        private System.Collections.IEnumerator LoadingAssetScene()
        {
            yield return null;

            yield return ResourceLoaderProxy.GetInstance().LoadSceneAssetAsync(m_curLoadingScene, (success) =>
            {
                Debug.Log($"[LoadingCanvas]  LoadSceneAssetAsync Success: {success}");
                if (success)
                {
                    HotFixMonoBehaviour.Instance.DoCoroutine(LoadingScene());
                }
            });
        }

        private System.Collections.IEnumerator LoadingScene()
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(m_curLoadingScene, LoadSceneMode.Additive);
            async.allowSceneActivation = false;
            Debug.Log($"[LoadingCanvas]  LoadingScene Success: {async.progress}");
            while (!async.isDone)
            {
                yield return null;
                float asyncValue = async.progress;
                Debug.Log($"[LoadingCanvas]  LoadingScene Success: {async.progress}");
                SetSliderValue(asyncValue);
                if (asyncValue >= 0.9f)
                {
                    break;
                }
            }
            float value = m_processSlider.value;
            Debug.Log($"[LoadingCanvas]  LoadingScene Success: {value}");
            while (value < 1.0f)
            {
                value += Time.deltaTime * 0.005f;
                SetSliderValue(value);
                if(value > 0.95f)
                {
                    async.allowSceneActivation = true;
                }
                yield return null;
            }
            SetSliderValue(1f);
            // 销毁 loading
            UIManager.Instance.CloseLoadingScene();
        }

        private void SetSliderValue(float value)
        {
            m_processSlider.value = value;
            m_processText.text = $"{(value * 100):F2}";
        }

    }
}
