using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class SubscriberManager : IDisposable
    {
        #region Private fields region

        private readonly AjusteeClient m_Client;
        private FakeWebSocketServer m_Server;
        private readonly List<string> m_ClientScenarios = new List<string>();
        private readonly List<string> m_ServerScenarios = new List<string>();
        private readonly Trigger m_Trigger = new Trigger();
        private readonly TraceListener m_ClientTrace;
        private readonly TraceListener m_ServerTrace;

        #endregion

        #region Private methods region

        private async Task ClientSenarioImpl()
        {
            using var _enumerator = new ScenarioEnumerator(m_ClientScenarios);
            while (_enumerator.MoveNext())
            {
                var _parameters = new Dictionary<object, object> {
                    [typeof(IAjusteeClient)] = m_Client,
                    [typeof(Trigger)] = m_Trigger
                };
                await _enumerator.Current.Run(_parameters);
            }
        }

        #endregion

        #region Public constructors region

        public SubscriberManager(AjusteeClient client)
            : base()
        {
            m_Client = client;
        }

        #endregion

        #region Public methods region

        public void ClientScenario(string scenario)
        {
            m_ClientScenarios.Add(scenario);
        }

        public void ServerScenario(string scenario)
        {
            m_ServerScenarios.Add(scenario);
        }

        public async Task Wait()
        {
            if (m_Server == null)
                m_Server = new FakeWebSocketServer(m_ServerScenarios, m_Trigger);

            await Task.WhenAll(m_Server.Wait(), Task.Run(ClientSenarioImpl));
        }

        public void Dispose()
        {
            if (m_Server != null)
            {
                m_Server.Dispose();
                m_Server = null;
            }
        }

        #endregion
    }
}
