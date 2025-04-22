using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;


namespace RL
{
    // this script must start before RenderTextureSensorComponent so it can set properties for the sensor
    
    /// <summary>
    /// Helper script for managing <see cref="RenderTexture"/> for given <see cref="CameraSensorComponent"/>.
    /// </summary>
    /// <remarks>Will automatically create textures and assign them to the sensor and the camera.</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(CameraSensorComponent))]
    public class DepthCameraHelper : MonoBehaviour
    {
        private CameraSensorComponent _sensor;
        private Camera                _depthCamera;
        private RawImage              _debugImage;

        [Tooltip(
            "RenderTexture will be created with RenderTexture.GetTemporary() method. Otherwise constructor will be used."
        )]
        public bool useTemporaryTexture;

        [Tooltip("RenderTexture color format.")]
        public GraphicsFormat colorFormat = GraphicsFormat.R8_SNorm;

        [Tooltip("RenderTexture filter mode.")]
        public FilterMode filterMode = FilterMode.Bilinear;
        
        [Tooltip("Use this canvas to display RenderTexture. Will automatically add RawImage to the canvas.")]
        public Canvas debugCanvas;

        private void OnEnable()
        {
            _depthCamera = GetComponent<Camera>();
            _sensor = gameObject.GetComponent<CameraSensorComponent>();

            if (useTemporaryTexture)
            {
                _depthCamera.targetTexture = RenderTexture.GetTemporary(_sensor.Width, _sensor.Height, 1, colorFormat);
                _depthCamera.targetTexture.name = $"DepthSensorTexture_{gameObject.GetInstanceID():x8}";
                _depthCamera.targetTexture.filterMode = filterMode;
            }
            else
            {
                _depthCamera.targetTexture = new RenderTexture(_sensor.Width, _sensor.Height, 1, colorFormat);
                _depthCamera.targetTexture.name = $"DepthSensorTexture_{gameObject.GetInstanceID():x8}";
                _depthCamera.targetTexture.filterMode = filterMode;
                _depthCamera.targetTexture.Create();
            }

            if (debugCanvas != null)
            {
                _debugImage = debugCanvas.gameObject.AddComponent<RawImage>();
                _debugImage.texture = _depthCamera.targetTexture;
            }
        }

        private void OnDisable()
        {
            _depthCamera.targetTexture.Release();
            //sensor.Dispose();
            Destroy(_depthCamera.targetTexture);
            //Destroy(sensor);
            Destroy(_debugImage);
        }
    }
}