public enum ServerToClientNetworkMessageId : ushort
{
    ServerConfirmClientVehicleEnterRequest = 1,
    ServerConfirmClientVehicleLeaveRequest,

    ServerSetNetworkEntityParent,
    ServerUnparentNetworkEntity,

    ServerSyncEntity,
    ServerLoadScene,
    ServerSpawnEntity,
    ServerDestroyEntity,
    ServerDestroyDefaultSceneEntity,
    ServerChangeEntityOwnership
}

public enum ClientToServerNetworkMessageId : ushort
{
    ClientSyncOwnedEntities = 1,
    ClientFinishedLoadingScene,

    ClientRequestVehicleControl, //used if multiple clients can control a vehicle (like a 2 seater hornet), and swaps controls
    ClientRequestVehicleEnter,
    ClientRequestVehicleLeave
}