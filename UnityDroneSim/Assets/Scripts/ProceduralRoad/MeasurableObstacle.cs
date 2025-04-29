using UnityEngine;
using UnityEngine.Serialization;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MeasurableObstacle : MeasurableObject
    {
        [SerializeField]
        [Tooltip("Obstacle can have random X coordinate when using procedural generation.")]
        private bool allowRandomX = true;
        
        [SerializeField]
        [Tooltip("Obstacle can have random Y coordinate when using procedural generation.")]
        private bool allowRandomY = false;

        public bool AllowRandomX => allowRandomX;
        public bool AllowRandomY => allowRandomY;

    }
}