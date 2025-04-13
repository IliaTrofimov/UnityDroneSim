using System.Collections;
using System.IO;
using Drone;
using UnityEngine;


namespace Telemetry
{
    public sealed class DroneInputsTelemetryRecorder : BaseTelemetryRecorder<DroneInputsRecord, DroneInputsLog>
    {
        [Header("Target object")] public DroneInputsController target;

        private void FixedUpdate()
        {
            if (!target) return;

            TryAddRecord(new DroneInputsRecord(target));
        }

        public void SaveToFile(string logName, bool onlyInputs = false)
        {
            if (string.IsNullOrEmpty(csvLogsPath) || string.IsNullOrEmpty(logName)) return;

            var path = Path.Combine(csvLogsPath, logName) + ".csv";
            recordsStorage.SaveToCsv(path, onlyInputs);
            Debug.LogFormat("Movement telemetry for '{0}' (asset '{1}') was saved to '{2}'",
                target.name,
                recordsStorage.name,
                path
            );
        }

        [ContextMenu("Save to file (movement + inputs)")]
        public void SaveToFileFull()
        {
            if (string.IsNullOrEmpty(csvLogsPath)) return;

            var path = Path.Combine(csvLogsPath, $"{target.name}_inputsFull.csv");
            recordsStorage.SaveToCsv(path, false);
            Debug.LogFormat("Movement telemetry for '{0}' (asset '{1}') was saved to '{2}'",
                target.name,
                recordsStorage.name,
                path
            );
        }

        [ContextMenu("Save to file (inputs)")]
        public void SaveToFileShort()
        {
            if (string.IsNullOrEmpty(csvLogsPath)) return;

            var path = Path.Combine(csvLogsPath, $"{target.name}_inputs.csv");
            recordsStorage.SaveToCsv(path, true);
            Debug.LogFormat("Movement telemetry for '{0}' (asset '{1}') was saved to '{2}'",
                target.name,
                recordsStorage.name,
                path
            );
        }

        [ContextMenu("Start replay")]
        public void StartReplay()
        {
            if (recordsStorage?.Records.Count > 0)
                StartCoroutine(Replay());
        }

        [ContextMenu("Start instant replay")]
        public void StartInstantReplay()
        {
            if (!(recordsStorage?.Records.Count > 0)) return;

            replayTimeScale = 0f;
            StartCoroutine(Replay());
        }

        private IEnumerator Replay()
        {
            target.manualInput = false;
            isRecording = false;
            IsReplaying = true;

            if (target.TryGetComponent<Rigidbody>(out var targetRigidbody))
            {
                targetRigidbody.angularVelocity = Vector3.zero;
                targetRigidbody.linearVelocity = Vector3.zero;
            }

            if (replayTimeScale < 1e-4)
            {
                Debug.LogFormat("Instant movement replay for '{0}'", target.name);

                foreach (var r in recordsStorage.Records)
                {
                    target.transform.position = r.Position;
                    transform.transform.rotation = r.Rotation;
                }
            }
            else
            {
                Debug.LogFormat("Starting movement replay for '{0}'", target.name);

                for (var i = 0; i < recordsStorage.Records.Count - 1; i++)
                {
                    var r = recordsStorage.Records[i];
                    target.transform.position = r.Position;
                    transform.transform.rotation = r.Rotation;

                    yield return new WaitForSeconds(
                        replayTimeScale * Mathf.Abs(r.Timestamp - recordsStorage.Records[i + 1].Timestamp)
                    );
                }

                Debug.LogFormat("Done movement replay for '{0}'", target.name);
            }

            target.manualInput = true;
            IsReplaying = false;
        }
    }
}