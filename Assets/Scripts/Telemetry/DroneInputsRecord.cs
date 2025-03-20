using System;
using System.IO;
using Drone;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Information about drone's control values at given time.</summary>
    [Serializable]
    public struct DroneInputsRecord : ICsvSerializable
    {
        public readonly float timestamp;
        public readonly float throttle;
        public readonly float pitch;
        public readonly float yaw;
        public readonly float roll;
        
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        
        public DroneInputsRecord(float throttle, float pitch, float yaw, float roll, Vector3 position, Quaternion rotation)
        {
            timestamp = Time.timeSinceLevelLoad;
            this.throttle = throttle;
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
            this.position = position;
            this.rotation = rotation;
        } 
        
        public DroneInputsRecord(DroneInputsController inputsController) 
            : this(inputsController.throttle, inputsController.pitch, inputsController.yaw, inputsController.roll,
                   inputsController.gameObject.transform.position, inputsController.gameObject.transform.rotation)
        {
        } 
        
        public void ToCsv(TextWriter writer, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}{0}",
                separator, 
                timestamp,
                throttle, pitch, yaw, roll);
            
            writer.Write("{1:F4}{0}{2:F4}{0}{3:F4}{0}{4:F4}{0}{5:F4}{0}{6:F4}\n",
                separator, 
                position.x, position.y, position.z,
                rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
        }
        
        public void ToCsv(TextWriter writer, bool onlyInputs, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}",
                separator, 
                timestamp,
                throttle, pitch, yaw, roll);

            if (!onlyInputs)
            {
                writer.Write("{0}{1:F4}{0}{2:F4}{0}{3:F4}{0}{4:F4}{0}{5:F4}{0}{6:F4}",
                    separator, 
                    position.x, position.y, position.z,
                    rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);   
            }
        }

        public void ToCsvNewLine(TextWriter writer, bool onlyInputs, string separator = "\t")
        {
            ToCsv(writer, onlyInputs, separator);
            writer.WriteLine();
        }
        
        public void ToCsvNewLine(TextWriter writer, string separator = "\t")
        {
            ToCsv(writer, separator);
            writer.WriteLine();
        }
    }
}