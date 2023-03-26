using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AssetBundleEditor
{
    public class AssetBundleProcess
    {
        private static readonly List<IAssetStep> m_assetSteps = null;

        static AssetBundleProcess()
        {
            m_assetSteps = new List<IAssetStep>();
            m_assetSteps.Add(new AssetLabelStep());
            m_assetSteps.Add(new AssetBuildStep());
            m_assetSteps.Add(new AssetVersionStep());
            m_assetSteps.Add(new AssetDatasStep());
        }


        public static void Excute(string output, BuildTarget target)
        {
            foreach (IAssetStep step in GetStepSorted())
            {
                step.Execute(target, output);
            }
        }

        private static List<IAssetStep> GetBuildSteps(BuildStepType type)
        {
            return m_assetSteps.Where(s => s.GetBuildType() == type).ToList();
        }

        private static List<IAssetStep> GetStepSorted()
        {
            List<IAssetStep> steps = new List<IAssetStep>();
            steps.AddRange(GetBuildSteps(BuildStepType.Pre));
            steps.AddRange(GetBuildSteps(BuildStepType.Direct));
            steps.AddRange(GetBuildSteps(BuildStepType.Post));
            return steps;
        }
    }
}