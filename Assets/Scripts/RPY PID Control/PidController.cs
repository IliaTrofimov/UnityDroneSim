using InspectorTools;
using Unity.Mathematics;
using UnityEngine;


namespace PID
{
    /// <summary>Proportional–integral–derivative (PID) controller.</summary>
    /// <remarks>See https://vazgriz.com/621/pid-controllers/</remarks>
    [System.Serializable]
    public class PidController
    {
        /// <summary>Proportional factor. Makes output proportionally large/smaller when error is large/small. Can cause output oscillation.</summary>
        [Min(0)] public float pFactor;
    
        /// <summary>Integral factor. Can reduce static noise and cause windup.</summary>
        [Min(0)] public float iFactor;
    
        /// <summary>Derivative factor. Makes output proportionally large/smaller when change in error is large/small. Can smooth oscillations.</summary>
        [Min(0)] public float dFactor;
    
        /// <summary>Minimal output value.</summary>
        public float minOutput = -1f;
    
        /// <summary>Maximum output value.</summary>
        public float maxOutput = 1f;

        private float p, i, d, lastError, lastValue;
        [SerializeField, ReadOnlyField] private float actual;
        [SerializeField, ReadOnlyField] private float target;
        [SerializeField, ReadOnlyField] private float output;

        public PidController() : this(1f, 0f, 0.2f) {}
    
        public PidController(float p, float i, float d)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
        }
        
        
        /// <summary>Calculate PID output without clamping. Uses error derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>(error - lastError) / dt</i>.
        /// </remarks>
        public float CalcNoClamp(float target, float actual, float dt)
        {
            p = target - actual; // error
            i += p * dt;
            d = (p - lastError) / dt;
            lastError = p;
            this.target = target;
            this.actual = actual;
            return output = p * pFactor + i * iFactor + d * dFactor;
        }
            
        /// <summary>Calculate PID output with clamping. Uses error derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>(error - lastError) / dt</i>.
        /// </remarks>
        public float Calc(float target, float actual, float dt) 
        {
            return output = math.clamp(CalcNoClamp(target, actual, dt), minOutput, maxOutput);
        } 
    
        /// <summary>Calculate PID output without clamping. Uses value derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>-(actual - lastValue) / dt</i>.
        /// </remarks>
        public float CalcNoClampDeriv(float target, float actual, float dt)
        {
            p = target - actual; // error
            i += p * dt;
            d = -(actual - lastValue) / dt;
            lastValue = actual;
            this.target = target;
            this.actual = actual;
            return output = p * pFactor + i * iFactor + d * dFactor;
        }
        
        /// <summary>Calculate PID output with clamping. Uses value derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>-(actual - lastValue) / dt</i>.
        /// </remarks>
        public float CalcDeriv(float target, float actual, float dt) 
        {
            return output = math.clamp(CalcNoClamp(target, actual, dt), minOutput, maxOutput);
        }
    }
}