using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    [Serializable]
    public class HeightRewardSettings
    {
        [Tooltip("Use Physics.Raycast to get precise height.")]
        public bool useRaycastHeight;
        
        [Range(0, 100f)] 
        [Tooltip("Minimal height to fly.")]
        public float minHeight = 0f;
        
        [Range(0, 100f)] 
        [Tooltip("Max height to fly.")]
        public float maxHeight = 20f;
        
        [Range(-10f, 0)] 
        [Tooltip("Penalty for flying below minHeight or above maxHeight.")]
        public float outOfHeightRangePenalty = -0.05f;
        
        [Range(0, 10f)] 
        [Tooltip("Reward for flying above minHeight and below maxHeight.")]
        public float heightReward = 0.05f;
    }
}