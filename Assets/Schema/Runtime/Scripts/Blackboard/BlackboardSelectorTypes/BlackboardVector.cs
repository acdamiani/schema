[System.Serializable]
public class BlackboardVector : BlackboardEntrySelector
{
    public BlackboardVector() : base(typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector2)) { }
}