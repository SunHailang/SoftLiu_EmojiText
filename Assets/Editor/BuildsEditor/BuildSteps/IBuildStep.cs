using UnityEditor;

public enum BuildStepType
{
    Pre, //  Executed before player
    Post, //  Executed after player
    Direct //  Used only for player building (not executed in manual builds)
}

public interface IBuildStep
{
    void Execute(BuildTarget target, BuildType type, string path);
    BuildStepType GetBuildType();
}