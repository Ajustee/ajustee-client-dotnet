using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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
    public class SubscriptionTest
    {
        [Fact]
        public async Task Test1()
        {
            var _socket = new ClientWebSocket();
            await _socket.ConnectAsync(new Uri("wss://viz8masph1.execute-api.us-west-2.amazonaws.com/demo?_keypath=key1"), CancellationToken.None);

            string _connectionId = "12";
            string _message = "{\"action\":\"notify\",\"data\":[\"" + _connectionId + "\"]}";
            var _buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(_message));
            await _socket.SendAsync(_buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            //_buffer = new ArraySegment<byte>();
            var _result = await _socket.ReceiveAsync(_buffer, CancellationToken.None);
        }

    }
}