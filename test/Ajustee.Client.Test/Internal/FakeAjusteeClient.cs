
namespace Ajustee
{
    internal class FakeAjusteeClient : AjusteeClient
    {
        public FakeAjusteeClient(AjusteeConnectionSettings settings)
            : base(settings)
        {
            ATL.Enabled = true;
        }

#if SUBSCRIBE
        internal override ISubscriber CreateSubscriber()
        {
            return new Subscriber<FakeSocketClient>(Settings, keys => OnConfigKeyChanged(new AjusteeConfigKeyEventArgs(keys)));
        }
#endif
    }
}
