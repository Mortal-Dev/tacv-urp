using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

public partial struct CheckWheelCollisionSystem : ISystem
{
    public void OnUpdate() 
    {

    }

    struct WheelCollisionJob : ICollisionEventsJob
    {
        //[ReadOnly] public ComponentLookup<FixedWingComponent> fixedWingComponent;

        public void Execute(CollisionEvent collisionEvent)
        {
            
        }
    }
}