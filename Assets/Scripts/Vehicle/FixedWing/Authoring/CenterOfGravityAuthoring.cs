using UnityEngine;
using Unity.Entities;

public class CenterOfGravityAuthoring : MonoBehaviour
{
    class Baking : Baker<CenterOfGravityAuthoring>
    {
        public override void Bake(CenterOfGravityAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CenterOfGravityComponent());
        }
    }
}
