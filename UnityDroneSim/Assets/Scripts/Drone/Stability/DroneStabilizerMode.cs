using System;


namespace Drone.Stability
{
    /// <summary>Flight stabilization mode.</summary>
    [Flags]
    public enum DroneStabilizerMode
    {
        /// <summary>No stabilization. Free controls.</summary>
        None = 0,

        /// <summary>Keep drone's altitude.</summary>
        StabAltitude = 1,

        /// <summary>Keep drone from overturning. Restricts pitch and roll a little.</summary>
        StabPitchRoll = 2,

        /// <summary>Keep drone from spinning too much.</summary>
        StabYaw = 4
    }
}