using System.IO;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Drone control input values record.</summary>
    public readonly struct DroneInputsTelemetry : ICsvSerializable
    {
        /// <summary>Time at which telemetry data was collected. Equals to <see cref="Time"/>.<see cref="Time.realtimeSinceStartup"/>.</summary>
        public readonly float timestamp;
        
        /// <summary>Throttle value.</summary>
        public readonly float throttle;
        
        /// <summary>Pitch value (rotation along X axis).</summary>
        public readonly float pitch;
        
        /// <summary>Yaw value (rotation along Y axis).</summary>
        public readonly float yaw;
        
        /// <summary>Roll value (rotation along Z axis).</summary>
        public readonly float roll;

        public DroneInputsTelemetry(float throttle, float pitch, float yaw, float roll)
        {
            timestamp = Time.realtimeSinceStartup;
            this.throttle = throttle;
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }
        
        public override string ToString() => 
            $"[{timestamp:F2}] pitch:{pitch:F3}, yaw:{yaw:F3}, roll:{roll:F3}, throt:{throttle:F3}";

        public string ToCsvString(char separator = ';') => 
            $"{timestamp}{separator}{pitch}{separator}{yaw}{separator}{roll}{separator}{throttle}";

        public void ToCsv(TextWriter stream, char separator = ';')
        {
            stream.Write(timestamp);
            stream.Write(separator);
            stream.Write(pitch);
            stream.Write(separator);
            stream.Write(yaw);
            stream.Write(separator);
            stream.Write(roll);
            stream.Write(separator);
            stream.WriteLine(throttle);
        }
    }
}