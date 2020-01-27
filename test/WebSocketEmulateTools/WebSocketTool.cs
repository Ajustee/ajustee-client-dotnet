using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee.Tools
{
    internal class WebSocketTool
    {
        #region Private fields region

        private const string m_AppIdName = "x-app-key";
        private const string m_KeyPathName = "x-key-path";
        private const string m_KeyPropsName = "x-key-props";

        private static readonly object m_WriteSyncRoot = new object();

        #endregion

        #region Private methods region

        private static ClientWebSocket CreateSocket(string appId, string keyPath, string properties)
        {
            var _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader(m_AppIdName, appId);
            _socket.Options.SetRequestHeader(m_KeyPathName, keyPath);
            _socket.Options.SetRequestHeader(m_KeyPropsName, properties);
            return _socket;
        }

        private static string ReadLine()
        {
            var _value = Console.ReadLine();
            Console.Write("> ");
            return _value;
        }

        private static void WriteLine(string value)
        {
            lock (m_WriteSyncRoot)
            {
                Console.WriteLine($"{value}");
                Console.Write("> ");
            }
        }

        private static Task Recieve(WebSocket socket, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var _message = new StringBuilder();
                    try
                    {
                        var _segment = new ArraySegment<byte>(new byte[4096]);
                        WebSocketReceiveResult _result = null;
                        do
                        {
                            _result = await socket.ReceiveAsync(_segment, cancellationToken);

                            if (_result.MessageType == WebSocketMessageType.Close)
                                throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely, _result.CloseStatusDescription);

                            _message.Append(Encoding.UTF8.GetString(_segment.Array, 0, _result.Count));
                        }
                        while (!_result.EndOfMessage);
                    }
                    catch (WebSocketException _ex) when (_ex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                    {
                        _message.Append(_ex.Message);
                    }

                    WriteLine(_message.ToString());
                    _message.Clear();
                }
            }, cancellationToken);
        }

        private static Task Send(WebSocket socket, CancellationToken cancellationToken, Predicate<string> messageFilter)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (socket.State != WebSocketState.Open) break;
                    var _message = ReadLine();
                    if (socket.State != WebSocketState.Open) break;
                    if (messageFilter(_message))
                        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(_message)), WebSocketMessageType.Text, true, cancellationToken);
                }
            }, cancellationToken);
        }

        #endregion

        #region Public methods region

        public static async Task ExecuteAsync()
        {
            Console.WriteLine("Ajustee web socket tools");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter url: "); var _url = Console.ReadLine();
                Console.Write("Enter app-id: "); var _appId = Console.ReadLine();
                Console.Write("Enter key path: "); var _keyPath = Console.ReadLine();
                Console.Write("Enter properties: "); var _properties = Console.ReadLine();

                var _cancellationTokenSource = new CancellationTokenSource();
                ClientWebSocket _socket = null;
                try
                {
                    _socket = CreateSocket(_appId, _keyPath, _properties);
                    await _socket.ConnectAsync(new Uri(_url), _cancellationTokenSource.Token);
                    WriteLine("Connected (press CTRL+C to quit)");

                    await Task.WhenAll(Recieve(_socket, _cancellationTokenSource.Token), Send(_socket, _cancellationTokenSource.Token, m =>
                    {
                        if (string.Equals(m, "quit", StringComparison.OrdinalIgnoreCase))
                        {
                            _cancellationTokenSource.Cancel();
                            _socket.Abort();
                            return false;
                        }
                        return true;
                    }));
                }
                catch (WebSocketException _ex) when (_ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    Console.WriteLine($"Disconnected: code ({(int?)_socket.CloseStatus})");
                }
                catch (Exception _ex)
                {
                    if (_socket.CloseStatus != null)
                        Console.WriteLine($"Disconnected: code {_socket.CloseStatus}");
                    else
                        Console.WriteLine($"Occured error: {_ex.Message}");
                }

                Console.WriteLine();
                Console.Write("Terminate all? (Y/N)");
                var _answer = Console.ReadLine();
                if (_answer.ToUpperInvariant() == "Y") break;
            }
        }

        #endregion
    }
}
