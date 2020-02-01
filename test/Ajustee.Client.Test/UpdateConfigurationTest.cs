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

        private static AjusteeClient CreateClient(bool skipAppId = false)
        {
            return new AjusteeClient(new AjusteeConnectionSettings
            {
                ApiUrl = m_API_URL,
                ApplicationId = skipAppId ? null : APPLICATION_ID,
            });
        }

        #endregion

        #region Test methods

        [Theory]
        [InlineData("https://api.ajustee.com", "", "https://api.ajustee.com/configurationKeys/")]
        [InlineData("https://api.ajustee.com/", "", "https://api.ajustee.com/configurationKeys/")]
        [InlineData("https://api.ajustee.com/path", "", "https://api.ajustee.com/path/configurationKeys/")]
        [InlineData("https://api.ajustee.com", "mypath", "https://api.ajustee.com/configurationKeys/mypath")]
        [InlineData("https://api.ajustee.com", "/mypath", "https://api.ajustee.com/configurationKeys/mypath")]
        [InlineData("https://api.ajustee.com", "/mypath/", "https://api.ajustee.com/configurationKeys/mypath/")]
        [InlineData("https://api.ajustee.com", "mypath/subpath", "https://api.ajustee.com/configurationKeys/mypath/subpath")]
        public void GetConfigurationsUri(string apiUrl, string keyPath, string expectedUrl)
        {
            var _actualUri = Helper.GetUpdateUrl(new Uri(apiUrl), keyPath);
            Assert.True(_actualUri == new Uri(expectedUrl));
        }

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
        [InlineData("namespace1/key1", true, "1")]
        public void InvalidUpdateConfiguration(string path, bool skipAppId, string value)
        {
            using var _client = CreateClient(skipAppId: skipAppId);
            try
            {
                _client.Update(path, value);
                Assert.True(false, "Expecting ajustee exception");
            }
            catch (AjusteeException)
            { }
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
#endif

        #endregion
    }
}