using Drone.Stability;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Collection of <see cref="PidControllerRecord"/> recordered during game.</summary>
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "NewPidControllerLog", menuName = "Telemetry/Pid Controller Log")]
    public sealed class PidControllerLog : TelemetryLog<PidControllerRecord>
    {
        public void Add(DebugPidController pid) => records.Add(new PidControllerRecord(pid));

        public void Add(float target,
                        float error,
                        float integral,
                        float output,
                        float p,
                        float i,
                        float d) =>
            records.Add(
                new PidControllerRecord(target, error, integral, output, p, i, d)
            );
    }
}