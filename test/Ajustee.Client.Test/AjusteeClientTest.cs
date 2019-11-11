using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
using InlineData = NUnit.Framework.TestCaseAttribute;
#endif

namespace Ajustee
{
    public class AjusteeClientTest
    {
        #region Private field region

        private const string APPLICATION_ID = "IUP2fmZaF0l2dLar~5mN91AvYTFbKkFw";
        private static readonly Uri m_API_URL = new Uri("https://7yrz26nhpe.execute-api.us-west-1.amazonaws.com/dev/configurationKeys");
        //private const string APPLICATION_ID = "nGN.agafP3fy7HbhEbfpVQqpOD0BQNzg";
        //private static readonly Uri m_API_URL = new Uri("https://api.beta.ajustee.com/configurationKeys");

        #endregion

        #region Private methods region

        private static AjusteeClient CreateClient()
        {
            return new AjusteeClient(new AjusteeConnectionSettings { ApiUrl = m_API_URL, ApplicationId = APPLICATION_ID });
        }

        #endregion

        #region Test methods

        [Fact]
        public void GetConfigurations()
        {
            using var _client = CreateClient();
            var _result = _client.GetConfigurations();
            Assert.True(_result.Count() > 0);
        }

        [Theory]
        [InlineData("", 6, null, null)]
        [InlineData("namespace1/", 6, null, null)]
        [InlineData("namespace1/key1", 1, ConfigKeyType.Integer, "2")]
        [InlineData("namespace1/key2", 1, ConfigKeyType.Boolean, "false")]
        [InlineData("namespace1/key3", 1, ConfigKeyType.String, "value2")]
        [InlineData("namespace1/key4", 1, ConfigKeyType.String, "secret1")]
        [InlineData("namespace1/key5", 1, ConfigKeyType.Date, "2019-10-28")]
        [InlineData("namespace1/key6", 1, ConfigKeyType.DateTime, "2019-10-28T01:02:03.000Z")]
        [InlineData("invalid", 0, null, null)]
        public void GetConfigurations_Path(string path, int expectedCount, ConfigKeyType? expectedType, string expectedValue)
        {
            using var _client = CreateClient();
            var _result = _client.GetConfigurations(path);
            Assert.True(object.Equals(expectedCount, _result.Count()));
            foreach (var _config in _result)
            {
                Assert.True((_config.Path.StartsWith(path)));
                if (expectedType != null)
                {
                    Assert.True(object.Equals(_config.DataType, expectedType));
                    Assert.True(object.Equals(_config.Value, expectedValue));
                }
            }
        }

        [Theory]
        [InlineData("param1", "value1")]
        public void GetConfigurations_Headers(string paramName, string paramValue)
        {
            using var _client = CreateClient();
            var _result = _client.GetConfigurations(new Dictionary<string, string> { { paramName, paramValue } });
            Assert.True(_result.Count() > 0);
        }

        [Theory]
        [InlineData("", "param1", "value1", 0, null, null)]
        [InlineData("namespace1/", "param1", "value1", 6, null, null)]
        [InlineData("namespace1/key1", "param1", "value1", 1, ConfigKeyType.Integer, "3")]
        [InlineData("namespace1/key2", "param1", "value1", 1, ConfigKeyType.Boolean, "true")]
        [InlineData("namespace1/key3", "param1", "value1", 1, ConfigKeyType.String, "value3")]
        [InlineData("namespace1/key4", "param1", "value1", 1, ConfigKeyType.String, "secret3")]
        [InlineData("namespace1/key5", "param1", "value1", 1, ConfigKeyType.Date, "2021-10-28")]
        [InlineData("namespace1/key6", "param1", "value1", 1, ConfigKeyType.DateTime, "2021-10-28T04:05:06.000Z")]
        [InlineData("invalid", "param1", "value1", 0, null, null)]
        public void GetConfigurations_PathHeaders(string path, string paramName, string paramValue, int expectedCount, ConfigKeyType? expectedType, string expectedValue)
        {
            using var _client = CreateClient();
            var _result = _client.GetConfigurations(path, new Dictionary<string, string> { { paramName, paramValue } });
            Assert.True(path == "" ? _result.Count() > 0 : expectedCount == _result.Count());
            foreach (var _config in _result)
            {
                Assert.True((_config.Path.StartsWith(path)));
                if (expectedType != null)
                {
                    Assert.True(object.Equals(_config.DataType, expectedType));
                    Assert.True(object.Equals(_config.Value, expectedValue));
                }
            }
        }

#if ASYNC
        [Fact]
        public async Task GetConfigurationsAsync()
        {
            using var _client = CreateClient();
            var _result = await _client.GetConfigurationsAsync();
            Assert.True(_result.Count() > 0);
        }

