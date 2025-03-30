using System.IO;
using Drone;
using UnityEngine;


namespace Telemetry
{
    /// <summary>Collection of <see cref="DroneInputsRecord"/> recorder during game.</summary>
    [CreateAssetMenu(fileName = "NewDroneInputsLog", menuName = "Telemetry/Drone Inputs Log")]
    public sealed class DroneInputsLog : TelemetryLog<DroneInputsRecord>
    {
        public void Add(DroneInputsController inputsController) 
            => Add(new DroneInputsRecord(inputsController)); 
        
        public void SaveToCsv(string path, bool onlyInputs)
        {
            if (RecordCount == 0) return;

            try
            {
                using var stream = new StreamWriter(path);
                foreach (var r in records)
                {
                    r.ToCsvNewLine(stream, onlyInputs);
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"Cannot save {GetType().Name} to CSV file. File {path} not found.");
            }
        }
    }
}