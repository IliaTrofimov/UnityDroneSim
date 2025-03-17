using System;
using Unity.Mathematics;

namespace Drone.Stability
{
    /// <summary>Proportional–integral–derivative (PID) controller with output and integral component clamping.</summary>
    /// <remarks>
    /// Calculates derivative component using actual value derivative: <i>D = -(actual - last) / dt</i>.
    /// </remarks>
    [Serializable]
    public sealed class ValueDerivativePidController : BasePidController
    {
        private float integral, lastValue; 
        
        public ValueDerivativePidController() : this(new PidParameters(1, 0.5f, 0.1f)) {}
    
        public ValueDerivativePidController(PidParameters parameters) => this.parameters = parameters;
        
        
        public override float Calc(float target, float actual, float dt) 
        {
            var err = target - actual;
            integral += err * dt;
            var deriv = 0f;
            
            if (errorWasSet)
            {
                deriv = (lastValue - actual) / dt; // simplify for -(actual - lastError) / dt
            }
            else // check if last error was set (not first call) to prevent initial derivative kick
            {
                errorWasSet = true;
            }

            var i = math.clamp(integral * parameters.iFactor, parameters.minIntegral, parameters.maxIntegral);
            lastValue = actual;
            
            return math.clamp(err*parameters.pFactor + i*parameters.iFactor + deriv*parameters.dFactor, 
                parameters.minOutput,
                parameters.maxOutput);
        }
        
        public override void Reset() 
        {
            integral = lastValue = 0f;
            errorWasSet = false;
        }
    }
}