using System;
using InspectorTools;
using Unity.Mathematics;
using UnityEngine;


namespace Drone.Stability
{
    /// <summary>Version of PID controller that can display its internal values in time.</summary>
    /// <remarks>Use only for debugging purposes. This implementation has extra memory and performance cost.</remarks>
    [Serializable]
    public class DebugPidController : BasePidController
    {
        /// <summary>
        /// Integral sum clamping mode: no clamping, clamp before after applying I factor.
        /// </summary>
        public enum IntegralClamping { None, PreClamping, PostClamping }


        private const int   STATS_BUFFER = 50; // avoid large values
        private float _lastValue, _lastError;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float targetValue;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float error;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float integral;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float p;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float i;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float d;

        [StatsCurve(STATS_BUFFER), SerializeField]
        public float output;

        public bool             useValueDerivative, clampOutput, relativeForm;
        public IntegralClamping integralClamping = IntegralClamping.PostClamping;

        public float PFactor     => parameters.pFactor;
        public float Factor     => parameters.iFactor;
        public float DFactor     => parameters.dFactor;
        public float MaxIntegral => parameters.maxIntegral;
        public float MinIntegral => parameters.minIntegral;
        public float MaxOutput   => parameters.maxOutput;
        public float MinOutput   => parameters.minOutput;


        public DebugPidController() : this(new PidParameters(1f, 0.5f, 0.1f)) { }

        public DebugPidController(PidParameters parameters) { this.parameters = parameters; }


        public override float Calc(float target, float actual, float dt)
        {
            output = CalcP(target, actual) + CalcI(dt) + CalcD(actual, dt);
            if (clampOutput)
                output = math.clamp(output, parameters.minOutput, parameters.maxOutput);

            return output;
        }

        public override void Reset()
        {
            integral = _lastError = _lastValue = 0f;
            ErrorWasSet = false;
        }


        private float CalcP(float target, float actual)
        {
            targetValue = target;
            error = target - actual;
            p = error * parameters.pFactor;
            return p;
        }

        private float CalcI(float dt)
        {
            integral += error * dt;

            switch (integralClamping)
            {
            case IntegralClamping.PreClamping:
                i = parameters.iFactor * math.clamp(integral, parameters.minIntegral, parameters.maxIntegral);
                break;
            case IntegralClamping.PostClamping:
                i = math.clamp(parameters.iFactor * integral, parameters.minIntegral, parameters.maxIntegral);
                break;
            default:
                i = parameters.iFactor * integral;
                break;
            }

            if (relativeForm)
                i *= parameters.pFactor;

            return i;
        }

        private float CalcD(float actual, float dt)
        {
            if (!ErrorWasSet) // do not calc derivative for the 1st time to avoid kick
            {
                d = 0f;
                ErrorWasSet = true;
            }
            else if (useValueDerivative)
            {
                d = parameters.dFactor * (_lastValue - actual) / dt; // simplify for -(actual - lastError) / dt
            }
            else
            {
                d = parameters.dFactor * (error - _lastError) / dt;
            }

            _lastError = error;
            _lastValue = actual;

            if (relativeForm)
                d *= parameters.pFactor;

            return d;
        }
    }
}