using Unity.Mathematics;
using UnityEngine;


namespace UI
{
    public sealed class TrackedVector3 : TrackedObject<Vector3>
    {
        public float Epsilon { get; set; }
        
        public TrackedVector3(float epsilon) : this(Vector3.zero, epsilon)
        {
        }
        
        public TrackedVector3(Vector3 initialValue, float epsilon) : base(initialValue)
        {
            Epsilon = epsilon;
        }
        
        protected override bool ValuesEqual(Vector3 oldValue, Vector3 newValue)
        {
            return math.cmax(math.abs(oldValue - newValue)) < Epsilon;
        }
    }
}