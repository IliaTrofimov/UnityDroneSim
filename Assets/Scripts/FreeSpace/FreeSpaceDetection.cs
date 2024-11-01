using Unity.Collections;
using UnityEngine;


namespace FreeSpace
{
    public class FreeSpaceDetection : MonoBehaviour
    {
        private NativeArray<RaycastCommand> commands;

        [Range(100, 5000)] 
        public int numHorizontalPoints;
        
        [Range(10, 1000)] 
        public int numBins;
        
        [Range(45, 360)] 
        public float fovDegrees; 
        
        [Range(1, 1000)] 
        public float maxDist;
    
        public void Start() 
        {
            if (Mathf.Abs(fovDegrees) < 1e-8)
                fovDegrees = UnityEngine.Camera.main!.fieldOfView;
        
            if (numBins > numHorizontalPoints)
                throw new UnityException("More bins than points!");
            if (numHorizontalPoints % numBins != 0)
                throw new UnityException("numHorizontalPoints must be a multiple of numBins!");
        
            commands = new NativeArray<RaycastCommand>(numHorizontalPoints, Allocator.Temp);
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
            handle.Complete();
        
            //set to zero
            var output = new float[numBins];
            for (var i = 0; i < numBins; i++)
                output[i] = 0.0f;
        
            //normalize and place in buckets
            var elsPerBin = results.Length / numBins;
            for (var i = 0; i < results.Length; i++)
            {
                var bin = i / elsPerBin;
                output[bin] += results[i].distance / elsPerBin; // average each bin over entries
            }

            var totalSum = 0.0f;
            for (var i = 0; i < output.Length; i++)
                totalSum += output[i];

            var normOutput = new float[numBins + 1];
            for (var i = 0; i < output.Length; i++)
                normOutput[i] = output[i] / totalSum;
        
            normOutput[numBins] = totalSum;
            results.Dispose();
            return normOutput;
        }
        
        private void OnDestroy()
        {
            commands.Dispose();
        }
    }
}
