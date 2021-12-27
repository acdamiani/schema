[System.Serializable]
public class BlackboardBoolean : BlackboardEntrySelector
{
    public BlackboardBoolean() : base(typeof(bool)) { }
}
[System.Serializable]
public class BlackboardFloat : BlackboardEntrySelector
{
    public BlackboardFloat() : base(typeof(float)) { }
}
[System.Serializable]
public class BlackboardGameObject : BlackboardEntrySelector
{
    public BlackboardGameObject() : base(typeof(UnityEngine.GameObject)) { }
}
[System.Serializable]
public class BlackboardInt : BlackboardEntrySelector
{
    public BlackboardInt() : base(typeof(int)) { }
}
[System.Serializable]
public class BlackboardNumber : BlackboardEntrySelector
{
    public BlackboardNumber() : base(typeof(float), typeof(int)) { }
}
[System.Serializable]
public class BlackboardString : BlackboardEntrySelector
{
    public BlackboardString() : base(typeof(string)) { }
}
[System.Serializable]
public class BlackboardVector2 : BlackboardEntrySelector
{
    public BlackboardVector2() : base(typeof(UnityEngine.Vector2)) { }
}
[System.Serializable]
public class BlackboardVector3 : BlackboardEntrySelector
{
    public BlackboardVector3() : base(typeof(UnityEngine.Vector3)) { }
}
[System.Serializable]
public class BlackboardVector : BlackboardEntrySelector
{
    public BlackboardVector() : base(typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector2)) { }
}
[System.Serializable]
public class BlackboardQuaternion : BlackboardEntrySelector
{
    public BlackboardQuaternion() : base(typeof(UnityEngine.Quaternion)) { }
}