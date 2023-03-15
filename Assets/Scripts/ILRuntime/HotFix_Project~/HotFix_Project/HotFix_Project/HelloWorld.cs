
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
            UnityEngine.Debug.Log($"Hello World! TestMethod, I am Int Method: {param}");
        }

        public static void StaticTestMethod(string param)
        {
            UnityEngine.Debug.Log($"Hello World! TestMethod, I am String Metho: {param}");
        }
    }
}
