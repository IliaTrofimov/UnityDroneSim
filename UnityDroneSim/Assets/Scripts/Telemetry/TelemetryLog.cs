using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Base type for all serializable telemetry logs.</summary>
    [PreferBinarySerialization]
    public abstract class TelemetryLog<T> : ScriptableObject
        where T : ICsvSerializable
    {
        [SerializeField] protected List<T> records = new(100);

        public IReadOnlyList<T> Records     => records;
        public int              RecordCount => records.Count;
        public T this[int index] => records[index];

        public void Add(T item) => records.Add(item);

        public bool Clear()
        {
            if (records.Count == 0) return false;

            records.Clear();
            return true;
        }

        public void SaveToCsv(string path)
        {
            if (RecordCount == 0) return;

            try
            {
                using var stream = new StreamWriter(path);
                foreach (var r in records) r.ToCsvNewLine(stream);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"Cannot save {GetType().Name} to CSV file. File {path} not found.");
            }
        }
    }
}