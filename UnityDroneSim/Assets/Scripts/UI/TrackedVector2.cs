using Unity.Mathematics;
using UnityEngine;


namespace UI
{
    /// <summary>
    /// Vector2 Property that can notify that its value has changed.
    /// </summary>
    public sealed class TrackedVector2 : TrackedObject<Vector2>
    {
        /// <summary>Precision for comparing new and old values.</summary>
        public float Epsilon { get; set; }

        public TrackedVector2(float epsilon) : this(Vector3.zero, epsilon) { }

        public TrackedVector2(Vector3 initialValue, float epsilon) : base(initialValue) { Epsilon = epsilon; }

        protected override bool ValuesEqual(Vector2 oldValue, Vector2 newValue) =>
            math.cmax(math.abs(oldValue - newValue)) < Epsilon;
    }
}