using System.IO;
using UnityEditor;
using UnityEngine;


namespace AssetBundleEditor
{
    public class AssetBuildStep : IAssetStep
    {
        public void Execute(BuildTarget buildTarget, string output)
        {
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            BuildPipeline.BuildAssetBundles(output, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);

            Debug.Log("BuildAssetBundles Complete!!!!!");
            Caching.ClearCache();
        }

        public BuildStepType GetBuildType()
        {
            return BuildStepType.Direct;
        }
    }
}