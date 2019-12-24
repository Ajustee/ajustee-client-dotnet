using static Ajustee.Helper;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using MemberData = NUnit.Framework.TestCaseSourceAttribute;
#endif

namespace Ajustee
{
    public class SubscriptionTest
    {
        #region Private fields region

        public static readonly object[][] ReceiveMessageDeserializeTestData = new object[][]
        {
            new object[] {
                new ReceiveMessage { Action = ReceiveMessage.ConfigKeys, Data = new[] { new ConfigKey { Path = "p", DataType = ConfigKeyType.Integer, Value = "v" } } },
                @"{""action"":""configkeys"",""data"":[{""path"": ""p"",""dataType"": ""Integer"",""value"":""v""}]}"
            }
        };

        #endregion

        #region Test methods

        [Theory]
        [MemberData(nameof(ReceiveMessageDeserializeTestData))]
        public void ReceiveMessageDeserialize(object expected, string json)
        {
            var _expected = (ReceiveMessage)expected;
            var _actual = JsonSerializer.Deserialize<ReceiveMessage>(json);
            Assert.True(object.Equals(_expected.Action, _actual.Action));
        }

        #endregion
    }
}