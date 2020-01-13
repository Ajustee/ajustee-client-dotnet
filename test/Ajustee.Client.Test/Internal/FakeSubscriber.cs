using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class FakeSubscriber : Subscriber
    {
        private readonly FakeAjusteeClient m_Client;
        public Queue<string> SubscribeScenarioSteps = new Queue<string>();
        public Queue<string> ReceiveScenarioSteps = new Queue<string>();
        private Task SubscribeTask;
        private SemaphoreSlim ReceiveScenarioWaiter = new SemaphoreSlim(0, 1);
        private int m_CurrentSubscribeFailed;
        private Dictionary<string, bool> m_Triggers = new Dictionary<string, bool>();

        public FakeSubscriber(AjusteeConnectionSettings settings, FakeAjusteeClient client)
            : base(settings)
        {
            m_Client = client;
            ReconnectInitDelay = 1;
        }

        private async Task SubscribeScenarioImpl()
        {
            while (SubscribeScenarioSteps.Count > 0)
            {
                var _scenario = SubscribeScenarioSteps.Dequeue();

                var _match = Regex.Match(_scenario, @"Subscribe\s+(?<result>failed|success)\s+on\s+(?<path>.+?)(?:\s+with\s+(?<props>.+?))?\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+))", RegexOptions.IgnoreCase);
                var _failed = _match.Groups["result"].Value == "failed";
                if (!int.TryParse(_match.Groups["after"].Value, out var _delay)) _delay = 1;
                var _trigger = _match.Groups["trigger"].Value;
                var _path = _match.Groups["path"].Value;
                var _propsGroup = _match.Groups["props"];
                var _props = _propsGroup.Success ? JsonSerializer.Deserialize<IDictionary<string, string>>(_propsGroup.Value) : null;

                if (!string.IsNullOrEmpty(_trigger))
                {
                    SpinWait.SpinUntil(() => { lock (m_Triggers) { return m_Triggers.TryGetValue(_trigger, out var _trigEnabled) && _trigEnabled; } });
                    System.Diagnostics.Debug.WriteLine("WAITED");
                }

                await Task.Delay(_delay);
                m_CurrentSubscribeFailed = _failed ? 1 : 0;
                await SubscribeAsync(_path, _props);
            }
        }

        public async Task WaitScenario()
        {
            var _waiters = new List<Task>(2);
            if (SubscribeScenarioSteps.Count != 0)
            {
                SubscribeTask = Task.Run(SubscribeScenarioImpl);
                _waiters.Add(SubscribeTask);
            }
            if (ReceiveScenarioSteps.Count != 0)
            {
                _waiters.Add(ReceiveScenarioWaiter.WaitAsync());
            }
            await Task.WhenAll(_waiters);
        }

        protected override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var _result = $"Connect";
            try
            {
                await Task.Delay(1);
                if (m_CurrentSubscribeFailed > 0)
                {
                    m_CurrentSubscribeFailed--;
                    throw new Exception("Connection failed");
                }

                _result += ": OK";
            }
            catch (Exception)
            {
                _result += ": FAILED";
                throw;
            }
            finally
            {
                m_Client.Output.Add(_result);
            }
        }

        protected override async Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            var _result = "Send";
            try
            {
                switch (command)
                {
                    case WsSubscribeCommand _subCommand:
                        {
                            var _subscribeData = (WsSubscribeCommand.SubscribeData)_subCommand.Data;
                            _result += $"Subscribe({_subscribeData.Path}, {(_subscribeData.Props == null ? "" : JsonSerializer.Serialize(_subscribeData.Props))})";
                            await Task.Delay(1);

                            if (m_CurrentSubscribeFailed > 0)
                            {
                                m_CurrentSubscribeFailed--;
                                throw new Exception("Send subscribe failed");
                            }

                            _result += ": OK";
                            break;
                        }
                }
            }
            catch (Exception)
            {
                _result += ": FAILED";
                throw;
            }
            finally
            {
                m_Client.Output.Add(_result);
            }
        }

        protected override async Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            if (ReceiveScenarioSteps == null || ReceiveScenarioSteps.Count == 0)
            {
                ReceiveScenarioWaiter.Release();
                throw new TaskCanceledException("Receive completed");
            }

            var _scenario = ReceiveScenarioSteps.Dequeue();

            var _match = Regex.Match(_scenario, @"Receive\s+(?<type>config\skeys|info|reset|failed|closed)(?:\s+for\s+(?<attempts>\d+)\s+attempt[s]?)?(?:\s+(?<data>.+?))?\s+after\s+(?<after>\d+)\s+ms", RegexOptions.IgnoreCase);
            if (_match.Success)
            {
                await Task.Delay(int.Parse(_match.Groups["after"].Value), cancellationToken);

                string _action = null;
                object _data = null;
                switch (_match.Groups["type"].Value.ToLowerInvariant())
                {
                    case "config keys":
                        _action = ReceiveMessage.ConfigKeys;
                        _data = JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(_match.Groups["data"].Value);
                        break;

                    case "info":
                        _action = ReceiveMessage.Info;
                        _data = _match.Groups["data"].Value;
                        break;

                    case "reset":
                        _action = ReceiveMessage.Reset;
                        _data = null;
                        break;

                    case "failed":
                        m_Client.Output.Add($"Receive: FAILED");
                        throw new Exception("Receive failed");

                    case "closed":
                        m_Client.Output.Add($"Receive: CLOSED");
                        if (int.TryParse(_match.Groups["attempts"].Value, out var _attempts))
                            m_CurrentSubscribeFailed = _attempts;
                        throw new ConnectionClosedException(true);
                }

                var _receiveMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReceiveMessage { Type = _action, Data = _data }));
                stream.Write(_receiveMessage, 0, _receiveMessage.Length);

                return;
            }

            _match = Regex.Match(_scenario, @"Run\s+(?<trigger>\w+)", RegexOptions.IgnoreCase);
            if (_match.Success)
            {
                await Task.Delay(1);
                lock (m_Triggers) m_Triggers[_match.Groups["trigger"].Value] = true;
                    System.Diagnostics.Debug.WriteLine("TRIGGERED");
            }
        }

        protected override void OnReceiveMessage(ReceiveMessage message)
        {
            try
            {
                object _data = null;
                switch (message.Type)
                {
                    case ReceiveMessage.ConfigKeys:
                        _data = JsonSerializer.Serialize(message.Data);
                        break;

                    default:
                        _data = message.Data;
                        break;
                }
                m_Client.Output.Add($"Receive({message.Type}, {_data}): OK");
            }
            catch
            {
                m_Client.Output.Add($"Receive({message.Type}): FAILED");
            }
        }
    }
}
