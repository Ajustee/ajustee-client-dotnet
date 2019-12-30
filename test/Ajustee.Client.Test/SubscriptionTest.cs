using System;
using System.Linq;
using System.Collections.Generic;

using static Ajustee.Helper;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
#endif

namespace Ajustee
{
    public class SubscriptionTest
    {

        #region Private methods region

        private static FakeAjusteeClient CreateClient()
        {
            return new FakeAjusteeClient(new AjusteeConnectionSettings
            {
                ApiUrl = new Uri("http://test.com"),
                ApplicationId = "test-app-id",
            });
        }

        #endregion

        #region Test methods

        [Fact]
        public void ReceiveMessageDeserialize()
        {
            // ConfigKeys
            var _actual = JsonSerializer.Deserialize<ReceiveMessage>(@"{""action"":""configkeys"",""data"":[{""path"": ""p"",""dataType"": ""Integer"",""value"":""v""}]}");
            Assert.True(object.Equals(ReceiveMessage.ConfigKeys, _actual.Action));
            Assert.NotNull(_actual.Data);
            Assert.True(typeof(IEnumerable<ConfigKey>).IsAssignableFrom(_actual.Data.GetType()));
            var _configKey = ((IEnumerable<ConfigKey>)_actual.Data).FirstOrDefault();
            Assert.NotNull(_configKey);
            Assert.True(_configKey.Path == "p");
            Assert.True(_configKey.DataType == ConfigKeyType.Integer);
            Assert.True(_configKey.Value == "v");

            // Info
            _actual = JsonSerializer.Deserialize<ReceiveMessage>(@"{""action"":""info"",""data"":""c-o-n-n-e-c-t-i-o-n-i-d""}");
            Assert.True(object.Equals(ReceiveMessage.Info, _actual.Action));
            Assert.NotNull(_actual.Data);
            Assert.True(typeof(string).IsAssignableFrom(_actual.Data.GetType()));
            var _info = (string)_actual.Data;
            Assert.NotNull(_info);
            Assert.True(_info == "c-o-n-n-e-c-t-i-o-n-i-d");

            // Reset
            _actual = JsonSerializer.Deserialize<ReceiveMessage>(@"{""action"":""reset""}");
            Assert.True(object.Equals(ReceiveMessage.Reset, _actual.Action));
        }

        [Fact]
        public void SubscribeInputs()
        {
            using var _client = CreateClient();
            _client.Subscribe("key1");
            _client.Subscribe("key2", new Dictionary<string, string> { { "p", "v" } });

            Assert.True(_client.Subscriber.Output.Count == 2);
            Assert.True(_client.Subscriber.Output[0] == "ConnectAsync(key1, ): OK");
            Assert.True(_client.Subscriber.Output[1] == "SendCommandAsync(key2, {\"p\":\"v\"}): OK");
        }

        [Fact]
        public void ReceiveConfigKeys()
        {
            using var _client = CreateClient();
            _client.Subscriber.SetReceiveScenario("Receive config keys []");

            Assert.True(_client.Subscriber.Output.Count == 2);
            Assert.True(_client.Subscriber.Output[0] == "ConnectAsync(key1, ): OK");
            Assert.True(_client.Subscriber.Output[1] == "SendCommandAsync(key2, {\"p\":\"v\"}): OK");
        }

        #endregion
    }
}