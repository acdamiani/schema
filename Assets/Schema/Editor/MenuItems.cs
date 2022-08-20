using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

//Utility script for file creation
namespace Schema.Utilities
{
    public static class NodeEditorUtilities
    {
        private static readonly Texture2D
            scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;

        private static void CreateFromTemplate(string fileName, string initialName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);

            if (guids.Length == 0) Debug.LogWarning($"Could not find file {fileName}");

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateCodeFile>(),
                initialName,
                scriptIcon,
                path
            );
        }

        internal static Object CreateScript(string pathName, string templatePath)
        {
            string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);
            string templateText = string.Empty;

            UTF8Encoding encoding = new UTF8Encoding(true, false);

            if (File.Exists(templatePath))
            {
                StreamReader reader = new StreamReader(templatePath);
                templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("#SCRIPTNAME#", className);

                StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
                writer.Write(templateText);
                writer.Close();

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }

            Debug.LogError($"The template file was not found: {templatePath}");
            return null;
        }

        [MenuItem("Assets/Create/Schema/Custom Node", false, 85)]
        private static void CreateNode()
        {
            CreateFromTemplate("Schema_NodeTemplate.cs", "CustomNode.cs");
        }

        private class DoCreateCodeFile : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScript(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
    }
}