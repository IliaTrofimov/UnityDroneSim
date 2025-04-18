using System;
using UnityEngine;


namespace Drone.Stability
{
    /// <summary>Base class for all PID controllers. Defines basic method.</summary>
    [Serializable]
    public abstract class BasePidController
    {
        protected bool ErrorWasSet;

        /// <summary>Static PID parameters (P, I, D factors and output clamping).</summary>
        [HideInInspector] public PidParameters parameters;

        /// <summary>Calculate output value.</summary>
        public abstract float Calc(float target, float actual, float dt);

        /// <summary>Reset internal values (error integral and last value).</summary>
        public abstract void Reset();

        /// <summary>Update static parameters values.</summary>
        public void ResetParameters(PidParameters pidParameters) { parameters = pidParameters; }

        public override string ToString() => parameters.ToString();
    }
}