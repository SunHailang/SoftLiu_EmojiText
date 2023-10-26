using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PlayerBuildStep : IBuildStep
{
    static PlayerBuildStep()
    {
        EditorApplication.delayCall += _Init;
    }

    private static void _Init()
    {
        bool use = SessionState.GetBool("EditorUseAssetBundleLoader", true);
        Menu.SetChecked("HotFix/Editor Use AssetBundle Loader", use);
        InitBuildSettingScene(use);
    }

    private static void InitBuildSettingScene(bool use)
    {
        HashSet<string> defaultScene = new HashSet<string>()
        {
            "GameLaunch.unity", "GameUpdate.unity", "GameEntry.unity"
        };
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (File.Exists(scene.path))
            {
                string sceneName = Path.GetFileName(scene.path);
                if (use)
                {
                    scene.enabled = defaultScene.Contains(sceneName);
                }
                else
                {
                    scene.enabled = true;
                }
            }
        }

        Debug.Log($"PlayerBuildStep InitBuildSettingScene : {use}");
        // 这里重新赋值给 EditorBuildSettings.scenes ，否则设置失败！
        EditorBuildSettings.scenes = scenes;
    }

    [MenuItem("HotFix/Editor Use AssetBundle Loader")]
    public static void SetEditorUserAssetBundleLoader()
    {
        bool use = !Menu.GetChecked("HotFix/Editor Use AssetBundle Loader");
        Menu.SetChecked("HotFix/Editor Use AssetBundle Loader", use);
        SessionState.SetBool("EditorUseAssetBundleLoader", use);
        InitBuildSettingScene(use);
    }

    public void Execute(BuildTarget target, BuildType type, string path)
    {
        string[] scenes = GetScenes();
        BuildOptions options = GetOptions(target);

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