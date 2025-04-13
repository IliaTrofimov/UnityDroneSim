using System;
using System.IO;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Information about position and rotation of object at given time.</summary>
    [Serializable]
    public struct MovementRecord : ICsvSerializable
    {
        public readonly float      Timestamp;
        public readonly Vector3    Position;
        public readonly Quaternion Rotation;

        public MovementRecord(Vector3 position, Quaternion rotation)
        {
            Timestamp = Time.timeSinceLevelLoad;
            Position = position;
            Rotation = rotation;
        }

        public MovementRecord(Component component)
            : this(component.gameObject)
        {
        }

        public MovementRecord(GameObject gameObject)
            : this(gameObject.transform)
        {
        }

        public MovementRecord(Transform transform)
            : this(transform.position, transform.rotation)
        {
        }


        public void ToCsv(TextWriter writer, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F4}{0}{3:F4}{0}{4:F4}{0}{5:F4}{0}{6:F4}{0}{7:F4}",
                separator,
                Timestamp,
                Position.x,
                Position.y,
                Position.z,
                Rotation.eulerAngles.x,
                Rotation.eulerAngles.y,
                Rotation.eulerAngles.z
            );
        }

        public void ToCsvNewLine(TextWriter writer, string separator = "\t")
        {
            ToCsv(writer, separator);
            writer.WriteLine();
        }
    }
}