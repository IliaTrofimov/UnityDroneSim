using System;
using System.IO;
using Drone.Stability;
using UnityEngine;


namespace Telemetry
{
    [Serializable]
    public struct PidControllerRecord : ICsvSerializable
    {
        public readonly float timestamp;

        public readonly float target;
        public readonly float error;
        public readonly float integral;
        public readonly float output;
        public readonly float p;
        public readonly float i;
        public readonly float d;

        public PidControllerRecord(float target, float error, float integral, float output, float p, float i, float d)
        {
            timestamp = Time.timeSinceLevelLoad;
            this.target = target;
            this.error = error;
            this.integral = integral;
            this.output = output;
            this.p = p;
            this.i = i;
            this.d = d;
        }
        
        public PidControllerRecord(DebugPidController pid) 
            : this(pid.targetValue, pid.error, pid.integral, pid.output, pid.p, pid.i, pid.d)
        {}

        public void ToCsv(TextWriter writer, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}{0}{6:F3}{0}{7:F3}{0}{8:F3}",
                separator, 
                timestamp,
                p, i, d,
                target, error, integral, output);
        }
        
        public void ToCsvNewLine(TextWriter writer, string separator = "\t")
        {
            ToCsv(writer, separator);
            writer.WriteLine();
        }
    }
}