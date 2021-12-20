[System.Serializable]
public class BlackboardVector3 : BlackboardEntrySelector
{
    public BlackboardVector3()
    {
        base.AddVector3Filter();
    }
}