using UnityEngine;


namespace Drone.Motors
{
    /// <summary>Object that contains information to destroy/repair drone motor.</summary>
    internal class MotorDestructionInfo
    {
        public bool IsDestroyed { get; set; }
        public Rigidbody  AttachedRigidbody { get; set; }
        public Vector3 InitialLocalPosition { get; }
        public Vector3 InitialLocalScale { get; }
        public Quaternion InitialLocalRotation { get; }
        public Transform  InitialParent { get; }
        public DroneMotor Motor { get; }

        public MotorDestructionInfo(DroneMotor motor)
        {
            IsDestroyed = false;
            InitialLocalPosition = motor.transform.localPosition;
            InitialLocalScale = motor.transform.localScale;
            InitialLocalRotation = motor.transform.localRotation;
            InitialParent = motor.transform.parent;
            Motor = motor;
            AttachedRigidbody = null;
        }

        public override string ToString() =>
            $"{(Motor is DestructibleMotor ? "dMotor" : "Motor")} '{InitialParent.name}'.'{Motor.name}' ({(IsDestroyed ? "broken" : "ok")})";
    }
}