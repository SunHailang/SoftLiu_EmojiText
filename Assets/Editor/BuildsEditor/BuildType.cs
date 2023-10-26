using UnityEditor;
using UnityEngine;

/// <summary>
/// Possible build types
/// </summary>
public enum BuildType
{
    /// <summary>
    /// 开发版本
    /// </summary>
    Development,
    /// <summary>
    /// 预发布版本
    /// </summary>
    Preproduction,
    /// <summary>
    /// 正式版本
    /// </summary>
    Production,
    Marketing
}