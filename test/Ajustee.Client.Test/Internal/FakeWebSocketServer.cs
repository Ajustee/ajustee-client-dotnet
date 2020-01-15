using Fleck;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeWebSocketServer : IDisposable
    {
        #region Private fields region

        private readonly WebSocketServer m_Server;
        private readonly IList<string> m_Scenarios;
        private readonly Trigger m_Trigger;
        private IWebSocketConnection m_Connection;
        private SemaphoreSlim m_ServerRunning;

        #endregion

        #region Private methods region

        private async Task ClientSenarioImpl()
        {
            try
            {
                using var _enumerator = new ScenarioEnumerator(m_Scenarios);
                while (_enumerator.MoveNext())
                {
                    var _parameters = new Dictionary<object, object>
                    {
                        [typeof(FakeWebSocketServer)] = this,
                        [typeof(Trigger)] = m_Trigger
                    };
                    await _enumerator.Current.Run(_parameters);
                }
            }
            finally
            {
                if (m_ServerRunning != null)
                    await m_ServerRunning.WaitAsync();
            }
        }

        private void StartServer(IWebSocketConnection config)
        {
            config.OnOpen = OnOpen;
            config.OnMessage = OnMessage;
            config.OnClose = OnClose;
            m_Connection = config;
        }

        private void OnOpen()
        {
        }

        private void OnMessage(string message)
        { }

        private void OnClose()
        {
            m_ServerRunning.Release();
        }

        #endregion

        public FakeWebSocketServer(IList<string> scenarios, Trigger trigger)
        {
            m_Server = new WebSocketServer("ws://127.0.0.1:8181");
            m_Scenarios = scenarios ?? new string[0];
            m_Trigger = trigger;
        }

        public Task Wait()
        {
            return Task.Run(ClientSenarioImpl);
        }

        public Task<bool> Start()
        {
            if (m_Connection == null)
            {
                if (m_ServerRunning == null)
                    m_ServerRunning = new SemaphoreSlim(1, 0);

                m_Server.Start(StartServer);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> Stop()
        {
            if (m_Connection != null)
            {
                m_Connection.Close();
                m_Connection = null;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public async Task<bool> SendConfigKey(IEnumerable<ConfigKey> configKeys)
        {
            return false;
        }

        public void Dispose()
        {
            m_Server.Dispose();
        }
    }
}
