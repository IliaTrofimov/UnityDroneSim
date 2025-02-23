using System.IO;
using UnityEngine;


namespace Telemetry
{
    [System.Serializable]
    public readonly struct PidControllerTelemetry : ICsvSerializable
    {
        public readonly float timestamp, p, i, d, actual, target, output, lastValue, lastError;

        public PidControllerTelemetry(float p, float i, float d, float lastVal, float lastErr, float actual, float target, float output)
        {
            timestamp = Time.realtimeSinceStartup;
            this.p = p;
            this.i = i;
            this.d = d;
            this.actual = actual;
            this.target = target;
            this.output = output;
            lastError = lastErr;
            lastValue = lastVal;
        }
        
        public override string ToString() => 
            $"[{timestamp:F2}] P:{p:F3}, I:{i:F3}, D:{d:F3}, actual:{actual:F3}, target:{target:F3}, out:{output:F3}";
        
        public string ToCsvString(char separator = ';') => 
            $"{timestamp}{separator}{p}{separator}{i}{separator}{d}{separator}{actual}{separator}{target}{separator}{output}{separator}{lastValue}{separator}{lastError}";

        public void ToCsv(TextWriter stream, char separator = ';')
        {
            stream.Write(timestamp);
            stream.Write(separator);
            stream.Write(p);
            stream.Write(separator);
            stream.Write(i);
            stream.Write(separator);
            stream.Write(d);
            stream.Write(separator);
            stream.Write(actual);
            stream.Write(separator);
            stream.Write(target);
            stream.Write(separator);
            stream.WriteLine(output);
            stream.Write(separator);
            stream.WriteLine(lastValue);
            stream.Write(separator);
            stream.WriteLine(lastError);
        }
    }
}