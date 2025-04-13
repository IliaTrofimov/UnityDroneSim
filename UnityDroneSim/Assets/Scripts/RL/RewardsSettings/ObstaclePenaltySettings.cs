using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Penalties for moving near any obstacle.
    /// </summary>
    [Serializable]
    public class ObstaclePenaltySettings
    {
        [Range(-50f, 0f)]
        [Tooltip("Penalty for moving towards an obstacle.")]
        public float nearObstaclePenalty = -10;

        [Range(0.01f, 10f)]
        [Tooltip(
            "Minimal distance between obstacles. Space is considered free if no colliders are inside sphere of this radius."
        )]
        public float freeSpaceRadius = 1;

        [Range(0.1f, 200f)]
        [Tooltip("Maximal range for obstacle checks.")]
        public float obstacleDetectionRange = 50;

        [Range(0f, 5f)]
        [Tooltip(
            "Obstacle detection range will be corrected based on distance an agent can travel with current speed for this time."
        )]
        public float reactionTime;
    }
}