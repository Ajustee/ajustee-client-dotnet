﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee.Tools
{
    internal class AjusteeClientTool
    {
        #region Private fields region

        private const string m_AppIdName = "x-app-key";
        private const string m_KeyPathName = "x-key-path";
        private const string m_KeyPropsName = "x-key-props";

        private static readonly object m_WriteSyncRoot = new object();

        #endregion

        #region Private methods region

        private static AjusteeClient CreateClient(string apiUrl, string appId)
        {
            return new AjusteeClient(new AjusteeConnectionSettings { ApiUrl = new Uri(apiUrl), ApplicationId = appId });
        }

        private static string ReadLine(bool indend = true)
        {
            var _value = Console.ReadLine();
            if (indend) Console.Write("> ");
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

        private static Task Subscribe(AjusteeClient client, CancellationToken cancellationToken, Func<string, object> messageFilter)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var _message = messageFilter(ReadLine());

                    if (_message is KeyValuePair<string, IDictionary<string, string>> _subsribe)
                        await client.SubscribeAsync(_subsribe.Key, _subsribe.Value, cancellationToken: cancellationToken);
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
                Console.Write("Enter url: "); var _url = ReadLine(indend: false);
                Console.Write("Enter app-id: "); var _appId = ReadLine();

                var _cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    var _client = CreateClient(_url, _appId);
                    _client.ConfigKeyChanged += (_, e) => WriteLine(JsonSerializer.Serialize(e.ConfigKeys));

                    await Subscribe(_client, cancellationToken: _cancellationTokenSource.Token, m =>
                    {
                        if (string.Equals(m, "quit", StringComparison.OrdinalIgnoreCase))
                        {
                            _cancellationTokenSource.Cancel();
                            _client.Dispose();
                            return false;
                        }
                        else if (string.Equals(m, "subscribe", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.Write("Enter key path: "); var _keyPath = ReadLine();
                            if (_keyPath == "exit") return null;
                            Console.Write("Enter properties: "); var _properties = ReadLine();
                            if (_properties == "exit") return null;
                            return KeyValuePair.Create(_keyPath, JsonSerializer.Deserialize<IDictionary<string, string>>(_properties));
                        }
                        return null;
                    });
                }
                catch (Exception _ex)
                {
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
