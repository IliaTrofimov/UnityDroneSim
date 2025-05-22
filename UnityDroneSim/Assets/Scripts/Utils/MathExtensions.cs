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

        public static Vector3 AxialAngularVelocity(this Rigidbody rigidBody)
        {
            return new Vector3
            {
                x = Vector3.Dot(rigidBody.rotation * Vector3.right, rigidBody.angularVelocity), 
                y = Vector3.Dot(rigidBody.rotation * Vector3.up, rigidBody.angularVelocity), 
                z = Vector3.Dot(rigidBody.rotation * Vector3.forward, rigidBody.angularVelocity)
            };
        }
        
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
            return WrapEulerRotation180(transform.eulerAngles);
        }

        public static float WrapEulerRotation180(float angle)
        {
            return angle >= 180f 
                ? angle - 360f 
                : angle <= -180 
                    ? angle + 360f 
                    : angle;
        }
        
        public static float WrapEulerRotation90(float angle)
        {
            return angle >= 90 
                ? angle - 180f 
                : angle <= -90 
                    ? angle + 180 
                    : angle;
        }
        
        public static Vector3 WrapEulerRotation180(Vector3 angles)
        {
            return new Vector3(WrapEulerRotation180(angles.x), WrapEulerRotation180(angles.y), WrapEulerRotation180(angles.z));
        }

        
        /// <summary>Returns square root of absolute value.</summary>
        /// <returns>√(abs(value))</returns>
        public static float AbsSqrt(float value) => math.sqrt(math.abs(value));
        
        /// <summary>
        /// Polar (θ) and azimuthal (φ) angles of spherical coordinates between current position and target.
        /// </summary>
        /// <remarks>
        /// * θ corresponds to vertical rotation (along local X axis) needed to look at the target.<br/>
        /// * φ corresponds to horizontal rotation (along local Y axis) needed to look at the target.<br/>
        /// * all angles are calculated with radians and then normalized.<br/>
        /// * value 0 means that target is right ahead.<br/>
        /// * values ±1 means that target is right behind (180 degrees turn is needed to look at it).<br/>
        /// * values ±0.5 means that target is to the left/right (90 degrees turn is needed).
        /// </remarks>
        /// <returns>Vector2 where x is normalized angle φ [-1, 1] and y is angle θ [-0.5, 0.5].</returns>
        public static Vector2 NormalizedHeadingAnglesTo(this Transform current, Vector3 target)
        {
            var dR = target - current.position;
            var relativeDr = current.InverseTransformDirection(dR);
            var angleVert = -WrapEulerRotation90(math.atan2(relativeDr.y, relativeDr.z) / math.PI);
            var angleHor = WrapEulerRotation180( math.atan2(relativeDr.x, relativeDr.z) / math.PI);
            return new Vector2(angleHor, angleVert);
        }
        
        /// <summary>
        /// Polar (θ) and azimuthal (φ) angles of spherical coordinates between current position and target.
        /// </summary>
        /// <remarks>
        /// * θ corresponds to vertical rotation (along local X axis) needed to look at the target.<br/>
        /// * φ corresponds to horizontal rotation (along local Y axis) needed to look at the target.<br/>
        /// </remarks>
        /// <returns>Vector2 where x is angle φ [-180, 180] and y is angle θ [-90, 90].</returns>
        public static Vector2 HeadingAnglesTo(this Transform current, Vector3 target)
        {
            var dR = target - current.position;
            var relativeDr = current.InverseTransformDirection(dR);
            var angleVert = -WrapEulerRotation90(math.atan2(relativeDr.y, relativeDr.z) * math.TODEGREES);
            var angleHor = WrapEulerRotation180(math.atan2(relativeDr.x, relativeDr.z) * math.TODEGREES);
            return new Vector2(angleHor, angleVert);
        }

        public static string GetTimeString(float seconds)
        {
            if (seconds >= 3600f)
            {
                int h = (int)(seconds / 3600f);
                int m = (int)((seconds - h*3600f) / 60);
                int s = (int)(seconds - h*3600f - m*60);
                return $"{h}h {m}m {s}s";
            }
            else if (seconds >= 60)
            {
                int m = (int)(seconds / 60f);
                int s = (int)(seconds - m * 60);
                return $"{m}m {s}s";
            }
            else if (seconds >= 1)
            {
                return $"{seconds:F2}s";
            }
            return $"{seconds * 1000:F2}ms";
        }
    }
}