
using Navigation;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


namespace RL
{
	/*
	public class DroneAgent: Agent
	{
		private GameObject currentStartRegion;
		private GameObject currentEndRegion;
		private FreeSpaceDetection freeSpace;
		private Bounds endBounds;

		private bool collided;
		private bool localIsDone;
		private System.Random random;

		private float maxX;
		private float minX;
		private float maxZ;
		private float minZ;
		private Vector3 initialPos;
		private Rigidbody rigidbody;

		public bool useNewState = true;
		
		public int startRegionIndex = -1; // -1 means do random
		public GameObject[] startRegions;
		public int endRegionIndex = -1; // -1 means do random
		public GameObject[] endRegions;

		public float forwardVelocity;
		public float yawRate;
		public float doneDistance;

		public override void Initialize()
		{
			freeSpace = gameObject.GetComponent<FreeSpaceDetection>();
			random = new System.Random();
			rigidbody = gameObject.GetComponent<Rigidbody>();

			if (startRegions.Length == 0 || startRegions.GetValue(0) == null)
			{
				startRegions = new GameObject[1];
				var startRegion = GameObject.CreatePrimitive(PrimitiveType.Quad);
				startRegion.transform.Rotate(new Vector3(90, 0, 0));
				startRegion.transform.localScale = new Vector3(3.0f, 3.0f, 1.0f);
				startRegion.transform.localPosition = new Vector3(startRegion.transform.localPosition.x,
					startRegion.transform.localPosition.y - 1,
					startRegion.transform.localPosition.z);
				startRegions.SetValue(startRegion, 0);
			}

			if (endRegions.Length == 0 || endRegions.GetValue(0) == null)
			{
				endRegions = new GameObject[1];
				var endRegion = GameObject.CreatePrimitive(PrimitiveType.Quad);
				endRegion.transform.Rotate(new Vector3(90, 0, 0));
				endRegion.transform.localScale = new Vector3(3.0f, 3.0f, 1.0f);
				endRegion.transform.localPosition = new Vector3(endRegion.transform.localPosition.x,
					endRegion.transform.localPosition.y - 1,
					endRegion.transform.localPosition.z + 100);
				endRegions.SetValue(endRegion, 0);
			}

			DefaultOrRandomizeSetStartEnd();

			Debug.Log ("Start BOUNDS");
			var rend = currentStartRegion.GetComponent<Renderer>();
			Debug.Log(rend.bounds.max);
			Debug.Log(rend.bounds.min);

			maxX = rend.bounds.max.x;
			minX = rend.bounds.min.x;
			maxZ = rend.bounds.max.z;
			minZ = rend.bounds.min.z;

			initialPos = new Vector3(transform.position.x, velocityControl.initial_height, transform.position.z);
			endBounds = currentEndRegion.GetComponent<Renderer>().bounds;

			var startX = (float)random.NextDouble() * (maxX - minX) + minX;
			var startZ = (float)random.NextDouble() * (maxZ - minZ) + minZ;

			transform.position = new Vector3 (startX, initialPos.y, startZ);

			collided = false;
			localIsDone = false;
		}

		private void DefaultOrRandomizeSetStartEnd()
		{
			var sri = startRegionIndex == -1 ? random.Next(startRegions.Length) : startRegionIndex;
			var eri = endRegionIndex == -1 ? random.Next(endRegions.Length) : endRegionIndex;

			if (sri >= startRegions.Length || startRegions[sri] is null)
				throw new UnityException($"Start Region at index: {sri}, is invalid");
			if (eri >= endRegions.Length || endRegions[eri] is null)
				throw new UnityException($"End Region at index: {eri}, is invalid");

			currentStartRegion = startRegions[sri];
			currentEndRegion = endRegions[eri];
		}

		public float NormalizedHeading(Vector3 current, Vector3 target)
		{
			var normalized = Vector3.Normalize(target - current);
			normalized.y = 0.0f;

			var currentHeading = Quaternion.Euler(new Vector3(0.0f, velocityControl.state.angles.y, 0.0f)) * Vector3.forward;
			currentHeading.y = 0.0f;

			var angle = Vector3.SignedAngle(currentHeading, normalized, Vector3.up);
			return angle;
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			localIsDone = IsDone() || collided;

			if (useNewState)
				CollectNewObservations(sensor);
			else
				CollectOldObservations(sensor);

			if (collided)
			{
				Debug.Log("COLLISION MSG SENT");
				collided = false;
			}
		}

		private void CollectOldObservations(VectorSensor sensor)
		{
			//13 elements
			sensor.AddObservation(velocityControl.state.velocityVector.z / 8.0f);          // VX scaled
			sensor.AddObservation(velocityControl.state.velocityVector.x / 8.0f);          // VY scaled
			sensor.AddObservation(velocityControl.state.angularVelocityVector.y / 360.0f); //Yaw scaled

			sensor.AddObservation(velocityControl.transform.position.x);
			sensor.AddObservation(velocityControl.transform.position.y);
			sensor.AddObservation(velocityControl.transform.position.z);

			sensor.AddObservation(velocityControl.transform.rotation.x);
			sensor.AddObservation(velocityControl.transform.rotation.y);
			sensor.AddObservation(velocityControl.transform.rotation.z);

			sensor.AddObservation(currentEndRegion.transform.position.x);
			sensor.AddObservation(currentEndRegion.transform.position.y);
			sensor.AddObservation(currentEndRegion.transform.position.z);
			sensor.AddObservation(collided ? 1.0f : 0.0f);
		}

		private void CollectNewObservations(VectorSensor sensor)
		{
			sensor.AddObservation(NormalizedHeading(transform.position, currentEndRegion.transform.position) / 180.0f); //-1 to 1
			sensor.AddObservation(Vector3.Magnitude(transform.position - currentEndRegion.transform.position));         // nonscaled magnitude
			//Velocities (v forward, yaw)
			sensor.AddObservation(velocityControl.state.velocityVector.z / forwardVelocity); // VX scaled -1 to 1
			sensor.AddObservation(velocityControl.state.angularVelocityVector.y / yawRate);  //Yaw rate scaled -1  to 1
			//collision
			sensor.AddObservation(collided ? 1.0f : 0.0f);
			sensor.AddObservation(freeSpace.BatchRaycast());
		}

		// 1 element input
		// -> -1 : STOP
		// -> 0 : LEFT + FORWARD
		// -> 1 : STRAIGHT + FORWARD
		// -> 2 : RIGHT + FORWARD
		public override void OnActionReceived(ActionBuffers actions)
		{
			if (IsDone())
				rigidbody.linearVelocity = Vector3.zero;

			velocityControl.desired_vx = actions.ContinuousActions[0] >= 0 ? forwardVelocity : 0.0f;
			velocityControl.desired_vy = 0.0f;

			if (actions.ContinuousActions[0] < -1 + 1e-8)
				velocityControl.desired_yaw = 0.0f;
			else if (actions.ContinuousActions[0] < 1e-8)
				velocityControl.desired_yaw = -yawRate;
			else if (actions.ContinuousActions[0] < 1 + 1e-8)
				velocityControl.desired_yaw = 0.0f;
			else
				velocityControl.desired_yaw = yawRate;

			if (localIsDone)
				rigidbody.isKinematic = true;
		}

		public bool IsDone()
		{
			return Vector3.Magnitude(transform.position - endBounds.center) <= doneDistance;
		}

		public void Reset()
		{
			Debug.Log("Resetting");

			localIsDone = false;

			//pick new start and end
			DefaultOrRandomizeSetStartEnd();

			//temporary
			velocityControl.enabled = false;
			// randomness
			float startX = ((float) random.NextDouble()) * (maxX - minX) + minX;
			float startZ = ((float) random.NextDouble()) * (maxZ - minZ) + minZ;

			transform.position = new Vector3 (startX, initialPos.y, startZ);
			transform.rotation = Quaternion.AngleAxis((float)random.NextDouble() * 2.0f * 180.0f, Vector3.up );
			//reset, which also re enables

			//StartCoroutine (Waiting (1.0f));
			//while (!wait) {
			//}

			GetComponent<Rigidbody>().isKinematic = false;
			velocityControl.Reset ();
		}
	}
	*/
}
