using UnityEngine;


// Basic gyroscope simulator.  Uses the zero and identity to calculate.  This one suffre from gimball lock effect
namespace RPY_PID_Control
{
	[System.Serializable]
	public class BasicGyro 
	{
		public float pitch;				// The current pitch for the given transform
		public float roll;				// The current roll for the given transform
		public float yaw;				// The current Yaw for the given transform
		public float altitude;			// The current altitude from the zero position
		public Vector3 velocityVector;	// Velocity vector
		public float velocityScalar;    // Velocity scalar value
		
		
		public void UpdateGyro(Rigidbody rigidbody) 
		{
			pitch = rigidbody.transform.eulerAngles.x;
			pitch = pitch > 180 ? pitch - 360 : pitch;
		
			roll = rigidbody.transform.eulerAngles.z;
			roll = roll > 180 ? roll - 360 : roll;

			yaw = rigidbody.transform.eulerAngles.y;
			yaw = yaw > 180 ? yaw - 360 : yaw;

			altitude = rigidbody.transform.position.y;
			velocityVector = rigidbody.linearVelocity;
			//velocityScalar = velocityVector.magnitude;
		}
	}
}
