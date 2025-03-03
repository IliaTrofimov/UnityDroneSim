using Drone.Propulsion;
using UnityEngine;


namespace Drone
{
    /// <summary>Object that contains information to destroy/repair drone motor.</summary>
    internal class MotorDestructionInfo
    {
        public bool isDestroyed;
        public Vector3 initialLocalPosition;
        public Vector3 initialLocalScale;
        public Quaternion initialLocalRotation;
        public Transform initialParent;
        public DroneMotor motor;
        public Rigidbody attachedRigidbody;

        public MotorDestructionInfo(DroneMotor motor)
        {
            isDestroyed = false;
            initialLocalPosition = motor.transform.localPosition;
            initialLocalScale = motor.transform.localScale;
            initialLocalRotation = motor.transform.localRotation;
            initialParent = motor.transform.parent;
            this.motor = motor;
            attachedRigidbody = null;
        }
    }
}