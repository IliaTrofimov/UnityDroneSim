using System;
using UnityEngine;


namespace RL
{
    [Serializable]
    public class ObservationSettings
    {
        [Tooltip("Use local space coordinates for some observations.")]
        public bool useLocalCoordinates = true;

        [Tooltip("Normalize some scalar observations.")]
        public bool useNormalization = true;
        
        [Tooltip("Max linear velocity absolute value before normalization.")]
        [Range(0f, 50f)]
        public float maxVelocityNormalization = 20f;

        [Tooltip("Max angular velocity absolute value before normalization.")]
        [Range(0f, 10f)]
        public float maxAngularVelocityNormalization = 6;

        [Tooltip("Max drone altitude value before normalization.")]
        [Range(0f, 100f)]
        public float maxAltitudeNormalization = 20f;

        [Tooltip("Max distance to the next waypoint before normalization.")]
        [Range(0f, 100f)]
        public float maxWaypointDistanceNormalization = 30f;
    }
}