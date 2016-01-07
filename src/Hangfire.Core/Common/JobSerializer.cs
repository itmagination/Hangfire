using System;

namespace Hangfire.Common
{
    public interface IJobSerializer
    {
        string Serialize(object @object);
        T Deserialize<T>(string data);
        object Deserialize(string data, Type type);
    }
}