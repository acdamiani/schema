namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IFramable
    {
        float xMin { get; }
        float yMin { get; }
        float xMax { get; }
        float yMax { get; }
        bool IsFramable();
    }
}