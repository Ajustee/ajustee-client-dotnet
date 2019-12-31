
namespace Ajustee
{
    internal class FakeAjusteeClient : AjusteeClient
    {
        private string[] m_ReceiveScenarioSteps;

        public FakeAjusteeClient(AjusteeConnectionSettings settings)
            : base(settings)
        { }

        public void SetReceiveScenario(params string[] steps)
        {
            m_ReceiveScenarioSteps = steps;
        }

#if SUBSCRIBE
        internal FakeSubscriber Subscriber;

        internal override Subscriber CreateSubscriber()
        {
            return Subscriber = new FakeSubscriber(Settings, m_ReceiveScenarioSteps);
        }
#endif
    }
}
