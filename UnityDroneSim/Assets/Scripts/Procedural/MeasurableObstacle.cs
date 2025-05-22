using UnityEngine;

namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MeasurableObstacle : MeasurableObject
    {
        [SerializeField]
        [Header("Procedural generation")]
        [Tooltip("Obstacle can have random X coordinate when using procedural generation.")]
        private bool allowRandomX = true;
        
        [SerializeField]
        [Tooltip("Obstacle can have random Y coordinate when using procedural generation.")]
        private bool allowRandomY = false;

        [SerializeField]
        [Tooltip("Obstacle can be rotated randomly along Y axis when using procedural generation.")]
        private bool allowRandomRotation = false;
        
        public bool AllowRandomX => allowRandomX;
        public bool AllowRandomY => allowRandomY;
        public bool AllowRandomRotation => allowRandomRotation;
    }
}