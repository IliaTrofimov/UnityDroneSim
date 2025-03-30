using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace Telemetry
{
    public enum TelemetryUpdateMode
    {
        Frames, Seconds
    }
        
    public abstract class BaseTelemetryRecorder<TRecord, TStorage> : MonoBehaviour
        where TStorage : TelemetryLog<TRecord>
        where TRecord: ICsvSerializable
    {
        protected bool CanRecord => enabled && !isReplaying && isRecording && Application.isPlaying && recordsStorage;

        protected bool isReplaying;
        private float lastRecordSeconds;
        private int lastRecordFrames;
        
        public bool isRecording;
        public TStorage recordsStorage;
        
        [Header("Logs storage options")]
        public bool clearOnStart = true;
        public TelemetryUpdateMode updateMode;
        [Range(0, 120)] public int updateFramesInterval = 15;
        [Range(0f, 5f)] public float updateSecondsInterval = 0.05f;
        [Range(0, 1_000_000)] public int maxRecordsCount = 10_000;
        [TextArea] public string csvLogsPath;
        
        [Header("Replay options")] [Range(0f, 5f)]
        public float replayTimeScale = 1f;

        private void Start()
        {
            if (clearOnStart) recordsStorage?.Clear();
        }

        protected bool TryAddRecord(TRecord record)
        {
            if (!CanRecord)
            {
                return false;
            }
            if (recordsStorage.RecordCount >= maxRecordsCount)
            {
                return false;
            }
            if (updateMode == TelemetryUpdateMode.Frames && math.abs(lastRecordFrames - Time.frameCount) < updateFramesInterval)
            {
                return false;
            }
            if (updateMode == TelemetryUpdateMode.Seconds && math.abs(lastRecordSeconds - Time.timeSinceLevelLoad) < updateSecondsInterval)
            {
                return false;
            }
            
            recordsStorage.Add(record);
            return true;
        }
    }
}