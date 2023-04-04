using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PerBuildStep : IBuildStep
{
    public static readonly Dictionary<BuildTarget, Dictionary<BuildType, string>> PreprocessorDefines = new Dictionary<BuildTarget, Dictionary<BuildType, string>>()
    {
        {
            BuildTarget.iOS, new Dictionary<BuildType, string>
            {
                {BuildType.Development, "SINGLE_APK;ENABLE_LOG;"},
                {BuildType.Preproduction, "SINGLE_APK;PREPRODUCTION;ENABLE_LOG;"},
                {BuildType.Production, "SINGLE_APK;PRODUCTION;"},
                {BuildType.Marketing, ";"}
            }
        },
        {
            BuildTarget.Android, new Dictionary<BuildType, string>
            {
                {BuildType.Development, "SINGLE_APK;ENABLE_LOG;"},
                {BuildType.Preproduction, "SINGLE_APK;PREPRODUCTION;ENABLE_LOG;"},
                {BuildType.Production, "SINGLE_APK;PRODUCTION;"},
                {BuildType.Marketing, ";"}
            }
        }
    };

    
    
    public void Execute(BuildTarget target, BuildType type, string path)
    {
        UpdatePreprocessorSymbols(target, type);
        if (target == BuildTarget.Android)
        {
            UpdateAndroid(target, type);
        }
        else if (target == BuildTarget.iOS)
        {
            
        }
    }

    public BuildStepType GetBuildType()
    {
        return BuildStepType.Direct;
    }
    
    private void UpdatePreprocessorSymbols(BuildTarget target, BuildType type)
    {
        BuildTargetGroup group = BuildTargetGroup.iOS;
        switch (target)
        {
            case BuildTarget.iOS:
                group = BuildTargetGroup.iOS;
                break;
            case BuildTarget.Android:
                group = BuildTargetGroup.Android;
                break;
            default:
                Debug.LogError("Build (PreBuildStep) Unknown Build Target - " + target);
                break;
        }
        string scriptingDefines = PreprocessorDefines[target][type];
        UnityEngine.Debug.LogFormat("PlayerSettings.SetScriptingDefineSymbolsForGroup({0}, {1})", group, scriptingDefines);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, scriptingDefines);
    }
    #region Android

    void UpdateAndroid(BuildTarget target, BuildType type)
    {
        //  Custom Android build step logic
        AndroidFillKeyStoreInfo();

        //  Check if SDK is setup
        CheckAndroidSKDPath();
        CheckOrientation(type);
        //  Check for OBB variant
        CheckForOBB(type);
        CheckForAndroidProject();

        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
        //PlayerSettings.strippingLevel = StrippingLevel.StripByteCode;

        //	Override ETC
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
    }
    
    void AndroidFillKeyStoreInfo()
    {
        string path = Path.Combine(Application.dataPath, "../JenkinsScripts/softliu.keystore");
        //PlayerSettings.Android.keystoreName = path;
        PlayerSettings.keystorePass = "123456";
        PlayerSettings.keyaliasPass = "123456";
    }
    
    void CheckAndroidSKDPath()
    {
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        if (sdkPath == null || sdkPath.Length == 0)
        {
            throw new Exception("Android SDK path is not set!!!");
        }
    }

    void CheckForOBB(BuildType type)
    {
        //  AAB FILES ARE NOW IN FASION - NO OBBs ANYMORE
        PlayerSettings.Android.useAPKExpansionFiles = false;

#if !SINGLE_APK
        //  Just in case we have to revert it
        PlayerSettings.Android.useAPKExpansionFiles = (type != BuildType.Development);
#endif
    }
    
    void CheckOrientation(BuildType type)
    {
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
    }
    
    void CheckForAndroidProject()
    {
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
    }

    #endregion
}