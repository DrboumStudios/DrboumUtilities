#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;


public static class LogHelper {
    private const string _WITH_TYPE_PREFIX_DEFAULT = "of type";
    public static bool LogIfInvalidRequiredField<TExecutor, TObject>(this TExecutor _, TObject authoring, string invalidFieldName, string prefixCategory)
        where TObject : Object
    {
        if ( authoring.IsNull() )
        {
            _.LogInvalidRequiredField(authoring, invalidFieldName, prefixCategory);
            return true;
        }
        return false;
    }
    public static void LogInvalidRequiredField<TExecutor>(this TExecutor _, Object authoring, string invalidFieldName, string prefixCategory)
    {
        LogErrorMessage($"the required field {invalidFieldName} is not filled or null", $"{prefixCategory} : {typeof(TExecutor).Name}", authoring);
    }
    [Conditional("UNITY_EDITOR")]
    public static void LogInfoMessage(string message, string category = "", object context = null)
    {
        Debug.Log(BuildStringMessage(category, message), context as Object);
    }
    [Conditional("UNITY_EDITOR")]
    public static void LogInfoTypedMessage<T>(in T message, string category = "",
        string prefixMessage = _WITH_TYPE_PREFIX_DEFAULT, object context = null)
    {
        Debug.Log(BuildTypedMessage(message, category, prefixMessage), context as Object);
    }
    [Conditional("DEBUG")]
    public static void LogDebugMessage(string message, string category = "", object context = null)
    {
        Debug.Log(BuildStringMessage(category, message), context as Object);
    }

    [Conditional("DEBUG")]
    public static void LogDebugTypedMessage<T>(in T message, string category = "",
        string prefixMessage = _WITH_TYPE_PREFIX_DEFAULT, object context = null)
    {
        Debug.Log(BuildTypedMessage(message, category, prefixMessage), context as Object);
    }
    [Conditional("DEBUG")]
    public static void LogDebugWarningMessage(string message, string category, object context = null)
    {
        Debug.LogWarning(BuildStringMessage(category, message), context as Object);
    }

    [Conditional("DEBUG")]
    public static void LogDebugWarningTypedMessage<T>(in T message, string category = "",
        string prefixMessage = _WITH_TYPE_PREFIX_DEFAULT)
    {
        Debug.LogWarning(BuildTypedMessage(message, category, prefixMessage), null);
    }

    public static void LogErrorMessage(string message, string category, object context = null)
    {
        Debug.LogError(BuildStringMessage(category, message), context as Object);
    }
    public static void LogErrorNullMessage(string message, string category, object context = null)
    {
        Debug.LogError(BuildStringMessage(category, $"null reference: {message}"), context as Object);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string BuildStringMessage(string category, string message,
        string prefixMessage = "")
    {
        string cat = string.IsNullOrEmpty(category) ? "" : $"[{category}]-> ";
        return $"{cat} {prefixMessage} {message}";
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string BuildTypedMessage<T>(T message, string category = "",
        string prefixMessage = _WITH_TYPE_PREFIX_DEFAULT)
    {
        return BuildStringMessage(category,
            $"{prefixMessage} {_WITH_TYPE_PREFIX_DEFAULT} {typeof(T).Name} click log to read the contents ->\n{ColorMessage(message.ToString(), nameof(Color.yellow))} ");
    }
    [Conditional("DEBUG")]
    public static void LogTime(string message)
    {
        Debug.Log($"[TimeSinceStartup: {Time.realtimeSinceStartup}]  {message}", null);
    }
    [Conditional("DEBUG")]
    public static void LogTime(double elapsedTime, string message)
    {
        Debug.Log($"[Time: {elapsedTime}] {message}", null);
    }

    public static string ColorMessage(string message, string colorName)
    {
        return $"<color={colorName}>{message}</color>";
    }
    /// <summary>
    ///     For debugging runtime only
    /// </summary>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    /// <param name="range"></param>
    /// <param name="fieldOfView"></param>
    /// <param name="coneColor"></param>
    /// <param name="targetOutside"></param>
    /// <param name="condition"></param>
    [Conditional("DEBUG")]
    public static void DrawWiredCone(Vector3 position, Vector3 forward, float range, float fieldOfView, Color coneColor,
        Color targetOutside, bool condition)
    {
        Color current = condition ? coneColor : targetOutside;
        ConstructWiredConePoints(forward, range, fieldOfView, out Vector3 upRayOffset, out Vector3 downRayOffset, out Vector3 rightOffset, out Vector3 leftRayOffset,
            out Vector3 forwardRayOffset);

        RenderWiredCone(position, upRayOffset, downRayOffset, rightOffset, leftRayOffset, forwardRayOffset, current);
    }
    [Conditional("DEBUG")]
    public static void DrawWiredCone(Vector3 position, Vector3 forward, float range, float fieldOfView, Color coneColor)
    {
        ConstructWiredConePoints(forward, range, fieldOfView, out Vector3 upRayOffset, out Vector3 downRayOffset, out Vector3 rightOffset, out Vector3 leftRayOffset,
            out Vector3 forwardRayOffset);

        RenderWiredCone(position, upRayOffset, downRayOffset, rightOffset, leftRayOffset, forwardRayOffset, coneColor);
    }
    [Conditional("DEBUG")]
    public static void RenderWiredCone(Vector3 position, Vector3 upRayOffset, Vector3 downRayOffset, Vector3 rightOffset, Vector3 leftRayOffset,
        Vector3 forwardRayOffset, Color renderColor)
    {
        DrawRay(position, upRayOffset, renderColor);
        DrawRay(position, downRayOffset, renderColor);
        DrawRay(position, rightOffset, renderColor);
        DrawRay(position, leftRayOffset, renderColor);
        DrawLine(position + downRayOffset, position + rightOffset, renderColor);
        DrawLine(position + rightOffset, position + upRayOffset, renderColor);
        DrawLine(position + upRayOffset, position + leftRayOffset, renderColor);
        DrawLine(position + leftRayOffset, position + downRayOffset, renderColor);
        DrawLine(position + forwardRayOffset, position + upRayOffset, renderColor);
        DrawLine(position + forwardRayOffset, position + downRayOffset, renderColor);
        DrawLine(position + forwardRayOffset, position + leftRayOffset, renderColor);
        DrawLine(position + forwardRayOffset, position + rightOffset, renderColor);
    }

    private static void DrawLine(Vector3 start, Vector3 end, Color renderColor)
    {
        Debug.DrawLine(start, end, renderColor, 0, true);

    }
    private static void DrawRay(Vector3 position, Vector3 upRayOffset, Color renderColor)
    {
        Debug.DrawRay(position, upRayOffset, renderColor, 0, true);
    }
    private static void ConstructWiredConePoints(Vector3 forward, float range, float fieldOfView,
        out Vector3 upRayOffset,
        out Vector3 downRayOffset,
        out Vector3 rightOffset,
        out Vector3 leftRayOffset,
        out Vector3 forwardRayOffset)
    {
        float halfFOV = fieldOfView / 2.0f;
        Quaternion upRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.right);
        Quaternion downRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.left);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
        Quaternion leftRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.down);

