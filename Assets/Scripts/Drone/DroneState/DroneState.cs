using UnityEngine;


namespace Drone.DroneState
{
    public class DroneState
    {
        public readonly DroneStateType state;
        public readonly Vector3 linearVelocity;
        public readonly Vector3 angularVelocity;
        public readonly Vector3 collisionVelocity;
        public readonly Vector3 rotation;
        public readonly float altitude;

        
        public DroneState(Rigidbody droneRb, float altitude) : this(droneRb, altitude, Vector3.zero) { }

        public DroneState(Rigidbody droneRb, float altitude, Vector3 collisionVelocity)
        {
            linearVelocity = droneRb.linearVelocity;
            angularVelocity = droneRb.angularVelocity;
            this.altitude = altitude;
            this.collisionVelocity = collisionVelocity;
        }
    }
}