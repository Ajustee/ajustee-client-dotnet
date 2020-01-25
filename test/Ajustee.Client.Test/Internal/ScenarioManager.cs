using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class ScenarioManager : IDisposable
    {
        private static readonly KeyValuePair<ScenarioAttribute, Type>[] m_ScenatioAttributes;

        private readonly IAjusteeClient m_Client;
        private readonly List<Scenario> m_ClientScenarios = new List<Scenario>();
        private readonly List<Scenario> m_ServerScenarios = new List<Scenario>();
        private readonly Trigger m_Trigger = new Trigger();

        private static Scenario Parse(string scenario, object[] args)
        {
            foreach (var entry in m_ScenatioAttributes)
            {
                if (entry.Key.Match(scenario, out var _match))
                    return (Scenario)Activator.CreateInstance(entry.Value, _match, args);
            }
            throw new ArgumentException($"Invalid scenario '{scenario}'");
        }

        private async Task SenarioImpl(IList<Scenario> scenarios)
        {
            foreach (var _scenario in scenarios)
            {
                var _parameters = new Dictionary<object, object>
                {
                    [typeof(IAjusteeClient)] = m_Client,
                    [typeof(Trigger)] = m_Trigger
                };
                await _scenario.Run(_parameters);
            }
        }

        static ScenarioManager()
        {
            m_ScenatioAttributes = typeof(Scenario).Assembly.GetTypes().Where(t => t.BaseType == typeof(Scenario)).Select(t => new KeyValuePair<ScenarioAttribute, Type>((ScenarioAttribute)t.GetCustomAttributes(typeof(ScenarioAttribute), false).First(), t)).ToArray();
        }

        public ScenarioManager(IAjusteeClient client)
            : base()
        {
            m_Client = client;
        }

        public void Client(string scenario, params object[] args)
        {
            m_ClientScenarios.Add(Parse(scenario, args));
        }

        public void Server(string scenario, params object[] args)
        {
            m_ServerScenarios.Add(Parse(scenario, args));
        }

        public void Wait()
        {
            WaitAsync().GetAwaiter().GetResult();
        }

        public async Task WaitAsync()
        {
            await Task.WhenAll(
                Task.Run(() => SenarioImpl(m_ClientScenarios)),
                Task.Run(() => SenarioImpl(m_ServerScenarios)));
        }

        public void Dispose()
        {
            m_Client.Dispose();
        }
    }
}
