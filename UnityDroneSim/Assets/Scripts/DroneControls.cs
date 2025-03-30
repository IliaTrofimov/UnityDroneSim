//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/DroneControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Inputs
{
    public partial class @DroneControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @DroneControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""DroneControls"",
    ""maps"": [
        {
            ""name"": ""Default"",
            ""id"": ""ebb85e28-803e-45b0-9984-214fde76438e"",
            ""actions"": [
                {
                    ""name"": ""Throttle"",
                    ""type"": ""Value"",
                    ""id"": ""deb46857-f2db-4c85-8320-e906f96fddc4"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": ""AxisDeadzone"",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Rotation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""9df7ec8b-e7d7-4171-8a0b-4c23f49bce04"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""FullStabilization"",
                    ""type"": ""Button"",
                    ""id"": ""17074d39-bec0-47cf-9afd-4b471da24090"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraMode"",
                    ""type"": ""Button"",
                    ""id"": ""22e3676f-bf05-4b15-aeeb-70c3ca69982c"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ControlsPanel"",
                    ""type"": ""Button"",
                    ""id"": ""4de994df-a8ba-440f-9e42-e10e2927d234"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MovementPanel"",
                    ""type"": ""Button"",
                    ""id"": ""6f670368-e3b1-4d27-82f7-ce9317ccd27d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""NavigationPanel"",
                    ""type"": ""Button"",
                    ""id"": ""d0080f43-f41b-4b0e-99f0-ad8404552ef4"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Repair"",
                    ""type"": ""Button"",
                    ""id"": ""dacb879a-4c2e-4de6-ba8a-95cd36bf386a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""EnableDrone"",
                    ""type"": ""Button"",
                    ""id"": ""a41fe771-ae36-4a0b-8be7-a13b6d8417d6"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""3D Vector"",
                    ""id"": ""a67324f5-f44b-4214-8a1e-bac10a979f14"",
                    ""path"": ""3DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Right"",
                    ""id"": ""e47b3696-96e3-4511-8d53-8a3db837366e"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Left"",
                    ""id"": ""26854c33-f646-4986-a18c-d44482976039"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Down"",
                    ""id"": ""06306337-aee2-45da-8987-06a7f94d7ef5"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Up"",
                    ""id"": ""c15fad8a-201f-4147-aedf-eb615a21706b"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""20938c4a-3cfc-4b84-b42a-db50d9b68f09"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""64477344-c0da-4df1-9754-e3c7d9a71650"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Forward"",
                    ""id"": ""628aa39f-6fed-4f0d-bc1b-183ec2c47f2c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Backward"",
                    ""id"": ""419bfd4d-0749-4302-9542-b79fc87af620"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""59dcc983-175e-47c6-b32f-b6c687943684"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FullStabilization"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""6de315e5-99e1-429e-ac2f-f6f844632fa6"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""76d76087-e4f5-4b93-90b7-43627b4d853d"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""704bd8df-900f-49f8-9296-383dce542b13"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""cc3ad2ae-cd41-4b56-a9a7-b31b9df7d87f"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e6610e55-cd8d-4339-9c61-8413d388288b"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ControlsPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f19620c0-4494-4837-8cbf-7d235b7bb505"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MovementPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""07a0ef8a-df02-4464-9846-2e597d0381c2"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NavigationPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1a078a50-b853-4cb9-82dd-597c810b8c5c"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Repair"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7fc95f14-77ba-4a46-bcab-6ff72745789b"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EnableDrone"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Default
            m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
            m_Default_Throttle = m_Default.FindAction("Throttle", throwIfNotFound: true);
            m_Default_Rotation = m_Default.FindAction("Rotation", throwIfNotFound: true);
            m_Default_FullStabilization = m_Default.FindAction("FullStabilization", throwIfNotFound: true);
            m_Default_CameraMode = m_Default.FindAction("CameraMode", throwIfNotFound: true);
            m_Default_ControlsPanel = m_Default.FindAction("ControlsPanel", throwIfNotFound: true);
            m_Default_MovementPanel = m_Default.FindAction("MovementPanel", throwIfNotFound: true);
            m_Default_NavigationPanel = m_Default.FindAction("NavigationPanel", throwIfNotFound: true);
            m_Default_Repair = m_Default.FindAction("Repair", throwIfNotFound: true);
            m_Default_EnableDrone = m_Default.FindAction("EnableDrone", throwIfNotFound: true);
        }

        ~@DroneControls()
        {
            UnityEngine.Debug.Assert(!m_Default.enabled, "This will cause a leak and performance issues, DroneControls.Default.Disable() has not been called.");
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Default
        private readonly InputActionMap m_Default;
        private List<IDefaultActions> m_DefaultActionsCallbackInterfaces = new List<IDefaultActions>();
        private readonly InputAction m_Default_Throttle;
        private readonly InputAction m_Default_Rotation;
        private readonly InputAction m_Default_FullStabilization;
        private readonly InputAction m_Default_CameraMode;
        private readonly InputAction m_Default_ControlsPanel;
        private readonly InputAction m_Default_MovementPanel;
        private readonly InputAction m_Default_NavigationPanel;
        private readonly InputAction m_Default_Repair;
        private readonly InputAction m_Default_EnableDrone;
        public struct DefaultActions
        {
            private @DroneControls m_Wrapper;
            public DefaultActions(@DroneControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Throttle => m_Wrapper.m_Default_Throttle;
            public InputAction @Rotation => m_Wrapper.m_Default_Rotation;
            public InputAction @FullStabilization => m_Wrapper.m_Default_FullStabilization;
            public InputAction @CameraMode => m_Wrapper.m_Default_CameraMode;
            public InputAction @ControlsPanel => m_Wrapper.m_Default_ControlsPanel;
            public InputAction @MovementPanel => m_Wrapper.m_Default_MovementPanel;
            public InputAction @NavigationPanel => m_Wrapper.m_Default_NavigationPanel;
            public InputAction @Repair => m_Wrapper.m_Default_Repair;
            public InputAction @EnableDrone => m_Wrapper.m_Default_EnableDrone;
            public InputActionMap Get() { return m_Wrapper.m_Default; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
            public void AddCallbacks(IDefaultActions instance)
            {
                if (instance == null || m_Wrapper.m_DefaultActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_DefaultActionsCallbackInterfaces.Add(instance);
                @Throttle.started += instance.OnThrottle;
                @Throttle.performed += instance.OnThrottle;
                @Throttle.canceled += instance.OnThrottle;
                @Rotation.started += instance.OnRotation;
                @Rotation.performed += instance.OnRotation;
                @Rotation.canceled += instance.OnRotation;
                @FullStabilization.started += instance.OnFullStabilization;
                @FullStabilization.performed += instance.OnFullStabilization;
                @FullStabilization.canceled += instance.OnFullStabilization;
                @CameraMode.started += instance.OnCameraMode;
                @CameraMode.performed += instance.OnCameraMode;
                @CameraMode.canceled += instance.OnCameraMode;
                @ControlsPanel.started += instance.OnControlsPanel;
                @ControlsPanel.performed += instance.OnControlsPanel;
                @ControlsPanel.canceled += instance.OnControlsPanel;
                @MovementPanel.started += instance.OnMovementPanel;
                @MovementPanel.performed += instance.OnMovementPanel;
                @MovementPanel.canceled += instance.OnMovementPanel;
                @NavigationPanel.started += instance.OnNavigationPanel;
                @NavigationPanel.performed += instance.OnNavigationPanel;
                @NavigationPanel.canceled += instance.OnNavigationPanel;
                @Repair.started += instance.OnRepair;
                @Repair.performed += instance.OnRepair;
                @Repair.canceled += instance.OnRepair;
                @EnableDrone.started += instance.OnEnableDrone;
                @EnableDrone.performed += instance.OnEnableDrone;
                @EnableDrone.canceled += instance.OnEnableDrone;
            }

            private void UnregisterCallbacks(IDefaultActions instance)
            {
                @Throttle.started -= instance.OnThrottle;
                @Throttle.performed -= instance.OnThrottle;
                @Throttle.canceled -= instance.OnThrottle;
                @Rotation.started -= instance.OnRotation;
                @Rotation.performed -= instance.OnRotation;
                @Rotation.canceled -= instance.OnRotation;
                @FullStabilization.started -= instance.OnFullStabilization;
                @FullStabilization.performed -= instance.OnFullStabilization;
                @FullStabilization.canceled -= instance.OnFullStabilization;
                @CameraMode.started -= instance.OnCameraMode;
                @CameraMode.performed -= instance.OnCameraMode;
                @CameraMode.canceled -= instance.OnCameraMode;
                @ControlsPanel.started -= instance.OnControlsPanel;
                @ControlsPanel.performed -= instance.OnControlsPanel;
                @ControlsPanel.canceled -= instance.OnControlsPanel;
                @MovementPanel.started -= instance.OnMovementPanel;
                @MovementPanel.performed -= instance.OnMovementPanel;
                @MovementPanel.canceled -= instance.OnMovementPanel;
                @NavigationPanel.started -= instance.OnNavigationPanel;
                @NavigationPanel.performed -= instance.OnNavigationPanel;
                @NavigationPanel.canceled -= instance.OnNavigationPanel;
                @Repair.started -= instance.OnRepair;
                @Repair.performed -= instance.OnRepair;
                @Repair.canceled -= instance.OnRepair;
                @EnableDrone.started -= instance.OnEnableDrone;
                @EnableDrone.performed -= instance.OnEnableDrone;
                @EnableDrone.canceled -= instance.OnEnableDrone;
            }

            public void RemoveCallbacks(IDefaultActions instance)
            {
                if (m_Wrapper.m_DefaultActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IDefaultActions instance)
            {
                foreach (var item in m_Wrapper.m_DefaultActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_DefaultActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public DefaultActions @Default => new DefaultActions(this);
        public interface IDefaultActions
        {
            void OnThrottle(InputAction.CallbackContext context);
            void OnRotation(InputAction.CallbackContext context);
            void OnFullStabilization(InputAction.CallbackContext context);
            void OnCameraMode(InputAction.CallbackContext context);
            void OnControlsPanel(InputAction.CallbackContext context);
            void OnMovementPanel(InputAction.CallbackContext context);
            void OnNavigationPanel(InputAction.CallbackContext context);
            void OnRepair(InputAction.CallbackContext context);
            void OnEnableDrone(InputAction.CallbackContext context);
        }
    }
}
