using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Ajustee.Helper;
using System.Net.WebSockets;

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

    [Scenario(@"^\s*(?:(?<release>\w+)\s*\:\s*)?(?<method>Subscribe|Unsubscribe)\s*(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?\s*$")]
    internal class SubscriberSubscriptionScenario : Scenario
    {
        public SubscriberSubscriptionScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _client = ((IAjusteeClient)parameters[typeof(IAjusteeClient)]);
            var _method = Match.Groups["method"].Value;

            switch (_method)
            {
                case "Subscribe":
                    {
                        var _path = GetArg<string>(0);
                        var _props = GetArg<IDictionary<string, string>>(1);
                        if (_props == null)
                            await _client.SubscribeAsync(_path);
                        else
                            await _client.SubscribeAsync(_path, _props);
                        break;
                    }

                case "Unsubscribe":
                    {
                        var _path = GetArg<string>(0);
                        await _client.UnsubscribeAsync(_path);
                        break;
                    }
            }

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^\s*(?:(?<release>\w+)\s*\:\s*)?Send\s+(?<type>subscribe|unsubscribe|changed|deleted|closed\((?<ccode>10(?:00|01|02|03|05|07|08|09|10|11))\))(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<trigger>\w+)))?\s*$")]
    internal class ServerSendScenario : Scenario
    {
        public ServerSendScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _server = ((ISocketServer)parameters[typeof(ISocketServer)]);
            var _type = Match.Groups["type"].Value;

            switch (_type)
            {
                case ReceiveMessage.SubscribeType:
                    {
                        var _path = GetArg<string>(0);
                        var _statusCode = GetArg<ReceiveMessageStatusCode>(1);
                        _server.Send(MessageEncoding.GetBytes(JsonSerializer.Serialize(ReceiveMessage.Subscribe(_path, _statusCode))));
                        break;
                    }

                case ReceiveMessage.UnsubscribeType:
                    {
                        var _path = GetArg<string>(0);
                        var _statusCode = GetArg<ReceiveMessageStatusCode>(1);
                        _server.Send(MessageEncoding.GetBytes(JsonSerializer.Serialize(ReceiveMessage.Unsubscribe(_path, _statusCode))));
                        break;
                    }

                case ReceiveMessage.ChangedType:
                    {
                        var _configKeys = GetArg<IEnumerable<ConfigKey>>(0);
                        _server.Send(MessageEncoding.GetBytes(JsonSerializer.Serialize(ReceiveMessage.Changed(_configKeys))));
                        break;
                    }

                case ReceiveMessage.DeletedType:
                    {
                        var _path = GetArg<string>(0);
                        _server.Send(MessageEncoding.GetBytes(JsonSerializer.Serialize(ReceiveMessage.Deleted(_path))));
                        break;
                    }

                default:
                    {
                        var _closeStatus = int.Parse(Match.Groups["ccode"].Value);
                        _server.Send(_closeStatus);
                        break;
                    }

            }

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^\s*(?:(?<release>\w+)\s*\:\s*)?Unavailable(?:\s+(?<attempts>\d+)\s+attempt[s]?)(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<triggers>[\w\,]+)))?\s*$")]
    internal class ServerUnavailableScenario : Scenario
    {
        public ServerUnavailableScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _trigger = Match.Groups["trigger"].Value;
            await (string.IsNullOrEmpty(_trigger) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_trigger));

            var _server = ((ISocketServer)parameters[typeof(ISocketServer)]);
            var _attempts = Match.Groups["attempts"].Value;

            _server.Unavailable(int.Parse(_attempts));

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }

    [Scenario(@"^\s*(?:(?<release>\w+)\s*\:\s*)?Continue\s*(?:\s+after\s+(?:(?:(?<after>\d+)\s+ms)|(?<triggers>[\w\,]+)))?\s*$")]
    internal class ContinueScenario : Scenario
    {
        public ContinueScenario(Match match, object[] args) : base(match, args) { }

        public override async Task Run(IDictionary<object, object> parameters)
        {
            if (!int.TryParse(Match.Groups["after"].Value, out var _delay)) _delay = 1;
            var _triggers = Match.Groups["triggers"].Value;
            await (string.IsNullOrEmpty(_triggers) ? Task.Delay(_delay) : ((Trigger)parameters[typeof(Trigger)]).WaitAsync(_triggers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));

            ((Trigger)parameters[typeof(Trigger)]).Release(Match.Groups["release"].Value);
        }
    }
}
