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

        public readonly double Target;
        public readonly double Error;
        public readonly double Integral;
        public readonly double Output;
        public readonly double P;
        public readonly double I;
        public readonly double D;

        public PidControllerRecord(
            double target,
            double error,
            double integral,
            double output,
            double p,
            double i,
            double d)
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