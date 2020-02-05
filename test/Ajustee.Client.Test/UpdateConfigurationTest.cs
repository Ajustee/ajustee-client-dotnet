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
    public class UpdateConfigurationTest
    {
        #region Private field region

        private static readonly Uri m_API_URL = new Uri("https://b3bke9zpxg.execute-api.us-west-2.amazonaws.com/fo/");
        private const string APPLICATION_ID = "nLnoagp.mKQk1t2YEfs5RlrPbcXrjg~8";

        #endregion

        #region Private methods region

        private static AjusteeClient CreateClient(bool skipApiUrl = false, bool skipAppId = false)
        {
            return new AjusteeClient(new AjusteeConnectionSettings
            {
                ApiUrl = skipApiUrl ? null : m_API_URL,
                ApplicationId = skipAppId ? null : APPLICATION_ID,
            });
        }

        #endregion

        #region Test methods

        [Fact]
        public void Update()
        {
            using var _client = CreateClient();
            _client.Update("namespace1/key1", "1");
            var _configKey = _client.GetConfigurations("namespace1/key1").FirstOrDefault();
            Assert.True(object.Equals(_configKey.Value, "1"));
        }

        [Theory]
        [InlineData("namespace1/key1", "1")]
        public void UpdateConfiguration(string path, string value)
        {
            using var _client = CreateClient();
            _client.Update(path, value);
            var _configKey = _client.GetConfigurations(path).FirstOrDefault();
            Assert.True(object.Equals(_configKey.Value, value));
        }

        [Theory]
        [InlineData("namespace1/key1", true, false, "1", AjusteeErrorCode.Invalid)]
        [InlineData("namespace1/key1", false, true, "1", AjusteeErrorCode.Forbidden)]
        [InlineData(null, false, false, "1", AjusteeErrorCode.Invalid)]
        [InlineData("invalid_key_path", false, false, "1", AjusteeErrorCode.NotFound)]
        public void InvalidUpdateConfiguration(string path, bool skipApiUrl, bool skipAppId, string value, AjusteeErrorCode expectedError)
        {
            using var _client = CreateClient(skipAppId: skipAppId, skipApiUrl: skipApiUrl);
            try
            {
                _client.Update(path, value);
                Assert.True(false, "Expecting ajustee exception");
            }
            catch (AjusteeException _ex)
            {
                Assert.True(expectedError == _ex.ErrorCode);
            }
        }

#if ASYNC
        [Theory]
        [InlineData("namespace1/key1", "1")]
        public async Task UpdateConfigurationAsync(string path, string value)
        {
            using var _client = CreateClient();
            await _client.UpdateAsync(path, value);
            var _configKey = (await _client.GetConfigurationsAsync(path)).FirstOrDefault();
            Assert.True(object.Equals(_configKey.Value, value));
        }

        [Theory]
        [InlineData("namespace1/key1", true, false, "1", AjusteeErrorCode.Invalid)]
        [InlineData("namespace1/key1", false, true, "1", AjusteeErrorCode.Forbidden)]
        [InlineData(null, false, false, "1", AjusteeErrorCode.Invalid)]
        [InlineData("invalid_key_path", false, false, "1", AjusteeErrorCode.NotFound)]
        public async Task InvalidUpdateConfigurationAsync(string path, bool skipApiUrl, bool skipAppId, string value, AjusteeErrorCode expectedError)
        {
            using var _client = CreateClient(skipAppId: skipAppId, skipApiUrl: skipApiUrl);
            try
            {
                await _client.UpdateAsync(path, value);
                Assert.True(false, "Expecting ajustee exception");
            }
            catch (AjusteeException _ex)
            {
                Assert.True(expectedError == _ex.ErrorCode);
            }
        }
#endif

        #endregion
    }
}