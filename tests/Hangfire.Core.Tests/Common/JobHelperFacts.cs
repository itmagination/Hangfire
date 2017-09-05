using System;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace Hangfire.Core.Tests.Common
{
    public class JobHelperFacts
    {
        private static readonly DateTime WellKnownDateTime = new DateTime(1988, 04, 20, 01, 12, 32, DateTimeKind.Utc);
        private const int WellKnownTimestamp = 577501952;

        [Fact]
        public void Serialize_EncodesNullValueAsNull()
        {
            var result = JobHelper.Serialize(null);
            Assert.Null(result);
        }

        [Fact]
        public void Serialize_EncodesGivenValue_ToJsonString()
        {
            var result = JobHelper.Serialize("hello");
            Assert.Equal("\"hello\"", result);
        }

        [Fact]
        public void Deserialize_DecodesNullAsDefaultValue()
        {
            var stringResult = JobHelper.Deserialize<string>(null);
            var intResult = JobHelper.Deserialize<int>(null);

            Assert.Null(stringResult);
            Assert.Equal(0, intResult);
        }

        [Fact]
        public void Deserialize_DecodesFromJsonString()
        {
            var result = JobHelper.Deserialize<string>("\"hello\"");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Deserialize_ThrowsAnException_WhenTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => JobHelper.Deserialize("1", null));
        }

        [Fact]
        public void Deserialize_WithType_DecodesFromJsonString()
        {
            var result = (string)JobHelper.Deserialize("\"hello\"", typeof(string));
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Deserialize_WithType_DecodesNullValue_ToNull()
        {
            var result = (string)JobHelper.Deserialize(null, typeof(string));
            Assert.Null(result);
        }

        [Fact]
        public void ToTimestamp_ReturnsUnixTimestamp_OfTheGivenDateTime()
        {
            var result = JobHelper.ToTimestamp(
                WellKnownDateTime);

            Assert.Equal(WellKnownTimestamp, result);
        }

        [Fact]
        public void ToTimestamp_ReturnsDateTime_ForGivenTimestamp()
        {
            var result = JobHelper.FromTimestamp(WellKnownTimestamp);

            Assert.Equal(WellKnownDateTime, result);
        }

        [Fact]
        public void SerializeDateTime_ReturnsString_InISO8601Format()
        {
            var result = JobHelper.SerializeDateTime(WellKnownDateTime);

            Assert.Equal(WellKnownDateTime.ToString("o"), result);
        }

        [Fact]
        public void DeserializeDateTime_CanDeserialize_Timestamps()
        {
            var result = JobHelper.DeserializeDateTime(WellKnownTimestamp.ToString());

            Assert.Equal(WellKnownDateTime, result);
        }

        [Fact]
        public void DeserializeDateTime_CanDeserialize_ISO8601Format()
        {
            var result = JobHelper.DeserializeDateTime(WellKnownDateTime.ToString("o"));
            Assert.Equal(WellKnownDateTime, result);
        }

        [Fact]
        public void DeserializeNullableDateTime_ReturnsNull_IfNullOrEmptyStringGiven()
        {
            Assert.Null(JobHelper.DeserializeNullableDateTime(""));
            Assert.Null(JobHelper.DeserializeNullableDateTime(null));
        }

        [Fact]
        public void DeserializeNullableDateTime_ReturnsCorrectValue_OnNonNullString()
        {
            var result = JobHelper.DeserializeNullableDateTime(WellKnownTimestamp.ToString());
            Assert.Equal(WellKnownDateTime, result);
        }

        [Fact]
        public void Deserialize_WithObjectType_DecodesFromJsonString()
        {
            var result = (ClassA)JobHelper.Deserialize(@"{ ""PropertyA"": ""hello"" }", typeof(ClassA));
            Assert.Equal("hello", result.PropertyA);
        }

        [Fact]
        public void ForSerializeUseDefaultConfigurationOfJsonNet()
        {
            var result = JobHelper.Serialize(new ClassA("A"));
            Assert.Equal(@"{""PropertyA"":""A""}", result);
        }

        [Fact]
        public void ForSerializeCanUseCustomConfigurationOfJsonNet()
        {
            try
            {
                JobHelper.SetDefaultJobSerializer(new JsonJobSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

                var result = JobHelper.Serialize(new ClassA("A"));
                Assert.Equal(@"{""propertyA"":""A""}", result);
            }
            finally
            {
                JobHelper.SetDefaultJobSerializer(new JsonJobSerializer(null));
            }
        }

        [Fact]
        public void DefaultSerializerCouldNotSerializeCircularReferences()
        {
                Assert.Throws<JsonSerializationException>(() => JobHelper.Serialize(new ClassWithCircularReference()));
        }

        [Fact]
        public void FallbackSerializerShouldSerializeCircularReferences()
        {
            try
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                var result = JobHelper.Serialize(new ClassWithCircularReference());
                Assert.Equal("{}", result);
            }
            finally
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(null));
            }
        }

        [Fact]
        public void ForDeserializeCanUseCustomConfigurationOfJsonNet()
        {
            try
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(
                                               new JsonSerializerSettings
                                               {
                                                   TypeNameHandling = TypeNameHandling.Objects
                                               })
                    );

                var result = (ClassA)JobHelper.Deserialize<IClass>(@"{ ""$type"": ""Hangfire.Core.Tests.Common.JobHelperFacts+ClassA, Hangfire.Core.Tests"", ""propertyA"":""A"" }");
                Assert.Equal("A", result.PropertyA);
            }
            finally
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(null));
            }
        }

        [Fact]
        public void ForDeserializeCanUseCustomConfigurationOfJsonNetWithInvocationData()
        {
            try
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(
                               new JsonSerializerSettings
                               {
                                   TypeNameHandling = TypeNameHandling.All,
                                   TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                               })
                    );

                var method = typeof (BackgroundJob).GetMethod("DoWork");
                var args = new object[] { "123", "Test" };
                var job = new Job(typeof(BackgroundJob), method, args);

                var invocationData = InvocationData.Serialize(job);
                var deserializedJob = invocationData.Deserialize();

                Assert.Equal(typeof(BackgroundJob), deserializedJob.Type);
                Assert.Equal(method, deserializedJob.Method);
                Assert.Equal(args, deserializedJob.Args);
            }
            finally
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(null));
            }
        }

        [Fact]
        public void ForDeserializeWithGenericMethodCanUseCustomConfigurationOfJsonNet()
        {
            try
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(
                                               new JsonSerializerSettings
                                               {
                                                   TypeNameHandling = TypeNameHandling.Objects
                                               })
                    );

                var result = (ClassA)JobHelper.Deserialize(@"{ ""$type"": ""Hangfire.Core.Tests.Common.JobHelperFacts+ClassA, Hangfire.Core.Tests"", ""propertyA"":""A"" }", typeof(IClass));
                Assert.Equal("A", result.PropertyA);
            }
            finally
            {
                JobHelper.SetJobSerializer(new JsonJobSerializer(null));
            }
        }

        private interface IClass
        {
        }

        private class ClassA : IClass
        {
            public ClassA(string propertyA)
            {
                PropertyA = propertyA;
            }

            public string PropertyA { get; }
        }

        private class ClassWithCircularReference : IClass
        {
            public ClassWithCircularReference CircularReference
            {
                get { return this; }
            }
        }

        private class BackgroundJob
        {
            [UsedImplicitly]
            public void DoWork(string workId, string message)
            {
            }
        }
    }
}
