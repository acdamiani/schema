using UnityEngine;

namespace Schema.Internal.Types
{
    [Color("#f34b7d")]
    [UseExternalTypeDefinition(typeof(UnityEngine.GameObject))]
    [ExcludePaths(
        "gameObject",
        "transform.gameObject",
        "transform.parent.transform",
        "transform.parent.gameObject",
        "transform.parent.root",
        "transform.root.transform",
        "transform.root.root",
        "transform.root.gameObject",
        "transform.root.parent"
    )]
    [ExcludeTypes(typeof(Matrix4x4))]
    public class GameObject : EntryType
    {
    }
}