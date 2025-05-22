using System.Collections.Generic;
using UnityEngine;


namespace RL
{
    [DisallowMultipleComponent]
    public class DroneTrainingStatistics : MonoBehaviour
    {
        public DroneTrainingManager trainingManager;

        private void Awake() => trainingManager = GetComponent<DroneTrainingManager>();

        /// <summary>List all agents inside child scene objects.</summary>
        public IReadOnlyCollection<DroneAgent> DroneAgents => trainingManager.DroneAgents;

        public void ClearDrones() => trainingManager.ClearDrones();
        public void UpdateDrones() => trainingManager.UpdateDrones();

    }
}