using System;
using Unity.Mathematics;


namespace Drone.Stability
{
    /// <summary>Base clas for all PID controllers. Defines basic variables.</summary>
    [Serializable]
    public abstract class BasePidController : IPid
    {
        protected float p, i, d, lastError = float.NaN;
        protected bool errorWasSet = false;

        
        /// <summary>Proportional factor. Makes output proportionally large/smaller when error is large/small. Can cause output oscillation.</summary>
        public float pFactor;
    
        /// <summary>Integral factor. Can reduce static noise but causee windup.</summary>
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
        
        
        /// <summary>Calculate output value.</summary>
        public abstract float Calc(float actual, float target, float dt);

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