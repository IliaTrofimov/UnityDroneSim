using System.Collections.Generic;
using Drone.Motors;
using UnityEngine;


namespace Drone
{
    // TODO: move important parts of drone as interface here
    
    /// <summary>Abstract drone flight computer.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneInputsController))]
    public abstract class DroneComputerBase : MonoBehaviour
    {
        /// <summary>Enumerate all drone motors. </summary>
        public abstract IEnumerable<DroneMotor> GetAllMotors();
    }
}