using System;
using System.Reflection;
using UnityEditor;

public static class EditorReflection
{
    /// <summary>
    ///     Return a delegate used to determine whether window is docked or not. It is faster to cache this delegate than
    ///     run the reflection required each time.
    /// </summary>
    public static Func<bool> GetIsDockedDelegate(this EditorWindow window)
    {
        BindingFlags fullBinding =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
        return (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), window, isDockedMethod);
    }
}