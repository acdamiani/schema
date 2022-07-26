namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IGraphObjectProvider
    {
        bool Equals(Schema.Internal.GraphObject graphObject);
    }
}