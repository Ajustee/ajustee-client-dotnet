using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Text;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class ScenarioEnumerator : IEnumerator<Scenario>
    {
        private static KeyValuePair<ScenarioAttribute, Type>[] m_Attributes;
        private readonly IList<string> m_Scenarios;
        private int m_CurrentIndex = -1;

        private static Scenario Parse(string scenario)
        {
            foreach (var entry in m_Attributes)
            {
                if (entry.Key.Match(scenario, out var _match))
                    return (Scenario)Activator.CreateInstance(entry.Value, _match);
            }
            throw new ArgumentException($"Invalid scenario '{scenario}'");
        }

        static ScenarioEnumerator()
        {
            m_Attributes = typeof(ScenarioEnumerator).Assembly.GetTypes().Where(t => t.BaseType == typeof(Scenario)).Select(t => new KeyValuePair<ScenarioAttribute, Type>((ScenarioAttribute)t.GetCustomAttributes(typeof(ScenarioAttribute), false).First(), t)).ToArray();
        }

        public ScenarioEnumerator(IList<string> scenarios)
        {
            m_Scenarios = scenarios ?? new string[0];
        }

        public Scenario Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        { }

        public bool MoveNext()
        {
            if (m_CurrentIndex + 1 < m_Scenarios.Count)
            {
                Current = Parse(m_Scenarios[++m_CurrentIndex]);
                return true;
            }
            return false;
        }

        public void Reset()
        { }
    }

    internal abstract class Scenario
    {
        protected readonly Match Match;
        public Scenario(Match match) => Match = match;
        public abstract Task Run(IDictionary<object, object> parameters);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class ScenarioAttribute : Attribute
    {
        private readonly string m_Pattern;

        public ScenarioAttribute(string pattern)
            : base()
        {
            m_Pattern = pattern;
        }

        public bool Match(string input, out Match match)
        {
            var _match = Regex.Match(input, m_Pattern, RegexOptions.IgnoreCase);
            if (_match.Success)
            {
                match = _match;
                return true;
            }
            match = null;
            return false;
        }
    }

    [Scenario(@"(?:(?<release>\w+)\:)?Subscribe\s+(?<result>failed|success)\s+on\s+(?<path>.+?)(?:\s+with\s+(?<props>.+?))?(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?")]
    internal class SubscriberSubscribeScenario : Scenario
    {
        public SubscriberSubscribeScenario(Match match) : base(match) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _propsGroup = Match.Groups["props"];
            var _path = Match.Groups["path"].Value;
            var _props = _propsGroup.Success ? JsonSerializer.Deserialize<IDictionary<string, string>>(_propsGroup.Value) : null;

            await ((IAjusteeClient)parameters[typeof(IAjusteeClient)]).SubscribeAsync(_path, _props);

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"(?:(?<release>\w+)\:)?Send\s+(?<result>config\skeys|info)(?:\s+(?<data>.+?))?(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?")]
    internal class WebSocketSendScenario : Scenario
    {
        public WebSocketSendScenario(Match match) : base(match) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            switch (Match.Groups["type"].Value.ToLowerInvariant())
            {
                case "config keys":
                    await ((FakeWebSocketServer)parameters[typeof(FakeWebSocketServer)]).SendConfigKey(JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(Match.Groups["data"].Value));
                    break;
            }

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"(?:(?<release>\w+)\:)?Start\s+server(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?")]
    internal class StartServerTriggerScenario : Scenario
    {
        public StartServerTriggerScenario(Match match) : base(match) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            await ((FakeWebSocketServer)parameters[typeof(FakeWebSocketServer)]).Start();

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"(?:(?<release>\w+)\:)?Stop\s+server(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?")]
    internal class StopServerTriggerScenario : Scenario
    {
        public StopServerTriggerScenario(Match match) : base(match) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            await ((FakeWebSocketServer)parameters[typeof(FakeWebSocketServer)]).Stop();

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }
}
