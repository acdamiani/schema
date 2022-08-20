using UnityEngine;

namespace Schema.Internal.Types
{
    [Color("#e34c26")]
    [UseExternalTypeDefinition(typeof(UnityEngine.MonoBehaviour))]
    [ExcludePaths(
        "transform",
        "gameObject.transform.gameObject",
        "gameObject.transform.root.parent",
        "gameObject.transform.root.transform",
        "gameObject.transform.root.gameObject.transform",
        "gameObject.transform.parent.root",
        "gameObject.transform.parent.transform",
        "gameObject.transform.parent.gameObject.transform"
    )]
    [ExcludeTypes(typeof(Matrix4x4))]
    public class MonoBehaviour : EntryType
    {
    }
}