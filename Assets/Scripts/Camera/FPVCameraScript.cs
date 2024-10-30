using UnityEngine;


namespace Camera
{
	public class FPVCameraScript : MonoBehaviour
	{
		private Transform target;
		
		public Vector3 offset;
		[Range(0,1)] public float temp;
		[Range(0,1)] public float rpLimitRatio;
	
		private void Awake()
		{
			target = GameObject.FindGameObjectWithTag("Player").transform; 
		}
		
		private void Update() 
		{
			transform.position = target.position + target.rotation * offset;

			var euler = target.rotation.eulerAngles;
			var x = (euler.x > 180.0f ? euler.x - 360.0f : euler.x) * rpLimitRatio;
			var z = (euler.z > 180.0f ? euler.z - 360.0f : euler.z) * rpLimitRatio;

			var nx = x > 0 ? x : 360.0f + x;
			var nz = z > 0 ? z : 360.0f + z;
		
			var newEuler = new Vector3(nx, euler.y, nz);
			var targetRotation = Quaternion.Euler(newEuler);

			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, temp);
		}
	}
}
