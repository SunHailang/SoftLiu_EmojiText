
namespace HotFix_Project
{
    public class HelloWorld
    {
        public static void StaticTestMethod()
        {
            UnityEngine.Debug.Log($"Hello World! TestMethod");
        }
        public static void StaticTestMethod(int param)
        {
            UnityEngine.Debug.Log($"Hello World! TestMethod : {param}");
        }
    }
}
