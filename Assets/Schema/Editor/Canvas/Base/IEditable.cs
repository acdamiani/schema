namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IEditable
    {
        UnityEngine.Object GetEditable();
        bool IsEditable();
    }
}