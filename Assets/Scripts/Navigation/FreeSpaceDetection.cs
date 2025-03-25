using System;
using Unity.Collections;
using UnityEngine;

namespace Navigation
{
    public class FreeSpaceDetection : MonoBehaviour
    {
        private NativeArray<RaycastCommand> commands;
        private float[] rayDistances;
        
        [Range(100, 5000)] public int numHorizontalPoints = 100;
        [Range(10, 1000)] public int numBins = 50;
        [Range(45, 360)] public float fovDegrees = 90f;
        [Range(1, 1000)] public float maxDist = 100f;
        
        public bool showRays = false;
    
        
        public void Start() 
        {
            if (Mathf.Abs(fovDegrees) < 1e-8)
                fovDegrees = Camera.main!.fieldOfView;
        
            if (numBins > numHorizontalPoints)
                throw new UnityException("More bins than points!");
            if (numHorizontalPoints % numBins != 0)
                throw new UnityException("numHorizontalPoints must be a multiple of numBins!");
        
            commands = new NativeArray<RaycastCommand>(numHorizontalPoints, Allocator.Persistent);
        }

        public void FixedUpdate()
        {
            BatchRaycast();
        }

        public float[] BatchRaycast()
        {
            var results = new NativeArray<RaycastHit>(numHorizontalPoints, Allocator.Temp);
            var origin = transform.localPosition;

            for (var i = 0; i < numHorizontalPoints; i++)
            {
                var theta = -(fovDegrees / 2.0f) + i * fovDegrees / numHorizontalPoints;
                var angleRot = Quaternion.AngleAxis(theta, Vector3.up);
                var direction = transform.rotation * angleRot * Vector3.forward;
                commands[i] = new RaycastCommand(origin, direction, new QueryParameters(), maxDist);
            }
      
            var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
            
            if (rayDistances == null) 
                rayDistances = new float[numBins + 1];
            else
                Array.Fill(rayDistances, 0);
            
            handle.Complete();
            
            //normalize and place in buckets
            var elsPerBin = results.Length / numBins;
            for (var i = 0; i < results.Length; i++)
            {
                var bin = i / elsPerBin;
                rayDistances[bin] += results[i].distance / elsPerBin;
            }
            
            results.Dispose();

            var totalSum = 0.0f;
            for (var i = 0; i < rayDistances.Length; i++)
                totalSum += rayDistances[i];
            
            for (var i = 0; i < rayDistances.Length; i++)
                rayDistances[i] /= totalSum;
            
            rayDistances[numBins] = totalSum;
            return rayDistances;
        }
        

        private void OnDestroy()
        {
            commands.Dispose();
        }
    }
}
