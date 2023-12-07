using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PlayerControllerComponent : IComponentData
{
    public PlayerState playerState;

    public float forwardForce;
    public float backwardForce;
    public float sideForce;

    public float maxForwardVelocity;
    public float maxBackwardsVelocity;
    public float maxSideVelocity;

    public float maxDownVelocity;
}
