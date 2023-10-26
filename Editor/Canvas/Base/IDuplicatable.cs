namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IDuplicatable
    {
        bool IsDuplicatable();
        GUIComponent Duplicate();
    }
}