using System.Runtime.CompilerServices;
using InspectorTools;
using Unity.Mathematics;
using UnityEngine;
using Utils;


namespace Drone.Stability
{
    /// <summary>Version of PID controller that can display its internal values in time.</summary>
    /// <remarks>Use only for debugging purposes. Have extra memory and performance cost.</remarks>
    [System.Serializable]
    public class DebugPidController : IPid
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
       
        #if UNITY_EDITOR
        [RealtimeCurve] public AnimationCurve actualValueCurve;
        [RealtimeCurve] public AnimationCurve targetValueCurve;
        [RealtimeCurve] public AnimationCurve pValueCurve;
        [RealtimeCurve] public AnimationCurve iValueCurve;
        [RealtimeCurve] public AnimationCurve dValueCurve;
        [RealtimeCurve] public AnimationCurve outputCurve;
        #endif
        
        
        public DebugPidController() : this(1f, 0f, 0.2f) {}
        
        public DebugPidController(float p, float i, float d)
        {
            pFactor = p;
            iFactor = i;
            dFactor = d;
        }
        
        
        public float CalcNoClamp(float target, float actual, float dt)
        {
            p = target - actual; 
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = (p - lastError) / dt;
            lastError = p;
            
            var output = p * pFactor + i * iFactor + d * dFactor;
            UpdateCurves(p, i, d, actual, target, output);
            return output;
        }
        
        public float Calc(float target, float actual, float dt) 
        {
            p = target - actual; 
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = (p - lastError) / dt;
            lastError = p;
            
            var output = p * pFactor + i * iFactor + d * dFactor;
            math.clamp(output, minOutput, maxOutput);
            UpdateCurves(p, i, d, actual, target, output);
            return output;
        } 
        
        public float CalcDerivNoClamp(float target, float actual, float dt)
        {
            p = target - actual; 
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = -(actual - lastValue) / dt;
            lastValue = actual;
            
            var output = p * pFactor + i * iFactor + d * dFactor;
            UpdateCurves(p, i, d, actual, target, output);
            return output;
        }
        
        public float CalcDeriv(float target, float actual, float dt) 
        {
            p = target - actual; 
            i = CalcIntegral(i, p, dt, minOutput, maxOutput);
            d = -(actual - lastValue) / dt;
            lastValue = actual;
            
            var output = p * pFactor + i * iFactor + d * dFactor;
            math.clamp(output, minOutput, maxOutput);
            UpdateCurves(p, i, d, actual, target, output);
            return output;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalcIntegral(float integral, float error, float dt, float minOutput, float maxOutput)
        {
            var range = 2 * math.abs(maxOutput - minOutput);  // clamp integral so it doesn't overwhelm other components
            return math.clamp(integral + error*dt, -range, range); 
        } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCurves(float p, float i, float d, float actual, float target, float output)
        {
            #if UNITY_EDITOR
            actualValueCurve.AddKeyWrapped(Time.frameCount, actual);
            targetValueCurve.AddKeyWrapped(Time.frameCount, target);
            outputCurve.AddKeyWrapped(Time.frameCount, output);
            pValueCurve.AddKeyWrapped(Time.frameCount, p);
            iValueCurve.AddKeyWrapped(Time.frameCount, i);
            dValueCurve.AddKeyWrapped(Time.frameCount, d);
            #endif
        }
    }
}