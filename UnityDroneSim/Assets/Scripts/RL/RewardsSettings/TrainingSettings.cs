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


        [Tooltip("Multiply each reward with Time.fixedDeltaTime value.")]
        public bool useTimeScaledRewards;

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
    }
}