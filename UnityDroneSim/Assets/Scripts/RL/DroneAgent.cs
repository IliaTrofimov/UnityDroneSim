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
        public enum LogsMode { Never, HeuristicOnly, Always }
        
        private DroneInputsController _inputsController;
        private DroneStateManager     _droneState;
        private Rigidbody             _droneRigidBody;
        private bool                  _logsEnabled;
        
        private bool IsInitialized => _droneState && drone && _droneRigidBody && navigator;

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
        
        [Tooltip("When to print debug messages.")]
        public LogsMode logsMode = LogsMode.HeuristicOnly;


        protected override void OnEnable()
        {
            InitComponents();
            base.OnEnable();
        }
        
        public override void Initialize()
        {   
            var behaviour = GetComponent<BehaviorParameters>();
            if (behaviour == null)
            {
                IsHeuristicsOnly = true;
                Debug.LogFormat("DroneAgent '{0}': running in heuristic mode (cannot find BehaviorParameters component).", drone.name);
            }
            else if (behaviour.IsInHeuristicMode())
            {
                IsHeuristicsOnly = true;
                Debug.LogFormat("DroneAgent '{0}': running in heuristic mode.", drone.name);
            }
            else
            {
                IsHeuristicsOnly = false;
            }
            
            _inputsController.manualInput = IsHeuristicsOnly;
            _logsEnabled = logsMode == LogsMode.Always || logsMode == LogsMode.HeuristicOnly && IsHeuristicsOnly;
            InitRewardsProvider();
        }
        
        public void InitRewardsProvider()
        {
            if (!IsInitialized) InitComponents();
            
            if (!trainingSettings)
            {
                if (_logsEnabled)
                {
                    Debug.LogFormat(
                        "DroneAgent '{0}': missing TrainingSettings parameter. Default one will be instantiated.",
                        drone.name
                    );     
                }
               
                trainingSettings = ScriptableObject.CreateInstance<TrainingSettings>();
                trainingSettings.InitDefault();
            }

            RewardProvider = _logsEnabled 
                ? new DroneAgentRewardProvider(trainingSettings, this, _droneState, true, drone.name) 
                : new DroneAgentRewardProvider(trainingSettings, this, _droneState);
        }

        private void InitComponents()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            ExceptionHelper.ThrowIfComponentIsMissing(this, navigator, nameof(navigator));

            if (navigator.drone != drone)
                throw new UnityException("WaypointNavigator's target drone is not the same as Agent's drone.");

            _inputsController = drone.GetComponent<DroneInputsController>();
            _droneState = drone.GetComponent<DroneStateManager>();
            _droneRigidBody = drone.Rigidbody;
        }

        
        private void OnDrawGizmos()
        {
            if (!displayRewards) return;
            
            VectorDrawer.DrawLabel(drone.transform.position,
                RewardProvider?.TimeLeft >= 0
                    ? $"R: {GetCumulativeReward():F0}\nt: {RewardProvider.TimeLeft:F0} s"
                    : $"R: {GetCumulativeReward():F0}\n",
                new GizmoOptions { LabelColor = IsHeuristicsOnly ? Color.yellow : Color.white, LabelOutline = true }
            );
        }
        
        private void OnDrawGizmosSelected()
        {
            if (displayRewards) RewardProvider?.DrawGizmos();
        }

        public override void OnEpisodeBegin()
        {
            _droneRigidBody.linearVelocity = Vector3.zero;
            _droneRigidBody.angularVelocity = Vector3.zero;
            _droneState.RepairAllMotors();
            spawnPoint.MoveInsideSpawnPoint(drone.transform);
            drone.ResetStabilizers();
            navigator.ResetWaypoint();
            RewardProvider.Reset();
            base.OnEpisodeBegin();

            if (_logsEnabled)
                Debug.LogFormat("DroneAgent '{0}': begin episode.", drone.name); 
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