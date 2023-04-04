
using UnityEngine;

public class HotFixMonoBehaviour : AutoGeneratedMonoSingleton<HotFixMonoBehaviour>
{
    private const string typeStr = "HotFix_Project.MonoBehaviourController";

    #region 生命周期函数

    private void Awake()
    {
        // 预先获得IMethod，可以减低每次调用查找方法耗用的时间
        // InstantiateType = HotFixMgr.Instance.AppDomain.LoadedTypes[typeStr];
        // 第二种方式
        // InstantiateObj = ((ILType) InstantiateType).Instantiate();

        CallMethod("Awake");
    }

    private  void OnEnable()
    {
        CallMethod("OnEnable");
    }

    private void Start()
    {
        CallMethod("Start");
    }

    private void Update()
    {
        CallMethod("Update");
    }

    private void FixedUpdate()
    {
        CallMethod("FixedUpdate");
    }

    private void LateUpdate()
    {
        CallMethod("LateUpdate");
    }

    private void OnDisable()
    {
        CallMethod("OnDisable");
    }

    private void OnDestroy()
    {
        CallMethod("OnDestroy");
        HotFixMgr.Instance.Dispose();
    }
    
    #endregion

    #region Public 函数
    
    public void DoCoroutine(string coroutineName)
    {
        StartCoroutine(coroutineName);
    }
    public void DoCoroutine(System.Collections.IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void DontCoroutine(string coroutineName)
    {
        StopCoroutine(coroutineName);
    }

    public void DontCoroutine(System.Collections.IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }

    public void DontAllCoroutines()
    {
        StopAllCoroutines();
    }

    #endregion
    
    private void CallMethod(string methodName)
    {
        HotFixMgr.Instance.AppDomain.Invoke(typeStr, methodName, null, null);
    }
}