using System;
using System.Collections.Generic;

namespace Ajustee
{
    public partial class AjusteeClient : IAjusteeClient, IDisposable
    {
        #region Private fields region

        private static readonly IJsonSerializer m_JsonSerializer;

        private Action m_InvokeOnDispose;

        #endregion

        #region Private methods region

        private void InvokeOnDispose(Action callback)
        {
            m_InvokeOnDispose += callback;
        }

        #endregion

        #region Public constructors region

        /// <summary>
        /// Initializes a new instance of the class <see cref="AjusteeClient"/> class.
        /// </summary>
        public AjusteeClient()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the class <see cref="AjusteeClient"/> class.
        /// <paramref name="settings">Client connection settings.</paramref>
        /// </summary>
        public AjusteeClient(AjusteeConnectionSettings settings)
            : base()
        {
            Settings = settings ?? throw new ArgumentException(nameof(settings));
        }

        static AjusteeClient()
        {
            m_JsonSerializer = JsonSerializerFactory.Create();
        }

        ~AjusteeClient()
        {
            Dispose(false);
        }

        #endregion

        #region Protected methods region

        protected virtual void Dispose(bool disposing)
        {
            m_InvokeOnDispose?.Invoke();
        }

        #endregion

        #region Public methods region

        /// <summary>
        /// Gets all configurations
        /// </summary>
        /// <returns>Returns enumerable configurations.</returns>
        public IEnumerable<ConfigKey> GetConfigurations()
        {
            return GetConfigurations(null, null);
        }

        /// <summary>
        /// Gets all configurations
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<ConfigKey> GetConfigurations(string path)
        {
            return GetConfigurations(path, null);
        }

        public IEnumerable<ConfigKey> GetConfigurations(IDictionary<string, string> properties)
        {
            return GetConfigurations(null, properties);
        }

        public IEnumerable<ConfigKey> GetConfigurations(string path, IDictionary<string, string> properties)
        {
            using var _requst = ApiRequestFactory.Create();

            // Requests and response stream
            using var _stream = _requst.GetStream(Settings, path, properties);

            // Deserialize stream and returns.
            return m_JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(_stream);
        }

        public void Update(string path, string value)
        {
            using var _requst = ApiRequestFactory.Create();

            // Updates the config key value.
            _requst.Update(Settings, path, value);
        }

#if ASYNC
        public async System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            return await GetConfigurationsAsync(null, null, cancellationToken);
        }

        public async System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, System.Threading.CancellationToken cancellationToken = default)
        {
            return await GetConfigurationsAsync(path, null, cancellationToken);
        }

        public async System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default)
        {
            return await GetConfigurationsAsync(null, properties, cancellationToken);
        }

        public async System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default)
        {
            using var _requst = ApiRequestFactory.Create();

            // Requests and response stream
            using var _stream = await _requst.GetStreamAsync(Settings, path, properties, cancellationToken: cancellationToken);

            // Deserialize stream and returns.
            return await m_JsonSerializer.DeserializeAsync<IEnumerable<ConfigKey>>(_stream);
        }

        public async System.Threading.Tasks.Task UpdateAsync(string path, string value, System.Threading.CancellationToken cancellationToken = default)
        {
            using var _requst = ApiRequestFactory.Create();

            // Updates the config key value.
            await _requst.UpdateAsync(Settings, path, value, cancellationToken: cancellationToken);
        }
#endif

        /// <summary>
        /// Releases client and dispose the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Public properties region

        /// <summary>
        /// Gets the client connection settings.
        /// </summary>
        public AjusteeConnectionSettings Settings { get; }

        #endregion
    }
}
