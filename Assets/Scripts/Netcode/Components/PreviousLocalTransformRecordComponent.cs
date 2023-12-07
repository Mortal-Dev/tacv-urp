using Unity.Entities;
using Unity.Transforms;

public partial struct PreviousLocalTransformRecordComponent : IComponentData
{
    public LocalTransform localTransformRecord;
}