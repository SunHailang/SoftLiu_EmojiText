using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class BuildProcess
{
    private static readonly List<IBuildStep> m_steps;

    static BuildProcess()
    {
        m_steps = new List<IBuildStep>();
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
        string path = Path.Combine(Application.dataPath, $"../Builds/{target}/{type}/");
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