using Unity.Mathematics;
using UnityEngine;


namespace UI
{
    /// <summary>
    /// Vector3 Property that can notify that its value has changed.
    /// </summary>
    public sealed class TrackedVector3 : TrackedObject<Vector3>
    {
        /// <summary>Precision for comparing new and old values.</summary>
        public float Epsilon { get; set; }

        public TrackedVector3(float epsilon) : this(Vector3.zero, epsilon) { }

        public TrackedVector3(Vector3 initialValue, float epsilon) : base(initialValue) { Epsilon = epsilon; }

        protected override bool ValuesEqual(Vector3 oldValue, Vector3 newValue) =>
            math.cmax(math.abs(oldValue - newValue)) < Epsilon;
    }
}