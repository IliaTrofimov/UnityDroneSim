using System.Runtime.CompilerServices;
using Noise;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Utils
{
    /// <summary>Some math helper functions.</summary>
    public static class MathExtensions
    {
        /// <summary>Get next random value from Gaussian distribution with given params.</summary>
        public static float Sample(float mean, float variance) => NextGaussianDouble() * math.sqrt(variance) + mean;

        /// <summary>Get next random value from Gaussian distribution with given params.</summary>
        public static float SamplePositive(NormalDistributionParam param) =>
            Mathf.Abs(NextGaussianDouble() * math.sqrt(param.variance) + param.mean);

        /// <summary>Get next random value from standard Gaussian distribution.</summary>
        public static float NextGaussianDouble()
        {
            float u, s;
            do
            {
                var v = 2.0f * Random.value - 1.0f;
                u = 2.0f * Random.value - 1.0f;
                s = u * u + v * v;
            } while (s >= 1.0f);

            var fac = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
            return u * fac;
        }

        /// <summary>Return given value if it is positive, otherwise return 0.</summary>
        public static float ClampPositive(float a) => a > 0f ? a : 0f;

        /// <summary>Set given value to zero if it is negative.</summary>
        public static void ClampPositive(ref float a)
        {
            if (a < 0f) a = 0f;
        }

        /// <summary>Create new vector with all coordinates equal to module of given vector's coordinates.</summary>
        public static Vector3 Abs(this Vector3 vector) =>
            new(math.abs(vector.x), math.abs(vector.y), math.abs(vector.z));

        /// <summary>Get rotation speed of the Rigidbody in rad/s along X axis.</summary>
        public static float PitchVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.right, rigidBody.angularVelocity);

        /// <summary>Get rotation speed of the Rigidbody in rad/s along Y axis.</summary>
        public static float YawVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.up, rigidBody.angularVelocity);

        /// <summary>Get rotation speed of the Rigidbody in rad/s along Z axis.</summary>
        public static float RollVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.forward, rigidBody.angularVelocity);

        /// <summary>Returns new Euler angles of transform with values wrapped between [0, 179] degrees.</summary>
        public static Vector3 WrapEulerRotation180(this Transform transform)
        {
            var eulerRotation = transform.eulerAngles;
            var x = eulerRotation.x;
            var y = eulerRotation.y;
            var z = eulerRotation.z;

            if (x >= 180f) x -= 360f;
            if (y >= 180f) y -= 360f;
            if (z >= 180f) z -= 360f;
            
            return new Vector3(x, y, z);
        }
        
        /// <summary>Returns square root of absolute value.</summary>
        /// <returns>√(abs(value))</returns>
        public static float AbsSqrt(float value) => math.sqrt(math.abs(value));
        
        /// <summary>
        /// Angles (rotation along X axis, and Y axis in local space) between current position and target.
        /// </summary>
        /// <remarks>
        /// X value means vertical angle to the target,
        /// Y value means horizontal angle (see default Unity coordinate system).
        /// </remarks>
        /// <returns>Normalized angles in range [-1, 1].</returns>
        public static Vector2 NormalizedHeadingAnglesTo(this Transform current, Vector3 target)
        {
            var drLocal = current.InverseTransformDirection(current.position - target);
            var angleHor = Mathf.Atan2(drLocal.x, drLocal.z) / Mathf.PI;
            var angleVert = Mathf.Atan2(-drLocal.y, drLocal.z) / Mathf.PI;
            return new Vector2(angleHor, angleVert);
        }
        
        /// <summary>
        /// Angles (rotation along X axis, and Y axis in local space) between current position and target.
        /// </summary>
        /// <remarks>
        /// X value means vertical angle to the target,
        /// Y value means horizontal angle (see default Unity coordinate system).
        /// </remarks>
        /// <returns>Angles in degrees in range [-180, 180].</returns>
        public static Vector2 HeadingAnglesTo(this Transform current, Vector3 target)
        {
            var drLocal = current.InverseTransformDirection(current.position - target);
            var angleHor = Mathf.Atan2(drLocal.x, drLocal.z) * Mathf.Rad2Deg;
            var angleVert = Mathf.Atan2(-drLocal.y, drLocal.z) * Mathf.Rad2Deg;
            return new Vector2(angleHor, angleVert);
        }
    }
}