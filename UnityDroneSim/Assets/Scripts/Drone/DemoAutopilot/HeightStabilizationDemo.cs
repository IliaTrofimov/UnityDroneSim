using System;
using System.Collections;
using Drone.Stability;
using Telemetry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace Drone.DemoAutopilot
{
    /// <summary>Script that automatically controls drone object to demonstrate vertical velocity stabilization.</summary>
    /// <remarks>Drone will accelerate up until reaches the trigger. Then throttle PID controller must stabilize drone's position.</remarks>
    [RequireComponent(typeof(DroneInputsTelemetryRecorder))]
    [RequireComponent(typeof(QuadcopterComputer))]
    public class HeightStabilizationDemo : MonoBehaviour
    {
        [Flags] public enum LogsType { None = 0, Pid = 1, Movement = 2 }

        private DroneInputsTelemetryRecorder _inputsRecorder;
        private PidTelemetryRecorder         _pidRecorder;
        private DroneInputsController        _inputsController;
        private Rigidbody                    _droneRigidbody;
        private QuadcopterComputer           _drone;

        private float _startTime;
        private bool  _hasReachedTarget;

        public Collider brakeTrigger;

        [Header("Settings")]
        [Range(0.5f, 60f)]
        public float maxTime = 7;

        public bool     freezeAfterFinish = true;
        public LogsType logType           = LogsType.Movement;
        public string   logPrefix         = "";


        private void Awake()
        {
            _drone = GetComponent<QuadcopterComputer>();
            _droneRigidbody = _drone.GetComponent<Rigidbody>();
            _inputsController = _drone.GetComponent<DroneInputsController>();
            _inputsRecorder = GetComponent<DroneInputsTelemetryRecorder>();
            _pidRecorder = GetComponent<PidTelemetryRecorder>();
        }

        private void Start()
        {
            if (!enabled) return;

            _startTime = Time.timeSinceLevelLoad;
            _inputsRecorder.isRecording = true;
            _pidRecorder.isRecording = true;
            _hasReachedTarget = false;

            _inputsController.manualInput = false;
            _inputsController.SetInputs(1, 0, 0, 0);

            Debug.LogFormat("Drone '{0}' is starting height stab demo", _drone.name);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != brakeTrigger || _hasReachedTarget || !enabled)
                return;

            Debug.LogFormat("Drone '{0}' has reached trigger '{1}' and now is slowing down",
                _drone.name,
                brakeTrigger.name
            );

            _hasReachedTarget = true;
            _inputsController.throttle = 0;
            StartCoroutine(StoppingTimer());
        }

        private IEnumerator StoppingTimer()
        {
            var isAscending = true;
            var maxHeight = _droneRigidbody.position.y;

            var avgStableSpeed = 0f;
            var stabFramesCount = 0;

            var minDropHeight = float.MaxValue;
            var maxJumpHeight = 0f;

            while (Time.timeSinceLevelLoad - _startTime <= maxTime)
            {
                if (_droneRigidbody.position.y < 0.05f)
                {
                    Debug.LogFormat("Drone '{0}' has finished height stab demo (fail): hit the ground", _drone.name);
                    break;
                }

                var height = _droneRigidbody.position.y;
                if (isAscending && height > maxHeight)
                {
                    maxJumpHeight = maxHeight = height;
                }
                else if (isAscending && height < maxHeight)
                {
                    isAscending = false;
                }
                else if (!isAscending)
                {
                    avgStableSpeed += _droneRigidbody.linearVelocity.y;
                    stabFramesCount++;

                    if (height > maxJumpHeight) maxJumpHeight = height;
                    if (height < minDropHeight) minDropHeight = height;
                }

                yield return null;
            }

            avgStableSpeed /= stabFramesCount;
            if (math.abs(avgStableSpeed) < 0.01f)
                Debug.LogFormat("Drone '{0}' has finished height stab demo: " +
                                "avg. stab. speed {1:F2} m/s, cur. height {2:F2}, max height {3:F2}, stab. height range [{4:F2}, {5:F2}]",
                    _drone.name,
                    avgStableSpeed,
                    _droneRigidbody.position.y,
                    maxHeight,
                    minDropHeight,
                    maxJumpHeight
                );
            else
                Debug.LogFormat("Drone '{0}' has finished height stab demo (fail): " +
                                "avg. stab. speed {1:F2} m/s, cur. height {2:F2}, max height {3:F2}, stab. height range [{4:F2}, {5:F2}]",
                    _drone.name,
                    avgStableSpeed,
                    _droneRigidbody.position.y,
                    maxHeight,
                    minDropHeight,
                    maxJumpHeight
                );

            if (freezeAfterFinish)
                _droneRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            if (logType.HasFlag(LogsType.Movement))
                SaveLog();

            EditorApplication.isPlaying = false;
        }

        private void SaveLog()
        {
            if (logType == LogsType.None) return;


            var pid = _drone.PidThrottle as DebugPidController
                      ?? throw new UnityException(
                          $"Cannot save log for PID controller of type {_drone.PidThrottle.GetType().Name}"
                      );

            var logName = string.Format("p{0:f2};i{1:f2};d{2:f2};imin{3:f1};imax{4:f1};f{5:f0};v{6:f0}",
                pid.PFactor,
                pid.Factor,
                pid.DFactor,
                pid.MinIntegral,
                pid.MaxIntegral,
                _drone.MaxLiftForce,
                _drone.MaxLiftSpeed
            );

            if (!string.IsNullOrEmpty(logPrefix))
                logName = $"{logPrefix}#{logName}";

            logName = logName.Replace(".", "_");

            if (logType.HasFlag(LogsType.Movement))
                _inputsRecorder.SaveToFile(logName);

            if (logType.HasFlag(LogsType.Pid))
                _pidRecorder.SaveToFile(logName);
        }
    }
}