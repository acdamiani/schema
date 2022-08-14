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
        private static Texture2D _gridTexture;

        public static Texture2D gridTexture => _gridTexture == null
            ? _gridTexture = GenerateGridTexture(Color.Lerp(Color.white, Styles.windowAccent, 0.8f), Styles.windowAccent, false)
            : _gridTexture;

        public static Texture2D gridTexture2x => _gridTexture2x == null
            ? _gridTexture2x = GenerateGridTexture(Color.Lerp(Color.white, Styles.windowAccent, 0.8f), Styles.windowAccent, true)
            : _gridTexture2x;

        private static Texture2D _gridTexture2x;

        public static Texture2D GetEditor(string iconName, bool doPrefix = true)
        {
            if (doPrefix)
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
        private static Texture2D GenerateGridTexture(Color dots, Color bg, bool large)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    Color col = bg;

                    if (!large && (y % 16 == 0 || x % 16 == 0))
                        col = Color.Lerp(dots, bg, 0.65f);

                    if (y == 0 || x == 0) col = Color.Lerp(dots, bg, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(dots, bg, 0.35f);

                    cols[y * 64 + x] = col;
                }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

    }
}