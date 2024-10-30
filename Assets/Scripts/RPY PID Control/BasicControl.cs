using RPY_PID_Control.Motors;
using UnityEngine;
using UnityEngine.Serialization;


namespace RPY_PID_Control
{
    public class BasicControl : MonoBehaviour
    {

        [Header("Control")]
        public Controller.Controller controller;
        public float throttleIncrease;
	
        [Header("Motors")]
        public Motor[] motors;
        public float throttleValue;

        [Header("Internal")]
        public ComputerModule computer;

        private void FixedUpdate() 
        {
            computer.UpdateComputer(controller.pitch, controller.roll, controller.throttle * throttleIncrease);
            throttleValue = computer.heightCorrection;
            ComputeMotors();
            if (computer != null)
                computer.UpdateGyro();
            ComputeMotorSpeeds();
        }

        private void ComputeMotors()
        {
            float yaw = 0.0f;
            Rigidbody rb = GetComponent<Rigidbody>();
            int i = 0;
            foreach (Motor motor in motors)
            {
                motor.UpdateForceValues();
                yaw += motor.sideForce;
                i++;
                Transform t = motor.GetComponent<Transform>();
                //			Debug.Log (i);
                //			Debug.Log (motor.upForce);
                rb.AddForceAtPosition(transform.up * motor.upForce, t.position, ForceMode.Impulse);
            }
            rb.AddTorque(Vector3.up * yaw, ForceMode.Force);
        }

        private void ComputeMotorSpeeds()
        {
            foreach (Motor motor in motors)
            {
                if (computer.gyro.altitude < 0.1)
                    motor.UpdatePropeller(0.0f);
                else
                    motor.UpdatePropeller(1200.0f);
            }
        }
    }
}