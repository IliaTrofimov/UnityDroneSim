using System;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Proportional–integral–derivative (PID) controller with output and integral component clamping.</summary>
    [Serializable]
    public sealed class PidController : BasePidController
    {
        // inherits pFactor, iFactor, dFactor, p, i, d, lastError
        
        public PidController() : this(1f, 0.5f, 0.01f) {}
    
        public PidController(float p, float i, float d, float min = -1f, float max = 1f, float intRange = 1f)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
            minOutput = min;
            maxOutput = max;
            integralRange = intRange;
        }
        
        
        public override float Calc(float target, float actual, float dt) 
        {
            p = target - actual;
            i = math.clamp(i + p * dt, -integralRange, integralRange);
            if (errorWasSet)
            {
                d = (p - lastError) / dt;
            }
            else // check if last error was set (not first call) to prevent initial derivative kick
            {
                d = 0f;
                errorWasSet = true;
            }
            lastError = p;
            return math.clamp(p * pFactor + i * iFactor + d * dFactor, minOutput, maxOutput);
        }
    }
}