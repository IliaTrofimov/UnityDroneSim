using System;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Proportional–integral–derivative (PID) controller with output and integral component clamping.</summary>
    /// <remarks>
    /// Calculates derivative component using actual value derivative to improve stability when target is changing rapidly.
    /// </remarks>
    [Serializable]
    public sealed class ValueDerivativePidController : BasePidController
    {
        // inherits pFactor, iFactor, dFactor, p, i, d, lastError
       
        public ValueDerivativePidController() : this(1f, 0.5f, 0.01f) {}
    
        public ValueDerivativePidController(float p, float i, float d, float min = -1f, float max = 1f, float intRange = 1f)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
            minOutput = min;
            maxOutput = max;
            integralRange = intRange;
        }

        
        /// <summary>Calculate PID output.</summary>
        /// <remarks>
        /// Derivative component is being calculated using value actual value derivative:<br/>
        /// <i>D = -(actual - last) / dt</i>
        /// </remarks>
        public override float Calc(float target, float actual, float dt) 
        {
            p = target - actual;
            i = math.clamp(i + p*dt, -integralRange, integralRange);
            if (errorWasSet)
            {
                d = (lastError - actual) / dt; // simplify for -(actual - lastError) / dt
            }
            else // check if last error was set (not first call) to prevent initial derivative kick
            {
                d = 0f;
                errorWasSet = true;
            }
            lastError = actual;  // Note: different last error value
            return p * pFactor + i * iFactor + d * dFactor;
        }
    }
}