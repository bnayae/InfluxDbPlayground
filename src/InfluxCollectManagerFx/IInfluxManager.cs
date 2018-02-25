using System;
using System.Collections.Generic;

namespace InfluxCollectManager
{
    public interface IInfluxManager
    {
        IDisposable Duration(string componentName, string actionName);
        bool TryAddTag<T>(string key, T value);
        bool TryAddTagRange(IDictionary<string, string> userTags);
        void Volatile(string componentName, string actionName);
    }
}