using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal abstract class Scenario
    {
        protected readonly Match Match;
        protected readonly object[] Args;
        public Scenario(Match match, object[] args) { Match = match; Args = args; }
        public abstract Task Run(IDictionary<object, object> parameters);
        public T GetArg<T>(int index) => Args != null && index < Args.Length ? (T)Args[index] : default;
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

    [Scenario(@"^(?:(?<release>\w+)\:)?Subscribe\s*(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?$")]
    internal class SubscriberSubscribeScenario : Scenario
    {
        public SubscriberSubscribeScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _path = GetArg<string>(0);
            var _props = GetArg<IDictionary<string, string>>(1);

            if (_props == null)
                await ((IAjusteeClient)parameters[typeof(IAjusteeClient)]).SubscribeAsync(_path);
            else
                await ((IAjusteeClient)parameters[typeof(IAjusteeClient)]).SubscribeAsync(_path, _props);

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^(?:(?<release>\w+)\:)?Send\s*(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?$")]
    internal class WebSocketSendScenario : Scenario
    {
        public WebSocketSendScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _message = GetArg<ReceiveMessage>(0);
            await ((ISocketServer)parameters[typeof(ISocketServer)]).Send(MessageEncoding.GetBytes(JsonSerializer.Serialize(_message)));

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^(?:(?<release>\w+)\:)?Start\s+server(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?$")]
    internal class StartServerTriggerScenario : Scenario
    {
        public StartServerTriggerScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            await ((ISocketServer)parameters[typeof(ISocketServer)]).Start();

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^(?:(?<release>\w+)\:)?Stop\s+server(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?$")]
    internal class StopServerTriggerScenario : Scenario
    {
        public StopServerTriggerScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            await ((ISocketServer)parameters[typeof(ISocketServer)]).Stop();

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }
}
