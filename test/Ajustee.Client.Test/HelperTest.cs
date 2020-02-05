using System;
using System.Collections.Generic;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
using InlineData = NUnit.Framework.TestCaseAttribute;
#endif

namespace Ajustee
{
    public class HelperTest
    {
        [Theory]
        [InlineData("https://api.ajustee.com", "", "https://api.ajustee.com/configurationKeys?path=")]
        [InlineData("https://api.ajustee.com/", "", "https://api.ajustee.com/configurationKeys?path=")]
        [InlineData("https://api.ajustee.com", "mypath", "https://api.ajustee.com/configurationKeys?path=mypath")]
        [InlineData("https://api.ajustee.com", "/mypath", "https://api.ajustee.com/configurationKeys?path=mypath")]
        [InlineData("https://api.ajustee.com", "/mypath/", "https://api.ajustee.com/configurationKeys?path=mypath/")]
        [InlineData("https://api.ajustee.com", "mypath/subpath", "https://api.ajustee.com/configurationKeys?path=mypath/subpath")]
        public void GetConfigurationKeysUrl(string apiUrl, string keyPath, string expectedUrl)
        {
            var _actualUri = Helper.GetConfigurationKeysUrl(new Uri(apiUrl), keyPath);
            Assert.True(_actualUri == new Uri(expectedUrl));
        }

        [Theory]
        [InlineData("https://api.ajustee.com", "mypath", "https://api.ajustee.com/configurationKeys/mypath")]
        [InlineData("https://api.ajustee.com", "/mypath", "https://api.ajustee.com/configurationKeys/mypath")]
        [InlineData("https://api.ajustee.com", "/mypath/", "https://api.ajustee.com/configurationKeys/mypath/")]
        [InlineData("https://api.ajustee.com", "mypath/subpath", "https://api.ajustee.com/configurationKeys/mypath/subpath")]
        public void GetUpdateUrl(string apiUrl, string keyPath, string expectedUrl)
        {
            var _actualUri = Helper.GetUpdateUrl(new Uri(apiUrl), keyPath);
            Assert.True(_actualUri == new Uri(expectedUrl));
        }

        [Theory]
        [InlineData("https://api.ajustee.com", null)]
        [InlineData("https://api.ajustee.com", "")]
        [InlineData("https://api.ajustee.com/", "")]
        [InlineData("https://api.ajustee.com/path", "")]
        public void GetUpdateUrlInvalid(string apiUrl, string keyPath)
        {
            try
            {
                Helper.GetUpdateUrl(new Uri(apiUrl), keyPath);
                Assert.True(false, "Should raised invalid exception.");
            }
            catch (AjusteeException _ex) when (_ex.ErrorCode == AjusteeErrorCode.Invalid)
            { }
        }

        [Theory]
        [InlineData("https://api.ajustee.com/", "wss://ws.ajustee.com/")]
        [InlineData("http://api.ajustee.com/", "wss://ws.ajustee.com/")]
        [InlineData("https://api.ajustee.com/path", "wss://ws.ajustee.com/path")]
        [InlineData("https://some.ajustee.com/", "wss://some.ajustee.com/")]
        public void GetSubscribeUrl(string apiUrl, string wssUrl)
        {
            var _wssUri = Helper.GetSubscribeUrl(new Uri(apiUrl));
            Assert.True(_wssUri == new Uri(wssUrl));
        }

        [Fact]
        public void GetMergedProperties()
        {
            IDictionary<string, string> _props;
            // none
            try
            {
                _props = Helper.GetMergedProperties();
                Assert.True(false, "Should raised argument exception.");
            }
            catch (ArgumentException)
            { }

            // null
            try
            {
                _props = Helper.GetMergedProperties(null);
                Assert.True(true, "Should raised argument exception.");
            }
            catch (ArgumentException)
            { }

            // null, null
            _props = Helper.GetMergedProperties(null, null);
            Assert.Null(_props);

            // null, not null
            _props = Helper.GetMergedProperties(null, new Dictionary<string, string> { { "p1", "v1" } });
            Assert.NotNull(_props);
            Assert.True(_props.Count == 1);
            Assert.True(_props["p1"] == "v1");

            // not null, null
            _props = Helper.GetMergedProperties(null, new Dictionary<string, string> { { "p1", "v1" } });
            Assert.NotNull(_props);
            Assert.True(_props.Count == 1);
            Assert.True(_props["p1"] == "v1");

            // not null, not null
            _props = Helper.GetMergedProperties(new Dictionary<string, string> { { "p1", "v1" }, { "p2", "v2" } }, new Dictionary<string, string> { { "p2", "vnew" }, { "p3", "v3" } });
            Assert.NotNull(_props);
            Assert.True(_props.Count == 3);
            Assert.True(_props["p1"] == "v1");
            Assert.True(_props["p2"] == "vnew");
            Assert.True(_props["p3"] == "v3");

            // null, not null, not null
            _props = Helper.GetMergedProperties(null, new Dictionary<string, string> { { "p1", "v1" }, { "p2", "v2" } }, new Dictionary<string, string> { { "p2", "vnew" }, { "p3", "v3" } });
            Assert.NotNull(_props);
            Assert.True(_props.Count == 3);
            Assert.True(_props["p1"] == "v1");
            Assert.True(_props["p2"] == "vnew");
            Assert.True(_props["p3"] == "v3");

            // null, not null, null
            _props = Helper.GetMergedProperties(null, new Dictionary<string, string> { { "p1", "v1" }, { "p2", "v2" } }, null);
            Assert.NotNull(_props);
            Assert.True(_props.Count == 2);
            Assert.True(_props["p1"] == "v1");
            Assert.True(_props["p2"] == "v2");
        }
    }
}