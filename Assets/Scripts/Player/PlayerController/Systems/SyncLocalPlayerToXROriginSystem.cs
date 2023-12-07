using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.XR.CoreUtils;
using UnityEngine;

[UpdateInGroup(typeof(TransformSystemGroup), OrderLast = true)]
public partial class SyncLocalPlayerToXROriginSystem : SystemBase
{
    public GameObject XROriginGameObject;

    public GameObject localHeadGameObject;
    public GameObject localLeftHandGameObject;
    public GameObject localRightHandGameObject;

    bool foundLocalVRObjects = true;

    protected override void OnUpdate()
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Server) return;

        if (localHeadGameObject == null || localLeftHandGameObject == null || localRightHandGameObject == null)
        {
            SetXRGameObjects();
            return;
        }

        SetPositionOfXROriginToEntity();

        SetPositionRotationOfLocalTransform(GetEntityLeftHandTransform(), localLeftHandGameObject.transform);

        SetPositionRotationOfLocalTransform(GetEntityRightHandTransform(), localRightHandGameObject.transform);

        SetPositionRotationOfLocalTransform(GetEntityHeadTransform(), localHeadGameObject.transform);

        foundLocalVRObjects = true;
    }

    private void SetPositionRotationOfLocalTransform(RefRW<LocalTransform> localTransform, Transform transform)
    {
        if (transform == null || !foundLocalVRObjects) return;

        transform.GetLocalPositionAndRotation(out Vector3 position, out Quaternion rotation);

        localTransform.ValueRW.Position = position;
        localTransform.ValueRW.Rotation = rotation;
    }

    private void SetPositionOfXROriginToEntity()
    {
        foreach (var (localToWorld, entity) in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<LocalOwnedNetworkedEntityComponent>().WithAll<PlayerComponent>().WithEntityAccess())
        {
            LocalTransform globalTransform = ConvertLocalEntityToGlobal(localToWorld.ValueRO, entity);

            XROriginGameObject.transform.position = globalTransform.Position;
            XROriginGameObject.transform.rotation = globalTransform.Rotation;
        }
    }

    private LocalTransform ConvertLocalEntityToGlobal(LocalToWorld localToWorld, Entity rootEntity)
    {
        float3 newPosition = localToWorld.Position;
        Quaternion newRotation = localToWorld.Rotation;

        bool hasParent = SystemAPI.HasComponent<Parent>(rootEntity);

        while (hasParent)
        {
            Entity parent = SystemAPI.GetComponent<Parent>(rootEntity).Value;

            LocalToWorld childLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(rootEntity);

            LocalTransform childTransform = new() { Position = childLocalToWorld.Position, Rotation = childLocalToWorld.Rotation, Scale = 1 };

            if (SystemAPI.HasComponent<Parent>(rootEntity))
            {
                Entity nextRootEntity = SystemAPI.GetComponent<Parent>(rootEntity).Value;

                if (SystemAPI.HasComponent<Parent>(nextRootEntity))
                {
                    newPosition = childTransform.TransformPoint(float3.zero);

                    break;
                }
            }

            newPosition = childTransform.TransformPoint(float3.zero);
            newRotation = childTransform.TransformRotation(quaternion.identity);

            rootEntity = parent;

            hasParent = SystemAPI.HasComponent<Parent>(rootEntity);
        }

        return new LocalTransform() { Position = newPosition, Rotation = newRotation, Scale = 1 };
    }

    private void SetXRGameObjects()
    {
        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            return;
        }

        XROriginGameObject = xrOrigin.gameObject;

        GameObject cameraOffset = xrOrigin.gameObject.GetNamedChild("Camera Offset");

        foreach (Transform transform in cameraOffset.transform)
        {
            Debug.Log(transform.name);

            if (transform.gameObject.name.Equals("Left Controller")) localLeftHandGameObject = transform.gameObject;
            else if (transform.gameObject.name.Equals("Right Controller")) localRightHandGameObject = transform.gameObject;
            else if (transform.gameObject.name.Equals("Main Camera")) localHeadGameObject = transform.gameObject;
        }
    }

    //would use a generic method for these, but unity DOTS code gen no likey
    private RefRW<LocalTransform> GetEntityLeftHandTransform()
    {
        foreach (RefRW<LocalTransform> localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<NetworkedEntityChildComponent>().WithAll<LeftHandComponent>())
            return localTransform;

        foundLocalVRObjects = false;

        return default;
    }

    private RefRW<LocalTransform> GetEntityRightHandTransform()
    {
        foreach (RefRW<LocalTransform> localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<NetworkedEntityChildComponent>().WithAll<RightHandComponent>())
            return localTransform;

        foundLocalVRObjects = false;

        return default;
    }

    private RefRW<LocalTransform> GetEntityHeadTransform()
    {
        foreach (RefRW<LocalTransform> localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<NetworkedEntityChildComponent>().WithAll<HeadComponent>())
            return localTransform;

        foundLocalVRObjects = false;

        return default;
    }
}