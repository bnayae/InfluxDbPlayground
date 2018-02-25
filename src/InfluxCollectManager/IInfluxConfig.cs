using System;

namespace InfluxCollectManager
{
    public interface IInfluxConfig
    {
        TimeSpan BatchInterval { get; }
        string Url { get; }
        string DatabaseName { get; }
    }
}