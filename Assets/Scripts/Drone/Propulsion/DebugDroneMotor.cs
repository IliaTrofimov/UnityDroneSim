using InspectorTools;
using UnityEngine;
using Utils;

namespace Drone.Propulsion
{
    /// <summary>
    /// Script that calculates propeller speed and animates it. Extends <see cref="DroneMotor"/> to add lift force debug plot.
    /// </summary>
    /// <remarks>
    /// Use <c>ApplyForce</c> and <c>ApplyForceClamp</c> to calculate and apply force and torque to some Rigidbody.
    /// </remarks>
    public class DebugDroneMotor : DroneMotor
    {
        #if UNITY_EDITOR
        [Header("Lift force")]
        [RealtimeCurve] 
        public AnimationCurve forceCurve = new();
        #endif

        private void Update()
        {
            #if UNITY_EDITOR
            forceCurve.AddKeyWrapped(Time.frameCount, liftForce);
            #endif
        }
    }
}