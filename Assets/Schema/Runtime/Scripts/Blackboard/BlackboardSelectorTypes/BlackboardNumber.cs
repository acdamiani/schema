[System.Serializable]
public class BlackboardNumber : BlackboardEntrySelector
{
	public BlackboardNumber()
	{
		base.AddFloatFilter();
		base.AddIntFilter();
	}
}