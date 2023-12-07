using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public partial struct NetworkedEntityComponent : IComponentData
{
    public ushort connectionId;

    public ulong networkEntityId;

    public int networkedPrefabHash;

    public bool allowNetworkedChildrenRequests;
}
