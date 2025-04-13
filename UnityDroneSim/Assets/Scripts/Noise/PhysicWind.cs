using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;
using UtilsDebug;
using Random = UnityEngine.Random;


namespace Noise
{
    public class PhysicWind : MonoBehaviour
    {
        private enum WindPulseMode { InitPulsing, Wait, Pulsing }

        private enum WindMotionMode { InitMotion, Motion }


        private float          _pulseTimer,   _pulsePeriod, _pulseDuration;
        private float          _baseStrength, _currentStrength;
        private float          _motionTimer,  _motionPeriod, _windChangeSpeed;
        private WindPulseMode  _pulseMode  = WindPulseMode.InitPulsing;
        private WindMotionMode _motionMode = WindMotionMode.InitMotion;

        [Header("Debug")] public bool showForceVector;

        [Header("Wind target objects")] public string targetTags;

        private string          _previousTargetTags;
        private List<Rigidbody> _targets;

        [Header("Wind force settings")] public Quaternion windDirection;

        public float strength         = 0.0015f;
        public float strengthOffSpeed = 50.0f;
        public float strengthOnSpeed  = 70.0f;
        public float strengthHold     = 1000.0f;

        public NormalDistributionParam normalStrength        = new(30f, 20f);
        public NormalDistributionParam normalPulsePeriod     = new(7f, 5f);
        public NormalDistributionParam normalPulseDuration   = new(10f, 2f);
        public NormalDistributionParam normalMotionPeriod    = new(8f, 3f);
        public NormalDistributionParam normalWindChangeSpeed = new(0.05f, 0.01f);


        public void Awake() => ResetTargetRigidBodies();

        private void OnValidate()
        {
            if (_previousTargetTags != targetTags)
                ResetTargetRigidBodies();
        }

        private void FixedUpdate()
        {
            switch (_pulseMode)
            {
            case WindPulseMode.InitPulsing:
                InitWindPulsing();
                break;
            case WindPulseMode.Wait:
                WindPulseWait();
                break;
            case WindPulseMode.Pulsing:
                WindPulsing();
                break;
            }

            if (_motionMode is WindMotionMode.InitMotion)
                InitWindMotion();
            else
                WindMotion();

            var windForce = strength * _currentStrength * (transform.rotation * Vector3.forward);
            for (var i = 0; i < _targets?.Count; i++)
                _targets[i].AddForce(windForce, ForceMode.Impulse);
        }

        private void OnDrawGizmos()
        {
            if (!showForceVector) return;

            var gizmoMain = new GizmoOptions(Color.blue, 0.3f, labelStyle: FontStyle.BoldAndItalic);
            var gizmoSmall = new GizmoOptions(Color.blue,
                0.05f,
                labelStyle: FontStyle.Italic,
                labelPlacement: GizmoLabelPlacement.Start
            );

            var windForce = strength * _currentStrength * (transform.rotation * Vector3.forward);
            VectorDrawer.DrawDirection(transform.position, windForce, name, gizmoMain);

            var windLabel = $"Wind {windForce.magnitude:F3}";
            foreach (var target in _targets)
                VectorDrawer.DrawDirection(target.position, windForce, windLabel, gizmoSmall);
        }

        private void OnDrawGizmosSelected()
        {
            windDirection = Handles.RotationHandle(new Handles.RotationHandleIds(), windDirection, transform.position);
        }


        private void ResetTargetRigidBodies()
        {
            if (string.IsNullOrEmpty(targetTags)) return;

            GameObject[] targetObjects;
            try
            {
                targetObjects = GameObject.FindGameObjectsWithTag(targetTags);
            }
            catch
            {
                return;
            }

            if (_targets == null)
                _targets = new List<Rigidbody>(targetObjects.Length);
            else
                _targets.Clear();

            foreach (var target in targetObjects)
            {
                if (target.TryGetComponent<Rigidbody>(out var targetRb))
                    _targets.Add(targetRb);
            }

            _previousTargetTags = targetTags;
        }

        private void InitWindPulsing()
        {
            _pulseTimer = 0.0f;
            _pulsePeriod = MathExtensions.SamplePositive(normalPulsePeriod);
            _pulseMode = WindPulseMode.Wait;
        }

        private void WindPulseWait()
        {
            _pulseTimer += Time.fixedDeltaTime;

            _currentStrength -= Time.deltaTime * strengthOffSpeed;
            MathExtensions.ClampPositive(_currentStrength);

            if (!(_pulseTimer >= _pulsePeriod)) return;

            _pulseTimer = 0.0f;
            _pulseDuration = MathExtensions.SamplePositive(normalPulseDuration);
            _baseStrength = MathExtensions.SamplePositive(normalStrength);
            _pulseMode = WindPulseMode.Pulsing;
        }

        private void WindPulsing()
        {
            _pulseTimer += Time.fixedDeltaTime;

            if (_pulseTimer >= _pulseDuration) // reset
            {
                _pulseTimer = 0.0f;
                _pulseMode = WindPulseMode.InitPulsing;
            }
            else
            {
                var targetStrength = MathExtensions.Sample(_baseStrength, strengthHold);

                if (math.abs(_currentStrength - targetStrength) / (targetStrength + 1e-8) < 0.4)
                {
                    _currentStrength = targetStrength;
                }
                else
                {
                    var dir = targetStrength > _currentStrength ? 1 : -1;
                    _currentStrength = +Time.fixedDeltaTime * strengthOnSpeed;

                    if (dir * _currentStrength > dir * targetStrength)
                        _currentStrength = targetStrength;
                }
            }
        }

        private void InitWindMotion()
        {
            _motionTimer = 0.0f;
            _motionPeriod = MathExtensions.SamplePositive(normalMotionPeriod);
            _windChangeSpeed = MathExtensions.SamplePositive(normalWindChangeSpeed);
            windDirection = Quaternion.Euler(new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f));
            _motionMode = WindMotionMode.Motion;
        }

        private void WindMotion()
        {
            _motionTimer += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, windDirection, Time.deltaTime * _windChangeSpeed);

            if (_motionTimer > _motionPeriod)
            {
                _motionTimer = 0.0f;
                _motionMode = WindMotionMode.InitMotion;
            }
        }
    }
}