using System;
using Unity.Mathematics;
using UnityEngine;


namespace Drone.Stability
{
    /// <summary>Set of static PID controller parameters that don't change over time.</summary>
    [Serializable]
    public sealed class PidParameters
    {
        [Tooltip("Proportional factor. Main value to play with. Large values create faster reaction but also cause self-oscillations.")]
        public float pFactor;
    
        [Tooltip("Integral factor. Compensates static errors (e.g. gravity) but can cause windup.")]
        public float iFactor;
    
        [Tooltip("Derivative factor. Makes output proportionally large/smaller when change in error is large/small. " +
                 "Can compensate oscillations caused by proportional component.")]
        public float dFactor;
        
        [Tooltip("Minimal output value.")]
        public float minOutput;

        [Tooltip("Maximum output value.")]
        public float maxOutput;
        
        [Tooltip("Minimal value integral component can have.")]
        public float minIntegral;

        [Tooltip("Maximum value integral component can have.")]
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