using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

//Utility script for file creation
namespace Schema.Utilities
{
    public static class NodeEditorUtilities
    {
        private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);
        internal static void AddItem(this GenericMenu menu, GUIContent content, bool on, GenericMenu.MenuFunction func, bool disabled)
        {
            if (disabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, func);
        }
        internal static void AddItem(this GenericMenu menu, string content, bool on, GenericMenu.MenuFunction func, bool disabled)
        {
            menu.AddItem(new GUIContent(content), on, func, disabled);
        }

        private static void CreateFromTemplate(string fileName, string initialName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);

            if (guids.Length == 0)
            {
                Debug.LogWarning($"Could not find file {fileName}");
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateCodeFile>(),
                initialName,
                scriptIcon,
                path
            );
        }

        private class DoCreateCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScript(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

        internal static UnityEngine.Object CreateScript(string pathName, string templatePath)
        {
            string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", String.Empty);
            string templateText = String.Empty;

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
            else
            {
                Debug.LogError($"The template file was not found: {templatePath}");
                return null;
            }
        }

        [MenuItem("Assets/Create/Schema/Custom Node", false, 89)]
        private static void CreateNode()
        {
            CreateFromTemplate("Schema_NodeTemplate.cs", "CustomNode.cs");
        }
    }
}
