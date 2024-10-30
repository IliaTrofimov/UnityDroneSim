using UnityEngine;
using UnityEngine.Serialization;


// Basic motor class.  Have to be applied to a BasicControl class.  The motor only compute its force individualy.  The force application must be done by the Rigidbody class.
namespace RPY_PID_Control.Motors
{
	public class Motor : MonoBehaviour 
	{
		public float upForce = 0.0f;		// Total force to be applied by this motor.  This may be transfered to the parent RigidBody
		public float sideForce = 0.0f;		// Torque or side force applied by this motor.  This may be transfered to the parent RigidBody and get computed with others motors
		public float power = 2;				// A power multiplier. An easy way to create more potent motors
		public float exceedForce = 0.0f;	// Negative force value when Upforce gets below 0

		public float yawFactor = 0.0f;      // A factor to be applied to the side force.  Higher values get a faster Yaw movement
		public bool invertDirection;        // Whether the direction of the motor is counter or counterclockwise
		public float pitchFactor = 0.0f;	// A factor to be applied to the pitch correction
		public float rollFactor = 0.0f;		// A factor to be applied to the roll correction
		
		public BasicControl mainController; // Parent main controller. Where usually may be found the RigidBody
		public GameObject propeller;		// The propeller object. Animation will be done here.
		private float speedPropeller = 0;

		// Method called by BasicControl class to calculate force value of this specific motor.  The force application itself will be done at BasicControl class
		public void UpdateForceValues() 
		{
			var upForce = Mathf.Clamp(mainController.throttleValue, 0, 1) * power;
			var upForceTotal = upForce;
			upForceTotal -= mainController.computer.pitchCorrection * pitchFactor;
			upForceTotal -= mainController.computer.rollCorrection * rollFactor;

			this.upForce = upForceTotal;
			Debug.Log (this.upForce);

			sideForce = PreNormalize (mainController.controller.yaw, yawFactor);

			speedPropeller = Mathf.Lerp(speedPropeller, this.upForce * 2500.0f, Time.deltaTime);
			UpdatePropeller(speedPropeller);
		}

		public void UpdatePropeller(float speed)
		{
			propeller.transform.Rotate(0.0f, 0.0f, speedPropeller * 2 * Time.deltaTime);
		}

		private float PreNormalize(float input, float factor)
		{
			input = invertDirection ? Mathf.Clamp(input, -1, 0) : Mathf.Clamp(input, 0, 1);
			return input * yawFactor;
		}
	}
}
