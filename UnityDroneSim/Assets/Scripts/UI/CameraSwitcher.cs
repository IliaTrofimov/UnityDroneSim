using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace UI
{
    [Serializable]
    public class CameraSwitcher
    {
        private int _currentCameraIndex;
        
        [Tooltip("List of all cameras.")]
        [SerializeField]
        private List<Camera> cameras = new();
        
        public int CamerasCount => cameras.Count;

        public int CurrentCameraIndex
        {
            get => _currentCameraIndex;
            set
            {
                if (value < 0) _currentCameraIndex = cameras.Count - 1;
                else if (value >= cameras.Count) _currentCameraIndex = 0;
                else _currentCameraIndex = value;
            }
        }
        
        /// <summary>Try to enable camera with given index. </summary>
        public bool TrySetCamera(int index)
        {
            if (index < 0 || index >= cameras.Count)
                return false;
            
            _currentCameraIndex = index;
            EnsureEnabled();
            return true;
        }
        
        /// <summary>Ensure that only one camera is enabled.</summary>
        public void EnsureEnabled()
        {
            if (cameras.Count == 0) return;
            
            foreach (var camera in cameras)
                camera.enabled = false;
            cameras[_currentCameraIndex].enabled = true;
        }
        
        /// <summary>Enable next camera.</summary>
        public void SwitchNext()
        {
            if (cameras.Count <= 1) return;

            cameras[_currentCameraIndex].enabled = false;
            
            _currentCameraIndex++;
            if (_currentCameraIndex == cameras.Count)
                _currentCameraIndex = 0;
            
            cameras[_currentCameraIndex].enabled = true;
        }
        
        /// <summary>Enable previous camera.</summary>
        public void SwitchPrevious()
        {
            if (cameras.Count <= 1) return;
            
            cameras[_currentCameraIndex].enabled = false;
            
            _currentCameraIndex--;
            if (_currentCameraIndex == -1)
                _currentCameraIndex = cameras.Count - 1;
            
            cameras[_currentCameraIndex].enabled = true;
        }
    }
}