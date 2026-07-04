namespace SubscriberWebApp.Components.Models
{
    public class ModbusData
    {
        public int Id { get; set; }
        public int FunctionCode { get; set; }
        public byte[] PayLoadData { get; set; } = Array.Empty<byte>();
        public DateTime TimeStamp { get; set; }
    }
}
