using System;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Set of static PID controller parameters that don't change over time.</summary>
    [Serializable]
    public sealed class PidParameters
    {
        /// <summary>Proportional factor. Main value to play with.</summary>
        /// <remarks>Large values create faster reaction but also cause self-oscillations.</remarks>
        public float pFactor;
    
        /// <summary>Integral factor.</summary>
        /// <remarks>Compensates static errors (e.g. gravity) but can cause windup.</remarks>
        public float iFactor;
    
        /// <summary>Derivative factor. Makes output proportionally large/smaller when change in error is large/small.</summary>
        /// <remarks>Can compensate oscillations caused by proportional component.</remarks>
        public float dFactor;
        
        /// <summary>Minimal output value.</summary>
        public float minOutput;

        /// <summary>Maximum output value.</summary>
        public float maxOutput;
        
        /// <summary>Minimal value integral component can have.</summary>
        public float minIntegral;

        /// <summary>Maximum value integral component can have.</summary>
        public float maxIntegral;

        
        public PidParameters(float p, float i, float d, float outputRange = float.PositiveInfinity)
            : this(p, i, d, outputRange, outputRange)
        {
        }
        
        public PidParameters(float p, float i, float d, float outputRange, float integralRange)
            : this(p, i, d, -outputRange, outputRange, -integralRange, integralRange)
        {
        }
        
        public PidParameters(float p, float i, float d, float minOutput, float maxOutput, float minIntegral, float maxIntegral)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
            this.minOutput = math.min(minOutput, maxOutput);
            this.maxOutput = math.max(minOutput, maxOutput);
            this.minIntegral = math.min(minIntegral, maxIntegral);
            this.maxIntegral = math.max(minIntegral, maxIntegral);
        }
        
        
        public void CopyFrom(PidParameters pidParameters)
        {
            pFactor = pidParameters.pFactor;
            iFactor = pidParameters.iFactor;
            dFactor = pidParameters.dFactor;
            minOutput = pidParameters.minOutput;
            maxOutput = pidParameters.maxOutput;
            minIntegral = pidParameters.minIntegral;
            maxIntegral = pidParameters.maxIntegral;
        }
        
        public void Reset(float p, float i, float d, float min, float max, float integralRange)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
            minOutput = min;
            maxOutput = max;
            minIntegral = -integralRange;
            maxIntegral = integralRange;
        }

        public void SetClamping(float range)
        {
            range = math.abs(range);
            minOutput = -range;
            maxOutput = range;
            minIntegral = -range;
            maxIntegral = range;
        }
        
        public void SetClamping(float outputRange, float integralRange)
        {
            outputRange = math.abs(outputRange);
            integralRange = math.abs(integralRange);
            minOutput = -outputRange;
            maxOutput = outputRange;
            minIntegral = -integralRange;
            maxIntegral = integralRange;
        }

        public override string ToString()
        {
            return string.Format("P:{0:F3}, I:{1:F3}, D:{2:F3}, int:[{3:F1}, {4:F1}], out:[{5:F1}, {6:F1}]",
                pFactor, iFactor, dFactor, minIntegral, maxIntegral,
                minOutput, maxOutput);
        }
    }
}