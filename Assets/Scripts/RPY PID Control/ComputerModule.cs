using UnityEngine;

// Module reuniting computing parts of a drone.  Used by a BasicControl.
namespace RPY_PID_Control
{
    public class ComputerModule : MonoBehaviour
    {
        public Rigidbody body;
        
        [Header("Settings")]
        [Range(0, 90)] public float pitchLimit;
        [Range(0, 90)] public float rollLimit;

        [Header("Parts")]
        public PID pidThrottle;
        public PID pidPitch;
        public PID pidRoll;
        public BasicGyro gyro;
        
        [Header("Values")]
        public float pitchCorrection;
        public float rollCorrection;
        public float heightCorrection;

        public void UpdateComputer(float controlPitch, float controlRoll, float controlHeight)
        {
            UpdateGyro();
            pitchCorrection = pidPitch.Update(controlPitch * pitchLimit, gyro.pitch, Time.deltaTime);
            rollCorrection = pidRoll.Update(gyro.roll, controlRoll * rollLimit, Time.deltaTime);
            heightCorrection = pidThrottle.Update(controlHeight, gyro.velocityVector.y, Time.deltaTime);
        }

        public void UpdateGyro()
        {
            gyro.UpdateGyro(body);
        }
    }
}
