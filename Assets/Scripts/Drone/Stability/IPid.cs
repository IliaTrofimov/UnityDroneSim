namespace Drone.Stability
{
    public interface IPid
    {
        public float Calc(float actual, float target, float dt);
       
        public void Reset();
        
        public void SetClamping(float minOutput, float maxOutput, float? integralRange = null);
        
        public void SetClamping(float maxRange) => SetClamping(-maxRange, maxRange, maxRange);
    }
}