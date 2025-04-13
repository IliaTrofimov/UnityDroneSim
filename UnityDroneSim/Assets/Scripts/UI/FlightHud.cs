using System;
using System.Collections.Generic;
using System.Globalization;
using Drone;
using Exceptions;
using Inputs;
using Navigation;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;


namespace UI
{
    [DisallowMultipleComponent]
    public class FlightHud : MonoBehaviour
    {
        private bool _elementsAreInitialized;

        private Label  _lblDroneEnabled;
        private Button _btnSwitchView;

        private Foldout _foldNavigation;
        private Foldout _foldMovement;
        private Foldout _foldControls;

        private Label _lblPosition;
        private Label _lblWaypointIndex;
        private Label _lblWaypointPosition;
        private Label _lblWaypointDistance;
        private Label _lblWaypointDirection;

        private Label _lblVelocity;
        private Label _lblYSpdTarget;
        private Label _lblYSpdActual;
        private Label _lblPitchTarget;
        private Label _lblPitchActual;
        private Label _lblYawTarget;
        private Label _lblYawActual;
        private Label _lblRollTarget;
        private Label _lblRollActual;

        private Toggle _tglStabilization;
        private Label  _lblStatus;
        private Slider _sldThrottle;
        private Slider _sldPitch;
        private Slider _sldYaw;
        private Slider _sldRoll;

        private const float MIN_DIRECTION_CHANGE     = 0.1F;
        private const float FLOAT_CHANGE_EPS_PRECISE = 0.0001f;
        private const float FLOAT_CHANGE_EPS         = 0.001f;

        private readonly TrackedVector3            _valPosition          = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedVector3            _valWaypointPos       = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedObject<int>        _valWaypointNumber    = new();
        private readonly TrackedObject<int>        _valWaypointsCount    = new();
        private readonly TrackedFloat              _valWaypointDist      = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<Vector3Int> _valWaypointDirection = new();
        private readonly TrackedObject<bool>       _valDroneFailure      = new(false);
        private readonly TrackedObject<bool>       _valDroneEnabled      = new(false);

        private readonly TrackedVector3 _valVelocity    = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedFloat   _valYSpdTarget  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYSpdActual  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valPitchTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valPitchActual = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYawTarget   = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYawActual   = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valRollTarget  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valRollActual  = new(FLOAT_CHANGE_EPS);

        private readonly TrackedFloat        _valThrottle      = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valPitch         = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valYaw           = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valRoll          = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<bool> _valStabilization = new();

        [Header("UI")] public UIDocument hudUIDocument;

        public  bool          startExpanded         = true;
        public  bool          enableMovementPanel   = true;
        public  bool          enableControlsPanel   = true;
        public  bool          enableNavigationPanel = true;
        private DroneControls _controls;

        [Header("Cameras")] public bool isFirstPersonView;

        public Camera cameraFpv;
        public Camera cameraTpv;

        [Header("Target")] public QuadcopterComputer drone;

        public  WaypointNavigator     navigator;
        private DroneInputsController _inputsController;
        private Rigidbody             _droneRigidbody;
        private DroneStateManager     _droneStateManager;

        private void OnEnable()
        {
            _controls = new DroneControls();
            _controls.Enable();

            ExceptionHelper.ThrowIfComponentIsMissing(this, hudUIDocument, nameof(hudUIDocument));
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));

            _inputsController = drone.GetComponent<DroneInputsController>();
            _droneRigidbody = drone.Rigidbody;
            _droneStateManager = drone.gameObject.GetComponent<DroneStateManager>();

            //if (!navigator) navigator = GetComponent<WaypointNavigator>();

            if (cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = isFirstPersonView;
                cameraTpv.enabled = !isFirstPersonView;
                cameraFpv.targetDisplay = 0;
                cameraTpv.targetDisplay = 0;
            }

