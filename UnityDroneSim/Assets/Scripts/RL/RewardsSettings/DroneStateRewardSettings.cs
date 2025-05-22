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
        [Header("Destruction state")]
        [Range(-100, 0)]
        [Tooltip("Penalty for destroying drone.")]
        public float destructionPenalty;

        [Tooltip("Should finish episode on destroying drone.")]
        public bool finishOnDestruction = true;
        
        
        [Header("Landing state")]
        [Range(0, 10)]
        [Tooltip("Reward for safely landing drone.")]
        public float landingReward;

        [Tooltip("Should finish episode on landing drone.")]
        public bool finishOnLanding;
    }
}