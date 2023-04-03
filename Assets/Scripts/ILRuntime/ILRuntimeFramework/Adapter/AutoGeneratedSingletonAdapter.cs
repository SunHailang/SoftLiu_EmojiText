using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class AutoGeneratedSingletonAdapter<T> : CrossBindingAdaptor where T : UnityAutoGeneratedSingleton<T>, new()
{
    public override Type BaseCLRType => typeof(UnityAutoGeneratedSingleton<T>);
    public override Type AdaptorType => typeof(Adapter);

    public override Type[] BaseCLRTypes
    {
        get
        {
            return new[] {typeof(System.IDisposable)};
        }
    }

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adapter(appdomain, instance);
    }
    
    internal class Adapter : UnityAutoGeneratedSingleton<T>, CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        
        //必须要提供一个无参数的构造函数
        public Adapter()
        {

        }
        
        public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public ILTypeInstance ILInstance => instance;
        
        
        IMethod mInstanceMethod;
        bool mInstanceMethodGot;
        public object Instance
        {
            get
            {
                if (!mInstanceMethodGot)
                {
                    mInstanceMethod = instance.Type.GetMethod("get_Instance", 0);
                    if (mInstanceMethod == null)
                    {
                        mInstanceMethod = instance.Type.GetMethod("UnityAutoGeneratedSingleton.get_Instance", 0);
                    }
                    mInstanceMethodGot = true;
                }

                if (mInstanceMethod != null)
                {
                    var res = appdomain.Invoke(mInstanceMethod, instance, null);
                    return res;
                }
                else
                {
                    return null;
                }
            }
        }
        IMethod mDisposeManagedMethod;
        bool mDisposeManagedMethodGot;
        protected virtual void DisposeManagedResources()
        {
            // 释放托管资源
            if (!mDisposeManagedMethodGot)
            {
                mDisposeManagedMethod = instance.Type.GetMethod("DisposeManagedResources", 0);
                if (mDisposeManagedMethod == null)
                {
                    mDisposeManagedMethod = instance.Type.GetMethod("UnityAutoGeneratedSingleton.DisposeManagedResources", 0);
                }
                mDisposeManagedMethodGot = true;
            }

            if (mDisposeManagedMethod != null)
            {
                appdomain.Invoke(mDisposeManagedMethod, instance, null);
            }
        }
        IMethod mDisposeUnManagedMethod;
        bool mDisposeUnManagedMethodGot;
        protected virtual void DisposeUnManagedResources()
        {
            // 释放非托管资源
            if (!mDisposeUnManagedMethodGot)
            {
                mDisposeUnManagedMethod = instance.Type.GetMethod("DisposeUnManagedResources", 0);
                if (mDisposeUnManagedMethod == null)
                {
                    mDisposeUnManagedMethod = instance.Type.GetMethod("UnityAutoGeneratedSingleton.DisposeUnManagedResources", 0);
                }
                mDisposeUnManagedMethodGot = true;
            }

            if (mDisposeUnManagedMethod != null)
            {
                appdomain.Invoke(mDisposeUnManagedMethod, instance, null);
            }
        }
        
    }
}