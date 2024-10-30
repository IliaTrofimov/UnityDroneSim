using System;
using UnityEngine;
using UnityEngine.Serialization;


namespace RPY_PID_Control.Controller
{
	public enum ThrottleMode { None, LockHeight};
	
	
	public class Controller : MonoBehaviour 
	{
		public bool isAgentControl;

		[Header("Values")]
		public float throttle;
		public float yaw;
		public float pitch;
		public float roll;
		
		[Header("Throttle command")]
		public string throttleCommand = "Throttle";
		public bool invertThrottle = true;

		[Header("Yaw Command")]
		public string yawCommand = "Yaw";
		public bool invertYaw = false;

		[NonSerialized] [Header("Pitch Command")]
		public string pitchCommand = "Pitch";
		public bool invertPitch = true;

		[Header("Roll Command")]
		public string rollCommand = "Roll";
		public bool invertRoll = true;


		private void Update()
		{
			if (isAgentControl) return;

			throttle = Input.GetAxisRaw(throttleCommand) * (invertThrottle ? -1 : 1);
			yaw = Input.GetAxisRaw(yawCommand) * (invertYaw ? -1 : 1);
			pitch = Input.GetAxisRaw(pitchCommand) * (invertPitch ? -1 : 1);
			roll = Input.GetAxisRaw(rollCommand) * (invertRoll ? -1 : 1);
		}

		public void InputAction(float throttle, float pitch, float roll, float yaw)
		{
			if (!isAgentControl) return;

			this.throttle = throttle;
			this.yaw = yaw;
			this.pitch = pitch;
			this.roll = roll;
		}
	}
}
