namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IApplicationConfig
    {
        int FlushSecondsInterval { get; }
        bool DebugMode { get; }
    }
}