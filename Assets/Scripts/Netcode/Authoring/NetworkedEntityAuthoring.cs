using UnityEngine;
using Unity.Entities;
using System;
using Unity.Transforms;

public class NetworkedEntityAuthoring : MonoBehaviour
{
    public GameObject OriginalNetworkedPrefab;

    public bool canHaveNetworkedChildren;

    class Baking : Baker<NetworkedEntityAuthoring>
    {
        static System.Random random = new System.Random();

        public override void Bake(NetworkedEntityAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            if (authoring.OriginalNetworkedPrefab == null)
                throw new Exception($"{nameof(authoring.OriginalNetworkedPrefab)} has not been set for {authoring.gameObject.name}");

            AddComponent(entity, new NetworkedEntityComponent() { connectionId = NetworkManager.SERVER_NET_ID, networkedPrefabHash = authoring.OriginalNetworkedPrefab.name.GetHashCode(), 
                networkEntityId = (ulong)random.Next(0, int.MaxValue), allowNetworkedChildrenRequests = authoring.canHaveNetworkedChildren });

            AddComponent(entity, new PreviousLocalTransformRecordComponent() { localTransformRecord = new LocalTransform() { Position = authoring.transform.localPosition, Rotation = authoring.transform.localRotation, Scale = authoring.transform.localScale.y } });
        }
    }
}