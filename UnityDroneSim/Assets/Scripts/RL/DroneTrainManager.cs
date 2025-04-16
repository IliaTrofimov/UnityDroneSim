using System;
using System.Collections.Generic;
using Drone;
using MBaske.Sensors.Grid;
using Navigation;
using RL.RewardsSettings;
using UnityEditor;
using UnityEngine;


namespace RL
{
    /// <summary>
    /// Script that manages global training options and keeps statistics.
    /// </summary>
    public class DroneTrainManager : MonoBehaviour
    {
        private DroneAgent[] _droneAgents;

        [Tooltip("Global training parameters.")]
        public TrainingSettings settings;

        [Tooltip("Selected waypoint path for all drones.")]
        public WaypointPath path;
        
        [Tooltip("Selected spawn point for all drones.")]
        public SpawnPoint spawn;
        
        /// <summary>List all agents inside child scene objects.</summary>
        public IReadOnlyCollection<DroneAgent> DroneAgents => _droneAgents;


        private void OnEnable()
        {
            UpdateDrones();
        }

        private void Start()
        {
            UpdateDrones();
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

            foreach (var agent in _droneAgents)
            {
                if (settings)
                {
                    agent.trainingSettings = settings;
                    agent.InitRewardsProvider();
                }

                if (path) agent.navigator.ResetPath(path);
              
                if (spawn) agent.spawnPoint = spawn;
                
                var stateManager = agent.drone.GetComponent<DroneStateManager>();
                if (stateManager)
                {
                    stateManager.hullBreakSpeed = settings.termination.hullBreakSpeed;
                    stateManager.motorBreakSpeed = settings.termination.motorBreakSpeed;
                    //stateManager.enableEffects = false;
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