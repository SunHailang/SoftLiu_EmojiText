using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;

#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

public class HotFixMgr : AutoGeneratedMonoSingleton<HotFixMgr>
{
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    private ILRuntime.Runtime.Enviorment.AppDomain m_appdomain = null;
    public ILRuntime.Runtime.Enviorment.AppDomain AppDomain => m_appdomain;

    private System.IO.MemoryStream m_fs = null;
    private System.IO.MemoryStream m_p = null;

    public void Init()
    {
        
    }
    
    private IEnumerator Start()
    {
        return LoadHotFixAssembly();
    }

    private IEnumerator LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        m_appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //正常项目中应该是自行从其他地方下载dll，或者打包在AssetBundle中读取，平时开发以及为了演示方便直接从StreammingAssets中读取，
        //正式发布的时候需要大家自行从其他地方读取dll

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //这个DLL文件是直接编译HotFix_Project.sln生成的，已经在项目中设置好输出目录为StreamingAssets，在VS里直接编译即可生成到对应目录，无需手动拷贝
        //工程目录在Assets\Scripts\ILRuntime\HotFix_Project~\HotFix_Project

        string abPath = "";
        //以下加载写法只为演示，并没有处理在编辑器切换到Android平台的读取，需要自行修改
#if UNITY_ANDROID && !UNITY_EDITOR
        abPath = Path.Combine(Application.persistentDataPath, "");
#elif UNITY_IPHONE && !UNITY_EDITOR
        abPath = Path.Combine(Application.persistentDataPath, "");
#else
        abPath = System.IO.Path.Combine(Application.dataPath, "../Datas/HotFix_Project/");
#endif
        using (FileStream fs = new FileStream(Path.Combine(abPath, "HotFix_Project.dll.bytes"), FileMode.Open, FileAccess.Read))
        {
            int length = (int) fs.Length;
            byte[] dll = new byte[fs.Length];
            fs.Read(dll, 0, (int) fs.Length);
            m_fs = new System.IO.MemoryStream(dll);
        }
        
#if UNICOM_DEBUG
        // 加载 PDB 文件
        using (FileStream fs = new FileStream(Path.Combine(abPath, "HotFix_Project.pdb.bytes"), FileMode.Open, FileAccess.Read))
        {
            int length = (int) fs.Length;
            byte[] pdb = new byte[fs.Length];
            fs.Read(pdb, 0, (int) fs.Length);
            m_p = new System.IO.MemoryStream(pdb);
        }
#endif
        yield return null;
        try
        {
            m_appdomain.LoadAssembly(m_fs, m_p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch(Exception msg)
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Scripts/HotFix_Project~/HotFix_Project/HotFix_Project.sln编译过热更DLL");
            Debug.LogError(msg.Message);
        }

        InitializeILRuntime();
        OnHotFixLoaded();
    }

    private void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        m_appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        //这里做一些ILRuntime的注册
        m_appdomain.RegisterCrossBindingAdaptor(new HotFixMonoBehaviourAdapter());

        //这里我们对LitJson进行注册
        LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(m_appdomain);

        SetupCLRRedirection();

        m_appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
        m_appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        m_appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
    }

    private bool m_isInitDomain = false;
    private void OnHotFixLoaded()
    {
        m_isInitDomain = true;
        MonoBehaviourManger.Instance.Awake();
        MonoBehaviourManger.Instance.OnEnable();
        MonoBehaviourManger.Instance.Start();
    }

    private void Update()
    {
        if (m_isInitDomain)
        {
            MonoBehaviourManger.Instance.Update();
        }
    }

    private void FixedUpdate()
    {
        if (m_isInitDomain)
        {
            MonoBehaviourManger.Instance.FixedUpdate();
        }
    }

    private void LateUpdate()
    {
        if (m_isInitDomain)
        {
            MonoBehaviourManger.Instance.LateUpdate();
        }
    }

    private void OnDisable()
    {
        if (m_isInitDomain)
        {
            MonoBehaviourManger.Instance.OnDisable();
        }
    }

    private void OnDestroy()
    {
        MonoBehaviourManger.Instance.OnDestroy();
        m_appdomain = null;
        if (m_fs != null) m_fs.Close();
        if (m_p != null) m_p.Close();
        m_fs = null;
        m_p = null;
        m_isInitDomain = false;
    }

    #region 适配函数

    unsafe void SetupCLRRedirection()
    {
        var arr = typeof(GameObject).GetMethods();
        // AddComponent
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                m_appdomain.RegisterCLRMethodRedirection(i, AddComponent);
            }
        }

        // GetComponent
        foreach (var i in arr)
        {
            if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
            {
                m_appdomain.RegisterCLRMethodRedirection(i, GetComponent);
            }
        }
    }

    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        StackObject* ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        IType[] genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                var ilInstance = new ILTypeInstance(type as ILType, false); //手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                //接下来创建Adapter实例
                var clrInstance = instance.AddComponent<HotFixMonoBehaviourAdapter.Adaptor>();
                //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance

                clrInstance.Awake(); //因为Unity调用这个方法时还没准备好所以这里补调一次
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        StackObject* ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        IType[] genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                var clrInstances = instance.GetComponents<HotFixMonoBehaviourAdapter.Adaptor>();
                for (int i = 0; i < clrInstances.Length; i++)
                {
                    var clrInstance = clrInstances[i];
                    if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    #endregion
}