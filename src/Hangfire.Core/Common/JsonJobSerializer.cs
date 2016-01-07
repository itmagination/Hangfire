using System;
using Newtonsoft.Json;

namespace Hangfire.Common
{
    public class JsonJobSerializer : IJobSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonJobSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public string Serialize(object @object)
        {
            return @object != null
                ? JsonConvert.SerializeObject(@object, _serializerSettings)
                : null;
        }

        public T Deserialize<T>(string data)
        {
            return data != null
                ? JsonConvert.DeserializeObject<T>(data, _serializerSettings)
                : default(T);
        }

        public object Deserialize(string data, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return data != null
                       ? JsonConvert.DeserializeObject(data, type, _serializerSettings)
                       : null;
        }
    }
} 