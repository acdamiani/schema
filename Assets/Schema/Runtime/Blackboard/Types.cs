using System.Collections.Generic;
using UnityEngine;

namespace Schema.Internal.Types
{
    [Name("float"), Color("#178600"), UseExternalTypeDefinition(typeof(float))]
    public class Float : EntryType
    {
    }

    [Color("#f34b7d"), UseExternalTypeDefinition(typeof(UnityEngine.GameObject)), ExcludePaths(
         "gameObject",
         "transform.gameObject",
         "transform.parent.transform",
         "transform.parent.gameObject",
         "transform.parent.root",
         "transform.root.transform",
         "transform.root.root",
         "transform.root.gameObject",
         "transform.root.parent"
     ), ExcludeTypes(typeof(Matrix4x4))]
    public class GameObject : EntryType
    {
    }

    [Color("#da3434"), UseExternalTypeDefinition(typeof(UnityEngine.Transform)), ExcludePaths(
         "gameObject.transform",
         "transform",
         "root.transform",
         "parent.transform",
         "parent.root",
         "root.parent",
         "root.gameObject.transform",
         "parent.gameObject.transform"
     ), ExcludeTypes(typeof(Matrix4x4))]
    public class Transform : EntryType
    {
    }

    [Name("int"), Color("#555555"), UseExternalTypeDefinition(typeof(int))]
    public class Int : EntryType
    {
    }

    [Color("#e34c26"), UseExternalTypeDefinition(typeof(UnityEngine.MonoBehaviour)), ExcludePaths(
         "transform",
         "gameObject.transform.gameObject",
         "gameObject.transform.root.parent",
         "gameObject.transform.root.transform",
         "gameObject.transform.root.gameObject.transform",
         "gameObject.transform.parent.root",
         "gameObject.transform.parent.transform",
         "gameObject.transform.parent.gameObject.transform"
     ), ExcludeTypes(typeof(Matrix4x4))]
    public class MonoBehaviour : EntryType
    {
    }

    [Color("#f1e05a"), UseExternalTypeDefinition(typeof(string))]
    public class String : EntryType
    {
    }

    [Color("#2b7489"), UseExternalTypeDefinition(typeof(UnityEngine.Vector2))]
    public class Vector2 : EntryType
    {
    }

    [Color("#563d7c"), UseExternalTypeDefinition(typeof(UnityEngine.Vector3))]
    public class Vector3 : EntryType
    {
    }

    [Color("#083fa1"), UseExternalTypeDefinition(typeof(UnityEngine.Vector4))]
    public class Vector4 : EntryType
    {
    }

    [Color("#292929"), UseExternalTypeDefinition(typeof(UnityEngine.Quaternion))]
    public class Quaternion : EntryType
    {
    }

    [Color("#b07219"), UseExternalTypeDefinition(typeof(List<UnityEngine.GameObject>))]
    public class GameObjectList : EntryType
    {
    }

    [Color("#dea584"), UseExternalTypeDefinition(typeof(List<UnityEngine.Transform>))]
    public class TransformList : EntryType
    {
    }

    [Color("#3572A5"), UseExternalTypeDefinition(typeof(UnityEngine.AnimationCurve))]
    public class AnimationCurve : EntryType
    {
    }

    [Color("#00ADD8"), UseExternalTypeDefinition(typeof(UnityEngine.Rect))]
    public class Rect : EntryType
    {
    }

    [Name("bool"), Color("#427819"), UseExternalTypeDefinition(typeof(bool))]
    public class Bool : EntryType
    {
    }

    [Color("#438eff"), UseExternalTypeDefinition(typeof(UnityEngine.Vector2Int))]
    public class Vector2Int : EntryType
    {
    }

    [Color("#6866fb"), UseExternalTypeDefinition(typeof(UnityEngine.Vector3Int))]
    public class Vector3Int : EntryType
    {
    }
}