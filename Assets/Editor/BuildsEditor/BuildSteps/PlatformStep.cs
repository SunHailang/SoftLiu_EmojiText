
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

public class PlatformStep : IBuildStep
{
    public void Execute(BuildTarget target, BuildType type, string path)
    {
        switch (target)
        {
            case BuildTarget.Android:
                if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Android)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                }
                break;
            case BuildTarget.iOS:
                if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.iOS)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS, BuildTarget.iOS);
                }
                break;
            default:
                if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                }
                break;
        }
        

        Debug.Log($"{EditorUserBuildSettings.selectedBuildTargetGroup} hahahha");
    }

    public BuildStepType GetBuildType()
    {
        return BuildStepType.Pre;
    }
}