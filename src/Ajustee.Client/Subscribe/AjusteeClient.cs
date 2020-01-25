using System.Collections.Generic;
using System.Threading;

namespace Ajustee
{
    public partial class AjusteeClient
    {
        #region Private fields region

        private ISubscriber m_Subscriber;
        private readonly object m_SubscriberSyncRoot = new object();

        #endregion

        #region Private methods region

        internal virtual ISubscriber CreateSubscriber()
        {
            return new Subscriber<WebSocketClient>(Settings, keys => OnConfigKeyChanged(new AjusteeConfigKeyEventArgs(keys)));
        }

        private ISubscriber EnsureSubscriber()
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

        #endregion

        #region Public methods region

        public void Subscribe(string path)
        {
            Subscribe(path, null);
        }

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            EnsureSubscriber().Subscribe(path, properties);
        }

        public async System.Threading.Tasks.Task SubscribeAsync(string path, CancellationToken cancellationToken = default)
        {
            await SubscribeAsync(path, null);
        }

        public async System.Threading.Tasks.Task SubscribeAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken = default)
        {
            await EnsureSubscriber().SubscribeAsync(path, properties);
        }

        #endregion

        #region Public events region

        /// <summary>
        /// Occurs when configuration key has been changed.
        /// </summary>
        public event AjusteeConfigKeyEventHandler ConfigKeyChanged;

        #endregion

        #region Protected methods region

        protected void OnConfigKeyChanged(AjusteeConfigKeyEventArgs e)
        {
            ConfigKeyChanged?.Invoke(this, e);
        }

        #endregion
    }
}
