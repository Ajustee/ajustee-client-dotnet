using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeAjusteeClient : AjusteeClient
    {
        public readonly List<string> Output = new List<string>();

        public FakeAjusteeClient(AjusteeConnectionSettings settings)
            : base(settings)
        {
#if SUBSCRIBE
            m_Subscriber = new FakeSubscriber(Settings, this);
#endif
        }

#if SUBSCRIBE
        private readonly FakeSubscriber m_Subscriber;

        public void SetSubscribeScenario(params string[] steps)
        {
            foreach (var _step in steps)
                m_Subscriber.SubscribeScenarioSteps.Enqueue(_step);
        }

        public void SetReceiveScenario(params string[] steps)
        {
            foreach (var _step in steps)
                m_Subscriber.ReceiveScenarioSteps.Enqueue(_step);
        }

        public Task WaitScenario()
        {
            return m_Subscriber.WaitScenario();
        }

        internal override Subscriber CreateSubscriber()
        {
            return m_Subscriber;
        }
#endif
    }
}
