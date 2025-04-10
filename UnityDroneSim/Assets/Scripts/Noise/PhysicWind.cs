﻿using System.Collections.Generic;
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
        
        
        private float pulseTimer, pulsePeriod, pulseDuration;
        private float baseStrength, currentStrength;
        private float motionTimer, motionPeriod, windChangeSpeed;
        private WindPulseMode pulseMode = WindPulseMode.InitPulsing;
        private WindMotionMode motionMode = WindMotionMode.InitMotion;

        [Header("Debug")] 
        public bool showForceVector;
        
        [Header("Wind target objects")] 
        public string targetTags;
        private string previousTargetTags;
        private List<Rigidbody> targets;
        
        [Header("Wind force settings")]
        public Quaternion windDirection; 
        public float strength = 0.0015f;
        public float strengthOffSpeed = 50.0f;
        public float strengthOnSpeed = 70.0f;
        public float strengthHold = 1000.0f;

        public NormalDistributionParam normalStrength = new(30f, 20f);
        public NormalDistributionParam normalPulsePeriod = new(7f, 5f);
        public NormalDistributionParam normalPulseDuration = new(10f, 2f);
        public NormalDistributionParam normalMotionPeriod = new(8f, 3f);
        public NormalDistributionParam normalWindChangeSpeed = new(0.05f, 0.01f);
        

        public void Awake() => ResetTargetRigidBodies();

        private void OnValidate()
        {
            if (previousTargetTags != targetTags) 
                ResetTargetRigidBodies();   
        }
        
        private void FixedUpdate()
        {
            switch (pulseMode)
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

            if (motionMode is WindMotionMode.InitMotion)
                InitWindMotion();
            else
                WindMotion();
            
            var windForce = strength * currentStrength * (transform.rotation * Vector3.forward);
            for (var i = 0; i < targets?.Count; i++)
                targets[i].AddForce(windForce, ForceMode.Impulse);
        }

        private void OnDrawGizmos()
        {
            if (!showForceVector) return;
            
            var gizmoMain = new GizmoOptions(Color.blue, 0.3f, labelStyle: FontStyle.BoldAndItalic);
            var gizmoSmall = new GizmoOptions(Color.blue, 0.05f, labelStyle: FontStyle.Italic, labelPlacement: GizmoLabelPlacement.Start);

            var windForce = strength * currentStrength * (transform.rotation * Vector3.forward);
            VectorDrawer.DrawDirection(transform.position, windForce, name, gizmoMain);

            var windLabel = $"Wind {windForce.magnitude:F3}";
            foreach (var target in targets)
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
            
            if (targets == null)
                targets = new List<Rigidbody>(targetObjects.Length);
            else 
                targets.Clear();

            foreach (var target in targetObjects)
            {
                if (target.TryGetComponent<Rigidbody>(out var targetRb))
                    targets.Add(targetRb);
            }
            
            previousTargetTags = targetTags;
        }

        private void InitWindPulsing()
        {
            pulseTimer = 0.0f;
            pulsePeriod = MathExtensions.SamplePositive(normalPulsePeriod);
            pulseMode = WindPulseMode.Wait;
        }

        private void WindPulseWait()
        {
            pulseTimer += Time.fixedDeltaTime;
          
            currentStrength -= Time.deltaTime * strengthOffSpeed;
            MathExtensions.ClampPositive(currentStrength);

            if (!(pulseTimer >= pulsePeriod)) return;

            pulseTimer = 0.0f;
            pulseDuration = MathExtensions.SamplePositive(normalPulseDuration);
            baseStrength = MathExtensions.SamplePositive(normalStrength);
            pulseMode = WindPulseMode.Pulsing;
        }

        private void WindPulsing()
        {
            pulseTimer += Time.fixedDeltaTime;
            
            if (pulseTimer >= pulseDuration) // reset
            {
                pulseTimer = 0.0f; 
                pulseMode = WindPulseMode.InitPulsing;
            } 
            else
            {
                var targetStrength = MathExtensions.Sample(baseStrength, strengthHold);
                    
                if (math.abs(currentStrength - targetStrength) / (targetStrength + 1e-8) < 0.4)
                    currentStrength = targetStrength;
                else
                {
                    var dir = targetStrength > currentStrength ? 1 : -1;
                    currentStrength =+ Time.fixedDeltaTime * strengthOnSpeed;
                        
                    if (dir * currentStrength > dir * targetStrength)
                        currentStrength = targetStrength;
                }
            }
        }
        
        private void InitWindMotion()
        {
            motionTimer = 0.0f;
            motionPeriod = MathExtensions.SamplePositive(normalMotionPeriod);
            windChangeSpeed = MathExtensions.SamplePositive(normalWindChangeSpeed);
            windDirection = Quaternion.Euler(new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f));
            motionMode = WindMotionMode.Motion;
        }
        
        private void WindMotion()
        {
            motionTimer += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, windDirection, Time.deltaTime * windChangeSpeed);
           
            if (motionTimer > motionPeriod) 
            {
                motionTimer = 0.0f;
                motionMode = WindMotionMode.InitMotion; 
            }
        }
    }
}
