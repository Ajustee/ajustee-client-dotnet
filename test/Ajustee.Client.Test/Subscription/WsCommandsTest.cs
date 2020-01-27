using System.Text;
using System.Collections.Generic;

using static Ajustee.Helper;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using InlineData = NUnit.Framework.TestCaseAttribute;
#endif

namespace Ajustee
{
    public class WsCommandsTest
    {
        [Theory]
        [InlineData("key1", null, @"{""action"":""subscribe"",""data"":{""path"":""key1""}}")]
        [InlineData("key2", @"{""p1"":""v1"",""p2"":""v2""}", @"{""action"":""subscribe"",""data"":{""path"":""key2"",""props"":{""p1"":""v1"",""p2"":""v2""}}}")]
        public void SubscribeCommandSerialize(string path, string props, string expected)
        {
            var _command = new WsSubscribeCommand(path, props == null ? null : JsonSerializer.Deserialize<IDictionary<string, string>>(props));
            var commandBinary = _command.GetBinary();
            var actual = MessageEncoding.GetString(commandBinary.Array, 0, commandBinary.Count);
            Assert.True(expected == actual);
        }

        [Theory]
        [InlineData("key1", @"{""action"":""unsubscribe"",""data"":{""path"":""key1""}}")]
        [InlineData("key2", @"{""action"":""unsubscribe"",""data"":{""path"":""key2""}}")]
        public void UnsubscribeCommandSerialize(string path, string expected)
        {
            var _command = new WsUnsubscribeCommand(path);
            var commandBinary = _command.GetBinary();
            var actual = MessageEncoding.GetString(commandBinary.Array, 0, commandBinary.Count);
            Assert.True(expected == actual);
        }
    }
}