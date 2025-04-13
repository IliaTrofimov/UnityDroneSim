using Unity.Mathematics;


namespace UI
{
    /// <summary>
    /// Float property that can notify that its value has changed.
    /// </summary>
    public sealed class TrackedFloat : TrackedObject<float>
    {
        /// <summary>Precision for comparing new and old values.</summary>
        public float Epsilon { get; set; }


        public TrackedFloat(float epsilon = 0.0001f) : this(0, epsilon) { }

        public TrackedFloat(float initialValue, float epsilon) : base(initialValue) { Epsilon = epsilon; }

        protected override bool ValuesEqual(float oldValue, float newValue) => math.abs(oldValue - newValue) < Epsilon;
    }
}