namespace Drone.Stability
{
    /// <summary>
    /// Abstract Proportional–integral–derivative (PID) controller.
    /// </summary>
    public interface IPid
    {
        /// <summary>Calculate PID output without clamping. Uses error derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>(error - lastError) / dt</i>.
        /// </remarks>
        public float CalcNoClamp(float target, float actual, float dt);

        /// <summary>Calculate PID output with clamping. Uses error derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>(error - lastError) / dt</i>.
        /// </remarks>
        public float Calc(float target, float actual, float dt);

        /// <summary>Calculate PID output without clamping. Uses value derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>-(actual - lastValue) / dt</i>.
        /// </remarks>
        public float CalcDerivNoClamp(float target, float actual, float dt);

        /// <summary>Calculate PID output with clamping. Uses value derivative for D term.</summary>
        /// <remarks>
        /// Error calculated as: <i>target - actual</i>.<br/>
        /// D term calculated as: <i>-(actual - lastValue) / dt</i>.
        /// </remarks>
        public float CalcDeriv(float target, float actual, float dt);
    }
}