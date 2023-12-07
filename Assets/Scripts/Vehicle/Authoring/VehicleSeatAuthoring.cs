using Unity.Entities;
using UnityEngine;

public class VehicleSeatAuthoring : MonoBehaviour
{
    public int seatPosition;

    public bool hasOwnershipCapability;

    class Baking : Baker<VehicleSeatAuthoring>
    {
        public override void Bake(VehicleSeatAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new VehicleSeatComponent() { hasOwnershipCapability = authoring.hasOwnershipCapability, seatPosition = authoring.seatPosition });
        }
    }
}