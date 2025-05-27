using System;
using System.Collections.Generic;
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
using UnityEngine.Serialization;
using Utils;
using UtilsDebug;


namespace RL
{
    /// <summary>
    /// AI agent that controls drone <see cref="DroneComputerBase"/> sending inputs to <see cref="DroneInputsController"/>.
    /// Agent uses <see cref="DroneAgentRewardProvider"/> to calculate its rewards.
    /// </summary>
    public class DroneAgent : Agent
    {
        public IReadOnlyList<(string label, string format)> ScalarObservationsLabels =>
            DroneAgentObservationLabels.GetLabels(observationsSettings.useNormalization, observationsSettings.useAdditionalObservations);
        
        
        public enum LogsMode { Never, HeuristicOnly, Always }
        
        private DroneInputsController _inputsController;
        private DroneStateManager     _droneState;
        private Rigidbody             _droneRigidBody;
        private GameObject            _trailObject;
        private TrailRenderer         _trailRenderer;
        private bool                  _logsEnabled;
        private bool                  _shouldResetState = true;
        
        private string _modelName;
        
        private Quaternion _initialRotation;
        private Vector3 _initialPosition;
        
        private bool IsInitialized => _droneState && drone && _droneRigidBody && navigator;

        /// <summary>Agent is using heuristics instead of neural network.</summary>
        public bool IsHeuristicsOnly { get; private set; }
        
        /// <summary>Agent is using inference.</summary>
        public bool HasModel { get; private set; }

        /// <summary>Composite rewards for drone agent.</summary>
        public DroneAgentRewardProvider RewardProvider { get; private set; }


        [Header("Main settings")]
        [Tooltip("Drone flight computer.")]
        public DroneComputerBase drone;

        [Tooltip("Waypoint navigation manager. This object must reference same QuadcopterComputer.")]
        public WaypointNavigator navigator;

        [Tooltip("Place where drone will start new episode.")]
        public SpawnPoint spawnPoint;

        [Tooltip("Scalar observations values parameters. Can be overridden by parent DroneTrainingManager component.")]
        public ObservationSettings observationsSettings;
        
        [Header("Training parameters")]
        [Tooltip("Training parameters. Can be overridden by parent DroneTrainingManager component.")]
        public TrainingSettings trainingSettings;
        
        [Header("Debug")]
        [Tooltip("When to print debug messages.")]
        public LogsMode logsMode = LogsMode.HeuristicOnly;
        
        [Tooltip("Automatically reset episode if final state is reached even in heuristics mode.")]
        public bool resetEpisodeInHeuristic;
        
        [Tooltip("Display current cumulative reward value near drone object.")]
        public bool displayRewards = true;
        
        [Tooltip("Display trail behind drone. Trail will be cleared after each new episode.")]
        public bool displayTrail = true;
        
        [Tooltip("This object will be used to create trail (must contain TrailRenderer component inside).")]
        public GameObject trailPrefab;
        
        
        protected override void OnEnable()
        {
            InitComponents();
            base.OnEnable();
            
        }

        private void Start()
        {
            _initialRotation = drone.transform.rotation;
            _initialPosition = drone.transform.position;
        }
        
        public override void Initialize()
        {   
            HasModel = false;
            IsHeuristicsOnly = false;
            
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
            else if (behaviour.Model != null)
            {
                _modelName = behaviour.Model.name;
                HasModel = true;
                Debug.LogFormat("DroneAgent '{0}': running in inference mode with model '{1}'.", drone.name, _modelName);
            }
            
            _inputsController.manualInput = IsHeuristicsOnly;
            _logsEnabled = logsMode == LogsMode.Always || logsMode == LogsMode.HeuristicOnly && IsHeuristicsOnly;
            InitRewardsProvider();
            base.Initialize();
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

        private void InitTrailRenderer()
        {
            if (!displayTrail || !trailPrefab) return;
            
            if (!_trailRenderer)
            {
                var trailObject = Instantiate(trailPrefab, drone.gameObject.transform);
                _trailRenderer = trailObject.GetComponent<TrailRenderer>();
            } 
            
            _trailRenderer.time = trainingSettings.termination.trainingEpisodeTime > 0 
                ? trainingSettings.termination.trainingEpisodeTime
                : 60f;

            _trailRenderer.Clear();
            _trailRenderer.autodestruct = true;
            _trailRenderer.enabled = true;
            _trailRenderer.emitting = true;
        }

        private void InitComponents()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            ExceptionHelper.ThrowIfComponentIsMissing(this, navigator, nameof(navigator));

            if (!navigator.drone)
                navigator.drone = drone;
            else if (navigator.drone != drone)
                throw new UnityException("WaypointNavigator's target drone is not the same as Agent's drone.");

            _inputsController = drone.GetComponent<DroneInputsController>();
            _droneState = drone.GetComponent<DroneStateManager>();
            _droneRigidBody = drone.Rigidbody;
        }

