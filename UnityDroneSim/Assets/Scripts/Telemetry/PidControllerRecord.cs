using System;
using System.IO;
using Drone.Stability;
using UnityEngine;


namespace Telemetry
{
    [Serializable]
    public struct PidControllerRecord : ICsvSerializable
    {
        public readonly float Timestamp;

        public readonly float Target;
        public readonly float Error;
        public readonly float Integral;
        public readonly float Output;
        public readonly float P;
        public readonly float I;
        public readonly float D;

        public PidControllerRecord(
            float target,
            float error,
            float integral,
            float output,
            float p,
            float i,
            float d)
        {
            Timestamp = Time.timeSinceLevelLoad;
            Target = target;
            Error = error;
            Integral = integral;
            Output = output;
            P = p;
            I = i;
            D = d;
        }

        public PidControllerRecord(DebugPidController pid)
            : this(pid.targetValue,
                pid.error,
                pid.integral,
                pid.output,
                pid.p,
                pid.i,
                pid.d
            )
        {
        }

        public void ToCsv(TextWriter writer, string separator = "\t")
        {
            writer.Write("{1:F4}{0}{2:F3}{0}{3:F3}{0}{4:F3}{0}{5:F3}{0}{6:F3}{0}{7:F3}{0}{8:F3}",
                separator,
                Timestamp,
                P,
                I,
                D,
                Target,
                Error,
                Integral,
                Output
            );
        }

        public void ToCsvNewLine(TextWriter writer, string separator = "\t")
        {
            ToCsv(writer, separator);
            writer.WriteLine();
        }
    }
}