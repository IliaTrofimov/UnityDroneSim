using System;
using System.Collections;
using System.Collections.Generic;
using Drone;
using MBaske.Sensors.Grid;
using Navigation;
using RL.RewardsSettings;
using Unity.MLAgents;
using UnityEngine;
using Utils;


namespace RL
{
    /// <summary>
    /// Script that manages global training options and keeps statistics.
    /// </summary>
    [DisallowMultipleComponent]
    public class DroneTrainingManager : MonoBehaviour
    {
        private DroneAgent[] _droneAgents;
        
        [Header("Global parameters")]
        [Tooltip("Global training parameters.")]
        public TrainingSettings trainSettings;
        
        [Tooltip("Global observations parameters.")]
        public ObservationSettings observationsSettings;
        
        [Header("Environment")]
        [Tooltip("Selected waypoint path for all drones.")]
        public WaypointPath path;
        
        [Tooltip("Selected spawn point for all drones.")]
        public SpawnPoint spawn;
        
        [Tooltip("Time in seconds between environment reset.")]
        [Range(0f, 1800f)]
        public float resetTime = 300;
        
        [Tooltip("Use real or scaled time to reset environment.")]
        public bool resetInRealTime = false;
        
        [Header("Visualization")]
        [Tooltip("Enable/disable trails for all drones.")]
        public bool displayTrails = true;
        
        [Tooltip("This object will be used to create trails. Must contain TrailRenderer component inside.")]
        public GameObject trailPrefab;
        
        
        /// <summary>List all agents inside child scene objects.</summary>
        public IReadOnlyCollection<DroneAgent> DroneAgents => _droneAgents;
        
        /// <summary>All agents must pause their training when this value is true.</summary>
        public bool EnvironmentResetMonitor { get; private set; }


        private void OnEnable()
        {
            UpdateDrones();
        }

        protected virtual void Start()
        {
            UpdateDrones();
            if (resetTime > 0)
            {
                Debug.LogFormat("DroneTrainManager '{0}': is using environment auto reset each {1} ({2} time)", 
                    name, MathExtensions.GetTimeString(resetTime), resetInRealTime ? "real" : "scaled");
                StartCoroutine(ResetEnvironmentLoop());
            }
        }

        private void OnValidate()
        {
            if (!trailPrefab)
            {
                trailPrefab = null;
                return;
            }
         
            if (_droneAgents == null) return;
            
            foreach (var agent in _droneAgents)
            {
                agent.displayTrail = displayTrails;
                agent.trailPrefab = trailPrefab;
            }
        }

        private IEnumerator ResetEnvironmentLoop()
        {
            if (Academy.IsInitialized)
                Academy.Instance.StatsRecorder.Add("Environment/Resets", 0, StatAggregationMethod.MostRecent);
            
            for (var envEpoch = 1; ; envEpoch++)
            {
                EnvironmentResetMonitor = false;
                yield return resetInRealTime 
                    ? new WaitForSecondsRealtime(resetTime)
                    : new WaitForSeconds(resetTime);

                Debug.LogFormat("DroneTrainManager '{0}': environment reset #{1}, next reset in {2} ({3})", 
                    name, envEpoch, MathExtensions.GetTimeString(resetTime), resetInRealTime ? "real" : "scaled");
                EnvironmentResetMonitor = true;
                
                yield return ResetEnvironment();

                if (Academy.IsInitialized)
                    Academy.Instance.StatsRecorder.Add("Environment/Resets", 1, StatAggregationMethod.MostRecent);
                
                foreach (var agent in _droneAgents)
                    agent.EndEpisode(false);
            }
        }

        protected virtual IEnumerator ResetEnvironment()
        {
            yield break;
        }
        
        [ContextMenu("Update drone agents")]
        public void UpdateDrones()
        {
            _droneAgents = GetComponentsInChildren<DroneAgent>();
            if (_droneAgents.Length == 0)
            {
                Debug.LogWarningFormat("TrainManager '{0}': cannot find any DroneAgent inside children objects.", name);
                return;
            }

            if (!trainSettings)
            {
                Debug.LogWarningFormat("TrainManager '{0}': missing TrainingSettings object. Agents must assign their own settings.", name);
            }

            foreach (var agent in _droneAgents)
            {
                agent.observationsSettings = observationsSettings;

                if (trainSettings)
                {
                    agent.trainingSettings = trainSettings;
                    agent.InitRewardsProvider();
                }
                
                if (trailPrefab && displayTrails)
                {
                    agent.displayTrail = displayTrails;
                    agent.trailPrefab = trailPrefab;
                }
                if (!displayTrails)
                {
                    agent.displayTrail = false;
                }

                if (path)
                {
                    agent.navigator.ResetPath(path);
                }
                if (spawn)
                {
                    agent.spawnPoint = spawn;
                }
                
                var stateManager = agent.drone.GetComponent<DroneStateManager>();
                if (stateManager)
                {
                    stateManager.hullBreakSpeed = trainSettings.termination.hullBreakSpeed;
                    stateManager.motorBreakSpeed = trainSettings.termination.motorBreakSpeed;
                    stateManager.enableBrokenMotorsPhysics = false;
                }
                else
                {
                    Debug.LogErrorFormat(
                        "TrainManager '{0}': cannot find required DroneState component for DroneAgent '{1}.{2}'.",
                        name,
                        agent.drone.name,
                        agent.name
                    );
                }
                
                var gridSensor = agent.drone.GetComponentInChildren<GridSensorComponent3D>();
                if (gridSensor)
                    gridSensor.gameObject.SetActive(true);
            }
        }

        [ContextMenu("Clear drone agents")]
        public void ClearDrones()
        {
            _droneAgents = Array.Empty<DroneAgent>();
        }
    }
}