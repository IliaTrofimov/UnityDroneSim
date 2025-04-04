using Unity.Mathematics;


namespace UI
{
    public sealed class TrackedFloat : TrackedObject<float>
    {
        public float Epsilon { get; set; }
        
        
        public TrackedFloat(float epsilon = 0.0001f) : this(0, epsilon)
        {
        }
        
        public TrackedFloat(float initialValue, float epsilon) : base(initialValue)
        {   
            Epsilon = epsilon;
        }
        
        protected override bool ValuesEqual(float oldValue, float newValue)
        {
            return math.abs(oldValue - newValue) < Epsilon;
        }
    }
}