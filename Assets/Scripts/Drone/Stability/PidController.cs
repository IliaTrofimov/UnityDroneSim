using System.Runtime.CompilerServices;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Proportional–integral–derivative (PID) controller.</summary>
    /// <remarks>See https://vazgriz.com/621/pid-controllers/</remarks>
    [System.Serializable]
    public class PidController : IPid
    {
        /// <summary>Proportional factor. Makes output proportionally large/smaller when error is large/small. Can cause output oscillation.</summary>
        public float pFactor;
    
        /// <summary>Integral factor. Can reduce static noise and cause windup.</summary>
        public float iFactor;
    
        /// <summary>Derivative factor. Makes output proportionally large/smaller when change in error is large/small. Can smooth oscillations.</summary>
        public float dFactor;
    
        /// <summary>Minimal output value.</summary>
        public float minOutput = -1f;
    
        /// <summary>Maximum output value.</summary>
        public float maxOutput = 1f;

        private float p, i, d, lastError, lastValue;
        
        public PidController() : this(1f, 0.5f, 0.01f) {}
    
        public PidController(float p, float i, float d)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
        }
        
        public float CalcNoClamp(float target, float actual, float dt)
        {
            p = target - actual;
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = (p - lastError) / dt;
            lastError = p;
            
            return p * pFactor + i * iFactor + d * dFactor;
        }
        
        public float Calc(float target, float actual, float dt) 
        {
            return math.clamp(CalcNoClamp(target, actual, dt), minOutput, maxOutput);
        } 
        
        public float CalcDerivNoClamp(float target, float actual, float dt)
        {
            p = target - actual;
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = -(actual - lastValue) / dt;
            lastValue = actual;

            return p * pFactor + i * iFactor + d * dFactor;
        }
        
        public float CalcDeriv(float target, float actual, float dt) 
        {
            return math.clamp(CalcDerivNoClamp(target, actual, dt), minOutput, maxOutput);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalcIntegral(float integral, float error, float dt, float minOutput, float maxOutput)
        {
            var range = 2 * math.abs(maxOutput - minOutput);  // clamp integral so it doesn't overwhelm other components
            return math.clamp(integral + error*dt, -range, range); 
        } 
    }
}