            InitializeElements();
        }

        private void OnDisable()
        {
            ClearValuesChangedEventHandlers();
            _controls.Disable();
        }

        private void Start()
        {
            if (!_elementsAreInitialized) return;

            _foldControls.value = startExpanded;
            _foldMovement.value = startExpanded;
            _foldNavigation.value = startExpanded;
            _valDroneEnabled.Value = drone?.enabled ?? false;
        }

        private void OnValidate()
        {
            if (cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = isFirstPersonView;
                cameraTpv.enabled = !isFirstPersonView;
            }

            if (_elementsAreInitialized)
            {
                _foldControls.visible = enableControlsPanel;
                _foldMovement.visible = enableMovementPanel;
                _foldNavigation.visible = enableNavigationPanel;
            }
        }

        private void Update()
        {
            if (!enabled) return;

            if (drone && _controls.Default.EnableDrone.WasPressedThisFrame())
            {
                drone.enabled = !drone.enabled;
                _valDroneEnabled.Value = drone.enabled;
            }

            if (_controls.Default.CameraMode.WasPressedThisFrame() && cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = !cameraFpv.enabled;
                cameraTpv.enabled = !cameraTpv.enabled;
                isFirstPersonView = cameraFpv.enabled;
            }

            if (!_elementsAreInitialized) InitializeElements();

            if (UpdateControlsPanel())
            {
                UpdateMovementPanel();
                UpdateNavigationPanel();
            }
        }

        private bool UpdateControlsPanel()
        {
            if (!drone)
            {
                _foldNavigation.value = false;
                _foldNavigation.enabledSelf = false;
                _foldControls.value = false;
                _foldControls.enabledSelf = false;
                _foldMovement.value = false;
                _foldMovement.enabledSelf = false;
                return false;
            }

            _foldControls.enabledSelf = true;
            if (_controls.Default.ControlsPanel.WasPressedThisFrame())
                _foldControls.value = !_foldControls.value;

            _valThrottle.Value = _inputsController.throttle;
            _valPitch.Value = _inputsController.pitch;
            _valYaw.Value = _inputsController.yaw;
            _valRoll.Value = _inputsController.roll;
            _valStabilization.Value = _inputsController.IsFullStabilization();

            if (_droneStateManager)
            {
                _valDroneFailure.Value = _droneStateManager.AnyMotorsDestroyed;
                if (_droneStateManager.AnyMotorsDestroyed && _controls.Default.Repair.WasPressedThisFrame())
                    _droneStateManager.RepairAllMotors();
            }

            return true;
        }

        private bool UpdateMovementPanel()
        {
            if (_droneRigidbody)
            {
                _foldMovement.enabledSelf = true;
                if (_controls.Default.MovementPanel.WasPressedThisFrame())
                    _foldMovement.value = !_foldMovement.value;

                _valVelocity.Value = _droneRigidbody.linearVelocity;
                _valYSpdTarget.Value = _inputsController.throttle * drone.MaxLiftSpeed;
                _valYSpdActual.Value = _droneRigidbody.linearVelocity.y;
                _valPitchTarget.Value = _inputsController.pitch * drone.MaxPitchAngle;
                _valPitchActual.Value = drone.transform.rotation.eulerAngles.x;
                _valYawTarget.Value = _inputsController.yaw * drone.MaxYawSpeed;
                _valYawActual.Value = _droneRigidbody.YawVelocity();
                _valRollTarget.Value = _inputsController.pitch * drone.MaxRollAngle;
                _valRollActual.Value = drone.transform.rotation.eulerAngles.z;

                return true;
            }

            _foldMovement.value = false;
            _foldMovement.enabledSelf = false;
            return false;
        }

        private bool UpdateNavigationPanel()
        {
            if (navigator && navigator.enabled)
            {
                _foldNavigation.enabledSelf = true;
                if (_controls.Default.NavigationPanel.WasPressedThisFrame())
                    _foldNavigation.value = !_foldNavigation.value;

                _valPosition.Value = drone.transform.position;
                _valWaypointsCount.Value = navigator.WaypointsCount;

                if (navigator.IsFinished || !navigator.CurrentWaypoint.HasValue)
                {
                    _valWaypointNumber.Value = navigator.WaypointsCount;
                    _valWaypointPos.Value = Vector3.zero;
                    _valWaypointDist.Value = float.NaN;
                    _valWaypointDirection.Value = Vector3Int.zero;
                }
                else
                {
                    _valWaypointNumber.Value = navigator.CurrentWaypointIndex + 1;
                    _valWaypointPos.Value = navigator.CurrentWaypoint.Value.position;
                    _valWaypointDist.Value =
                        (drone.transform.position - navigator.CurrentWaypoint.Value.position).magnitude;
                }

                return true;
            }

            _foldNavigation.value = false;
            _foldNavigation.enabledSelf = false;
            return false;
        }


        private void InitializeElements()
        {
            _foldNavigation  = hudUIDocument.rootVisualElement.Q<Foldout>(nameof(_foldNavigation));
            _foldMovement    = hudUIDocument.rootVisualElement.Q<Foldout>(nameof(_foldMovement));
            _foldControls    = hudUIDocument.rootVisualElement.Q<Foldout>(nameof(_foldControls));
            _lblDroneEnabled = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblDroneEnabled));
            _btnSwitchView   = hudUIDocument.rootVisualElement.Q<Button>(nameof(_btnSwitchView));

            _lblPosition          = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblPosition));
            _lblWaypointIndex     = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblWaypointIndex));
            _lblWaypointPosition  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblWaypointPosition));
            _lblWaypointDistance  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblWaypointDistance));
            _lblWaypointDirection = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblWaypointDirection));

            _lblVelocity    = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblVelocity));
            _lblYSpdTarget  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblYSpdTarget));
            _lblYSpdActual  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblYSpdActual));
            _lblPitchTarget = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblPitchTarget));
            _lblPitchActual = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblPitchActual));
            _lblYawTarget   = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblYawTarget));
            _lblYawActual   = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblYawActual));
            _lblRollTarget  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblRollTarget));
            _lblRollActual  = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblRollActual));

            _tglStabilization = hudUIDocument.rootVisualElement.Q<Toggle>(nameof(_tglStabilization));
            _lblStatus        = hudUIDocument.rootVisualElement.Q<Label>(nameof(_lblStatus));
            _sldThrottle      = hudUIDocument.rootVisualElement.Q<Slider>(nameof(_sldThrottle));
            _sldPitch         = hudUIDocument.rootVisualElement.Q<Slider>(nameof(_sldPitch));
            _sldYaw           = hudUIDocument.rootVisualElement.Q<Slider>(nameof(_sldYaw));
            _sldRoll          = hudUIDocument.rootVisualElement.Q<Slider>(nameof(_sldRoll));

            InitializeValuesChangedEventHandlers();
            SetTemplatedLabels();

            _elementsAreInitialized = true;
        }

        private void SetTemplatedLabels()
        {
            try
            {
                _foldControls.text = string.Format(_foldControls.text,
                    _controls.Default.ControlsPanel.controls[0].displayName
                );

                _foldNavigation.text = string.Format(_foldNavigation.text,
                    _controls.Default.NavigationPanel.controls[0].displayName
                );

                _foldMovement.text = string.Format(_foldMovement.text,
                    _controls.Default.MovementPanel.controls[0].displayName
                );

                _tglStabilization.text = string.Format(_tglStabilization.text,
                    _controls.Default.FullStabilization.controls[0].displayName
                );

                _lblDroneEnabled.text = string.Format(_lblDroneEnabled.text,
                    _controls.Default.EnableDrone.controls[0].displayName
                );

                _lblStatus.text = string.Format(_lblStatus.text, _controls.Default.Repair.controls[0].displayName);
                _btnSwitchView.text =
                    string.Format(_btnSwitchView.text, _controls.Default.CameraMode.controls[0].displayName);
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("Cannot update templated label: {0}", ex);
            }
        }

        private void InitializeValuesChangedEventHandlers()
        {
            _valPosition.ValueChanged += PositionChangeHandler;
            _valWaypointPos.ValueChanged += WaypointPositionChangeHandler;
            _valWaypointDist.ValueChanged += WaypointDistanceChangeHandler;
            _valWaypointNumber.ValueChanged += WaypointNumberChangeHandler;
            _valWaypointsCount.ValueChanged += WaypointsCountChangeHandler;
            _valWaypointDirection.ValueChanged += WaypointDirectionChangeHandler;

            _valVelocity.ValueChanged += VelocityChangeHandler;
            _valYSpdActual.ValueChanged += YSpdActualChangeHandler;
            _valYSpdTarget.ValueChanged += YSpdTargetChangeHandler;
            _valPitchActual.ValueChanged += PitchActualChangeHandler;
            _valPitchTarget.ValueChanged += PitchTargetChangeHandler;
            _valYawActual.ValueChanged += YawActualChangeHandler;
            _valYawTarget.ValueChanged += YawTargetChangeHandler;
            _valRollActual.ValueChanged += RollActualChangeHandler;
            _valRollTarget.ValueChanged += RollTargetChangeHandler;

            _valThrottle.ValueChanged += ThrottleChangeHandler;
            _valPitch.ValueChanged += PitchChangeHandler;
            _valYaw.ValueChanged += YawChangeHandler;
            _valRoll.ValueChanged += RollChangeHandler;
            _valStabilization.ValueChanged += StabilizationChangeHandler;
            _valDroneFailure.ValueChanged += DroneFailureChangeHandler;
            _valDroneEnabled.ValueChanged += DroneEnabledChangeHandler;
        }

        private void ClearValuesChangedEventHandlers()
        {
            _valPosition.ValueChanged -= PositionChangeHandler;
            _valWaypointPos.ValueChanged -= WaypointPositionChangeHandler;
            _valWaypointDist.ValueChanged -= WaypointDistanceChangeHandler;
            _valWaypointNumber.ValueChanged -= WaypointNumberChangeHandler;
            _valWaypointsCount.ValueChanged -= WaypointsCountChangeHandler;
            _valWaypointDirection.ValueChanged -= WaypointDirectionChangeHandler;

            _valVelocity.ValueChanged -= VelocityChangeHandler;
            _valYSpdActual.ValueChanged -= YSpdActualChangeHandler;
            _valYSpdTarget.ValueChanged -= YSpdTargetChangeHandler;
            _valPitchActual.ValueChanged -= PitchTargetChangeHandler;
            _valPitchTarget.ValueChanged -= PitchActualChangeHandler;
            _valYawActual.ValueChanged -= YawActualChangeHandler;
            _valYawTarget.ValueChanged -= YawTargetChangeHandler;
            _valRollActual.ValueChanged -= RollTargetChangeHandler;
            _valRollTarget.ValueChanged -= RollActualChangeHandler;

            _valThrottle.ValueChanged -= ThrottleChangeHandler;
            _valPitch.ValueChanged -= PitchChangeHandler;
            _valYaw.ValueChanged -= YawChangeHandler;
            _valRoll.ValueChanged -= RollChangeHandler;
            _valStabilization.ValueChanged -= StabilizationChangeHandler;
            _valDroneFailure.ValueChanged -= DroneFailureChangeHandler;
            _valDroneEnabled.ValueChanged -= DroneEnabledChangeHandler;
        }

        private static void UpdateNumericLabel<T>(T value, Label targetLabel, string format = "F2")
            where T : struct, IFormattable
        {
            targetLabel.text = value.ToString(format, CultureInfo.CurrentCulture);
        }

        private static void UpdateSlider(float value, Slider targetSlider) { targetSlider.value = value; }

        private void PositionChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            UpdateNumericLabel(newValue, _lblPosition, "F3");

        private void WaypointPositionChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            UpdateNumericLabel(newValue, _lblWaypointPosition, "F3");

        private void WaypointDistanceChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblWaypointDistance);

        private void WaypointNumberChangeHandler(int newValue, int oldValue) =>
            SetWaypointIndex(newValue, _valWaypointsCount.Value);

        private void WaypointsCountChangeHandler(int newValue, int oldValue) =>
            SetWaypointIndex(_valWaypointNumber.Value, newValue);

        private void SetWaypointIndex(int current, int max)
        {
            if (max <= 0) _lblWaypointIndex.text = "NONE";
            else if (max == current) _lblWaypointIndex.text = $"{current}/{max} FINISHED";
            else _lblWaypointIndex.text = $"{current}/{max}";
        }

        private void WaypointDirectionChangeHandler(Vector3Int newValue, Vector3Int oldValue)
        {
            var sb = new List<char>(3);
            if (newValue.x != 0) sb.Add(newValue.x > 0 ? 'R' : 'L');
            if (newValue.y != 0) sb.Add(newValue.y > 0 ? 'U' : 'D');
            if (newValue.z != 0) sb.Add(newValue.z > 0 ? 'F' : 'B');
            _lblWaypointDirection.text = string.Join("|", sb);
        }

        private void VelocityChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            UpdateNumericLabel(newValue, _lblVelocity, "F3");

        private void YSpdTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYSpdTarget);

        private void YSpdActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYSpdActual);

        private void PitchTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblPitchTarget);

        private void PitchActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblPitchActual);

        private void YawTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYawTarget);

        private void YawActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYawActual);

        private void RollTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblRollTarget);

        private void RollActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblRollActual);

        private void ThrottleChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldThrottle);

        private void PitchChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldPitch);

        private void YawChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldYaw);

        private void RollChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldRoll);

        private void StabilizationChangeHandler(bool newValue, bool oldValue) => _tglStabilization.value = newValue;

        private void DroneFailureChangeHandler(bool newValue, bool previousValue)
        {
            _lblStatus.style.display = newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void DroneEnabledChangeHandler(bool newValue, bool previousValue)
        {
            _lblDroneEnabled.style.display = newValue ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}