using InspectorTools;
using Unity.Mathematics;
using UnityEngine;

namespace Drone.Stability
{
    /// <summary>Version of PID controller that can display its internal values in time.</summary>
    /// <remarks>Use only for debugging purposes. Have extra memory and performance cost.</remarks>
    [System.Serializable]
    public class DebugPidController : IPid
    {
        private const int STATS_BUFFER = 20; // set 10-30
        private bool errorWasSet;
        
        [StatsCurve(STATS_BUFFER), SerializeField] 
        protected float p;
        
        [StatsCurve(STATS_BUFFER), SerializeField] 
        private float i;
        
        [StatsCurve(STATS_BUFFER), SerializeField] 
        private float d;
        
        [StatsCurve(STATS_BUFFER), SerializeField] 
        private float lastError;
        
        [StatsCurve(STATS_BUFFER), SerializeField] 
        private float lastValue;

        [StatsCurve(STATS_BUFFER), SerializeField] 
        private float output;
        
        
        /// <summary>Proportional factor. Makes output proportionally large/smaller when error is large/small. Can cause output oscillation.</summary>
        public float pFactor;
    
        /// <summary>Integral factor. Can reduce static noise but cause windup.</summary>
        public float iFactor;
    
        /// <summary>Derivative factor. Makes output proportionally large/smaller when change in error is large/small. Can smooth oscillations.</summary>
        public float dFactor;
        
        /// <summary>Minimal output value.</summary>
        public float minOutput;

        /// <summary>Maximum output value.</summary>
        public float maxOutput;
        
        /// <summary>Maximum absolute value integral component can have.</summary>
        /// <remarks>This value can prevent integral windup.</remarks>
        public float integralRange;

        
        public bool useValueDerivative;
        public bool clampIntegralComponent = true;
        public bool clampOutput = true;
        
        public DebugPidController() : this(1f, 0f, 0.2f) {}
        
        public DebugPidController(float p, float i, float d, float min = -1f, float max = 1f, float intRange = 1f)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
            minOutput = min;
            maxOutput = max;
            integralRange = intRange;
        }
        
        
        public float Calc(float target, float actual, float dt) 
        {
            p = target - actual;
            
            i = clampIntegralComponent
                ? math.clamp(i + p * dt, -integralRange, integralRange)
                : i + p * dt;

            if (!errorWasSet)   // do not calc derivative for the 1st time to avoid kick
            {
                d = 0f;
                errorWasSet = false;
            }
            else if (useValueDerivative)
            {
                d = (lastValue - actual) / dt; 
            }
            else
            {
                d = (p - lastError) / dt;
            }
            
            output = clampOutput
                ? math.clamp(p * pFactor + i * iFactor + d * dFactor, minOutput, maxOutput)
                : p * pFactor + i * iFactor + d * dFactor;
            
            lastError = p;
            lastValue = actual;
            
            return output;
        } 
        
        /// <summary>Reset internal values (error integral and last value).</summary>
        public virtual void Reset()
        {
            i = lastError = 0f;
            errorWasSet = false;
        }

        /// <summary>Set output and integral clamping values.</summary>
        public virtual void SetClamping(float minOutput, float maxOutput, float? integralRange = null)
        {
            this.minOutput = minOutput;
            this.maxOutput = maxOutput;
            this.integralRange = integralRange ?? math.abs(minOutput - maxOutput);
        }
        
        public void SetClamping(float maxRange) => SetClamping(-maxRange, maxRange, maxRange);
    }
}