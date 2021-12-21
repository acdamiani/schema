using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Schema.Runtime;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Schema.Editor
{
    public static class NodeEditorFileHandler
    {
        public static void Save(NodeEditor editor)
        {
            if (NodeEditor.NodeEditorPrefs.formatOnSave)
                editor.BeautifyTree(new Vector2(50f, 150f));

            //If file does not exist in project or doesn't exist at all, Save As
            if (editor.original == null || !AssetDatabase.Contains(editor.original))
            {
                SaveAs(editor);
                return;
            }
            //Get the path of the original
            string path = AssetDatabase.GetAssetPath(editor.original);

            List<SchemaAgent> toUpdate = Object.FindObjectsOfType<SchemaAgent>().Where(a => a.target == editor.original).ToList();

            //Delete the previous original, and create a new one at the path (essentially overwriting it)
            AssetDatabase.DeleteAsset(path);

            Object.DestroyImmediate(editor.original);

            //Destroy reference to asset, and instantiate our working copy
            editor.original = Duplicate(editor.target);

            List<ScriptableObject> blackboardAndKeys = new List<ScriptableObject> { editor.original.blackboard };
            blackboardAndKeys.AddRange(editor.original.blackboard.entries);

            if (editor.original.blackboard.entries != null) blackboardAndKeys.AddRange(editor.original.blackboard.entries);

            List<ScriptableObject> nodes = new List<ScriptableObject>(editor.original.nodes);

            List<ScriptableObject> decorators = editor.original.nodes.SelectMany(node => node.decorators).Cast<ScriptableObject>().ToList();

            HelperMethods.SetHideFlags(HideFlags.HideInHierarchy, blackboardAndKeys, nodes, decorators);
            AssetDatabase.CreateAsset(editor.original, path);

            //Remember that we set our HideFlags when creating our object to HideAndDontSave. We need to reset this here
            editor.original.hideFlags = HideFlags.None;
            EditorUtility.SetDirty(editor.original);

            foreach (Node node in editor.original.nodes)
            {
                AssetDatabase.AddObjectToAsset(node, path);
                foreach (Decorator decorator in node.decorators)
                {
                    AssetDatabase.AddObjectToAsset(decorator, path);
                }
            }

            AssetDatabase.AddObjectToAsset(editor.original.blackboard, path);

            foreach (BlackboardEntry entry in editor.original.blackboard.entries)
            {
                AssetDatabase.AddObjectToAsset(entry, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            foreach (SchemaAgent a in toUpdate)
            {
                a.target = editor.original;
            }

            editor.windowInfo.graphSaved = true;
            editor.titleContent = new GUIContent(editor.original.name);
        }
        public static void SaveAs(NodeEditor editor)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save As", "", "asset", "Save Behavior Tree");

            if (String.IsNullOrEmpty(path) || !path.EndsWith(".asset")) return;

            editor.original = Duplicate(editor.target);

            List<SchemaAgent> toUpdate = Object.FindObjectsOfType<SchemaAgent>().Where(a => a.target == editor.original).ToList();

            List<ScriptableObject> blackboardAndKeys = new List<ScriptableObject> { editor.original.blackboard };
            blackboardAndKeys.AddRange(editor.original.blackboard.entries);

            if (editor.original.blackboard.entries != null) blackboardAndKeys.AddRange(editor.original.blackboard.entries);

            List<ScriptableObject> nodes = new List<ScriptableObject>(editor.original.nodes);

            List<ScriptableObject> decorators = editor.original.nodes.SelectMany(node => node.decorators).Cast<ScriptableObject>().ToList();

            HelperMethods.SetHideFlags(HideFlags.HideInHierarchy, blackboardAndKeys, nodes, decorators);

            if (File.Exists(Application.dataPath + path.Substring(6)))
                AssetDatabase.DeleteAsset(path);

            editor.original.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(editor.original, path);

            foreach (Node node in editor.original.nodes)
            {
                AssetDatabase.AddObjectToAsset(node, path);
                foreach (Decorator decorator in node.decorators)
                {
                    AssetDatabase.AddObjectToAsset(decorator, path);
                }
            }

            AssetDatabase.AddObjectToAsset(editor.original.blackboard, path);

            foreach (BlackboardEntry entry in editor.original.blackboard.entries)
            {
                AssetDatabase.AddObjectToAsset(entry, path);
            }

            //Remember that we set our HideFlags when creating our object to HideAndDontSave. We need to reset this here
            EditorUtility.SetDirty(editor.original);

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            foreach (SchemaAgent a in toUpdate)
            {
                a.target = editor.original;
            }

            editor.windowInfo.graphSaved = true;
            editor.titleContent = new GUIContent(editor.original.name);
        }
        private static Graph Duplicate(Graph graph)
        {
            Graph ret = Object.Instantiate(graph);
            ret.name = graph.name;
            List<Node> topLevel = graph.nodes.Where(node => node.parent == null).ToList();

            ret.nodes.Clear();

            foreach (Node node in topLevel)
            {
                DuplicateRecursive(ret.nodes, null, node);
            }

            ret.root = (Root)ret.nodes.Find(node => node.GetType() == typeof(Root));

            try
            {
                ret.blackboard = Object.Instantiate(graph.blackboard);
                ret.blackboard.name = "Blackboard";
                ret.blackboard.entries = graph.blackboard.entries.Select(entry =>
                {
                    BlackboardEntry cache = Object.Instantiate(entry);
                    cache.name = cache.Name;
                    return cache;
                }).ToList();
            }
            catch
            {
                // ignored
            }

            return ret;
        }
        public static Graph CreateNew()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New", "", "asset", "Create New Behavior Tree");

            if (String.IsNullOrEmpty(path) || !path.EndsWith(".asset")) return null;
            Graph graph = ScriptableObject.CreateInstance<Graph>();

            if (File.Exists(Application.dataPath + path.Substring(6)))
                AssetDatabase.DeleteAsset(path);

            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            return graph;
        }
        private static Node DuplicateRecursive(List<Node> graph, Node parent, Node duplicate)
        {
            Node ret = Object.Instantiate(duplicate);
            ret.name = duplicate.name;

            for (int i = 0; i < ret.decorators.Count; i++)
            {
                string name = ret.decorators[i].name;

                ret.decorators[i] = Object.Instantiate(ret.decorators[i]);
                ret.decorators[i].name = name;
                ret.decorators[i].node = ret;
            }

            ret.parent = parent;

            foreach (Node node in ret.children.Where(n => n == null))
            {
                Debug.Log($"Child node {node.Name} was null. Child of node {parent.Name}");
            }
            ret.children.RemoveAll(n => n == null);
            ret.children = ret.children.Select(node => DuplicateRecursive(graph, ret, node)).ToList();

            graph.Add(ret);

            return ret;
        }
        public static void Load(this NodeEditor editor, Graph graph)
        {
            editor.original = graph;
            editor.original.nodes ??= new List<Node>();

            editor.windowInfo.pan = graph.pan;
            editor.windowInfo.zoom = graph.zoom;

            //Cache GUIDs of selected before destroying
            List<string> guids = new List<string>();

            if (editor.target)
            {
                guids = editor.target.nodes.Select(node => node.uID).ToList();
            }

            Object.DestroyImmediate(editor.target);
            editor.target = Duplicate(editor.original);

            if (editor.target.blackboard == null)
                editor.target.blackboard = ScriptableObject.CreateInstance<Blackboard>();

            //Transfer selection to file's nodes
            for (int i = 0; i < editor.windowInfo.selected.Count; i++)
            {
                editor.windowInfo.selected[i] = editor.target.nodes.Find(node => node.uID.Equals(guids[i]));
            }

            editor.windowInfo.selected.RemoveAll(node => node == null);

            List<ScriptableObject> blackboardAndKeys = new List<ScriptableObject> { editor.target.blackboard };
            blackboardAndKeys.AddRange(editor.target.blackboard.entries);

            List<ScriptableObject> nodes = new List<ScriptableObject>(editor.target.nodes);
            List<ScriptableObject> decorators = editor.target.nodes.SelectMany(node => node.decorators).Cast<ScriptableObject>().ToList();

            editor.target.hideFlags = HideFlags.HideAndDontSave;
            HelperMethods.SetHideFlags(HideFlags.HideAndDontSave, blackboardAndKeys, nodes, decorators);
        }
        public static void Load(NodeEditor editor)
        {
            string path = EditorUtility.OpenFilePanel("Open Graph", "", "asset");

            switch (String.IsNullOrEmpty(path))
            {
                case false when AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)) && path.EndsWith(".asset"):
                    {
                        Graph graph = AssetDatabase.LoadAssetAtPath<Graph>("Assets" + path.Substring(Application.dataPath.Length));
                        editor.Open(graph);
                        break;
                    }
                case false when !AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)):
                    EditorUtility.DisplayDialog("Could not open file",
                        $"The file {Path.GetFileName(path)} could not be opened because it is not inside the project.", "OK", "");
                    break;
            }
        }
        public static void Screenshot(this NodeEditor editor)
        {
            string path = Path.Combine(Application.dataPath, NodeEditor.NodeEditorPrefs.screenshotPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            EditorApplication.delayCall += () =>
            {
                int width = (int)editor.position.width;
                int height = (int)editor.position.height;

                Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(editor.position.position, width, height);

                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.SetPixels(pixels);

                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(path + "/Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff") + ".png", bytes);
            };
        }
    }
}
