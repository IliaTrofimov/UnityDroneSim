using Exceptions;
using UnityEngine;


namespace Camera
{
	/// <summary>
	/// Third person view camera movement settings.
	/// </summary>
	public enum FollowMode
	{
		/// <summary>Camera is stationary and can only rotate.</summary>
		None,
		
		/// <summary>Camera can only move within XZ plane to follow player.</summary>
		KeepPlane,
		
		/// <summary>Camera can only move along Y axis to follow player.</summary>
		KeepHeight,
		
		/// <summary>Camera can move within XZ plane and along Y axis (separately) to follow player.</summary>
		KeepPlaneAndHeight,
		
		/// <summary>Camera always moves directly to player.</summary>
		Follow
	}

	
	/// <summary>
	/// Third person view camera that always keeps player in focus and follows him.
	/// </summary>
	public class CameraLookAt : MonoBehaviour 
	{
		private Transform target;
		private UnityEngine.Camera cameraObj;

		[Range(1f, 100f)] public float minDistance = 1f;
		[Range(1f, 100f)] public float maxDistance = 50f;
		public FollowMode followMode = FollowMode.KeepPlane;
		
		private void Awake()
		{
			target = GameObject.FindGameObjectWithTag("Player")?.transform;
			ExceptionHelper.ThrowIfComponentIsMissing(this, target, nameof(target));
			
			cameraObj = GetComponent<UnityEngine.Camera>();
			ExceptionHelper.ThrowIfComponentIsMissing(this, cameraObj);
		}

		private void OnValidate()
		{
			if (minDistance > maxDistance)
			{
				Debug.LogWarning($"{nameof(maxDistance)} must be greater than {nameof(minDistance)}. Swapping {nameof(maxDistance)} with {nameof(minDistance)} ({maxDistance} to {minDistance})");
				(minDistance, maxDistance) = (maxDistance, minDistance);
			}
		}

		public void Update()
		{
			transform.LookAt(target);
			switch (followMode)
			{
				case FollowMode.KeepPlane:
				{
					var r = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
					MoveCamera(Mathf.Abs(r.magnitude), r.normalized);	
				} break;
				case FollowMode.KeepHeight:
					MoveCamera(Mathf.Abs(target.position.y - transform.position.y), Vector3.up);
					break;
				case FollowMode.KeepPlaneAndHeight:
				{
					var r = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
					MoveCamera(Mathf.Abs(r.magnitude), r.normalized);	
					MoveCamera(Mathf.Abs(target.position.y - transform.position.y), Vector3.up);
				} break;
				case FollowMode.Follow:
				{
					var r = target.position - transform.position;
					MoveCamera(Mathf.Abs(r.magnitude), r.normalized);	
				} break;
			}
		}

		private void MoveCamera(float distance, Vector3 directionNorm)
		{
			if (distance > maxDistance)
				transform.position -= directionNorm * (distance - maxDistance);
			else if (distance < minDistance)
				transform.position += directionNorm * (minDistance - distance);
		}
	}
}
