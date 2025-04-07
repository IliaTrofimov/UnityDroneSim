using UnityEngine;
using Unity.MLAgents.Sensors;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;


namespace RL
{
    /// <summary>
    /// Helper script for managing <see cref="RenderTexture"/> for given <see cref="RenderTextureSensorComponent"/>.
    /// </summary>
    /// <remarks>Will automatically create textures and assign them to the sensor and the camera.</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class DepthCameraHelper : MonoBehaviour
    {
        private Camera depthCamera;
        private RenderTextureSensorComponent sensor;

        [Tooltip("RenderTexture will be created with RenderTexture.GetTemporary() method. Otherwise constructor will be used.")]
        public bool useTemporaryTexture;
        
        [Tooltip("RenderTexture color format.")]
        public GraphicsFormat colorFormat = GraphicsFormat.R8_SNorm;

        [Tooltip("RenderTexture width. Keep it as small as possible.")]
        [Range(8, 1024)] public int width = 86;
        
        [Tooltip("RenderTexture height. Keep it as small as possible.")]
        [Range(8, 1024)] public int height = 86;

        [Tooltip("Use this canvas to display RenderTexture. Will automatically add RawImage to the canvas.")]
        public Canvas debugCanvas;
        private RawImage debugImage;

        private void OnEnable()
        {
            depthCamera = GetComponent<Camera>();
            
            if (useTemporaryTexture)
            {
                depthCamera.targetTexture = RenderTexture.GetTemporary(width, height, 1, colorFormat);
                /*Debug.LogFormat(
                    "[{0}] camera '{1}:{2:x8}' - renderTexture '{3:x8}' {4}x{5} {6} (Temporary)",
                    gameObject.name,
                    depthCamera.name, depthCamera.GetInstanceID(),
                    depthCamera.targetTexture.GetInstanceID(),
                    width, height, colorFormat);*/
            }
            else
            {
                depthCamera.targetTexture = new RenderTexture(width, height, 1, colorFormat);
                depthCamera.targetTexture.Create();
                /*Debug.LogFormat(
                    "[{0}] camera '{1}:{2:x8}' - renderTexture '{3:x8}' {4}x{5} {6} (Permanent)",
                    gameObject.name,
                    depthCamera.name, depthCamera.GetInstanceID(),
                    depthCamera.targetTexture.GetInstanceID(),
                    width, height, colorFormat);*/
            }
            
            sensor = gameObject.AddComponent<RenderTextureSensorComponent>();
            sensor.Grayscale = true;
            sensor.CompressionType = SensorCompressionType.PNG;
            sensor.SensorName = "DepthMap";
            sensor.RenderTexture = depthCamera.targetTexture;

            if (debugCanvas)
            {
                debugImage = debugCanvas.gameObject.AddComponent<RawImage>();
                debugImage.texture = depthCamera.targetTexture;
            }
        }

        private void OnDisable()
        {
            depthCamera.targetTexture.Release();
            sensor.Dispose();
            //Destroy(depthCamera.targetTexture);
            Destroy(sensor);
            Destroy(debugImage);
        }
    }
}