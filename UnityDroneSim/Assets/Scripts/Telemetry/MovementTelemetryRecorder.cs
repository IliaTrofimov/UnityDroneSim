using System.Collections;
using System.IO;
using UnityEngine;


namespace Telemetry
{
    public sealed class MovementTelemetryRecorder : BaseTelemetryRecorder<MovementRecord, ObjectMovementLog>
    {
        [Header("Target object")] public Transform target;

        private void FixedUpdate()
        {
            if (!target) return;

            TryAddRecord(new MovementRecord(target));
        }

        [ContextMenu("Save to file")]
        public void SaveToFile()
        {
            if (string.IsNullOrEmpty(csvLogsPath)) return;

            var path = Path.Combine(csvLogsPath, $"{target.name}_movement.csv");
            recordsStorage.SaveToCsv(path);
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

            IsReplaying = false;
        }
    }
}