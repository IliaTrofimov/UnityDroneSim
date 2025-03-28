using System;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Proportional–integral–derivative (PID) controller with output and integral component clamping.</summary>
    /// <remarks>
    /// Calculates D component using error derivative: <i>D = (error - lastError) / dt</i>.
    /// </remarks>
    [Serializable]
    public class PidController : BasePidController
    {
        private float integral, lastError; 
        
        public PidController() : this(new PidParameters(1, 0.5f, 0.1f)) {}
    
        public PidController(PidParameters parameters) => this.parameters = parameters;

        
        public override float Calc(float target, float actual, float dt)
        {
            var error = target - actual;
            
            integral += error * dt;
            var i = math.clamp(integral * parameters.iFactor, 
                parameters.minIntegral, 
                parameters.maxIntegral);

            var derivative = 0f;
            if (errorWasSet) derivative = (error - lastError) / dt;
            else errorWasSet = true;
            
            lastError = error;
            
            return math.clamp(error * parameters.pFactor + i + derivative * parameters.dFactor, 
                parameters.minOutput,
                parameters.maxOutput);
        }

        public override void Reset() 
        {
            integral = lastError = 0f;
            errorWasSet = false;
        }
    }
}