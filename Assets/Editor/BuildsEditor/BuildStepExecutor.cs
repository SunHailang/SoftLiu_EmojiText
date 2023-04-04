using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildStepExecutor
{
    public static void Execute(List<IBuildStep> steps, BuildTarget target, BuildType type, string path)
    {
        Debug.Log("Starting build step executor...");
        DateTime totalStart = DateTime.Now;

        try
        {
            int current = 1;
            foreach (IBuildStep step in steps)
            {
                DateTime start = DateTime.Now;
                string index = current + "/" + steps.Count;
                Debug.Log("Build step: " + index + " " + step + " - Starting...");
                EditorUtility.DisplayProgressBar("Custom build step " + index, step.ToString(), (current / (float) steps.Count));

                // Execute step
                step.Execute(target, type, path);
                double secondsPassed = (DateTime.Now - start).TotalSeconds;
                Debug.Log("Build step: " + index + " " + step + " - Done. Took : " + secondsPassed.ToString("0.00") + " seconds");
                current++;
            }
        }
        catch (Exception e)
        {
            throw new Exception("BuildExecutor failed.", e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        double totalSecondsPassed = (DateTime.Now - totalStart).TotalSeconds;
        Debug.Log("Ending build step execuor... Took " + totalSecondsPassed.ToString("0.00") + " seconds");
    }
}