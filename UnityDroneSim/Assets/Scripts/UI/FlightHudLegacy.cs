using System;
using Drone;
using Drone.Stability;
using Navigation;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;


namespace UI
{
    [DisallowMultipleComponent]
    public class FlightHudLegacy : MonoBehaviour
    {
        private float _collapsedHeight = 34;
        private float _screenOffset    = 2;

        private float _movementPanelWRatio = 0.48f;
        private float _movementPanelHRatio = 0.25f;
        private float _movementPanelMaxW   = 450f;
        private float _movementPanelMaxH   = 300f;

        private float _controlsPanelWRatio = 0.48f;
        private float _controlsPanelHRatio = 0.25f;
        private float _controlsPanelMaxW   = 450f;
        private float _controlsPanelMaxH   = 300f;

        private float _navigationPanelWRatio = 0.5f;
        private float _navigationPanelHRatio = 0.15f;
        private float _navigationPanelMaxW   = 700f;
        private float _navigationPanelMaxH   = 200f;

        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private bool _isMovementVisible, _isNavigationVisible, _isControlsVisible;
        private bool _stylesInitialized;

        private GUIStyle _textStyle, _boldTextStyle, _italicTextStyle, _sliderStyle, _checkBoxStyle;


        [Header("Target objects")] 
        public QuadcopterComputer drone;

        public  WaypointNavigator     navigator;
        private DroneInputsController _inputsController;

        [Header("Settings")] 
        public bool initialExpanded = true;
        public bool enableControlPanel    = true;
        public bool enableNavigationPanel = true;
        public bool enableMovementPanel   = true;

        [Header("Cameras")] 
        public bool isFirstPersonView;
        public Camera cameraFPV;
        public Camera camera3PV;


        private void OnEnable()
        {
            _isMovementVisible = _isNavigationVisible = _isControlsVisible = initialExpanded;
            if (drone) _inputsController = drone.GetComponent<DroneInputsController>();

            if (cameraFPV && camera3PV)
            {
                cameraFPV.enabled = isFirstPersonView;
                camera3PV.enabled = !isFirstPersonView;
            }
        }

        private void OnValidate() { _inputsController = drone ? drone.GetComponent<DroneInputsController>() : null; }

        private void InitStyles(out int width, out int height)
        {
            width = Screen.width;
            height = Screen.height;

            if (_stylesInitialized || width == _lastScreenWidth || height == _lastScreenHeight) return;

            _lastScreenWidth = width;
            _lastScreenHeight = height;

            _textStyle = new GUIStyle("label")
            {
                alignment = TextAnchor.LowerLeft, fontSize = GetFontSize(width, height)
            };

            _boldTextStyle = new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold };
            _italicTextStyle = new GUIStyle(_textStyle) { fontStyle = FontStyle.Italic };
            _sliderStyle = new GUIStyle(GUI.skin.verticalSlider)
            {
                padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0)
            };

            _checkBoxStyle = new GUIStyle(GUI.skin.toggle) { fontSize = _textStyle.fontSize };

