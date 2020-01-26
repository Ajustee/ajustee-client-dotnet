using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    public partial class AjusteeClient
    {
        private ISubscriber m_Subscriber;
        private readonly object m_SubscriberSyncRoot = new object();

        internal virtual ISubscriber CreateSubscriber()
        {
            return new Subscriber<WebSocketClient>(Settings, keys => OnConfigKeyChanged(new AjusteeConfigKeyEventArgs(keys)));
        }

        internal ISubscriber Subscriber
        {
            get
            {
                if (m_Subscriber == null)
                {
                    lock (m_SubscriberSyncRoot)
                    {
                        if (m_Subscriber == null)
                        {
                            // Creates a new instance of the subscriber.
                            m_Subscriber = CreateSubscriber();

                            // Handle the dispose of the subscriber.
                            InvokeOnDispose(() =>
                            {
                                if (m_Subscriber != null)
                                {
                                    m_Subscriber.Dispose();
                                    m_Subscriber = null;
                                }
                            });
                        }
                    }
                }
                return m_Subscriber;
            }
        }

        public void Subscribe(string path)
        {
            Subscriber.Subscribe(path, null);
        }

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            Subscriber.Subscribe(path, properties);
        }

        public async Task SubscribeAsync(string path, CancellationToken cancellationToken = default)
        {
            await Subscriber.SubscribeAsync(path, null, cancellationToken: cancellationToken);
        }

        public async Task SubscribeAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken = default)
        {
            await Subscriber.SubscribeAsync(path, properties, cancellationToken);
        }


        public void Unsubscribe(string path)
        {
            Subscriber.Unsubscribe(path);
        }

        public async Task UnsubscribeAsync(string path, CancellationToken cancellationToken = default)
        {
            await Subscriber.UnsubscribeAsync(path, cancellationToken);
        }

        /// <summary>
        /// Occurs when configuration key has been changed.
        /// </summary>
        public event AjusteeConfigKeyEventHandler ConfigKeyChanged;

        protected void OnConfigKeyChanged(AjusteeConfigKeyEventArgs e)
        {
            ConfigKeyChanged?.Invoke(this, e);
        }
    }
}
