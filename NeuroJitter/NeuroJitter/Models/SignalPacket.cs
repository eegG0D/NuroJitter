namespace NeuroJitter.Models
{
    public struct SignalPacket
    {
        public double RawVoltage { get; set; }
        public double JitterMetric { get; set; }
        public double Timestamp { get; set; }
        public bool IsConductive { get; set; }
    }
}