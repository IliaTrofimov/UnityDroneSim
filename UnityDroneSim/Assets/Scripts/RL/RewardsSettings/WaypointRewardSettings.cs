using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Rewards and penalties for interacting with waypoints.
    /// </summary>
    [Serializable]
    public class WaypointRewardSettings
    {
        [Range(0f, 1000f)]
        [Tooltip("Reward for reaching last waypoint and finishing episode.")]
        public float finishReward = 500;

        [Range(0f, 100f)]
        [Tooltip("Reward for reaching waypoint.")]
        public float waypointReward = 100;

        [Range(0f, 50f)]
        [Tooltip("Reward for moving towards the next waypoint.")]
        public float movingToWaypointReward = 15;

        [Range(-50f, 0)]
        [Tooltip("Penalty for moving away from the next waypoint.")]
        public float movingAwayFromWaypointPenalty;
    }
}