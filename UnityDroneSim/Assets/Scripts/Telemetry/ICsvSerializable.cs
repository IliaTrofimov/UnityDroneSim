using System.IO;


namespace Telemetry
{
    /// <summary>
    /// Object that can be written as CSV.
    /// </summary>
    public interface ICsvSerializable
    {
        public void ToCsv(TextWriter writer, string separator = "\t");

        public void ToCsvNewLine(TextWriter writer, string separator = "\t");
    }
}