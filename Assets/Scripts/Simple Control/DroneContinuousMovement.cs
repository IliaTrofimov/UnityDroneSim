using UnityEngine;


namespace Simple_Control
{
	public class DroneContinuousMovement : MonoBehaviour 
	{
		private float movementForwardSpeed = 500.0f;
		private float tiltAmountForward = 0;
		private float tiltVelocityForward;
		
		private float wantedYRotation;
		private float rotateAmountByKeys = 2.5f;
		private float rotationYVelocity;
		private Vector3 velocityToSmoothDampingToZero;
		
		private float sideMovementAmount = 300.0f;
		private float tiltAmountSideways;
		private float tiltAmountVelocity;
		
		private Rigidbody drone;
		
		public float upForce;
		public float currentYRotation;

		private void Awake()
		{
			drone = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			var vertical = Input.GetAxis("Vertical");
			var horizontal = Input.GetAxis("Horizontal");
			
			MovementUpDown(vertical, horizontal);
			MovementForward(vertical);
			Rotation();
			ClampingSpeedValues(vertical, horizontal);
			Swerve(vertical, horizontal);

			drone.AddRelativeForce(Vector3.up * upForce);
			drone.rotation = Quaternion.Euler(
				new Vector3(tiltAmountForward, currentYRotation, tiltAmountSideways)
			);
		}

		private void MovementUpDown(float vertical, float horizontal)
		{
			var verticalAboveThresh = Mathf.Abs(vertical) > .2f;
			var horizontalAboveThresh = Mathf.Abs(horizontal) > .2f;
			
			if (verticalAboveThresh || horizontalAboveThresh)
			{
				if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.K))
				{
					drone.linearVelocity = drone.linearVelocity;
				}

				var velocityY = Mathf.Lerp(drone.linearVelocity.y, 0, Time.deltaTime * 5);
				
				if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.L))
				{
					drone.linearVelocity = new Vector3(drone.linearVelocity.x, velocityY, drone.linearVelocity.z);
					upForce = 281;
				}
				if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
				{
					drone.linearVelocity = new Vector3(drone.linearVelocity.x, velocityY, drone.linearVelocity.z);
					upForce = 110;
				}
				if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L)) 
				{
					upForce = 410;
				}
			}
			if (!verticalAboveThresh && horizontalAboveThresh)
			{
				upForce = 135; 
			}
			
			if (Input.GetKey(KeyCode.I))
			{
				upForce = horizontalAboveThresh ? 500 : 450;
			} 
			else if (Input.GetKey(KeyCode.K))
			{
				upForce = -200;
			} 
			else if (!Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.I) && 
			         !horizontalAboveThresh && 
			         !verticalAboveThresh)
			{
				upForce = 98.1f;
			}
		}

		private void MovementForward(float vertical)
		{
			if (vertical == 0) return;

			drone.AddRelativeForce(Vector3.forward * (vertical * movementForwardSpeed));
			tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * vertical, ref tiltVelocityForward, 0.1f);
		}

		private void Rotation()
		{
			if (Input.GetKey(KeyCode.J))
				wantedYRotation -= rotateAmountByKeys;
			
			if (Input.GetKey(KeyCode.L))
				wantedYRotation += rotateAmountByKeys;
			
			currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, .25f);
		}

		private void ClampingSpeedValues(float vertical, float horizontal)
		{
			if (Mathf.Abs(vertical) > .2f || Mathf.Abs(horizontal) > .2f)
				drone.linearVelocity = Vector3.ClampMagnitude(drone.linearVelocity, Mathf.Lerp(drone.linearVelocity.magnitude, 10.0f, Time.deltaTime * 5f));
			else
				drone.linearVelocity = Vector3.SmoothDamp(drone.linearVelocity, Vector3.zero, ref velocityToSmoothDampingToZero, .95f);
		}
		
		private void Swerve(float vertical, float horizontal) 
		{
			if (horizontal > .2f)
			{
				drone.AddRelativeForce(Vector3.right * (horizontal * sideMovementAmount));
				tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * horizontal, ref tiltAmountVelocity, .1f);
			} 
			else
			{
				tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, 0, ref tiltAmountVelocity, .1f);
			}
		}
	}
}

