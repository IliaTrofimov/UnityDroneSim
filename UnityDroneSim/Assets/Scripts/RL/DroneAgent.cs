using Drone;
using Exceptions;
using Navigation;
using RL.Rewards;
using RL.RewardsSettings;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace RL
{
    /// <summary>
    /// AI agent for flying drone. Uses <see cref="DroneInputsController"/> to pass controls for <see cref="DroneComputerBase"/>.
    /// </summary>
    public class DroneAgent : Agent
    {
        private DroneInputsController _inputsController;
        private DroneStateManager     _droneState;
        private Rigidbody             _droneRigidBody;

        /// <summary>Agent is using heuristics instead of neural network.</summary>
        public bool IsHeuristicsOnly { get; private set; }

        /// <summary>Composite rewards for drone agent.</summary>
        public DroneAgentRewardProvider RewardProvider { get; private set; }


        [Header("Main settings")]
        [Tooltip("Drone flight computer.")]
        public DroneComputerBase drone;

        [Tooltip("Waypoint navigation manager. This object must reference same QuadcopterComputer.")]
        public WaypointNavigator navigator;

        [Tooltip("Place where drone will start new episode.")]
        public SpawnPoint spawnPoint;

        
        [Header("Training parameters")]
        [Tooltip("Training parameters. Can be overridden by parent DroneTrainingManager component.")]
        public TrainingSettings trainingSettings;

        [Tooltip("Display current cumulative reward value near drone object.")]
        public bool displayRewards = true;


        private new void Awake()
        {
            if (!trainingSettings)
            {
                trainingSettings = ScriptableObject.CreateInstance<TrainingSettings>();
                Debug.LogFormat(
                    "DroneAgent '{0}': [1] missing TrainingSettings parameter. Default one will be instantiated.",
                    drone.name
                );
            }

            base.Awake();
        }

        public override void Initialize()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            ExceptionHelper.ThrowIfComponentIsMissing(this, navigator, nameof(navigator));

            if (navigator.drone != drone)
                throw new UnityException("WaypointNavigator's target drone is not the same as Agent's drone.");

            _inputsController = drone.GetComponent<DroneInputsController>();
            _droneState = drone.GetComponent<DroneStateManager>();
            _droneRigidBody = drone.Rigidbody;

            IsHeuristicsOnly = GetComponent<BehaviorParameters>()?.IsInHeuristicMode() ?? false;
            _inputsController.manualInput = IsHeuristicsOnly;
            if (IsHeuristicsOnly)
                Debug.LogFormat("DroneAgent '{0}': running in heuristic mode", drone.name);

            if (!trainingSettings)
            {
                trainingSettings = ScriptableObject.CreateInstance<TrainingSettings>();
                Debug.LogFormat(
                    "DroneAgent '{0}': [2] missing TrainingSettings parameter. Default one will be instantiated.",
                    drone.name
                );
            }

            RewardProvider = new DroneAgentRewardProvider(trainingSettings, this, _droneState);
        }

        public void InitRewardsProvider()
        {
            RewardProvider = new DroneAgentRewardProvider(trainingSettings, this, _droneState);
        }

        private void OnDrawGizmos()
        {
            if (!displayRewards) return;

            VectorDrawer.DrawLabel(drone.transform.position,
                $"R: {GetCumulativeReward():F0}",
                new GizmoOptions { Color = IsHeuristicsOnly ? Color.yellow : Color.white, LabelOutline = true }
            );
        }

        public override void OnEpisodeBegin()
        {
            spawnPoint.MoveInsideSpawnPoint(drone.transform);
            RewardProvider.Reset();
            _droneRigidBody.linearVelocity = Vector3.zero;
            _droneRigidBody.angularVelocity = Vector3.zero;
            _droneState.RepairAllMotors();
            drone.ResetStabilizers();
            navigator.ResetWaypoint();
            base.OnEpisodeBegin();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float distance = 0, direction = 0;
            if (navigator.CurrentWaypoint.HasValue)
            {
                distance = navigator.GetCurrentDistance(drone.transform.position);
                direction = drone.transform.NormalizedHeadingTo(navigator.CurrentWaypoint.Value.position);
            }

            sensor.AddObservation(_droneState.AnyMotorsDestroyed);
            sensor.AddObservation(_droneState.Landed);
            sensor.AddObservation(distance);
            sensor.AddObservation(direction);
            sensor.AddObservation(_droneRigidBody.linearVelocity.x);
            sensor.AddObservation(_droneRigidBody.linearVelocity.y);
            sensor.AddObservation(_droneRigidBody.linearVelocity.z);
            sensor.AddObservation(_droneRigidBody.angularVelocity.x);
            sensor.AddObservation(_droneRigidBody.angularVelocity.y);
            sensor.AddObservation(_droneRigidBody.angularVelocity.z);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var reward = RewardProvider.CalculateReward();
            AddReward(reward);
            if (RewardProvider.IsFinalReward)
            {
                EndEpisode();
                return;
            }

            if (!IsHeuristicsOnly)
            {
                _inputsController.manualInput = false;
                _inputsController.SetInputs(
                    actions.ContinuousActions[0],
                    actions.ContinuousActions[1],
                    actions.ContinuousActions[2],
                    actions.ContinuousActions[3]
                );
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (!IsHeuristicsOnly) return;

            _inputsController.manualInput = true;
            actionsOut.ContinuousActions.Array[0] = _inputsController.throttle;
            actionsOut.ContinuousActions.Array[1] = _inputsController.pitch;
            actionsOut.ContinuousActions.Array[2] = _inputsController.yaw;
            actionsOut.ContinuousActions.Array[3] = _inputsController.roll;
        }
    }
}