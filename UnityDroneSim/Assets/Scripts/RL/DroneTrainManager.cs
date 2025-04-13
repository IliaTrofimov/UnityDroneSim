using System;
using System.Collections.Generic;
using Drone;
using RL.RewardsSettings;
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

        /// <summary>List all agents inside child scene objects.</summary>
        public IReadOnlyCollection<DroneAgent> DroneAgents => _droneAgents;


        private void OnEnable()
        {
            Debug.Log("DroneTrainManager: OnEnable");
            UpdateDrones();
        }

        private void Start()
        {
            Debug.Log("DroneTrainManager: Start");
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
                agent.trainingSettings = settings;
                agent.InitRewardsProvider();

                var stateManager = agent.drone.GetComponent<DroneStateManager>();
                if (stateManager)
                {
                    stateManager.hullBreakSpeed = settings.termination.hullBreakSpeed;
                    stateManager.motorBreakSpeed = settings.termination.motorBreakSpeed;
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
            }
        }

        [ContextMenu("Clear drone agents")]
        public void ClearDrones()
        {
            _droneAgents = Array.Empty<DroneAgent>();
        }
    }
}