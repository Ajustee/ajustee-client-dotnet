using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net.WebSockets;
using System;

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
        private static IAjusteeClient CreateClient(bool reconnect = false, bool invalidUrl = false)
        {
            return new FakeAjusteeClient(new AjusteeConnectionSettings
            {
                ApiUrl = invalidUrl ? FakeAjusteeClient.InvalidUri : FakeAjusteeClient.ValidUri,
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

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null));
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", new Dictionary<string, string> { { "p", "v" } }));
        }

        [Fact]
        public async Task SubscribeAsync()
        {
            using var _client = CreateClient();
            await _client.SubscribeAsync("key1");
            await _client.SubscribeAsync("key2", new Dictionary<string, string> { { "p", "v" } });

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null));
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", new Dictionary<string, string> { { "p", "v" } }));
        }

        [Fact]
        public void Unsubscribe()
        {
            using var _client = CreateClient();
            _client.Unsubscribe("key1");

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsUnsubscribeCommand("key1"));
        }

        [Fact]
        public void SubscribeConnectFailed()
        {
            using var _client = CreateClient(invalidUrl: true);
            Assert.Throws<WebSocketException>(() => _client.Subscribe("key1"));
        }

        [Fact]
        public void SubscribeConnectFailedAsync()
        {
            using var _client = CreateClient(invalidUrl: true);
            Assert.ThrowsAsync<WebSocketException>(() => _client.SubscribeAsync("key1"));
        }

        [Fact]
        public void ReceiveSubscribeMessage()
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient());
            _scenario.Client("1:Subscribe", "key1");
            _scenario.Client("2:Subscribe", "key2");
            _scenario.Client("3:Subscribe", "key3");
            _scenario.Client("4:Subscribe", "key4");
            _scenario.Server("Send subscribe after 1", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Server("Send subscribe after 2", "key2", ReceiveMessageStatusCode.Not_Found_App);
            _scenario.Server("Send subscribe after 3", "key3", ReceiveMessageStatusCode.Not_Found_KeyPath);
            _scenario.Server("Send subscribe after 4", "key4", ReceiveMessageStatusCode.Already_Exists);
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success));
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Not_Found_App));
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key3", ReceiveMessageStatusCode.Not_Found_KeyPath));
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key4", ReceiveMessageStatusCode.Already_Exists));
        }

        [Fact]
        public void ReceiveUnsubscribeMessage()
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient());
            _scenario.Client("1:Unsubscribe", "key1");
            _scenario.Server("Send unsubscribe after 1", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsUnsubscribeCommand("key1")).NextBy("Subscriber received: {0}", ReceiveMessage.Unsubscribe("key1", ReceiveMessageStatusCode.Success));
        }

        [Fact]
        public void ReceiveChangedMessage()
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient());
            _scenario.Client("1:Subscribe", "key1");
            var _configKeys = new[] { new ConfigKey { Path = "key1", DataType = ConfigKeyType.Integer, Value = "1" }, new ConfigKey { Path = "key2", DataType = ConfigKeyType.Boolean, Value = "true" } };
            _scenario.Server("Send changed after 1", (object)_configKeys);
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Changed(_configKeys));
        }

        [Fact]
        public void ReceiveDeletedMessage()
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient());
            _scenario.Client("1:Subscribe", "key1");
            _scenario.Server("Send deleted after 1", "key1");
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null)).NextBy("Subscriber received: {0}", ReceiveMessage.Deleted("key1"));
        }

        [Theory]
        [InlineData(WebSocketCloseStatus.NormalClosure)]
        [InlineData(WebSocketCloseStatus.EndpointUnavailable)]
        [InlineData(WebSocketCloseStatus.ProtocolError)]
        [InlineData(WebSocketCloseStatus.InvalidMessageType)]
        [InlineData(WebSocketCloseStatus.Empty)]
        [InlineData(WebSocketCloseStatus.InvalidPayloadData)]
        [InlineData(WebSocketCloseStatus.PolicyViolation)]
        [InlineData(WebSocketCloseStatus.MessageTooBig)]
        [InlineData(WebSocketCloseStatus.MandatoryExtension)]
        [InlineData(WebSocketCloseStatus.InternalServerError)]
        public void ReceiveClosedMessage(WebSocketCloseStatus closeStatus)
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient());
            _scenario.Client("1:Subscribe", "key1");
            _scenario.Server($"Send closed({closeStatus:d}) after 1");
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null)).NextBy($"Subscriber closed({closeStatus:d})");
        }

        [Fact]
        public void CloseAndReconnect()
        {
            using var _scenario = new SubscriberScenarioManager(CreateClient(reconnect: true));
            _scenario.Client("1:Subscribe", "key1");
            _scenario.Client("2:Subscribe", "key2");
            _scenario.Client("3:Subscribe after s19", "key3");
            _scenario.Server("s1:Send subscribe after 1", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Server("s2:Send subscribe after 2", "key2", ReceiveMessageStatusCode.Success);
            _scenario.Server("s3:Send changed after s1", (object)new[] { new ConfigKey("key1", ConfigKeyType.Integer, "1") });
            _scenario.Server("s4:Send changed after s2", (object)new[] { new ConfigKey("key2", ConfigKeyType.Integer, "2") });
            _scenario.Server("s5:Continue after s3,s4");
            _scenario.Server("s6:Send closed(1000)");
            _scenario.Server("s7:Continue after 3000 ms");
            _scenario.Server("s8:Send subscribe", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Server("s9:Send subscribe", "key2", ReceiveMessageStatusCode.Success);
            _scenario.Server("s10:Continue after s8,s9");
            _scenario.Server("s11:Send changed", (object)new[] { new ConfigKey("key1", ConfigKeyType.Integer, "3") });
            _scenario.Server("s12:Send changed", (object)new[] { new ConfigKey("key2", ConfigKeyType.Integer, "4") });
            _scenario.Server("s13:Continue after s11,s12");
            _scenario.Server("s14:Unavailable 4 attempts");
            _scenario.Server("s15:Send closed(1001)");
            _scenario.Server("s16:Continue after 3000 ms");
            _scenario.Server("s17:Send subscribe after s7", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Server("s18:Send subscribe after s7", "key2", ReceiveMessageStatusCode.Success);
            _scenario.Server("s19:Continue after s17,s18");
            _scenario.Server("s20:Send subscribe after 3", "key3", ReceiveMessageStatusCode.Success);
            _scenario.Server("s21:Send changed", (object)new[] { new ConfigKey("key3", ConfigKeyType.Integer, "5") });
            _scenario.Server("s22:Send closed(1002)");
            _scenario.Server("s23:Continue after 3000 ms");
            _scenario.Server("s24:Send subscribe", "key1", ReceiveMessageStatusCode.Success);
            _scenario.Server("s25:Send subscribe", "key2", ReceiveMessageStatusCode.Success);
            _scenario.Server("s26:Send subscribe", "key3", ReceiveMessageStatusCode.Success);
            _scenario.Wait(1000);

            var _connected = ATLAssert.Expect("Subscriber connected");
            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Changed(new[] { new ConfigKey("key1", ConfigKeyType.Integer, "1") }));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Success))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Changed(new[] { new ConfigKey("key2", ConfigKeyType.Integer, "2") }));

            _connected = _connected.NextBy("Subscriber closed(1000)")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber connected");
            _connected.NextBy("Subscriber reconnected");

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber received: {0}", ReceiveMessage.Changed(new[] { new ConfigKey("key1", ConfigKeyType.Integer, "3") }));
            _connected.NextBy("Subscriber received: {0}", ReceiveMessage.Changed(new[] { new ConfigKey("key2", ConfigKeyType.Integer, "4") }));

            _connected = _connected.NextBy("Subscriber closed(1001)")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber reconnect failed")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber reconnect failed")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber reconnect failed")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber reconnect failed")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber connected");
            _connected.NextBy("Subscriber reconnected");

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key3", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key3", ReceiveMessageStatusCode.Success))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Changed(new[] { new ConfigKey("key3", ConfigKeyType.Integer, "5") }));

            _connected = _connected.NextBy("Subscriber closed(1002)")
                .NextBy("Subscriber reconnecting")
                .NextBy("Subscriber connected");
            _connected.NextBy("Subscriber reconnected");

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key1", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key1", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key2", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key2", ReceiveMessageStatusCode.Success));

            _connected.NextBy("Subscriber send: {0}", new WsSubscribeCommand("key3", null))
                .NextBy("Subscriber received: {0}", ReceiveMessage.Subscribe("key3", ReceiveMessageStatusCode.Success));
        }
    }
}