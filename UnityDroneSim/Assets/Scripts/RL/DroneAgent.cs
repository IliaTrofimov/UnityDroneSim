using System;
using Drone;
using Exceptions;
using Navigation;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using UtilsDebug;


namespace RL
{
    public class DroneAgent : Agent
    {
        private DroneInputsController inputsController;
        private DroneState droneState;
        private Rigidbody droneRigidBody;
        private ObstacleTracker obstacleTracker;

        private bool isHeuristicsOnly;
        private int currentWaypointIndex;
        private float lastWaypointDistance;
        
        [Header("Drone")]
        [Tooltip("Drone flight computer.")]
        public QuadcopterComputer drone;
        
        [Tooltip("Waypoint navigation manager. This object must reference same QuadcopterComputer.")]
        public WaypointNavigator navigator;
        
        
        [Header("Train scenarios")]
        public SpawnPoint spawnPoint;
        
        
        [Header("Rewards")] 
        [Tooltip("Reward for reaching last waypoint and finishing episode.")]
        [Range(10f, 1000f)] public float finishReward = 500;
        
        [Tooltip("Reward for reaching waypoint.")]
        [Range(1f, 100f)] public float waypointReward = 100;
        
        [Tooltip("Reward for moving towards waypoint.")]
        [Range(0f, 10f)] public float nearWaypointReward = 15;
        
        [Tooltip("Penalty for destroying drone.")]
        [Range(-1000f, 0f)] public float destructionPenalty = -100;
        
        [Tooltip("Penalty for moving towards an obstacle.")]
        [Range(-10f, 0f)] public float nearObstaclePenalty = 10;
        
        public bool displayRewards = true;
        
        
        [Header("Obstacle avoidance")]
        [Tooltip("SphereCast radius for finding obstacles in front of the drone.")]        
        [Range(0.01f, 5f)] public float freeSpaceRadius = 1f;
        
        [Tooltip("SphereCast distance for finding obstacles in front of the drone.")]       
        [Range(0.01f, 100f)] public float obstacleDetectionDistance = 20f;

        
        public override void Initialize()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            ExceptionHelper.ThrowIfComponentIsMissing(this, navigator, nameof(navigator));
            
            if (navigator.drone != drone)
                throw new UnityException("WaypointNavigator's target drone is not the same as Agent's drone.");
            
            inputsController = drone.GetComponent<DroneInputsController>();
            droneState = drone.GetComponent<DroneState>();
            droneRigidBody = drone.rigidBody;
            
            isHeuristicsOnly = GetComponent<BehaviorParameters>()?.IsInHeuristicMode() ?? false;
            inputsController.manualInput = isHeuristicsOnly;
            
            obstacleTracker = GetComponent<ObstacleTracker>() ?? gameObject.AddComponent<ObstacleTracker>();
            obstacleTracker.Initialize(droneRigidBody, navigator, freeSpaceRadius, obstacleDetectionDistance);
        }

        private void OnDestroy()
        {
            Destroy(obstacleTracker);
        }

        private void OnDrawGizmos()
        {
            if (!displayRewards) return;
            VectorDrawer.DrawLabel(drone.transform.position, $"R: {GetCumulativeReward():F0}", new ()
            {
                color = Color.white,
                labelOutline = true,
            });
        }


        public override void OnEpisodeBegin()
        {
            spawnPoint.MoveInsideSpawnPoint(drone.transform);
            droneRigidBody.linearVelocity = Vector3.zero;
            droneRigidBody.angularVelocity = Vector3.zero;
            droneState.RepairAllMotors();
            drone.ResetStabilizers();
            navigator.ResetWaypoint();
            currentWaypointIndex = navigator.CurrentWaypointIndex;

            base.OnEpisodeBegin();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float distance = 0, direction = 0;
            if (!navigator.IsFinished)
            {
                 distance = navigator.GetCurrentDistance(drone.transform.position);
                 direction = drone.transform.NormalizedHeadingTo(navigator.CurrentWaypoint.position);   
            }
            
            sensor.AddObservation(droneState.AnyMotorsDestroyed);
            sensor.AddObservation(droneState.Landed);
            sensor.AddObservation(distance);
            sensor.AddObservation(direction);
            sensor.AddObservation(droneRigidBody.linearVelocity.x);
            sensor.AddObservation(droneRigidBody.linearVelocity.y);
            sensor.AddObservation(droneRigidBody.linearVelocity.z);
            sensor.AddObservation(droneRigidBody.angularVelocity.x);
            sensor.AddObservation(droneRigidBody.angularVelocity.y);
            sensor.AddObservation(droneRigidBody.angularVelocity.z);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (navigator && navigator.IsFinished)
            {
                this.EndEpisode(finishReward);
                return;
            }
            if (droneState.AnyMotorsDestroyed)
            {
                this.EndEpisode(destructionPenalty);
                return;
            }
            
            AddWaypointReward();
            AddObstaclePenalty();
            
            if (!isHeuristicsOnly)
            {
                inputsController.manualInput = false;
                inputsController.SetInputs(
                    actions.ContinuousActions[0],
                    actions.ContinuousActions[1],
                    actions.ContinuousActions[2], 
                    actions.ContinuousActions[3]
                );     
            }
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (isHeuristicsOnly)
            {
                inputsController.manualInput = true;
                actionsOut.ContinuousActions.Array[0] = inputsController.throttle;
                actionsOut.ContinuousActions.Array[1] = inputsController.pitch;
                actionsOut.ContinuousActions.Array[2] = inputsController.yaw;
                actionsOut.ContinuousActions.Array[3] = inputsController.roll;      
            }
        }
        
        private void AddWaypointReward()
        {
            if (currentWaypointIndex != navigator.CurrentWaypointIndex)
            {   
                currentWaypointIndex = navigator.CurrentWaypointIndex;
                AddReward(waypointReward);
            }
            
            var waypointDistance = (droneRigidBody.position - navigator.CurrentWaypoint.position).magnitude;
            if (lastWaypointDistance > waypointDistance)
            {
                var reward = nearWaypointReward / (waypointDistance + 1) * Time.fixedDeltaTime;
                AddReward(reward);
            }

            lastWaypointDistance = waypointDistance;
        }
        
        private void AddObstaclePenalty()
        {
            if (obstacleTracker.NextObstacleDistance > 0 && !droneState.Landed)
            {
                var penalty = nearObstaclePenalty / (obstacleTracker.NextObstacleDistance + 1) * Time.fixedDeltaTime;
                AddReward(penalty);
            }
        }
    }
}