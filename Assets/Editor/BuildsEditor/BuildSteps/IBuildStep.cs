using UnityEditor;

public enum BuildStepType
{
    /// <summary>
    /// Executed before player
    /// </summary>
    Pre,
    /// <summary>
    /// Used only for player building (not executed in manual builds)
    /// </summary>
    Direct,
    /// <summary>
    /// Executed after player
    /// </summary>
    Post,
}

public interface IBuildStep
{
    void Execute(BuildTarget target, BuildType type, string path);
    BuildStepType GetBuildType();
}