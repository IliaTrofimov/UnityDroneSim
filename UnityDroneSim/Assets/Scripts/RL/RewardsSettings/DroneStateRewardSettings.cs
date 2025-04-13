using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Rewards and penalties for drone destruction or landing.
    /// </summary>
    [Serializable]
    public class DroneStateRewardSettings
    {
        [Range(-1000, 0)]
        [Tooltip("Penalty for destroying drone.")]
        public float destructionPenalty;

        [Range(0, 100)]
        [Tooltip("Reward for safely landing drone.")]
        public float landingReward;
    }
}