using Exceptions;
using Inputs;
using UnityEngine;

namespace VelocityControl
{
	public class InputControl : MonoBehaviour 
	{
		private float absHeight = 1;
		private Vector3 movement;
		private Vector2 rotation;
		private DroneControls controls;
		
		public VelocityControl velocityControl;

		private void Awake()
		{
			ExceptionHelper.ThrowIfComponentIsMissing(this, velocityControl, nameof(velocityControl));
			controls = new DroneControls();
			Debug.Log("Start");
		}

		private void OnGUI()
		{ 
			GUI.Label(new Rect(10, 10, 200, 100), $"Movement: {movement};\nRotation: {rotation}");
		}

		private void FixedUpdate()
		{
			rotation = controls.Default.Rotation.ReadValue<Vector2>();
			//movement = controls.Default.Movement.ReadValue<Vector3>();

			velocityControl.desired_vx = movement.z;
			velocityControl.desired_vy = movement.x;
			velocityControl.desired_yaw = rotation.x;
			absHeight += movement.y * 0.1f;
			velocityControl.desired_height = absHeight;
		}

		public void OnEnable()
		{
			controls?.Enable();	
		} 

		public void OnDisable() 
		{
			controls?.Disable();	
		} 
	}
}
