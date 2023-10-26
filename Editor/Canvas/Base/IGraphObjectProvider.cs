using Schema.Internal;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IGraphObjectProvider
    {
        bool Equals(GraphObject graphObject);
    }
}