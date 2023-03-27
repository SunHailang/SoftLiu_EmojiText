using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

public class IDisposableAdapter : CrossBindingAdaptor
{
    public override System.Type BaseCLRType => typeof(System.IDisposable);
    public override System.Type AdaptorType => typeof(Adaptor);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance); //创建一个新的实例
    }

    /// <summary>
    /// 实际的适配器类需要继承你想继承的那个类，并且实现CrossBindingAdaptorType接口
    /// </summary>
    class Adaptor : System.IDisposable, CrossBindingAdaptorType
    {
        private ILTypeInstance m_instance;
        private AppDomain m_appdomain;

        private IMethod m_DisposeMethod = null;
        private bool m_DisposeMethodGot = false;
        
        public Adaptor(AppDomain appdomain, ILTypeInstance instance)
        {
            this.m_appdomain = appdomain;
            this.m_instance = instance;
        }

        public ILTypeInstance ILInstance => m_instance;

        public void Dispose()
        {
            if (!m_DisposeMethodGot)
            {
                m_DisposeMethod = m_instance.Type.GetMethod("Dispose", 0);
                m_DisposeMethodGot = true;
            }
            if (m_DisposeMethod != null)
            {
                // 没有参数建议显式传递null为参数列表，否则会自动new object[0]导致GC Alloc
                m_appdomain.Invoke(m_DisposeMethod, m_instance, null);
            }
        }
        
    }
}