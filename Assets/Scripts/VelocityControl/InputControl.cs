using UnityEngine;


namespace VelocityControl
{
	public class InputControl : MonoBehaviour 
	{
		private float absHeight = 1;
	
		public VelocityControl vc;
	
		private void FixedUpdate ()
		{
			vc.desired_vx = Input.GetAxisRaw("Pitch")*4.0f;
			vc.desired_vy = Input.GetAxisRaw("Roll")*4.0f;
			vc.desired_yaw = Input.GetAxisRaw("Yaw")*0.5f;
			absHeight += Input.GetAxisRaw("Throttle") * 0.1f;
			vc.desired_height = absHeight;
		}
	}
}
