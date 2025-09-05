using UnityEngine;
using System.Diagnostics;

/// <summary>
/// 《字里人间》统一日志管理器
/// 
/// 使用说明：
/// - LogDev: 开发环境专用日志，Release版本完全移除
/// - LogError: 错误日志，所有环境都保留
/// - LogWarning: 警告日志，可通过DISABLE_WARNINGS控制
/// - LogUser: 用户相关重要日志，保留用于问题追踪
/// - LogSystem: 系统关键操作日志，可配置
/// </summary>
public static class GameLogger
{
    // 全局日志开关配置
    private static readonly bool ENABLE_USER_LOGS = true;
    private static readonly bool ENABLE_SYSTEM_LOGS = false;  // Release版本建议关闭

    #region 开发环境专用日志 (Release版本完全移除)
    
    /// <summary>
    /// 开发环境专用日志 - Release版本完全移除，零性能开销
    /// 用于：调试信息、详细操作流程、状态切换等
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogDev(string message)
    {
        UnityEngine.Debug.Log($"[DEV] {message}");
    }

    /// <summary>
    /// 开发环境专用日志 - 带对象上下文
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogDev(string message, Object context)
    {
        UnityEngine.Debug.Log($"[DEV] {message}", context);
    }

    /// <summary>
    /// 开发环境专用日志 - 格式化消息
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogDevFormat(string format, params object[] args)
    {
        UnityEngine.Debug.Log($"[DEV] {string.Format(format, args)}");
    }

    #endregion

    #region 错误日志 (始终保留)

    /// <summary>
    /// 错误日志 - 始终保留，用于追踪严重问题
    /// 用于：组件缺失、资源加载失败、系统崩溃等
    /// </summary>
    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError($"[ERROR] {message}");
    }

    /// <summary>
    /// 错误日志 - 带对象上下文
    /// </summary>
    public static void LogError(string message, Object context)
    {
        UnityEngine.Debug.LogError($"[ERROR] {message}", context);
    }

    /// <summary>
    /// 错误日志 - 格式化消息
    /// </summary>
    public static void LogErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogError($"[ERROR] {string.Format(format, args)}");
    }

    /// <summary>
    /// 异常日志
    /// </summary>
    public static void LogException(System.Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }

    /// <summary>
    /// 异常日志 - 带上下文
    /// </summary>
    public static void LogException(System.Exception exception, Object context)
    {
        UnityEngine.Debug.LogException(exception, context);
    }

    #endregion

    #region 警告日志 (可配置)

    /// <summary>
    /// 警告日志 - 可通过DISABLE_WARNINGS控制
    /// 用于：非致命问题、性能警告、配置问题等
    /// </summary>
    public static void LogWarning(string message)
    {
#if !DISABLE_WARNINGS
        UnityEngine.Debug.LogWarning($"[WARNING] {message}");
#endif
    }

    /// <summary>
    /// 警告日志 - 带对象上下文
    /// </summary>
    public static void LogWarning(string message, Object context)
    {
#if !DISABLE_WARNINGS
        UnityEngine.Debug.LogWarning($"[WARNING] {message}", context);
#endif
    }

    /// <summary>
    /// 警告日志 - 格式化消息
    /// </summary>
    public static void LogWarningFormat(string format, params object[] args)
    {
#if !DISABLE_WARNINGS
        UnityEngine.Debug.LogWarning($"[WARNING] {string.Format(format, args)}");
#endif
    }

    #endregion

    #region 用户相关日志 (重要操作保留)

    /// <summary>
    /// 用户相关日志 - 用于追踪用户操作和问题
    /// 用于：关卡完成、存档操作、重要UI交互等
    /// </summary>
    public static void LogUser(string message)
    {
        if (ENABLE_USER_LOGS)
        {
            UnityEngine.Debug.Log($"[USER] {message}");
        }
    }

    /// <summary>
    /// 用户相关日志 - 格式化消息
    /// </summary>
    public static void LogUserFormat(string format, params object[] args)
    {
        if (ENABLE_USER_LOGS)
        {
            UnityEngine.Debug.Log($"[USER] {string.Format(format, args)}");
        }
    }

    #endregion

    #region 系统日志 (可配置的重要系统操作)

    /// <summary>
    /// 系统日志 - 重要系统操作，可配置
    /// 用于：场景切换、资源管理、关键初始化等
    /// </summary>
    public static void LogSystem(string message)
    {
        if (ENABLE_SYSTEM_LOGS)
        {
            UnityEngine.Debug.Log($"[SYSTEM] {message}");
        }
    }

    /// <summary>
    /// 系统日志 - 格式化消息
    /// </summary>
    public static void LogSystemFormat(string format, params object[] args)
    {
        if (ENABLE_SYSTEM_LOGS)
        {
            UnityEngine.Debug.Log($"[SYSTEM] {string.Format(format, args)}");
        }
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 检查是否启用开发日志
    /// </summary>
    public static bool IsDevLogEnabled()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// 获取当前日志配置信息
    /// </summary>
    public static string GetLogConfig()
    {
        return $"日志配置 - Dev: {IsDevLogEnabled()}, User: {ENABLE_USER_LOGS}, System: {ENABLE_SYSTEM_LOGS}, Warnings: {!IsWarningsDisabled()}";
    }

    private static bool IsWarningsDisabled()
    {
#if DISABLE_WARNINGS
        return true;
#else
        return false;
#endif
    }

    #endregion
}
