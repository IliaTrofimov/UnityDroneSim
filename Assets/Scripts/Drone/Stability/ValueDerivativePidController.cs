using System;
using Unity.Mathematics;
using UnityEngine;


namespace Drone.Stability
{
    /// <summary>
    /// Proportional–integral–derivative (PID) controller with output and integral component clamping.
    /// Useful when target values can change fast.
    /// </summary>
    /// <remarks>
    /// Calculates D component using actual value derivative: <i>D = -(actual - last) / dt</i>.
    /// </remarks>
    [Serializable]
    public sealed class ValueDerivativePidController : BasePidController
    {
        private float integral, lastValue; 
        
        public ValueDerivativePidController() : this(new PidParameters(1, 0.5f, 0.1f)) {}
    
        public ValueDerivativePidController(PidParameters parameters) => this.parameters = parameters;
        
        
        public override float Calc(float target, float actual, float dt) 
        {
            var error = target - actual;
            
            integral += error * dt;
            var i = math.clamp(integral * parameters.iFactor, 
                parameters.minIntegral,
                parameters.maxIntegral);

            var derivative = 0f;
            if (errorWasSet) derivative = (lastValue - actual) / dt;
            else errorWasSet = true;

            lastValue = actual;
            
            return math.clamp(error * parameters.pFactor + i + derivative * parameters.dFactor, 
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