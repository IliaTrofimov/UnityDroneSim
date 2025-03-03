using Drone.Propulsion;
using Exceptions;
using UnityEngine;


namespace Drone
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneInputsController))]
    public abstract class DroneComputerBase : MonoBehaviour
    {
        public Rigidbody rigidBody;
        protected DroneInputsController inputController;
        
        private void Awake()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(rigidBody, nameof(rigidBody));
            inputController = GetComponent<DroneInputsController>();
        }

        /// <summary>Get array with all drone motors. </summary>
        public abstract DroneMotor[] GetAllMotors();
    }
}