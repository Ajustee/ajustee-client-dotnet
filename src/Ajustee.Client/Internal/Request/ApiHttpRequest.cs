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

        private static HttpRequestMessage CreateGetRequestMessage(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Creates get http request with api url and specified configuration path.
            var _message = new HttpRequestMessage(HttpMethod.Get, GetConfigurationKeysUrl(settings.ApiUrl, path ?? settings.DefaultPath));

            // Adds headers of specify customers.
            _message.Headers.Add(AppIdName, settings.ApplicationId);
            if (settings.TrackerId != null) _message.Headers.Add(TrackerIdName, FormatPropertyValue(settings.TrackerId));

            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(settings.DefaultProperties);

            // Adds the specified properties to the request message.
            foreach (var _propertyEntry in GetMergedProperties((IEnumerable<KeyValuePair<string, string>>)properties, settings.DefaultProperties))
                _message.Headers.Add(_propertyEntry.Key, _propertyEntry.Value);

            return _message;
        }

        private static HttpRequestMessage CreateUpdateRequestMessage(AjusteeConnectionSettings settings, string path, string value)
        {
            // Creates get http request with api url and specified configuration path.
            var _message = new HttpRequestMessage(HttpMethod.Put, GetUpdateUrl(settings.ApiUrl, path));

            // Adds headers of specify customers.
            _message.Headers.Add(AppIdName, settings.ApplicationId);
            if (settings.TrackerId != null) _message.Headers.Add(TrackerIdName, FormatPropertyValue(settings.TrackerId));

            // Sets update value payload.
            _message.Content = new StringContent(JsonSerializer.Serialize(new RequestUpdateContent(value)), MessageEncoding, "application/json");

            return _message;
        }

        #endregion

        #region Public methods region

        public Stream GetStream(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Initializes http client istance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = m_Client.SendAsync(CreateGetRequestMessage(settings, path, properties)).GetAwaiter().GetResult();

            // Returns streamed payload of the configurations.
            return m_Response.Content.ReadAsStreamAsync().Result;
        }

        public async Task<Stream> GetStreamAsync(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties, CancellationToken cancellationToken = default)
        {
            // Initializes http client istance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = await m_Client.SendAsync(CreateGetRequestMessage(settings, path, properties));

            // Returns streamed payload of the configurations.
            return await m_Response.Content.ReadAsStreamAsync();
        }

        public void Update(AjusteeConnectionSettings settings, string path, string value)
        {
            // Initializes http client instance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = m_Client.SendAsync(CreateUpdateRequestMessage(settings, path, value)).GetAwaiter().GetResult();

            // Validate status code, throw exception if it is not success.
            ValidateResponseStatus((int)m_Response.StatusCode);
        }

        public async Task UpdateAsync(AjusteeConnectionSettings settings, string path, string value, CancellationToken cancellationToken = default)
        {
            // Initializes http client instance.
            m_Client = new HttpClient();

            // Create message and send to a server.
            m_Response = await m_Client.SendAsync(CreateUpdateRequestMessage(settings, path, value));

            // Validate status code, throw exception if it is not success.
            ValidateResponseStatus((int)m_Response.StatusCode);
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
