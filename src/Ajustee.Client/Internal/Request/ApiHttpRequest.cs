using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class ApiHttpRequest : IApiRequest
    {
        #region Private fields region

        private HttpClient m_Client;
        private HttpResponseMessage m_Response;

        #endregion

        #region Private methods region

        private static HttpRequestMessage CreateRequestMessage(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Creates get http request with api url and specified configuration path.
            var _message = new HttpRequestMessage(HttpMethod.Get, GetConfigurationKeysUrl(settings.ApiUrl, path ?? settings.DefaultPath));

            // Adds header to specify customer's application id.
            _message.Headers.Add(AppIdName, settings.ApplicationId);

            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(settings.DefaultProperties);

            // Adds the specified properties to the request message.
            foreach (var _propertyEntry in GetMergedProperties(properties, settings.DefaultProperties))
                _message.Headers.Add(_propertyEntry.Key, _propertyEntry.Value);

            return _message;
        }

        #endregion

        #region Public methods region

        public Stream GetStream(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Initializes http client istance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = m_Client.SendAsync(CreateRequestMessage(settings, path, properties)).Result;

            // Returns streamed payload of the configurations.
            return m_Response.Content.ReadAsStreamAsync().Result;
        }

        public async Task<Stream> GetStreamAsync(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties, CancellationToken cancellationToken = default)
        {
            // Initializes http client istance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = await m_Client.SendAsync(CreateRequestMessage(settings, path, properties));

            // Returns streamed payload of the configurations.
            return await m_Response.Content.ReadAsStreamAsync();
        }

        public void Dispose()
        {
            // Response
            if (m_Response != null)
            {
                m_Response.Dispose();
                m_Response = null;
            }

            // Client
            if (m_Client != null)
            {
                m_Client.Dispose();
                m_Client = null;
            }
        }

        #endregion
    }
}
