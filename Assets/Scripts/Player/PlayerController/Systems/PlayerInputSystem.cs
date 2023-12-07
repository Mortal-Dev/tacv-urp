using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    XRIDefaultInputActions xriDefaultInputActions;

    protected override void OnCreate()
    {
        xriDefaultInputActions = new XRIDefaultInputActions();
        xriDefaultInputActions.Enable();
    }

    protected override void OnUpdate()
    {
        foreach (RefRW<PlayerControllerInputComponent> playerInput in SystemAPI.Query<RefRW<PlayerControllerInputComponent>>().WithAll<LocalOwnedNetworkedEntityComponent>())
        {
            SetInputs(playerInput);
        }
    }

    private void SetInputs(RefRW<PlayerControllerInputComponent> playerInput)
    {
        playerInput.ValueRW.rightControllerThumbstick = xriDefaultInputActions.XRIRightHandLocomotion.Move.ReadValue<Vector2>();
        playerInput.ValueRW.rightControllerThumbstickPress = xriDefaultInputActions.XRIRightHandButtons.RightHandThumbstickPress.ReadValue<bool>();
        playerInput.ValueRW.rightControllerTrigger = xriDefaultInputActions.XRIRightHandInteraction.ActivateValue.ReadValue<float>();
        playerInput.ValueRW.rightControllerGrip = xriDefaultInputActions.XRIRightHandInteraction.SelectValue.ReadValue<float>();
        playerInput.ValueRW.rightControllerButtonOne = xriDefaultInputActions.XRIRightHandButtons.RightHandFirstButton.ReadValue<float>();
        playerInput.ValueRW.rightControllerButtonTwo = xriDefaultInputActions.XRIRightHandButtons.RightHandSecondButton.ReadValue<float>();

        playerInput.ValueRW.leftControllerThumbstick = xriDefaultInputActions.XRILeftHandLocomotion.Move.ReadValue<Vector2>();
        playerInput.ValueRW.leftControllerThumbstickPress = xriDefaultInputActions.XRILeftHandButtons.LeftHandThumbstickPressed.ReadValue<bool>();
        playerInput.ValueRW.leftControllerTrigger = xriDefaultInputActions.XRILeftHandInteraction.ActivateValue.ReadValue<float>();
        playerInput.ValueRW.leftControllerGrip = xriDefaultInputActions.XRILeftHandInteraction.SelectValue.ReadValue<float>();
        playerInput.ValueRW.leftControllerButtonOne = xriDefaultInputActions.XRILeftHandButtons.LeftHandFirstButton.ReadValue<float>();
        playerInput.ValueRW.leftControllerButtonTwo = xriDefaultInputActions.XRILeftHandButtons.LeftHandSecondButton.ReadValue<float>();
    }
}