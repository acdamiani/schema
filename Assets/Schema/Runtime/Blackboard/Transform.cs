using UnityEngine;

namespace Schema.Internal.Types
{
    [Color("#da3434")]
    [UseExternalTypeDefinition(typeof(UnityEngine.Transform))]
    [ExcludePaths(
        "gameObject.transform",
        "transform",
        "root.transform",
        "parent.transform",
        "parent.root",
        "root.parent",
        "root.gameObject.transform",
        "parent.gameObject.transform"
    )]
    [ExcludeTypes(typeof(Matrix4x4))]
    public class Transform : EntryType
    {
    }
}