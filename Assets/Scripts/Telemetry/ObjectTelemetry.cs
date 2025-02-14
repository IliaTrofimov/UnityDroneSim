using UnityEngine;


namespace Telemetry
{
    /// <summary>Physics object position and movement data.</summary>
    public readonly struct ObjectTelemetry
    {
        /// <summary>Object's absolute position.</summary>
        public readonly Vector3 position;
        
        /// <summary>Object's rotation. Stores Euler angles.</summary>
        public readonly Vector3 rotation;
        
        /// <summary>Object's linear velocity vector.</summary>
        public readonly Vector3 linearVelocity;
        
        /// <summary>Object's linear angular vector.</summary>
        public readonly Vector3 angularVelocity;
        
        /// <summary>Object's linear acceleration vector.</summary>
        public readonly Vector3 linearAcceleration;
        
        /// <summary>Time at which telemetry data was collected. Equals to <see cref="Time"/>.<see cref="Time.realtimeSinceStartup"/>.</summary>
        public readonly float timestamp;
        
        
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
            timestamp = Time.realtimeSinceStartup;
            
            position = pos;
            this.rotation = rotation;
            linearVelocity = linearVel;
            angularVelocity = angularVel;
            linearAcceleration = linearAcc;
        }
        

        public override string ToString() => 
            $"[{timestamp:F2}] r:{rotation:F1}, v:{linearVelocity:F1}, ω:{angularVelocity:F1}, a:{linearAcceleration:F1}";
    }
}