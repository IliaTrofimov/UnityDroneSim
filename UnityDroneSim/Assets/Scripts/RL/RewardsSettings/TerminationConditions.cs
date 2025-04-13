using System;
using UnityEngine;


namespace RL.RewardsSettings
{
    /// <summary>
    /// Set of conditions for finishing training episode.
    /// </summary>
    [Serializable]
    public class TerminationConditions
    {
        [Range(0, 120)]
        [Tooltip("Max training episode time in seconds. Leave to make episode infinite.")]
        public float trainingEpisodeTime = 30;

        [Range(-500, 0)]
        [Tooltip("Min cumulative reward. Leave 0 to disable.")]
        public float maxPenalty;

        [Range(0f, 20f)]
        [Tooltip("Maximal collision speed (m/s) that drone hull can survive.")]
        public float hullBreakSpeed = 7f;

        [Range(0f, 20f)]
        [Tooltip("Maximal collision speed (m/s) that motor can survive.")]
        public float motorBreakSpeed = 5f;
    }
}