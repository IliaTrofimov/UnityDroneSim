using UnityEngine;


namespace VelocityControl
{
	[System.Serializable]
	public class StateFinder : MonoBehaviour 
	{
		public float altitude; // The current altitude from the zero position
		public Vector3 angles;
		public Vector3 velocityVector;        // Velocity vector
		public Vector3 angularVelocityVector; // Angular Velocity
		public Vector3 inertia;
		public float mass;

		private bool flag = true; // Only get mass and inertia once 

		public VelocityControl vc; // linked externally

		public void GetState()
		{
			Vector3 worldDown = vc.transform.InverseTransformDirection (Vector3.down);
			float Pitch = worldDown.z; // Small angle approximation
			float Roll = -worldDown.x; // Small angle approximation
			float Yaw = vc.transform.eulerAngles.y;

			//		float Pitch = cc.transform.eulerAngles.x;
			//		Pitch = (Pitch > 180) ? Pitch - 360 : Pitch;
			//		Pitch = Pitch / 180.0f * 3.1416f; // Convert to radians
			//
			//		float Roll = cc.transform.eulerAngles.z;
			//		Roll = (Roll > 180.0f) ? Roll - 360.0f : Roll;
			//		Roll = Roll / 180.0f * 3.1416f; // Convert to radians
			//
			//		float Yaw = cc.transform.eulerAngles.y;
			//		Yaw = (Yaw > 180.0f) ? Yaw - 360.0f : Yaw;
			//		Yaw = Yaw / 180.0f * 3.1416f; // Convert to radians

			//		Altitude = cc.transform.position.y;
			//
			angles = new Vector3(Pitch, Yaw, Roll);

			altitude = vc.transform.position.y;

			velocityVector = vc.transform.GetComponent<Rigidbody> ().linearVelocity;
			velocityVector = vc.transform.InverseTransformDirection (velocityVector);

			angularVelocityVector = vc.transform.GetComponent<Rigidbody> ().angularVelocity;
			angularVelocityVector = vc.transform.InverseTransformDirection (angularVelocityVector);

			if (flag) 
			{
				inertia = vc.transform.GetComponent<Rigidbody> ().inertiaTensor;
				mass = vc.transform.GetComponent<Rigidbody> ().mass;
				flag = false;
			}
		}

		public void Reset()
		{
			flag = true;
			velocityVector = Vector3.zero;
			angularVelocityVector = Vector3.zero;
			angles = Vector3.zero;
			altitude = 0.0f;
			enabled = true;
		}
	}
}
