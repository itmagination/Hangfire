// This file is part of Hangfire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using Hangfire.Annotations;
using Hangfire.Logging;

namespace Hangfire.Common
{
    public static class JobHelper
    {
        private static IJobSerializer _defaultJobSerializer = new JsonJobSerializer(null);
        private static IJobSerializer _jobSerializer = null;

        private static readonly ILog Logger = LogProvider.GetLogger("JobSerializer");

        public static void SetDefaultJobSerializer(IJobSerializer jobSerializer)
        {
            if (jobSerializer == null)
                throw new ArgumentNullException(nameof(jobSerializer));

            _defaultJobSerializer = jobSerializer;
        }

        public static void SetJobSerializer(IJobSerializer jobSerializer)
        {
            _jobSerializer = jobSerializer;
        }

        public static string Serialize(object value)
        {
            if (value == null)
                return null;

            try
            {
                return _defaultJobSerializer.Serialize(value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, () => ex.Message, ex);

                if (_jobSerializer != null)
                    return _jobSerializer.Serialize(value);

                throw;
            }
        }

        public static T Deserialize<T>(string value)
        {
            if (value == null)
                return default(T);

            try
            {
                return _defaultJobSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, () => ex.Message, ex);

                if (_jobSerializer != null)
                    return _jobSerializer.Deserialize<T>(value);

                throw;
            }
        }

        public static object Deserialize(string value, [NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (value == null)
                return null;

            try
            {
                return _defaultJobSerializer.Deserialize(value, type);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, () => ex.Message, ex);

                if (_jobSerializer != null)
                    return _jobSerializer.Deserialize(value, type);

                throw;
            }
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }

        public static DateTime FromTimestamp(long value)
        {
            return Epoch.AddSeconds(value);
        }

        public static string SerializeDateTime(DateTime value)
        {
            return value.ToString("o", CultureInfo.InvariantCulture);
        }

        public static DateTime DeserializeDateTime(string value)
        {
            long timestamp;
            if (long.TryParse(value, out timestamp))
            {
                return FromTimestamp(timestamp);
            }

            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public static DateTime? DeserializeNullableDateTime(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            return DeserializeDateTime(value);
        }
    }
}
