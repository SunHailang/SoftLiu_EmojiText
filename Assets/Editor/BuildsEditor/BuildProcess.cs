using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuildInfoData
{
    public string BuildOutputPath { get; private set; }

    public void SetBuildOutputPath(string path)
    {
        BuildOutputPath = path;
        EditorPrefs.SetString("OutputPath", BuildOutputPath);
    }
    public BuildTargetGroup BuildTarget { get; private set; } = BuildTargetGroup.Standalone;

    public void SetBuildTarget(BuildTargetGroup target)
    {
        BuildTarget = target;
    }
    public BuildType BuildType{ get; private set; } = BuildType.Development;

    public void SetBuildType(BuildType type)
    {
        BuildType = type;
    }
    public bool ExportAndroidProject{ get; private set; } = false;

    public void SetExportAndroidProject(bool isExport)
    {
        ExportAndroidProject = isExport;
    }
}

public static class BuildProcess
{

    public static readonly BuildInfoData BuildInfoData;
    
    private static readonly List<IBuildStep> m_steps;

    static BuildProcess()
    {
        BuildInfoData = new BuildInfoData();
        string output = EditorPrefs.GetString("OutputPath", "");
        if (string.IsNullOrEmpty(output))
        {
            output = Path.Combine(Application.dataPath, "../Builds/");
            output = Path.GetFullPath(output).Replace('\\', '/');
            EditorPrefs.SetString("OutputPath", output);
        }
        BuildInfoData.SetBuildOutputPath(output);
        
        m_steps = new List<IBuildStep>();
        m_steps.Add(new PlatformStep());
        
        m_steps.Add(new PerBuildStep());
        m_steps.Add(new PlayerBuildStep());
    }

    public static void Excute(BuildTarget target, BuildType type)
    {
        string path = GetBuildPath(target, type);
        Debug.Log("Marking build " + target + " to " + path);
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog("Compiling", "Please wait for the Editor to finish compiling.", "OK");
            return;
        }

        // Execute all steps in sequence
        BuildStepExecutor.Execute(GetStepSorted(), target, type, path);
    }

    private static string GetBuildPath(BuildTarget target, BuildType type)
    {
        string path = Path.Combine(BuildInfoData.BuildOutputPath, $"{target}/{type}/");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string absolutePath = Path.GetFullPath(path);
        return absolutePath.Replace('\\', '/');
    }

    private static List<IBuildStep> GetBuildSteps(BuildStepType type)
    {
        return m_steps.Where(s => s.GetBuildType() == type).ToList();
    }

    private static List<IBuildStep> GetStepSorted()
    {
        List<IBuildStep> steps = new List<IBuildStep>();
        steps.AddRange(GetBuildSteps(BuildStepType.Pre));
        steps.AddRange(GetBuildSteps(BuildStepType.Direct));
        steps.AddRange(GetBuildSteps(BuildStepType.Post));
        return steps;
    }
}