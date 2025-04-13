using System;
using System.IO;
using Drone;
using Drone.Stability;
using UnityEngine;


namespace Telemetry
{
    public sealed class PidTelemetryRecorder : BaseTelemetryRecorder<PidControllerRecord, PidControllerLog>
    {
        public enum TargetPidType { Throttle, Pitch, Yaw, Roll }

        [Header("Target object")] public QuadcopterComputer target;
        public                           TargetPidType      targetPid;

        private DebugPidController _debugPidController;

        private void Awake()
        {
            if (!target) return;

            switch (targetPid)
            {
            case TargetPidType.Throttle when target.PidThrottle is DebugPidController pidThrottle:
                _debugPidController = pidThrottle;
                break;
            case TargetPidType.Pitch when target.PidPitch is DebugPidController pidPitch:
                _debugPidController = pidPitch;
                break;
            case TargetPidType.Yaw when target.PidYaw is DebugPidController pidYaw:
                _debugPidController = pidYaw;
                break;
            case TargetPidType.Roll when target.PidRoll is DebugPidController pidRoll:
                _debugPidController = pidRoll;
                break;
            default:
                throw new ArgumentException("Cannot find target DebugPidController");
            }
        }

        private void FixedUpdate()
        {
            if (!target || _debugPidController == null) return;

            TryAddRecord(new PidControllerRecord(_debugPidController));
        }

        public void SaveToFile(string logName)
        {
            if (string.IsNullOrEmpty(csvLogsPath) || string.IsNullOrEmpty(logName)) return;

            var path = Path.Combine(csvLogsPath, logName) + ".csv";
            recordsStorage.SaveToCsv(path);
            Debug.LogFormat("PID controller telemetry for '{0}' (asset '{1}') was saved to '{2}'",
                target.name,
                recordsStorage.name,
                path
            );
        }

        [ContextMenu("Save to file")]
        public void SaveToFileAction()
        {
            var path = Path.Combine(csvLogsPath, $"{target.name}_inputs.csv");
            SaveToFile(path);
        }
    }
}