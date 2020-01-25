using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using static Ajustee.Helper;
using System.Text;
using System.Diagnostics;

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
        private static IAjusteeClient CreateClient(bool reconnect = false)
        {
            return new FakeAjusteeClient(new AjusteeConnectionSettings
            {
                ApiUrl = new Uri("http://test.com"),
                ApplicationId = "test-app-id",
                ReconnectSubscriptions = reconnect
            });
        }

        [Fact]
        public void Subscribe()
        {
            using var _client = CreateClient();
            _client.Subscribe("key1");
            _client.Subscribe("key2", new Dictionary<string, string> { { "p", "v" } });

            var _messages = ATL.GetMessages();

            Assert.True(_messages.Count == 3);
            Assert.True(_messages[0] == "Subscriber connected");
            Assert.True(_messages[1] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key1""}}");
            Assert.True(_messages[2] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key2"",""props"":{""p"":""v""}}}");
        }

        [Fact]
        public void SubscribeAsync()
        {
            using var _client = CreateClient();
            _client.SubscribeAsync("key1");
            _client.SubscribeAsync("key2", new Dictionary<string, string> { { "p", "v" } });

            var _messages = ATL.GetMessages();

            Assert.True(_messages.Count == 3);
            Assert.True(_messages[0] == "Subscriber connected");
            Assert.True(_messages[1] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key1""}}");
            Assert.True(_messages[2] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key2"",""props"":{""p"":""v""}}}");
        }

        [Fact]
        public void ReceiveSubscribeMessage()
        {
            using var _scenario = new ScenarioManager(CreateClient());
            _scenario.Client("1:Subscribe", "key1");
            _scenario.Client("2:Subscribe", "key2");
            _scenario.Client("3:Subscribe", "key4");
            _scenario.Client("4:Subscribe", "key5");
            _scenario.Server("Send after 1", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success));
            _scenario.Server("Send after 2", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Not_Found_App));
            _scenario.Server("Send after 3", ReceiveMessage.Subscribe("key3", ReceiveMessageStatusCode.Not_Found_KeyPath));
            _scenario.Server("Send after 4", ReceiveMessage.Subscribe("key4", ReceiveMessageStatusCode.Already_Exists));
            _scenario.Wait();

            var _messages = ATL.GetMessages();

            Assert.True(_messages.Count == 3);
            Assert.True(_messages[0] == "Subscriber connected");
            Assert.True(_messages[1] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key1""}}");
            Assert.True(_messages[2] == @"Subscriber send: {""action"":""subscribe"",""data"":{""path"":""key2"",""props"":{""p"":""v""}}}");
        }

        //[Theory]
        //[InlineData(@"Receive config keys [{""path"":""p1"",""datatype"":""integer"",""value"":""v1""}] after 10 ms", ReceiveMessage.ConfigKeys, @"[{""Path"":""p1"",""DataType"":""Integer"",""Value"":""v1""}]")]
        //public async Task ReceiveValidMessage(string scenario, string expectReceiveAction, string expectReceiveData)
        //{
        //    using var _client = CreateFakeClient();
        //    _client.SetSubscribeScenario(@"Subscribe success on key1 with { ""p1"": ""v1""} after 10 ms");
        //    _client.SetReceiveScenario(scenario);

        //    await _client.WaitScenario();

        //    Assert.True(_client.Output.Count == 3);
        //    Assert.True(_client.Output[0] == @"Connect: OK");
        //    Assert.True(_client.Output[1] == @"SendSubscribe(key1, {""p1"":""v1""}): OK");
        //    Assert.True(_client.Output[2] == $@"Receive({expectReceiveAction}, {expectReceiveData}): OK");
        //}

        //[Theory]
        //[InlineData("Subscribe failed on key1 after 100 ms")]
        //[InlineData("Subscribe success on key1 after 50 ms", "Subscribe failed on key2 after 50 ms")]
        //public void SubscribeConnectFailed(params string[] scenario)
        //{
        //    using var _client = CreateClient();
        //    _client.SetSubscribeScenario(scenario);
        //    Assert.Throws<Exception>(() => _client.WaitScenario().GetAwaiter().GetResult());
        //}

        //[Fact]
        //public async Task ReceiveClosedAndReconnect()
        //{
        //    using var _client = CreateFakeClient(reconnect: true);
        //    _client.SetSubscribeScenario("Subscribe success on key1 after 10 ms");
        //    _client.SetSubscribeScenario("Subscribe success on key2 after 50 ms");
        //    _client.SetSubscribeScenario("Subscribe success on key3 after trigger1");
        //    _client.SetReceiveScenario(@"Receive config keys [{""path"":""key1"",""datatype"":""integer"",""value"":""value1""}] after 5 ms");
        //    _client.SetReceiveScenario(@"Receive config keys [{""path"":""key2"",""datatype"":""integer"",""value"":""value2""}] after 150 ms");
        //    _client.SetReceiveScenario("Receive failed after 50 ms");
        //    _client.SetReceiveScenario("Receive closed after 50 ms");
        //    _client.SetReceiveScenario(@"Receive config keys [{""path"":""key3"",""datatype"":""integer"",""value"":""value3""}] after 50 ms");
        //    _client.SetReceiveScenario(@"Receive config keys [{""path"":""key4"",""datatype"":""integer"",""value"":""value4""}] after 50 ms");
        //    _client.SetReceiveScenario("Receive closed for 4 attempts after 50 ms");
        //    _client.SetReceiveScenario(@"Receive config keys [{""path"":""key5"",""datatype"":""integer"",""value"":""value5""}] after 50 ms");
        //    _client.SetReceiveScenario(@"Run trigger1");
        //    _client.SetReceiveScenario("Receive closed after 50 ms");

        //    await _client.WaitScenario();

        //    Assert.True(_client.Output.Count == 27);
        //    Assert.True(_client.Output[0] == "Connect: OK");
        //    Assert.True(_client.Output[1] == "SendSubscribe(key1, ): OK");
        //    Assert.True(_client.Output[2] == $@"Receive({ReceiveMessage.ConfigKeys}, [{{""Path"":""key1"",""DataType"":""Integer"",""Value"":""value1""}}]): OK");
        //    Assert.True(_client.Output[3] == "SendSubscribe(key2, ): OK");
        //    Assert.True(_client.Output[4] == $@"Receive({ReceiveMessage.ConfigKeys}, [{{""Path"":""key2"",""DataType"":""Integer"",""Value"":""value2""}}]): OK");
        //    Assert.True(_client.Output[5] == "Receive: FAILED");
        //    Assert.True(_client.Output[6] == "Receive: CLOSED");
        //    Assert.True(_client.Output[7] == "Connect: OK");
        //    Assert.True(_client.Output[8] == "SendSubscribe(key1, ): OK");
        //    Assert.True(_client.Output[9] == "SendSubscribe(key2, ): OK");
        //    Assert.True(_client.Output[10] == $@"Receive({ReceiveMessage.ConfigKeys}, [{{""Path"":""key3"",""DataType"":""Integer"",""Value"":""value3""}}]): OK");
        //    Assert.True(_client.Output[11] == $@"Receive({ReceiveMessage.ConfigKeys}, [{{""Path"":""key4"",""DataType"":""Integer"",""Value"":""value4""}}]): OK");
        //    Assert.True(_client.Output[12] == "Receive: CLOSED");
        //    Assert.True(_client.Output[13] == "Connect: FAILED");
        //    Assert.True(_client.Output[14] == "Connect: FAILED");
        //    Assert.True(_client.Output[15] == "Connect: FAILED");
        //    Assert.True(_client.Output[16] == "Connect: FAILED");
        //    Assert.True(_client.Output[17] == "Connect: OK");
        //    Assert.True(_client.Output[18] == "SendSubscribe(key1, ): OK");
        //    Assert.True(_client.Output[19] == "SendSubscribe(key2, ): OK");
        //    Assert.True(_client.Output[20] == $@"Receive({ReceiveMessage.ConfigKeys}, [{{""Path"":""key5"",""DataType"":""Integer"",""Value"":""value5""}}]): OK", "20 - FAILED");
        //    Assert.True(_client.Output[21] == "SendSubscribe(key3, ): OK", "21 - FAILED = '" + _client.Output[21] + "'=='" + "SendSubscribe(key3, ): OK" + "' = " + (_client.Output[21] == "SendSubscribe(key3, ): OK"));
        //    Assert.True(_client.Output[22] == "Receive: CLOSED", "22 - FAILED");
        //    Assert.True(_client.Output[23] == "Connect: OK");
        //    Assert.True(_client.Output[24] == "SendSubscribe(key1, ): OK");
        //    Assert.True(_client.Output[25] == "SendSubscribe(key2, ): OK");
        //    Assert.True(_client.Output[26] == "SendSubscribe(key3, ): OK");
        //}

        //[Fact]
        //public async Task Subscribe()
        //{
        //    using var _manager = new SubscriberManager(CreateClient());
        //    _manager.Server("Start server");
        //    _manager.Server("Stop server after 50000 ms");

        //    await _manager.Wait();
        //}
    }
}