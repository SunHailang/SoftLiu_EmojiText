using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{
    public class AssetDatasStep : IAssetStep
    {
        public void Execute(BuildTarget buildTarget, string output)
        {
            
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Post;
        }

        [MenuItem("HotFix/AssetBundle/Copy Datas")]
        public static void CopyDatas()
        {
            
        }

        private static void CopyDatasToRes(string output)
        {
            string originPath = Path.Combine(Application.dataPath, "../Datas/");

            string targetPath = Path.Combine(output, "../");
            
        }
    }
}