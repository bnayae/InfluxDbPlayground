using Metrics;
using Metrics.MetricData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// https://github.com/Recognos/Metrics.NET/wiki/Meters

namespace MetricsNetRaw
{
    class Program
    {
        // keep a histogram of the duration of a type 
        // of event and a meter of the rate of its occurrence
        private static readonly Metrics.Timer _timerA = 
            Metric.Context("Eshet.Context").Timer("ScopeA", Unit.Requests);
        private static readonly Metrics.Timer _timerB = 
            Metric.Context("Eshet.Context").Timer("ScopeB", Unit.Requests);

        // 4 bit integers that can be incremented or decremented
        private static readonly Counter _counterA = 
            Metric.Context("Eshet.Context").Counter("ValueA", Unit.Requests);
        private static readonly Counter _counterB =
            Metric.Context("Eshet.Context").Counter("ValueB", Unit.Requests);

        // record the rate at which an event occurs
        private static readonly Meter _meterA =
                    Metric.Context("Eshet.Context").Meter("Calls A", Unit.Calls, TimeUnit.Seconds);
        private static readonly Meter _meterB =
                    Metric.Context("Eshet.Context").Meter("Calls B", Unit.Events, TimeUnit.Seconds);

        // A Histogram measures the distribution of values in a stream
        private static readonly Histogram _histogram = 
            Metric.Context("Eshet.Context").Histogram("Search Results", Unit.Items);



        private static readonly Random _rnd = new Random();

        static void Main(string[] args)
        {
            
            Metric.Config
                
                .WithHttpEndpoint("http://localhost:1234/");
            //.WithAllCounters();

            int i = 0;
            while (true)
            {
                _counterA.Increment();
                int delay = _rnd.Next(500, 2000);
                _timerB.Record(delay, TimeUnit.Microseconds, "Bnaya");
                _counterB.Increment(delay);
                _meterA.Mark();
                i++;
                //_meterB.Mark("Open", i % 30);
                //_meterB.Mark("Close", i);
                //if(i % 40 < 20)
                //    _meterB.Mark("Drop", i);
                if (i % 40 < 20)
                    _meterB.Mark(i);

                switch ((i % 50) / 10)
                {
                    case 0:
                        _histogram.Update(i);
                        break;
                    case 1:
                        _histogram.Update(i % 10);
                        break;
                    case 2:
                        if(i % 2 == 0)
                            _histogram.Update(i);
                        else
                            _histogram.Update(-i);
                        break;
                    case 4:
                        _histogram.Update(i % 1000);
                        break;
                    default:
                        break;
                }
                using (var x = _timerA.NewContext()) // measure until disposed
                {
                    Thread.Sleep(delay);
                }
                _counterA.Decrement();
                
                // A gauge is the simplest metric type. It represents an instantaneous value

                // gauge from Func<double>
                Metric.Gauge("MyValue", () => _rnd.Next(30, 100), Unit.Items);

                // gauge that reads its value from a performance counter
                Metric.PerformanceCounter("CPU Usage", "Processor", "% Processor Time",
                    "_Total", Unit.Custom("%"));
            }
        }
    }
}
