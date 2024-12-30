using UnityEngine;


namespace Telemetry
{
    /// <summary>Physics object position and movement data.</summary>
    public readonly struct ObjectTelemetry
    {
        /// <summary>Object's absolute position.</summary>
        public readonly Vector3 Position;
        
        /// <summary>Object's rotation. Stores Euler angles.</summary>
        public readonly Vector3 Rotation;
        
        /// <summary>Object's linear velocity vector.</summary>
        public readonly Vector3 LinearVelocity;
        
        /// <summary>Object's linear angular vector.</summary>
        public readonly Vector3 AngularVelocity;
        
        /// <summary>Object's linear acceleration vector.</summary>
        public readonly Vector3 LinearAcceleration;
        
        /// <summary>Time at which telemetry data was collected. Equals to <see cref="Time"/>.<see cref="Time.realtimeSinceStartup"/>.</summary>
        public readonly float Timestamp;
        
        
        /// <summary>Create telemetry data from rigid body.</summary>
        public ObjectTelemetry(Rigidbody rigidbody)
            : this(rigidbody.position, rigidbody.rotation.eulerAngles, rigidbody.linearVelocity, rigidbody.angularVelocity)
        {}
        
        /// <summary>Create telemetry data from transform object.</summary>
        public ObjectTelemetry(Transform transform, Vector3 linearVel = default, Vector3 angularVel = default, Vector3 linearAcc = default)
            : this(transform.position, transform.rotation.eulerAngles, linearVel, angularVel, linearAcc)
        {}
        
        /// <summary>Create telemetry data manually.</summary>
        public ObjectTelemetry(Vector3 pos, Vector3 rotation, 
                               Vector3 linearVel = default, Vector3 angularVel = default, Vector3 linearAcc = default)
        {
            Timestamp = Time.realtimeSinceStartup;
            
            Position = pos;
            Rotation = rotation;
            LinearVelocity = linearVel;
            AngularVelocity = angularVel;
            LinearAcceleration = linearAcc;
        }
        

        public override string ToString() => 
            $"[{Timestamp:F2}] r:{Rotation:F1}, v:{LinearVelocity:F1}, ω:{AngularVelocity:F1}, a:{LinearAcceleration:F1}";
    }
}