        [Theory]
        [InlineData("", 6, null, null)]
        [InlineData("namespace1/", 6, null, null)]
        [InlineData("namespace1/key1", 1, ConfigKeyType.Integer, "2")]
        [InlineData("namespace1/key2", 1, ConfigKeyType.Boolean, "false")]
        [InlineData("namespace1/key3", 1, ConfigKeyType.String, "value2")]
        [InlineData("namespace1/key4", 1, ConfigKeyType.String, "secret1")]
        [InlineData("namespace1/key5", 1, ConfigKeyType.Date, "2019-10-28")]
        [InlineData("namespace1/key6", 1, ConfigKeyType.DateTime, "2019-10-28T01:02:03.000Z")]
        [InlineData("invalid", 0, null, null)]
        public async Task GetConfigurationsAsync_Path(string path, int expectedCount, ConfigKeyType? expectedType, string expectedValue)
        {
            using var _client = CreateClient();
            var _result = await _client.GetConfigurationsAsync(path);
            Assert.True(path == "" ? _result.Count() > 0 : expectedCount == _result.Count());
            foreach (var _config in _result)
            {
                Assert.True((_config.Path.StartsWith(path)));
                if (expectedType != null)
                {
                    Assert.True(object.Equals(_config.DataType, expectedType));
                    Assert.True(object.Equals(_config.Value, expectedValue));
                }
            }
        }

        [Theory]
        [InlineData("param1", "value1")]
        public async Task GetConfigurationsAsync_Headers(string paramName, string paramValue)
        {
            using var _client = CreateClient();
            var _result = await _client.GetConfigurationsAsync(new Dictionary<string, string> { { paramName, paramValue } });
            Assert.True(_result.Count() > 0);
        }

        [Theory]
        [InlineData("", "param1", "value1", 0, null, null)]
        [InlineData("namespace1/", "param1", "value1", 6, null, null)]
        [InlineData("namespace1/key1", "param1", "value1", 1, ConfigKeyType.Integer, "3")]
        [InlineData("namespace1/key2", "param1", "value1", 1, ConfigKeyType.Boolean, "true")]
        [InlineData("namespace1/key3", "param1", "value1", 1, ConfigKeyType.String, "value3")]
        [InlineData("namespace1/key4", "param1", "value1", 1, ConfigKeyType.String, "secret3")]
        [InlineData("namespace1/key5", "param1", "value1", 1, ConfigKeyType.Date, "2021-10-28")]
        [InlineData("namespace1/key6", "param1", "value1", 1, ConfigKeyType.DateTime, "2021-10-28T04:05:06.000Z")]
        [InlineData("invalid", "param1", "value1", 0, null, null)]
        public async Task GetConfigurationsAsync_PathHeaders(string path, string paramName, string paramValue, int expectedCount, ConfigKeyType? expectedType, string expectedValue)
        {
            using var _client = CreateClient();
            var _result = await _client.GetConfigurationsAsync(path, new Dictionary<string, string> { { paramName, paramValue } });
            Assert.True(path == "" ? _result.Count() > 0 : expectedCount == _result.Count());
            foreach (var _config in _result)
            {
                Assert.True((_config.Path.StartsWith(path)));
                if (expectedType != null)
                {
                    Assert.True(object.Equals(_config.DataType, expectedType));
                    Assert.True(object.Equals(_config.Value, expectedValue));
                }
            }
        }
#endif

        #endregion
    }
}