            if (height > 1080)
            {
                _movementPanelMaxW = 600;
                _movementPanelMaxH = 400;
                _navigationPanelMaxW = 900;
                _navigationPanelMaxH = 350;
                _controlsPanelMaxW = 600;
                _controlsPanelMaxH = 400;
            }
            else
            {
                _movementPanelMaxW = 450;
                _movementPanelMaxH = 300;
                _navigationPanelMaxW = 700;
                _navigationPanelMaxH = 200;
                _controlsPanelMaxW = 450;
                _controlsPanelMaxH = 300;
            }
        }

        private static int GetFontSize(int screenWidth, int screenHeight)
        {
            if (screenHeight <= 500) return 12;
            if (screenHeight <= 768) return 18;
            if (screenHeight <= 1080) return 22;

            return 28;
        }

        private void OnGUI()
        {
            if (!enabled) return;

            InitStyles(out var width, out var height);

            if (enableControlPanel) ControlsBox(width, height);
            if (enableMovementPanel) MovementBox(width, height);
            if (enableNavigationPanel) NavigationBox(width, height);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) _isMovementVisible = !_isMovementVisible;

            if (Input.GetKeyDown(KeyCode.N)) _isNavigationVisible = !_isNavigationVisible;

            if (Input.GetKeyDown(KeyCode.C)) _isControlsVisible = !_isControlsVisible;

            if (Input.GetKeyDown(KeyCode.V) && cameraFPV && camera3PV)
            {
                cameraFPV.enabled = !cameraFPV.enabled;
                camera3PV.enabled = !camera3PV.enabled;
            }
        }

        private void ControlsBox(int w, int h)
        {
            var boxW = Math.Min(w * _controlsPanelWRatio, _controlsPanelMaxW);
            var boxH = _isControlsVisible
                ? Math.Min(h * _controlsPanelHRatio, _controlsPanelMaxH)
                : _collapsedHeight;

            GUILayout.BeginArea(new Rect(w - boxW - _screenOffset, h - boxH - _screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("CONTROLS", _boldTextStyle);
                if (GUILayout.Button(_isControlsVisible ? "x" : "+"))
                    _isControlsVisible = !_isControlsVisible;
            }

            if (!_isControlsVisible)
            {
                GUILayout.EndArea();
                return;
            }

            if (!_inputsController)
            {
                GUILayout.Label($"{nameof(QuadcopterComputer)} with {nameof(DroneInputsController)} are not attached",
                    _italicTextStyle
                );

                GUILayout.EndArea();
                return;
            }

            using (new GUILayout.HorizontalScope())
            {
                var hasStab = _inputsController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude);
                if (GUILayout.Toggle(hasStab, "Stabilization (press SPACE)", _checkBoxStyle))
                    _inputsController.stabilizerMode = DroneStabilizerMode.StabAltitude |
                                                      DroneStabilizerMode.StabPitchRoll |
                                                      DroneStabilizerMode.StabYaw;
                else
                    _inputsController.stabilizerMode = DroneStabilizerMode.None;
            }

            using (new GUILayout.HorizontalScope())
            {
                var sliderOpt = GUILayout.ExpandWidth(true);
                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(_inputsController.throttle,
                        1,
                        -1,
                        _sliderStyle,
                        GUI.skin.verticalSliderThumb,
                        sliderOpt
                    );

                    GUILayout.Label($"Throt. {_inputsController.throttle:F1}", _textStyle);
                }

                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(_inputsController.pitch, 1, -1, _sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Pitch {_inputsController.pitch:F1}", _textStyle);
                }

                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(_inputsController.yaw, 1, -1, _sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Yaw {_inputsController.yaw:F1}", _textStyle);
                }

                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(_inputsController.roll, 1, -1, _sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Roll {_inputsController.roll:F1}", _textStyle);
                }
            }

            GUILayout.EndArea();
        }

        private void MovementBox(int w, int h)
        {
            var boxW = Math.Min(w * _movementPanelWRatio, _movementPanelMaxW);
            var boxH = _isMovementVisible
                ? Math.Min(h * _movementPanelHRatio, _movementPanelMaxH)
                : _collapsedHeight;

            GUILayout.BeginArea(new Rect(_screenOffset, h - boxH - _screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("MOVEMENT", _boldTextStyle);
                if (GUILayout.Button(_isMovementVisible ? "x" : "+"))
                    _isMovementVisible = !_isMovementVisible;
            }

            if (!_isMovementVisible)
            {
                GUILayout.EndArea();
                return;
            }

            if (!drone || !drone.Rigidbody)
            {
                GUILayout.Label($"{nameof(QuadcopterComputer)} is not attached", _italicTextStyle);
                GUILayout.EndArea();
                return;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Velocity", _textStyle);
                GUILayout.Label($"{drone.Rigidbody.linearVelocity:F3}", _textStyle);
            }

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Target / actual Y spd.", _textStyle);
                    GUILayout.Label("Target / actual pitch ang.", _textStyle);
                    GUILayout.Label("Target / actual yaw spd.", _textStyle);
                    GUILayout.Label("Target / actual roll ang.", _textStyle);
                }

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label($"{drone.MaxLiftForce * _inputsController.throttle:F2}", _textStyle);
                    GUILayout.Label($"{drone.MaxPitchAngle * _inputsController.pitch:F2}", _textStyle);
                    GUILayout.Label($"{drone.MaxYawSpeed * _inputsController.yaw:F2}", _textStyle);
                    GUILayout.Label($"{drone.MaxRollAngle * _inputsController.roll:F2}", _textStyle);
                }

                using (new GUILayout.VerticalScope())
                {
                    var rot = transform.WrapEulerRotation180();
                    GUILayout.Label($"{drone.Rigidbody.linearVelocity.y:F3}", _textStyle);
                    GUILayout.Label($"{rot.x:F3}", _textStyle);
                    GUILayout.Label($"{drone.Rigidbody.YawVelocity():F3}", _textStyle);
                    GUILayout.Label($"{rot.z:F3}", _textStyle);
                }
            }

            GUILayout.EndArea();
        }

        private void NavigationBox(int w, int h)
        {
            var boxW = Math.Min(w * _navigationPanelWRatio, _navigationPanelMaxW);
            var boxH = _isNavigationVisible
                ? Math.Min(h * _navigationPanelHRatio, _navigationPanelMaxH)
                : _collapsedHeight;

            GUILayout.BeginArea(new Rect(_screenOffset, _screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("NAVIGATION", _boldTextStyle);
                if (GUILayout.Button(_isNavigationVisible ? "x" : "+"))
                    _isNavigationVisible = !_isNavigationVisible;
            }

            if (!_isNavigationVisible)
            {
                GUILayout.EndArea();
                return;
            }

            if (!navigator)
            {
                GUILayout.Label($"{nameof(WaypointNavigator)} is not attached", _italicTextStyle);
                GUILayout.EndArea();
                return;
            }

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Position", _textStyle);
                    GUILayout.Label($"{navigator.transform.position:F3}", _textStyle);
                }

                if (navigator.IsFinished || !navigator.CurrentWaypoint.HasValue)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Waypoint", _textStyle);
                        GUILayout.Label("FINISHED", _italicTextStyle);
                    }
                }
                else
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label($"Waypoint [{navigator.CurrentWaypointIndex + 1}/{navigator.WaypointsCount}]",
                            _textStyle
                        );

                        GUILayout.Label($"{navigator.CurrentWaypoint.Value.position:F3}", _textStyle);
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Distance", _textStyle);
                        GUILayout.Label(
                            $"{Vector3.Distance(navigator.transform.position, navigator.CurrentWaypoint.Value.position):F3}",
                            _textStyle
                        );
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Direction", _textStyle);
                        GUILayout.Label("-/-", _textStyle);
                    }
                }
            }

            GUILayout.EndArea();
        }

        private static string GetDirectionString(Vector3 direction)
        {
            var xStr = direction.x < 0
                ? "left"
                : direction.x > 0
                    ? "right"
                    : "";

            var yStr = direction.y < 0
                ? "up"
                : direction.y > 0
                    ? "down"
                    : "";

            var zStr = direction.z < 0
                ? "fwd"
                : direction.z > 0
                    ? "back"
                    : "";

            return $"{xStr}/{yStr}/{zStr}";
        }
    }
}