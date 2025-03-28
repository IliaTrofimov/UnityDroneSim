using System.Collections.Generic;
using Drone.Propulsion;
using UnityEngine;
using UtilsDebug;


namespace Drone
{
    /// <summary>Abstract drone flight computer.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneInputsController))]
    public abstract class DroneComputerBase : MonoBehaviour
    {
        /// <summary>Enumerate all drone motors. </summary>
        public abstract IEnumerable<DroneMotor> GetAllMotors();
        
    }
}