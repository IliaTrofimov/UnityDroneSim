using System.IO;


namespace Telemetry
{
    public interface ICsvSerializable
    {
        public string ToCsvString(char separator = ';');
        
        public void ToCsv(TextWriter stream,  char separator = ';');
    }
}