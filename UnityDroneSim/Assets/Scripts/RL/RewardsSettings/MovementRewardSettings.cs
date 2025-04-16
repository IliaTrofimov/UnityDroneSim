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
        [Range(0f, 10f)]
        [Tooltip("Reward for moving somehow.")]
        public float movementReward = 0.05f;

        [Range(-10f, 0f)]
        [Tooltip("Penalty for staying on the ground for too long.")]
        public float noMovementPenalty = -0.01f;

        [Range(0f, 1f)]
        [Tooltip("Maximal speed of the agent considered as 'idle'.")]
        public float idleSpeed = 0.05f;
    }
}