        public void OnEpisodeBegin(bool shouldResetState)
        {
            if (!shouldResetState)
            {
                base.OnEpisodeBegin();
                return;
            }
            
            _droneRigidBody.linearVelocity = Vector3.zero;
            _droneRigidBody.angularVelocity = Vector3.zero;
            _droneState.RepairAll();
            drone.ResetStabilizers();
            navigator.ResetWaypoint();
            
            if (spawnPoint)
            {
                spawnPoint.MoveInsideSpawnPoint(drone.transform);
            }
            else
            {
                drone.transform.rotation = _initialRotation;
                drone.transform.position = _initialPosition;
            }
            
            InitTrailRenderer();
            base.OnEpisodeBegin();

            if (_logsEnabled)
                Debug.LogFormat("DroneAgent '{0}': begin episode.", drone.name); 
        }

        public override void OnEpisodeBegin() => OnEpisodeBegin(true);

        public void EndEpisode(bool shouldSaveStats)
        {
            RewardProvider.Reset(shouldSaveStats);
            EndEpisode();
        }

        
        public override void CollectObservations(VectorSensor sensor)
        {
            var distance = navigator.GetCurrentDistance(drone.transform.position);
            var heading = navigator.GetCurrentHeading(drone.transform, observationsSettings.useNormalization);
            var altitude = RewardProvider.GetDroneAltitude();
            
            Vector3 velocity;
            Vector3 angularVelocity;

            if (observationsSettings.useLocalCoordinates)
            {
                velocity = drone.transform.InverseTransformVector(_droneRigidBody.linearVelocity);
                angularVelocity = _droneRigidBody.AxialAngularVelocity();
            }
            else
            {
                velocity = _droneRigidBody.linearVelocity;
                angularVelocity = _droneRigidBody.angularVelocity;
            }
            
            if (observationsSettings.useNormalization)
            {
                distance /= observationsSettings.maxWaypointDistanceNormalization;
                altitude /= observationsSettings.maxAltitudeNormalization;
                velocity /= observationsSettings.maxVelocityNormalization;
                angularVelocity /= observationsSettings.maxAngularVelocityNormalization;
            }

            if (observationsSettings.useAdditionalObservations)
            {
                sensor.AddObservation(_droneState.AnyMotorsDestroyed); // x1
                sensor.AddObservation(_droneState.Landed);             // x1   
            }
            
            sensor.AddObservation(distance);                       // x1
            sensor.AddObservation(heading);                        // x2
            sensor.AddObservation(altitude);                       // x2
            sensor.AddObservation(velocity);                       // x3
            
            if (observationsSettings.useAdditionalObservations)
            {
                sensor.AddObservation(angularVelocity);                // x3 [TOTAL 12] 
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (!enabled) return;

            // All rewards are summed up inside
            AddReward(RewardProvider.CalculateReward());
            
            if (RewardProvider.IsFinalReward && (!(IsHeuristicsOnly || HasModel) || resetEpisodeInHeuristic))
            {
                if (_logsEnabled)
                    Debug.LogFormat("DroneAgent '{0}': ended episode (total reward {1:F2}).", drone.name, GetCumulativeReward());

                EndEpisode(true);
                return;
            }

            if (RewardProvider.IsWaypointReachedAtThisStep() && trainingSettings.termination.endEpisodeAfterEachWaypoint)
            {
                _shouldResetState = trainingSettings.termination.resetStateAfterEachWaypoint;
                EndEpisode(true);
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
            if (!IsHeuristicsOnly || !enabled) return;

            _inputsController.manualInput = true;
            actionsOut.ContinuousActions.Array[0] = _inputsController.throttle;
            actionsOut.ContinuousActions.Array[1] = _inputsController.pitch;
            actionsOut.ContinuousActions.Array[2] = _inputsController.yaw;
            actionsOut.ContinuousActions.Array[3] = _inputsController.roll;
        }
        
        
        private void OnDrawGizmos()
        {
            var shouldDisplayRewards = displayRewards && RewardProvider != null;
            
            if (!shouldDisplayRewards && HasModel)
            {
                VectorDrawer.DrawLabel(drone.transform.position,
                    _modelName,
                    new GizmoOptions { LabelColor = Color.yellow, LabelOutline = true, LabelSize = 0.6f }
                );
            }
            else if (shouldDisplayRewards && HasModel)
            {
                VectorDrawer.DrawLabel(drone.transform.position,
                    float.IsFinite(RewardProvider.TimeLeft)
                        ? $"{_modelName}\nR: {GetCumulativeReward():F0}\nt: {RewardProvider.TimeLeft:F0} s"
                        : $"{_modelName}\nR: {GetCumulativeReward():F0}\n",
                    new GizmoOptions { LabelColor = Color.yellow, LabelOutline = true, LabelSize = 0.6f }
                );
            }
            else if (shouldDisplayRewards)
            {
                VectorDrawer.DrawLabel(drone.transform.position,
                    float.IsFinite(RewardProvider.TimeLeft)
                        ? $"R: {GetCumulativeReward():F0}\nt: {RewardProvider.TimeLeft:F0} s"
                        : $"R: {GetCumulativeReward():F0}\n",
                    new GizmoOptions { LabelColor = Color.white, LabelOutline = true, LabelSize = 0.6f}
                );
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (displayRewards) RewardProvider?.DrawGizmos();
        }
    }
}