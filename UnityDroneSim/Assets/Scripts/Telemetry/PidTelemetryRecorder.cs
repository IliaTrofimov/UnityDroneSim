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
        
        [Header("Target object")]
        public QuadcopterComputer target;
        public TargetPidType targetPid;

        private DebugPidController debugPidController;

        private void Awake()
        {
            if (!target) return;

            switch (targetPid)
            {
                case TargetPidType.Throttle when target.pidThrottle is DebugPidController pidThrottle:
                    debugPidController = pidThrottle;
                    break;
                case TargetPidType.Pitch when target.pidPitch is DebugPidController pidPitch:
                    debugPidController = pidPitch;
                    break;
                case TargetPidType.Yaw when target.pidYaw is DebugPidController pidYaw:
                    debugPidController = pidYaw;
                    break;
                case TargetPidType.Roll when target.pidRoll is DebugPidController pidRoll:
                    debugPidController = pidRoll;
                    break;
                default:
                    throw new ArgumentException("Cannot find target PID controller");
            }
        }

        private void FixedUpdate()
        {
            if (!target || debugPidController == null) return;

            TryAddRecord(new PidControllerRecord(debugPidController));
        }
        
        public void SaveToFile(string logName)
        {
            if (string.IsNullOrEmpty(csvLogsPath) || string.IsNullOrEmpty(logName)) return;
           
            var path = Path.Combine(csvLogsPath, logName) + ".csv";
            recordsStorage.SaveToCsv(path);
            Debug.LogFormat("PID controller telemetry for '{0}' (asset '{1}') was saved to '{2}'", target.name, recordsStorage.name, path);
        }
        
        [ContextMenu("Save to file")]
        public void SaveToFileAction()
        {
            var path = Path.Combine(csvLogsPath, $"{target.name}_inputs.csv");
            SaveToFile(path);
        }
    }
}