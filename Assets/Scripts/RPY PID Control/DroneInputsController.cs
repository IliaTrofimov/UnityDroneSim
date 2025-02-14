using System;
using Inputs;
using UnityEngine;

namespace RPY_PID_Control
{
	[Flags]
	public enum DroneStabilizerMode
	{
		None = 0, 
		StabAltitude = 1, 
		StabRotation = 2
	}
	
	/// <summary>Drone control inputs manager.</summary>
	/// <remarks>Reads manual inputs with <see cref="DroneControls"/> at Update function, or takes AI inputs from SetInputs on demand.</remarks>
	[DisallowMultipleComponent]
	public class DroneInputsController : MonoBehaviour 
	{
		/* y (yaw)
		   |
		   |  z|fwd (roll)
		   |/
		   +------x (pitch - nose up/down) */

		private DroneControls controls;
		
		public bool manualInput = true;
		public bool useLegacyInputs = false;
		public DroneStabilizerMode stabilizerMode = DroneStabilizerMode.StabAltitude | DroneStabilizerMode.StabRotation;

		[HideInInspector] public Vector3 rotation;

		/// <summary>Current throttle value.</summary>
		[HideInInspector] public float throttle;
		
		/// <summary>Current pitch value.</summary>
		[HideInInspector] public float pitch;
		
		/// <summary>Current yaw value.</summary>
		[HideInInspector] public float yaw;
		
		/// <summary>Current roll value.</summary>
		[HideInInspector] public float roll;
		
		private void Awake() => controls = new DroneControls();

		private void OnEnable() => controls?.Enable();

		private void OnDisable() => controls?.Disable();

		private void OnValidate()
		{
			if (manualInput) controls?.Enable();
			else controls?.Disable();
		}

		private void Update()
		{
			if (!manualInput) return;

			if (!useLegacyInputs) ReadInputs();
			else ReadInputsLegacy();
		}

		private void ReadInputs()
		{
			rotation = controls.Default.Rotation.ReadValue<Vector3>();
			pitch = rotation.x;
			yaw = rotation.y;
			roll = rotation.z;
			
			throttle = controls.Default.Throttle.ReadValue<float>();
			
			var lastStab = stabilizerMode;
			if (controls.Default.StabAltitude.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabAltitude;
			if (controls.Default.StabRotation.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabRotation;
			
			if (lastStab != stabilizerMode) 
				Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
		}

		private void ReadInputsLegacy()
		{
			pitch = Input.GetAxis("Pitch");
			yaw = Input.GetAxis("Yaw");
			roll = Input.GetAxis("Roll");
			throttle = Input.GetAxis("Throttle");
			
			var lastStab = stabilizerMode;
			if (controls.Default.StabAltitude.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabAltitude;
			if (controls.Default.StabRotation.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabRotation;
			
			if (lastStab != stabilizerMode) 
				Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
		}

		
		/// <summary>Set control inputs manually.</summary>
		public void SetInputs(float throttle, float pitch, float yaw, float roll)
		{
			this.throttle = throttle;
			this.yaw = yaw;
			this.pitch = pitch;
			this.roll = roll;
		}
	}
}
