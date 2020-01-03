using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using static Ajustee.Helper;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
using InlineData = NUnit.Framework.TestCaseAttribute;
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

            Assert.True(_client.Output.Count == 2);
            Assert.True(_client.Output[0] == "ConnectAsync(key1, ): OK");
            Assert.True(_client.Output[1] == "SendCommandAsync_Subscribe(key2, {\"p\":\"v\"}): OK");
        }

        [Fact]
        public async Task SubscribeAsyncInputs()
        {
            using var _client = CreateClient();
            await _client.SubscribeAsync("key1");
            await _client.SubscribeAsync("key2", new Dictionary<string, string> { { "p", "v" } });

            Assert.True(_client.Output.Count == 2);
            Assert.True(_client.Output[0] == "ConnectAsync(key1, ): OK");
            Assert.True(_client.Output[1] == "SendCommandAsync_Subscribe(key2, {\"p\":\"v\"}): OK");
        }

        [Theory]
        [InlineData(@"Receive config keys [{""path"":""p1"",""datatype"":""integer"",""value"":""v1""}] after 10 ms", ReceiveMessage.ConfigKeys, @"[{""Path"":""p1"",""DataType"":""Integer"",""Value"":""v1""}]")]
        [InlineData("Receive info connectionid after 10 ms", ReceiveMessage.Info, "connectionid")]
        [InlineData("Receive reset after 10 ms", ReceiveMessage.Reset, null)]
        public async Task ReceiveValidMessage(string scenario, string expectReceiveAction, string expectReceiveData)
        {
            using var _client = CreateClient();
            _client.SetSubscribeScenario(@"Subscribe success on key1 with { ""p1"": ""v1""} after 10 ms");
            _client.SetReceiveScenario(scenario);

            await _client.WaitScenario();

            Assert.True(_client.Output.Count == 2);
            Assert.True(_client.Output[0] == @"ConnectAsync(key1, {""p1"":""v1""}): OK");
            Assert.True(_client.Output[1] == $@"OnReceiveMessage({expectReceiveAction}, {expectReceiveData}): OK");
        }

        [Theory]
        [InlineData(@"Subscribe failed on key1 after 100 ms")]
        [InlineData("Subscribe success on key1 after 50 ms", "Subscribe failed on key2 after 50 ms")]
        public void SubscribeConnectFailed(params string[] scenario)
        {
            using var _client = CreateClient();
            _client.SetSubscribeScenario(scenario);
            Assert.Throws<Exception>(() => _client.WaitScenario().GetAwaiter().GetResult());
        }

        #endregion
    }
}