        upRayOffset = upRayRotation * forward * range;
        downRayOffset = downRayRotation * forward * range;
        rightOffset = rightRayRotation * forward * range;
        leftRayOffset = leftRayRotation * forward * range;
        forwardRayOffset = forward * range;

    }
    public static Vector3[] ConstructWiredConePoints(Vector3 forward, float range, float fieldOfView)
    {
        ConstructWiredConePoints(forward, range, fieldOfView,
            out Vector3 upRayPosition,
            out Vector3 downRayPosition,
            out Vector3 rightRayPosition,
            out Vector3 leftRayPosition,
            out Vector3 forwardRayOffset
        );

        return new[] {
            upRayPosition,
            downRayPosition,
            rightRayPosition,
            leftRayPosition,
            forwardRayOffset,
            Vector3.zero
        };

    }
    public static void DrawWiredConeGizmo(Vector3 position, Vector3 forward, float range, float fieldOfView, Color color)
    {
        DrawWiredConeGizmo(position, forward, range, fieldOfView, color, Color.clear, true);
    }

    /// <summary>
    ///     require to be called in a OnGizmoxxx method in a monobehaviour
    /// </summary>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    /// <param name="range"></param>
    /// <param name="fieldOfView"></param>
    /// <param name="targetInside"></param>
    /// <param name="targetOutside"></param>
    /// <param name="condition"></param>
    public static void DrawWiredConeGizmo(Vector3 position, Vector3 forward, float range, float fieldOfView,
        Color targetInside, Color targetOutside, bool condition)
    {
        float halfFOV = fieldOfView / 2.0f;
        Color current = condition ? targetInside : targetOutside;
        ConstructWiredConePoints(forward, range, fieldOfView, out Vector3 upRayOffset, out Vector3 downRayOffset, out Vector3 rightOffset, out Vector3 leftRayOffset,
            out Vector3 forwardRayOffset);

        Gizmos.color = current;
        Gizmos.DrawRay(position, upRayOffset);
        Gizmos.color = current;
        Gizmos.DrawRay(position, downRayOffset);
        Gizmos.color = current;
        Gizmos.DrawRay(position, rightOffset);
        Gizmos.color = current;
        Gizmos.DrawRay(position, leftRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + downRayOffset, position + rightOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + rightOffset, position + upRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + upRayOffset, position + leftRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + leftRayOffset, position + downRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + forwardRayOffset, position + upRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + forwardRayOffset, position + downRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + forwardRayOffset, position + leftRayOffset);
        Gizmos.color = current;
        Gizmos.DrawLine(position + forwardRayOffset, position + rightOffset);
    }

    /// <summary>
    ///     require to be called in a OnGizmoxxx method in a monobehaviour
    /// </summary>
    /// <param name="range"></param>
    /// <param name="position"></param>
    /// <param name="condition"></param>
    /// <param name="inRange"></param>
    /// <param name="outOfRange"></param>
    public static void DrawRangeGizmo(float range, Vector3 position, bool condition, Color inRange, Color outOfRange)
    {
        if ( condition )
        {
            Gizmos.color = inRange;
        }
        else
        {
            Gizmos.color = outOfRange;
        }
        Gizmos.DrawWireSphere(position, range);
    }

    public static void LogNullParameterErrorMessage(string parameterName, string methodName, string additionalMessage = "", string category = "")
    {
        string info = string.IsNullOrEmpty(additionalMessage) ? string.Empty : $"\n more info: {additionalMessage}";
        LogErrorMessage($"the parameter '{parameterName}' is null and the {methodName} could not proceed any further.{info}", category);
    }
    public static void LogStackTraceErrorMessage(string additionalMessage = null, bool debugLog = true)
    {
        if ( debugLog )
        {
            string info = string.IsNullOrEmpty(additionalMessage) ? string.Empty : $"\n more info: {additionalMessage}";
            Debug.LogError($"UserLog Error {info} \n {StackTraceUtility.ExtractStackTrace()}");
        }
    }
    public static void LogStackTraceMessage(string additionalMessage = null, bool debugLog = true)
    {
        if ( debugLog )
        {
            string info = string.IsNullOrEmpty(additionalMessage) ? string.Empty : $"\n more info: {additionalMessage}";
            Debug.Log($"UserLog stackTrace {info} \n {StackTraceUtility.ExtractStackTrace()}");
        }
    }
    public static void LogMethodMessage(string className, string methodName, string additionalMessage = null,
        bool debugLog = true)
    {
        if ( debugLog )
        {
            Debug.Log($"{className}.{methodName}: {additionalMessage}");
        }
    }

