
namespace Ajustee
{
    internal class FakeAjusteeClient : AjusteeClient
    {
        public FakeAjusteeClient(AjusteeConnectionSettings settings)
            : base(settings)
        { }

#if SUBSCRIBE
        internal FakeSubscriber Subscriber;

        internal override Subscriber CreateSubscriber()
        {
            return Subscriber = new FakeSubscriber(Settings);
        }
#endif
    }
}
