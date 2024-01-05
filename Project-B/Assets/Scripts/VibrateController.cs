using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Input;

public class VibrateController : MonoBehaviour
{

	private void Update()
	{
		var device = InputSystem.GetDevice<XRController>(CommonUsages.RightHand);
		var command = UnityEngine.InputSystem.XR.Haptics.SendHapticImpulseCommand.Create(0, 0.5f, 0.2f);
		device.ExecuteCommand(ref command);
	}
}
