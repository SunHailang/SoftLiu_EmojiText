﻿// A Monolithic state keeper class

public class UnityAutoGeneratedSingleton<T> : System.IDisposable where T : UnityAutoGeneratedSingleton<T>, new()
{
    private static T m_instance = null;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (m_instance == null)
                {
                    m_instance = new T();
                }

                return m_instance;
            }
        }
    }

    protected bool IsDispose = false;
    
    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!IsDispose)
        {
            if (disposing)
            {
                // 释放托管资源
                DisposeManagedResources();
                m_instance = null;
            }
            // 释放非托管资源
            DisposeUnManagedResources();
            IsDispose = true;
        }
    }
    protected virtual void DisposeManagedResources()
    {
        // 释放托管资源
    }

    protected virtual void DisposeUnManagedResources()
    {
        // 释放非托管资源
    }
}