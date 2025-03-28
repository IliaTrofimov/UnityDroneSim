using System;
using Unity.Mathematics;
using UnityEngine;
using UtilsDebug;


namespace Drone.Stability
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrailRenderer))]
    [RequireComponent(typeof(DroneInputsController))]
    public class PidShowcaseAutopilot : MonoBehaviour
    {
        private TrailRenderer trailRenderer;
        private DroneInputsController inputsController;
        private Rigidbody rigidBody;

        public Color color = Color.black;
        public Rect labelPosition = new Rect(0f, 0f, 600f, 40f);
        
        public Collider targetStart;
        public Collider targetRelease;
        public Collider targetBrake;
        public Collider targetStop;

        private Collider currentTarget;
        private bool isFinalBrake, freezeNext;
        private float lastXSpeed;
        private float avgXSpeed;
        
        private void Awake()
        {
            inputsController = GetComponent<DroneInputsController>();
            rigidBody = GetComponent<Rigidbody>();
            trailRenderer = gameObject.GetComponent<TrailRenderer>();
        }

        private void Start()
        {
            inputsController.manualInput = false;
        }

        private void OnDisable() => inputsController.manualInput = true;

        private void OnGUI()
        {
            if (!didAwake) return;

            var settings = inputsController.GetComponent<QuadcopterComputer>()?.controlSettings;
            if (settings == null) return;

            var pid = settings.pidPitch;
            
            GUI.color = color;
            
            GUI.Label(labelPosition, 
                $"Pitch: Kp={pid.pFactor:F3}, Ki={pid.iFactor:F3}, Kd={pid.dFactor:F3}, Fp={settings.maxPitchForce:F2}, Pmax={settings.maxPitchAngle:f0} | " +
                $"Throt.: Kp={settings.pidThrottle.pFactor}, Ki={settings.pidThrottle.iFactor}, Kd={settings.pidThrottle.dFactor}, Tmax={settings.maxLiftForce:F0}, SPDmax={settings.maxLiftSpeed:F0}",
                new GUIStyle("label")
                {
                    fontSize = 24,
                });
        }

        private void Update()
        {
            if (enabled && isFinalBrake && !freezeNext)
            {
                if (math.sign(rigidBody.linearVelocity.x * lastXSpeed) < 0)
                {
                    inputsController.pitch = 0;
                    freezeNext = true;
                    Debug.LogFormat("Drone '{0}' has executed last brake and will be frozen", gameObject.name);
                }
            }
            lastXSpeed = rigidBody.linearVelocity.x;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (freezeNext)
            {
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                return;
            }

            if (!enabled || currentTarget == other || isFinalBrake)
                return;
            
            currentTarget = other;
            
            if (other == targetRelease)
            {
                if (inputsController.pitch > 0)
                {
                    inputsController.pitch = 0;
                }
                else
                {
                    isFinalBrake = true;
                    inputsController.pitch = 1;
                    Debug.LogFormat("Drone '{0}' is executing final brake", gameObject.name);
                }
            }
            else if (other == targetBrake)
            {
                inputsController.pitch = inputsController.pitch >= 0f ? -1f : 0f;
            }
            else if (other == targetStop)
            {
                inputsController.pitch = -1f;
            }
            
            Debug.LogFormat("Drone '{0}' is entering trigger '{1}': T={2:F0}, P={3:F0}, Y={4:F0}, R={5:F0}",
                gameObject.name, other.name, inputsController.throttle, inputsController.pitch, inputsController.yaw, inputsController.roll);
        } 
        
        private void OnTriggerStay(Collider other)
        {
            if (!enabled || other == currentTarget || isFinalBrake) return;
            currentTarget = other;
            
            if (other == targetStart)
            {
                trailRenderer.Clear();
                inputsController.pitch = 1f;
            }
            
            Debug.LogFormat("Drone '{0}' is staying in trigger '{1}': T={2:F0}, P={3:F0}, Y={4:F0}, R={5:F0}",
                gameObject.name, other.name, inputsController.throttle, inputsController.pitch, inputsController.yaw, inputsController.roll);
        } 
        
        private void OnTriggerExit(Collider other)
        {
            return;
            if (!enabled || other != targetStart || !isFinalBrake) return;

            freezeNext = true;
            
            Debug.LogFormat("Drone '{0}' is exiting trigger '{1}' (will be frozen): T={2:F0}, P={3:F0}, Y={4:F0}, R={5:F0}",
                gameObject.name, other.name, inputsController.throttle, inputsController.pitch, inputsController.yaw, inputsController.roll);
        } 
    }
}