using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class PlayerBuildStep : IBuildStep
{
    public void Execute(BuildTarget target, BuildType type, string path)
    {
        string[] scenes = GetScenes();
        BuildOptions options = GetOptions(target);

#if SINGLE_APK
        switch (target)
        {
            case BuildTarget.Android:
                path = Path.Combine(path, "build.apk");
                break;
            case BuildTarget.iOS:
                path = Path.Combine(path, "build.ipa");
                break;
            case BuildTarget.StandaloneWindows:
                path = Path.Combine(path, "build.exe");
                break;
        }
#endif

        BuildPipeline.BuildPlayer(scenes, path, target, options);
    }

    public BuildStepType GetBuildType()
    {
        return BuildStepType.Direct;
    }

    private BuildOptions GetOptions(BuildTarget target)
    {
        BuildOptions options = BuildOptions.None;

        if (target == BuildTarget.Android)
        {
            //options |= BuildOptions.AcceptExternalModificationsToPlayer;
        }
#if !PRODUCTION && !PRE_PRODUCTION
        options |= BuildOptions.Development;
#endif

        return options;
    }

    private string[] GetScenes()
    {
        List<string> toRet = new List<string>();

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && File.Exists(scene.path))
            {
                toRet.Add(scene.path);
            }
        }

        return toRet.ToArray();
    }
}