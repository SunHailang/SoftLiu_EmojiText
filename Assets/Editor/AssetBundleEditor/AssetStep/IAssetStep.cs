using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetBundleEditor
{
    public interface IAssetStep
    {
        void Execute(BuildTarget buildTarget, string output);
        BuildStepType GetBuildType();
    }
}