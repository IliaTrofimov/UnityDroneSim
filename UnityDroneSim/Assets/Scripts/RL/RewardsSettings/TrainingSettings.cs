using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Global training parameters.
    /// </summary>
    [CreateAssetMenu(menuName = "ML-Agents/Training Settings", fileName = nameof(TrainingSettings))]
    public class TrainingSettings : ScriptableObject
    {
        [Header("Global parameters")]
        [Tooltip("Set of conditions for finishing training episode.")]
        public TerminationConditions termination;

        
        [Header("Drone state rewards")]
        [Tooltip("Enables/disables drone state rewards.")]
        public bool stateRewardEnabled;

        [Tooltip("Rewards and penalties for changing state of the drone.")]
        public DroneStateRewardSettings stateRewardSettings;

        
        [Header("Simple movement rewards")]
        [Tooltip("Enables/disables movement reward.")]
        public bool movementRewardEnabled;

        [Tooltip("Rewards and penalties for simple movement.")]
        public MovementRewardSettings movementRewardSettings;

        
        [Header("Waypoint rewards")]
        [Tooltip("Enables/disables waypoint rewards.")]
        public bool waypointRewardEnabled;

        [Tooltip("Rewards and penalties for interacting with waypoints.")]
        public WaypointRewardSettings waypointRewardSettings;

        
        [Header("Obstacle penalties")]
        [Tooltip("Enables/disables obstacle penalties.")]
        public bool obstaclePenaltyEnabled;

        [Tooltip("Penalties for moving near any obstacle.")]
        public ObstaclePenaltySettings obstaclePenaltySettings;

        
        [Header("Height rewards")]
        [Tooltip("Enables/disables fly height rewards.")]
        public bool heightRewardEnabled;

        [Tooltip("Rewards and penalties based on flight height..")]
        public HeightRewardSettings heightRewardSettings;
        
        private void Reset()
        {
            Debug.Log("RewardsSettings: Reset");
            InitDefault();
        }

        public void InitDefault()
        {
            stateRewardEnabled = false;
            movementRewardEnabled = false;
            obstaclePenaltyEnabled = false;
            waypointRewardEnabled = false;
            heightRewardEnabled = false;
            
            termination = new TerminationConditions();
            stateRewardSettings = new DroneStateRewardSettings();
            movementRewardSettings = new MovementRewardSettings();
            obstaclePenaltySettings = new ObstaclePenaltySettings();
            waypointRewardSettings = new WaypointRewardSettings();
            heightRewardSettings = new HeightRewardSettings();
        }
    }
}