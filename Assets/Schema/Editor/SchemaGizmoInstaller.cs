using System.IO;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal
{
    [InitializeOnLoad]
    public static class SchemaGizmoInstaller
    {
        private static readonly string[] gizmosToImport =
            { "d_Graph Icon.png", "Graph Icon.png", "d_SchemaAgent Icon.png", "SchemaAgent Icon.png" };

        static SchemaGizmoInstaller()
        {
            for (int i = 0; i < gizmosToImport.Length; i++)
            {
                string gizmo = gizmosToImport[i];
                string path = Path.Combine(Application.dataPath, string.Format("Gizmos/Schema/{0}", gizmo));

                FileInfo file = new FileInfo(path);

                if (file.Exists)
                    continue;

                string resPath = Path.Combine(Path.GetDirectoryName(gizmo), Path.GetFileNameWithoutExtension(gizmo));

                Texture2D image = Resources.Load<Texture2D>(resPath);

                if (image == null)
                    continue;

                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Gizmos/Schema"));

                string databasePath = string.Format("Assets/Gizmos/Schema/{0}", gizmo);

                if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(image), databasePath))
                    Debug.LogWarning("Failed to copy icon to Gizmos folder!");
            }
        }
    }
}