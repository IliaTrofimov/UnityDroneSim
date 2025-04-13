using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Simple rewards and penalties for just moving or idling.
    /// </summary>
    [Serializable]
    public class MovementRewardSettings
    {
        [Range(0f, 50f)]
        [Tooltip("Reward for moving somehow.")]
        public float movementReward = 10;

        [Range(-50f, 0f)]
        [Tooltip("Penalty for staying on the ground for too long.")]
        public float noMovementPenalty = -10;

        [Range(0f, 1f)]
        [Tooltip("Maximal speed of the agent considered as 'idle'.")]
        public float idleSpeed = 0.01f;
    }
}