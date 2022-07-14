namespace SchemaEditor.Internal
{
    public interface IEditable
    {
        UnityEngine.Object GetEditable();
        bool IsEditable();
    }
}