using System;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;


namespace Noise
{
    public class RandomPulseNoise : MonoBehaviour 
    {
        private enum WindPulseMode { InitPulsing, Wait, Pulsing }

        private enum WindMotionMode { InitMotion, Motion }
        
        
        private float pulseTimer;
        private float pulsePeriod;
        private float pulseDuration;
        private float baseStrength;
        private float currentStrength;
        private float motionTimer;
        private float motionPeriod;
        private float windChangeSpeed;
        private WindPulseMode pulseMode = WindPulseMode.InitPulsing;
        private WindMotionMode motionMode = WindMotionMode.InitMotion;
        
        
        public Rigidbody body;
        public float strengthCoef = 0.0015f;
        public float strengthOffSpeed = 50.0f;
        public float strengthOnSpeed = 70.0f;
        public Quaternion targetDirection; 

        public float strengthHold = 1000.0f;

        public RandomDistributionParam randomStrength = new(30f, 20f);
        
        public RandomDistributionParam randomPulsePeriod = new(7f, 5f);

        public RandomDistributionParam randomPulseDuration = new(10f, 2f);

        public RandomDistributionParam randomMotionPeriod = new(8f, 3f);
       
        public RandomDistributionParam randomWindChangeSpeed = new(0.05f, 0.01f);


        public void Awake()
        {
            if (body is null)
                throw new UnityException("Body is null");
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
            
            var ray = currentStrength * (transform.rotation * Vector3.forward);
            body.AddForce(ray * strengthCoef, ForceMode.Impulse);
            Debug.DrawRay(body.position, ray, Color.green);
        }
        
        private void InitWindPulsing()
        {
            pulseTimer = 0.0f;
            pulsePeriod = MathExtensions.SamplePositive(randomPulsePeriod);
            pulseMode = WindPulseMode.Wait;
        }

        private void WindPulseWait()
        {
            pulseTimer += Time.deltaTime;

            currentStrength -= Time.deltaTime * strengthOffSpeed;
            if (currentStrength < 0.0f) 
                currentStrength = 0.0f;

            if (pulseTimer >= pulsePeriod)
            {
                pulseTimer = 0.0f; //reset
                pulseDuration = MathExtensions.SamplePositive(randomPulseDuration);
                baseStrength = MathExtensions.SamplePositive(randomStrength);
                pulseMode = WindPulseMode.Pulsing;
            }
        }

        private void WindPulsing()
        {
            pulseTimer += Time.deltaTime;
            if (pulseTimer >= pulseDuration) 
            {
                pulseTimer = 0.0f; //reset
                pulseMode = 0;
            } 
            else 
            {
                //apply force here
                var targetStrength = MathExtensions.Sample(baseStrength, strengthHold);
                    
                if (Mathf.Abs(currentStrength - targetStrength) / (targetStrength + 1e-8) < 0.4)
                    currentStrength = targetStrength;
                else
                {
                    var dir = targetStrength > currentStrength ? 1 : -1;
                    currentStrength =+ Time.deltaTime * strengthOnSpeed;
                        
                    if (dir * currentStrength > dir * targetStrength)
                        currentStrength = targetStrength;
                }
            }
        }
        
        
        private void InitWindMotion()
        {
            motionTimer = 0.0f;
            motionPeriod = MathExtensions.SamplePositive(randomMotionPeriod);
            windChangeSpeed = MathExtensions.SamplePositive(randomWindChangeSpeed);
            targetDirection = Quaternion.Euler(new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f));
            motionMode = WindMotionMode.Motion;
        }
        
        private void WindMotion()
        {
            motionTimer += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * windChangeSpeed);
           
            if (motionTimer > motionPeriod) 
            {
                motionTimer = 0.0f;
                motionMode = WindMotionMode.InitMotion; 
            }
        }
    }
}
