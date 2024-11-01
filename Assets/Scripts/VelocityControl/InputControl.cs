
using System;
using Inputs;
using UnityEngine;
using UnityEngine.InputSystem;


namespace VelocityControl
{
	public class InputControl : MonoBehaviour 
	{
		private float absHeight = 1;
		private Vector3 movement;
		private Vector2 rotation;
		private SimpleControls controls;

		
		public VelocityControl velocityControl;

		private void Awake()
		{
			controls = new SimpleControls();
			if (velocityControl is null)
				throw new UnityException("Velocity control not found");
			Debug.Log("Start");
		}

		private void OnGUI()
		{ ;
			GUI.Label(new Rect(10, 10, 200, 100), $"Movement: {movement};\nRotation: {rotation}");
		}

		private void FixedUpdate()
		{
			rotation = controls.gameplay.rotate.ReadValue<Vector2>();
			movement = controls.gameplay.move.ReadValue<Vector3>();
			Debug.Log($"Movement: {movement};\nRotation: {rotation}");

			velocityControl.desired_vx = movement.z;
			velocityControl.desired_vy = movement.x;
			velocityControl.desired_yaw = rotation.x;
			absHeight += movement.y * 0.1f;
			velocityControl.desired_height = absHeight;
		}


		public void OnEnable()
		{
			Debug.Log($"OnEnable: controls is null: {controls == null}");
			controls?.Enable();	
		} 

		public void OnDisable() 
		{
			Debug.Log($"OnDisable: controls is null: {controls == null}");
			controls?.Disable();	
		} 
	}
}
