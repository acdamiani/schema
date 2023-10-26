namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IDeletable
    {
        bool IsDeletable();
        void Delete();
    }
}