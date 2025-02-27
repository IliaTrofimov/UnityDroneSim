using System;
using Inputs;
using Unity.Mathematics;
using UnityEngine;


namespace Drone
{
	/// <summary>Flight stabilization mode.</summary>
	[Flags]
	public enum DroneStabilizerMode
	{
		None = 0, StabAltitude = 1, StabPitchRoll = 2, StabYaw = 4
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
		
		/// <summary>Turn on/off reading inputs from user.</summary>
		public bool manualInput = true;
		
		/// <summary>Use legacy Unity Input manager instead of new Inputs system.</summary>
		public bool useLegacyInputs;
		
		/// <summary>Flight stabilization mode.</summary>
		public DroneStabilizerMode stabilizerMode = DroneStabilizerMode.StabAltitude | DroneStabilizerMode.StabPitchRoll;
		
		/// <summary>Desired throttle value. Range [-1, 1].</summary>
		[HideInInspector] public float throttle;
		
		/// <summary>Desired pitch value. Rotation along X (right) axis. Range [-1, 1].</summary>
		[HideInInspector] public float pitch;
		
		/// <summary>Desired yaw value. Rotation along Y (up) axis. Range [-1, 1].</summary>
		[HideInInspector] public float yaw;
		
		/// <summary>Desired roll value. Rotation along Z (forward) axis. Range [-1, 1].</summary>
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

			if (useLegacyInputs) ReadLegacyInputs();
			else ReadInputs();
		}

		private void ReadInputs()
		{
			var rotation = controls.Default.Rotation.ReadValue<Vector3>();
			pitch = rotation.x;                                      // w(negative)       | s(positive)
			yaw = rotation.y;                                        // q(negative)       | e(positive)
			roll = rotation.z;                                       // d/right(negative) | e/left(positive)
			throttle = controls.Default.Throttle.ReadValue<float>(); // up(negative)      | down(positive)
			
			var lastStab = stabilizerMode;
			if (controls.Default.StabAltitude.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabAltitude;
			if (controls.Default.StabRotation.WasPressedThisFrame())
				stabilizerMode ^= DroneStabilizerMode.StabPitchRoll;
			
			if (lastStab != stabilizerMode) 
				Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
		}

		private void ReadLegacyInputs()
		{
			pitch = Input.GetAxis("Pitch");       // w(negative)       | s(positive)
			yaw = Input.GetAxis("Yaw");           // q(negative)       | e(positive)
			roll = Input.GetAxis("Roll");         // d/right(negative) | e/left(positive)
			throttle = Input.GetAxis("Throttle"); // up(negative)      | down(positive)
			
			var lastStab = stabilizerMode;
			if (Input.GetKey(KeyCode.LeftShift))
				stabilizerMode ^= DroneStabilizerMode.StabAltitude;
			if (Input.GetKey(KeyCode.Space))
				stabilizerMode ^= DroneStabilizerMode.StabPitchRoll;
			
			if (lastStab != stabilizerMode) 
				Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
		}

		
		/// <summary>Set control inputs manually.</summary>
		/// <remarks>Values will always be clamped in [-1, 1] range.</remarks>
		public void SetInputs(float throttle, float pitch, float yaw, float roll)
		{
			this.throttle = math.clamp(throttle, -1, 1);
			this.pitch = math.clamp(pitch, -1, 1);
			this.yaw = math.clamp(yaw, -1, 1);
			this.roll =  math.clamp(roll, -1, 1);
		}
	}
}
