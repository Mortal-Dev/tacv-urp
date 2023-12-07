using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

public struct PlayerControllerInputComponent : IComponentData
{
    public float rightControllerTrigger;
    public float rightControllerGrip;

    public float rightControllerButtonOne;
    public float rightControllerButtonTwo;

    public float2 rightControllerThumbstick;
    public bool rightControllerThumbstickPress;

    public float leftControllerTrigger;
    public float leftControllerGrip;

    public float leftControllerButtonOne;
    public float leftControllerButtonTwo;

    public float2 leftControllerThumbstick;
    public bool leftControllerThumbstickPress;
}