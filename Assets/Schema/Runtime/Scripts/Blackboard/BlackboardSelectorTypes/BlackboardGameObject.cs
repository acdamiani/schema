[System.Serializable]
public class BlackboardGameObject : BlackboardEntrySelector
{
    public BlackboardGameObject()
    {
        base.AddGameObjectFilter();
    }
}