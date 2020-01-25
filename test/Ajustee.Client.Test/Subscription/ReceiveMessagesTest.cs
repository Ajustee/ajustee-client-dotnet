using System.Linq;
using System.Collections.Generic;

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
    public class ReceiveMessagesTest
    {
        [Fact]
        public void ChangedMessage()
        {
            var json = @"{""type"":""changed"",""data"":[{""path"":""p1"",""DataType"":""Integer"",""Value"":""1""},{""path"":""p2"",""DataType"":""Boolean"",""Value"":true}]}";
            var message = JsonSerializer.Deserialize<ReceiveMessage>(json);

            Assert.True(message.Type == "changed");
            Assert.True(typeof(IEnumerable<ConfigKey>).IsAssignableFrom(message.Data.GetType()));
            var data = ((IEnumerable<ConfigKey>)message.Data).ToArray();
            Assert.True(data.Length == 2);
            Assert.True(data[0].Path == "p1");
            Assert.True(data[0].DataType == ConfigKeyType.Integer);
            Assert.True(data[0].Value == "1");
            Assert.True(data[1].Path == "p2");
            Assert.True(data[1].DataType == ConfigKeyType.Boolean);
            Assert.True(data[1].Value == "true");

            var message2 = JsonSerializer.Deserialize<ReceiveMessage>(JsonSerializer.Serialize(message));

            Assert.True(message.Type == message2.Type);
            Assert.True(typeof(IEnumerable<ConfigKey>).IsAssignableFrom(message2.Data.GetType()));
            var data2 = ((IEnumerable<ConfigKey>)message2.Data).ToArray();
            Assert.True(data.Length == data2.Length);
            Assert.True(data[0].Path == data2[0].Path);
            Assert.True(data[0].DataType == data2[0].DataType);
            Assert.True(data[0].Value == data2[0].Value);
            Assert.True(data[1].Path == data2[1].Path);
            Assert.True(data[1].DataType == data2[1].DataType);
            Assert.True(data[1].Value == data2[1].Value);
        }

        [Theory]
        [InlineData(@"{""type"":""subscribe"",""data"":{""path"":""p1"",""statuscode"":""success""}}", "subscribe", "p1", ReceiveMessageStatusCode.Success)]
        [InlineData(@"{""type"":""subscribe"",""data"":{""path"":""p2"",""statuscode"":""not_found_app""}}", "subscribe", "p2", ReceiveMessageStatusCode.Not_Found_App)]
        [InlineData(@"{""type"":""subscribe"",""data"":{""path"":""p3"",""statuscode"":""not_found_keypath""}}", "subscribe", "p3", ReceiveMessageStatusCode.Not_Found_KeyPath)]
        [InlineData(@"{""type"":""subscribe"",""data"":{""path"":""p4"",""statuscode"":""already_exists""}}", "subscribe", "p4", ReceiveMessageStatusCode.Already_Exists)]
        [InlineData(@"{""type"":""unsubscribe"",""data"":{""path"":""p1"",""statuscode"":""success""}}", "unsubscribe", "p1", ReceiveMessageStatusCode.Success)]
        [InlineData(@"{""type"":""unsubscribe"",""data"":{""path"":""p2"",""statuscode"":""not_found_app""}}", "unsubscribe", "p2", ReceiveMessageStatusCode.Not_Found_App)]
        [InlineData(@"{""type"":""unsubscribe"",""data"":{""path"":""p3"",""statuscode"":""not_found_keypath""}}", "unsubscribe", "p3", ReceiveMessageStatusCode.Not_Found_KeyPath)]
        [InlineData(@"{""type"":""unsubscribe"",""data"":{""path"":""p4"",""statuscode"":""already_exists""}}", "unsubscribe", "p4", ReceiveMessageStatusCode.Already_Exists)]
        public void SubscribeMessage(string json, string type, string path, object statusCode)
        {
            var message = JsonSerializer.Deserialize<ReceiveMessage>(json);

            Assert.True(message.Type == type);
            Assert.True(message.Data.GetType() == typeof(SubscriptionMessageData));
            var data = (SubscriptionMessageData)message.Data;
            Assert.True(data.Path == path);
            Assert.True(object.Equals(data.StatusCode, statusCode));

            var message2 = JsonSerializer.Deserialize<ReceiveMessage>(JsonSerializer.Serialize(message));

            Assert.True(message.Type == message2.Type);
            Assert.True(message2.Data.GetType() == typeof(SubscriptionMessageData));
            var data2 = (SubscriptionMessageData)message2.Data;
            Assert.True(data.Path == data2.Path);
            Assert.True(data.StatusCode == data2.StatusCode);
        }

        [Theory]
        [InlineData(@"{""type"":""deleted"",""data"":""path""}", "deleted", "path")]
        public void DeletedMessage(string json, string type, string path)
        {
            var message = JsonSerializer.Deserialize<ReceiveMessage>(json);

            Assert.True(message.Type == type);
            Assert.True(message.Data.GetType() == typeof(string));
            Assert.True(object.Equals(message.Data, path));

            var message2 = JsonSerializer.Deserialize<ReceiveMessage>(JsonSerializer.Serialize(message));

            Assert.True(message.Type == message2.Type);
            Assert.True(message2.Data.GetType() == typeof(string));
            Assert.True(object.Equals(message.Data, message2.Data));
        }
    }
}