#if UNITY_EDITOR
    public static void DrawTest(Vector3 center, float radius, float height, Quaternion rotation, Color color,
        Vector3 scale)
    {
        rotation.Normalize();
        Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, scale);

        using ( var handle = new Handles.DrawingScope(color, matrix) )
        {
            Vector3 offsetFromCenter = GetPointOffset(rotation, height);

            Vector3 point0 = -offsetFromCenter;
            Vector3 point1 = offsetFromCenter;
            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            DrawWireCapsuleRaw(point0, point1, radius, up, right, forward);

        }
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = rotation * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation, Color color,
        Vector3 scale)
    {
        rotation.Normalize();
        Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, scale);

        using ( var handle = new Handles.DrawingScope(color, matrix) )
        {
            Vector3 offsetFromCenter = GetPointOffset(rotation, height);
            Vector3 point0 = -offsetFromCenter;
            Vector3 point1 = offsetFromCenter;
            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            DrawWireCapsuleRaw(point0, point1, radius, up, right, forward);

        }
    }
    private static Vector3 GetPointOffset(Quaternion rotation, float height)
    {
        return rotation * Vector3.up * (height / 2f);
    }

    /// <summary>
    /// </summary>
    /// <param name="point0"></param>
    /// <param name="point1"></param>
    /// <param name="radius"></param>
    /// <param name="rotation"></param>
    public static void DrawWireCapsule(Vector3 point0, Vector3 point1, float radius, Quaternion rotation, Color color,
        Vector3 scale)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, scale);
        using ( var handle = new Handles.DrawingScope(color, matrix) )
        {

            DrawWireCapsuleWorldSpace(point0, point1, radius, rotation, color);
        }
    }

    /// <summary>
    ///     require a sceneGui to work
    /// </summary>
    /// <param name="point0"></param>
    /// <param name="point1"></param>
    /// <param name="radius"></param>
    /// <param name="rotation"></param>
    public static void DrawWireCapsuleWorldSpace(Vector3 point0, Vector3 point1, float radius, Quaternion rotation,
        Color color = default)
    {
        Vector3 up = rotation * Vector3.up;
        Vector3 right = rotation * Vector3.right;
        Vector3 forward = rotation * Vector3.forward;
        if ( color != default )
        {
            Handles.color = color;
        }
        DrawWireCapsuleRaw(point0, point1, radius, up, right, forward);
    }

    private static void DrawWireCapsuleRaw(Vector3 point0, Vector3 point1, float radius, Vector3 up, Vector3 right,
        Vector3 forward)
    {
        Handles.DrawWireDisc(point1, up, radius);
        Handles.DrawWireDisc(point0, up, radius);

        Handles.DrawWireArc(point1, -right, forward, 180, radius);
        Handles.DrawWireArc(point1, forward, right, 180, radius);
        Handles.DrawWireArc(point0, right, forward, 180, radius);
        Handles.DrawWireArc(point0, -forward, right, 180, radius);

        Handles.DrawLine(point0 + radius * right, point1 + radius * right);
        Handles.DrawLine(point0 + radius * -right, point1 + radius * -right);
        Handles.DrawLine(point0 + radius * forward, point1 + radius * forward);
        Handles.DrawLine(point0 + radius * -forward, point1 + radius * -forward);
    }

#endif
}
