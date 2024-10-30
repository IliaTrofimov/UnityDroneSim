using UnityEngine;


namespace Camera
{
	public class CameraLookAt : MonoBehaviour 
	{
		private Transform target;
		private Quaternion originalRotation;
		
		public Vector3 offset;

		private void Awake()
		{
			target = GameObject.FindGameObjectWithTag("Player").transform; 
			originalRotation = transform.rotation;
		}

		public void Update()
		{
			var yaw = target.localEulerAngles.y;
			var relativeOffset = Quaternion.AngleAxis(yaw, Vector3.up) * offset;
			transform.position = transform.transform.position + relativeOffset;
			transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * originalRotation;
		}
	}
}
