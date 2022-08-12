using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor
{
    public static class Icons
    {
        private static readonly Dictionary<string, Texture2D> editorCache = new();

        private static readonly Dictionary<string, Texture2D> resourceCache = new();

        public static Texture2D GetEditor(string iconName)
        {
            iconName = (EditorGUIUtility.isProSkin ? "d_" : "") + iconName;

            editorCache.TryGetValue(iconName, out Texture2D value);

            if (value == null)
                editorCache[iconName] = value = (Texture2D)EditorGUIUtility.IconContent(iconName).image;

            return value;
        }

        public static Texture2D GetResource(string iconName, bool doPrefix = true)
        {
            string file = Path.GetFileName(iconName);

            if (doPrefix)
                file = (EditorGUIUtility.isProSkin ? "d_" : "") + file;

            iconName = Path.Join(Path.GetDirectoryName(iconName), file);

            resourceCache.TryGetValue(iconName, out Texture2D value);

            if (value == null)
                resourceCache[iconName] = value = Resources.Load<Texture2D>(iconName);

            return value;
        }
    }
}