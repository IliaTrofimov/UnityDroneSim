using System;
using Exceptions;
using UnityEngine;


namespace Camera
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class SmoothCamera : MonoBehaviour
	{
		public Transform target;
		[Range(0f, 1f)] public float followRate;
		[Range(0f, 1f)] public float rotationErrorRatio;

		private Quaternion initialLocalRotation;
		
		private void Awake() => ExceptionHelper.ThrowIfComponentIsMissing(this, target, nameof(target));

		private void OnEnable() => initialLocalRotation = transform.localRotation;
		private void OnDisable() => transform.localRotation = initialLocalRotation;

		private void LateUpdate() 
		{
			if (!enabled)
			{
				transform.localRotation = initialLocalRotation;
				return;
			}
			else if (target is null)
			{
				return;
			}
			
			var euler = target.rotation.eulerAngles;
			var x = (euler.x > 180.0f ? euler.x - 360.0f : euler.x) * rotationErrorRatio;
			var z = (euler.z > 180.0f ? euler.z - 360.0f : euler.z) * rotationErrorRatio;

			var nx = x > 0 ? x : 360.0f + x;
			var nz = z > 0 ? z : 360.0f + z;
		
			var targetRotation = Quaternion.Euler(nx, euler.y, nz);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followRate);
		}
	}
}
