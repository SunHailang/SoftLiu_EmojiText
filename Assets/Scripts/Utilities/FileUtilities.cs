using System.IO;
using UnityEngine;

public static class FileUtilities
{
    public static bool IsExistsDirectory(string dirPath, bool create = true)
    {
        if (!Directory.Exists(dirPath))
        {
            if (create)
            {
                Directory.CreateDirectory(dirPath);
                Debug.Log($"[FileUtilities] IsExistsDirectory Create:{dirPath}");
            }
            return false;
        }
        return true;
    }
}