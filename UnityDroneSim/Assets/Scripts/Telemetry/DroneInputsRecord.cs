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
        public readonly float Timestamp;
        public readonly float Throttle;
        public readonly float Pitch;
        public readonly float Yaw;
        public readonly float Roll;

        public readonly Vector3    Position;
        public readonly Quaternion Rotation;

        public DroneInputsRecord(
            float throttle,
            float pitch,
            float yaw,
            float roll,
            Vector3 position,
            Quaternion rotation)
        {
            Timestamp = Time.timeSinceLevelLoad;
            Throttle = throttle;
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
            Position = position;
            Rotation = rotation;
        }

        public DroneInputsRecord(DroneInputsController inputsController)
            : this(inputsController.throttle,
                inputsController.pitch,
                inputsController.yaw,
                inputsController.roll,
                inputsController.gameObject.transform.position,
                inputsController.gameObject.transform.rotation
            )
        {
        }

        public void ToCsv(TextWriter writer, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}{0}",
                separator,
                Timestamp,
                Throttle,
                Pitch,
                Yaw,
                Roll
            );

            writer.Write("{1:F4}{0}{2:F4}{0}{3:F4}{0}{4:F4}{0}{5:F4}{0}{6:F4}\n",
                separator,
                Position.x,
                Position.y,
                Position.z,
                Rotation.eulerAngles.x,
                Rotation.eulerAngles.y,
                Rotation.eulerAngles.z
            );
        }

        public void ToCsv(TextWriter writer, bool onlyInputs, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}",
                separator,
                Timestamp,
                Throttle,
                Pitch,
                Yaw,
                Roll
            );

            if (!onlyInputs)
                writer.Write("{0}{1:F4}{0}{2:F4}{0}{3:F4}{0}{4:F4}{0}{5:F4}{0}{6:F4}",
                    separator,
                    Position.x,
                    Position.y,
                    Position.z,
                    Rotation.eulerAngles.x,
                    Rotation.eulerAngles.y,
                    Rotation.eulerAngles.z
                );
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