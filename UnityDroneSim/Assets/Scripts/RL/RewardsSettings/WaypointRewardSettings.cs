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
        [Header("Waypoint reached")]
        [Range(0f, 100f)]
        [Tooltip("Reward for reaching last waypoint and finishing episode.")]
        public float finishReward = 50;

        [Range(0f, 50f)]
        [Tooltip("Reward for reaching waypoint.")]
        public float waypointReward = 10;

        
        [Header("Waypoint reaching")]
        [Range(0, 5f)]
        [Tooltip("Reward or penalty will be set if absolute approach speed is above this value.")]
        public float minimalApproachSpeed = 0.1f;
        
        [Range(0f, 10f)]
        [Tooltip("Reward for moving towards the next waypoint.")]
        public float movingToWaypointReward = 0.5f;

        [Range(-10f, 0)]
        [Tooltip("Penalty for moving away from the next waypoint.")]
        public float movingAwayFromWaypointPenalty = 0;
        
        
        [Header("Looking at waypoint")]
        [Range(0f, 1f)]
        public float lookAtWaypointAngle = 0.5f;
        
        [Range(-10f, 0f)]
        [Tooltip("Penalty for looking away from the next waypoint.")]
        public float lookAwayPenalty = 0;
        
        [Range(0f, 10f)]
        [Tooltip("Penalty for looking at the next waypoint.")]
        public float lookAtReward = 0;
    }
}