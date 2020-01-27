using System;

namespace Ajustee
{
    internal class FakeAjusteeClient : AjusteeClient
    {
        public static readonly Uri ValidUri = new Uri("http://valid.url");
        public static readonly Uri InvalidUri = new Uri("http://invalid.url");

        public FakeAjusteeClient(AjusteeConnectionSettings settings)
            : base(settings)
        {
            ATL.Enabled = true;
        }

#if SUBSCRIBE
        internal override ISubscriber CreateSubscriber()
        {
            return new Subscriber<FakeSocketClient>(Settings,
                k => OnChanged(new AjusteeConfigKeyChangedEventArgs(k)),
                p => OnDeleted(new AjusteeConfigKeyDeletedEventArgs(p)))
            {
                ReconnectInitDelay = 0 // Skips increasing delay
            };
        }
#endif
    }
}
