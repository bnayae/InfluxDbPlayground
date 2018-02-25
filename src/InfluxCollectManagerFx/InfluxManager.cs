using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Immutable;
using System.Threading;

namespace InfluxCollectManager
{
    public class InfluxManager : IInfluxManager
    {
        private const string VERSION = "V20";
        private readonly AsyncLocal<ImmutableDictionary<string, string>> _context =
            new AsyncLocal<ImmutableDictionary<string, string>>();

        #region Remove on IoC Injection

        private static InfluxManager _instance;

        public static InfluxManager Default => _instance;

        public static void Init(IInfluxConfig config)
        {
            var value = new InfluxManager(config);
            if (Interlocked.CompareExchange(ref _instance, value, null) != null)
                throw new InvalidOperationException("Duplicate assignment of InfluxManager.Default");
            //LazyInitializer.EnsureInitialized(ref _instance, () => value);
        }

        #endregion // Remove on IoC Injection

        private InfluxManager(IInfluxConfig config)
        {
            var collector = new CollectorConfiguration()
                 .Tag.With("influx_manager_version", VERSION)
                 .Tag.With("host", Environment.MachineName)
                 .Tag.With("user", Environment.UserName)
                 .Batch.AtInterval(config.BatchInterval)
                 .WriteTo.InfluxDB(config.Url, database: config.DatabaseName)
                 .CreateCollector();

            InfluxDB.Collector.Metrics.Collector = collector;
        }

        public bool TryAddTag<T>(string key, T value)
        {
            var tags = _context.Value ?? ImmutableDictionary<string, string>.Empty;
            if (tags.ContainsKey(key))
                return false;
            var newTags = tags.Add(key, value?.ToString());
            _context.Value = newTags;
            return true;
        }

        public bool TryAddTagRange(IDictionary<string, string> userTags)
        {
            try
            {
                var tags = _context.Value ?? ImmutableDictionary<string, string>.Empty;
                var newTags = tags.AddRange(userTags);
                _context.Value = newTags;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Volatile(string componentName, string actionName)
        {
            var tags = _context.Value ?? ImmutableDictionary<string, string>.Empty;
            var localTags = tags.Add("component", componentName);
            Metrics.Increment($"{actionName}.count", tags: tags);
        }

        public IDisposable Duration(string componentName, string actionName)
        {
            var tags = _context.Value ?? ImmutableDictionary<string, string>.Empty;
            var localTags = tags.Add("component", componentName);
            var result = Metrics.Time($"{actionName}.time", tags: tags);
            return result;
            
        }
    }
}
