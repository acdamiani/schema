[System.Serializable]
public class BlackboardBoolean : BlackboardEntrySelector
{
	public BlackboardBoolean()
	{
		base.AddBoolFilter();
	}
}