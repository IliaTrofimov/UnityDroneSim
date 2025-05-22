using System;
using UnityEngine;
using UnityEngine.Serialization;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Simple rewards and penalties for just moving or idling.
    /// </summary>
    [Serializable]
    public class MovementRewardSettings
    {
        [Header("Linear movement")]
        [Range(0f, 1f)]
        [Tooltip("Maximal speed of the agent considered as 'idle'.")]
        public float idleLinearSpeed = 0.05f;
        
        [Range(0f, 10f)]
        [Tooltip("Reward for moving somehow.")]
        public float movementReward = 0.05f;

        [Range(-10f, 0f)]
        [Tooltip("Penalty for staying on the ground for too long.")]
        public float noMovementPenalty = -0.01f;
        
        
        [Header("Rotational movement")]
        [Range(0f, 10f)]
        [Tooltip("Maximal allowed angular speed before applying penalty.")]
        public float maxAngularSpeed = 2.8f;
        
        [FormerlySerializedAs("angularPenalty")]
        [Range(-10f, 0f)]
        [Tooltip("Penalty for rotating too fast.")]
        public float angularSpeedPenalty = -0.05f;
    }
}