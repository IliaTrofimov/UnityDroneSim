using System;
using System.Collections;
using Telemetry;
using Unity.Mathematics;
using Unity.VisualScripting;
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
        [Flags]
        public enum LogsType { None = 0, Pid = 1, Movement = 2 }
        
        private DroneInputsTelemetryRecorder inputsRecorder;
        private PidTelemetryRecorder pidRecorder;
        private DroneInputsController inputsController;
        private Rigidbody droneRigidbody;
        private QuadcopterComputer drone;

        private float startTime;
        private bool hasReachedTarget;
        
        public Collider brakeTrigger;
        
        [Header("Settings")]
        [Range(0.5f, 60f)] public float maxTime = 7;
        public bool freezeAfterFinish = true;
        public LogsType logType = LogsType.Movement;
        public string logPrefix = "";
        
        
        private void Awake()
        {
            drone = GetComponent<QuadcopterComputer>();
            droneRigidbody = drone.GetComponent<Rigidbody>();
            inputsController = drone.GetComponent<DroneInputsController>();
            inputsRecorder = GetComponent<DroneInputsTelemetryRecorder>();
            pidRecorder = GetComponent<PidTelemetryRecorder>();
        }
        
        private void Start()
        {
            if (!enabled) return;
            
            startTime = Time.timeSinceLevelLoad;
            inputsRecorder.isRecording = true;
            pidRecorder.isRecording = true;
            hasReachedTarget = false;
            
            inputsController.manualInput = false;
            inputsController.SetInputs(1, 0, 0, 0);
            
            Debug.LogFormat("Drone '{0}' is starting height stab demo", drone.name);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != brakeTrigger || hasReachedTarget || !enabled) 
                return;

            Debug.LogFormat("Drone '{0}' has reached trigger '{1}' and now is slowing down", 
                drone.name, brakeTrigger.name);

            hasReachedTarget = true;
            inputsController.throttle = 0;
            StartCoroutine(StoppingTimer());
        }

        private IEnumerator StoppingTimer()
        {
            var isAscending = true;
            var maxHeight = droneRigidbody.position.y;
            
            var avgStableSpeed = 0f;
            var stabFramesCount = 0;

            var minDropHeight = float.MaxValue;
            var maxJumpHeight = 0f;
            
            while (Time.timeSinceLevelLoad - startTime <= maxTime)
            {
                if (droneRigidbody.position.y < 0.05f)
                {
                    Debug.LogFormat("Drone '{0}' has finished height stab demo (fail): hit the ground", drone.name);
                    break;
                }
                
                var height = droneRigidbody.position.y;
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
                    avgStableSpeed += droneRigidbody.linearVelocity.y;
                    stabFramesCount++;
                    
                    if (height > maxJumpHeight) maxJumpHeight = height;
                    if (height < minDropHeight) minDropHeight = height;
                }
                
                yield return null;
            }
            
            avgStableSpeed /= stabFramesCount;
            if (math.abs(avgStableSpeed) < 0.01f)
            {
                Debug.LogFormat("Drone '{0}' has finished height stab demo: " +
                                "avg. stab. speed {1:F2} m/s, cur. height {2:F2}, max height {3:F2}, stab. height range [{4:F2}, {5:F2}]",
                    drone.name, avgStableSpeed, droneRigidbody.position.y, maxHeight, minDropHeight, maxJumpHeight);
            }
            else
            {
                Debug.LogFormat("Drone '{0}' has finished height stab demo (fail): " +
                                "avg. stab. speed {1:F2} m/s, cur. height {2:F2}, max height {3:F2}, stab. height range [{4:F2}, {5:F2}]",
                    drone.name, avgStableSpeed, droneRigidbody.position.y, maxHeight, minDropHeight, maxJumpHeight);
            }
            
            if (freezeAfterFinish)
                droneRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            if (logType.HasFlag(LogsType.Movement))
                SaveLog();
            
            EditorApplication.isPlaying = false;
        }
        
        private void SaveLog()
        {
            if (logType == LogsType.None) return;
            
            
            var pid = drone.controlSettings.pidThrottle;
            var force = drone.controlSettings.maxLiftForce;
            var speed = drone.controlSettings.maxLiftSpeed;
            
            var logName = string.Format("p{0:f2};i{1:f2};d{2:f2};imin{3:f1};imax{4:f1};f{5:f0};v{6:f0}",
                pid.pFactor, pid.iFactor, pid.dFactor, pid.minIntegral, pid.maxIntegral, force, speed);
  
            if (!string.IsNullOrEmpty(logPrefix))
                logName = $"{logPrefix}#{logName}";

            logName = logName.Replace(".", "_");
            
            if (logType.HasFlag(LogsType.Movement))
                inputsRecorder.SaveToFile(logName);
            
            if (logType.HasFlag(LogsType.Pid))
                pidRecorder.SaveToFile(logName);
        }
    }
}