using Schema.Runtime;
using UnityEngine;

public class SetMatrixValue : Action
{
    public BlackboardEntrySelector<float> m00;
    public BlackboardEntrySelector<float> m01;
    public BlackboardEntrySelector<float> m02;
    public BlackboardEntrySelector<float> m03;
    public BlackboardEntrySelector<float> m10;
    public BlackboardEntrySelector<float> m11;
    public BlackboardEntrySelector<float> m12;
    public BlackboardEntrySelector<float> m13;
    public BlackboardEntrySelector<float> m20;
    public BlackboardEntrySelector<float> m21;
    public BlackboardEntrySelector<float> m22;
    public BlackboardEntrySelector<float> m23;
    public BlackboardEntrySelector<float> m30;
    public BlackboardEntrySelector<float> m31;
    public BlackboardEntrySelector<float> m32;
    public BlackboardEntrySelector<float> m33;
    [WriteOnly][DisableDynamicBinding] public BlackboardEntrySelector<Matrix4x4> target;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Matrix4x4 mat = new Matrix4x4();

        return NodeStatus.Success;
    }
}