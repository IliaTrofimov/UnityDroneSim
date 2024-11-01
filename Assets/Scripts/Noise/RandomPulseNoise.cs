using UnityEngine;
using UnityEngine.Serialization;


namespace Noise
{
    public class RandomPulseNoise : MonoBehaviour 
    {
        private enum WindPulseMode { InitPulsing, Wait, Pulsing }

        private enum WindMotionMode { InitMotion, Motion }
        
        private System.Random random = new();
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
        public bool applyForce = true;
        public float strengthCoef = 0.0015f;
        public float strengthOffSpeed = 50.0f;
        public float strengthOnSpeed = 70.0f;
        public Quaternion targetDirection; // wind noise direction

        //mean/variance of force currentStrength setpt
        public RandomDistributionParam randomStrength = new(30f, 20f);

        //variance of force vector magnitude during the pulse around the setpt
        public float strengthHold = 1000.0f;

        //mean/variance time between pulses
        public RandomDistributionParam randomPulsePeriod = new(7f, 5f); // seconds

        //mean/variance duration of pulses
        public RandomDistributionParam randomPulseDuration = new(10f, 2f); // seconds

        //mean/variance amount of time for each motion direction target
        public RandomDistributionParam randomMotionPeriod = new(8f, 3f); //seconds
       
        //mean/variance speed of wind vector rotation
        public RandomDistributionParam randomWindChangeSpeed = new(0.05f, 0.01f);
        
	
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
            if (applyForce)
                body.AddForce(ray * strengthCoef, ForceMode.Impulse);
            
            Debug.DrawRay(body.position, ray, Color.green);
        }
        
        private void InitWindPulsing()
        {
            pulseTimer = 0.0f; //reset
            pulsePeriod = SamplePositive(randomPulsePeriod);
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
                pulseDuration = SamplePositive(randomPulseDuration);
                baseStrength = SamplePositive(randomStrength);
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
                var targetStrength = Sample(baseStrength, strengthHold);
                    
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
            motionPeriod = SamplePositive(randomMotionPeriod);
            windChangeSpeed = SamplePositive(randomWindChangeSpeed);
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
        
        private float Sample(RandomDistributionParam param)
        {
            return NextGaussianDouble() * Mathf.Sqrt(param.variance) + param.mean;
        }
        
        private float Sample(float mean, float variance)
        {
            return NextGaussianDouble() * Mathf.Sqrt(variance) + mean;
        }

        private float SamplePositive(RandomDistributionParam param)
        {
            return Mathf.Abs(Sample(param));
        }

        private float NextGaussianDouble()
        {
            float u, S;
            do
            {
                var v = 2.0f * (float)random.NextDouble() - 1.0f;
                u = 2.0f * (float)random.NextDouble() - 1.0f;
                S = u*u + v*v;
            }
            while (S >= 1.0f);
            var fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
            return u * fac;
        }
    }
}
