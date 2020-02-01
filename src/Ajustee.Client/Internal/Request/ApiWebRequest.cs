using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class ApiWebRequest : IApiRequest
    {
        #region Private fields region

        private WebRequest m_Request;
        private WebResponse m_Response;

        #endregion

        #region Private constructors region

        static ApiWebRequest()
        {
            // Allows TLS1.1 and TLS1.2 security protocols.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072 | (SecurityProtocolType)768;
        }

        #endregion

        #region Private methods region

        private static WebRequest CreateGetRequest(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Creates get http request with api url and specified configuration path.
            var _request = WebRequest.Create(GetConfigurationKeysUrl(settings.ApiUrl, path ?? settings.DefaultPath));

            // Sets method name.
            _request.Method = "GET";

            // Adds headers of specify customers.
            _request.Headers.Add(AppIdName, settings.ApplicationId);
            if (settings.TrackerId != null) _request.Headers.Add(TrackerIdName, FormatPropertyValue(settings.TrackerId));

            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(settings.DefaultProperties);

            // Adds the specified properties to the request message.
            foreach (var _propertyEntry in GetMergedProperties((IEnumerable<KeyValuePair<string, string>>)properties, settings.DefaultProperties))
                _request.Headers.Add(_propertyEntry.Key, _propertyEntry.Value);

            return _request;
        }

        private static WebRequest CreateUpdatRequest(AjusteeConnectionSettings settings, string path)
        {
            // Creates get http request with api url and specified configuration path.
            var _request = WebRequest.Create(GetConfigurationKeysUrl(settings.ApiUrl, path));

            // Sets method name.
            _request.Method = "POST";

            // Adds headers of specify customers.
            _request.Headers.Add(AppIdName, settings.ApplicationId);

            return _request;
        }

        #endregion

        #region Public methods region

        public Stream GetStream(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Creates request.
            m_Request = CreateGetRequest(settings, path, properties);

            // Gets response.
            m_Response = m_Request.GetResponse();

            // Returns response's stream.
            return m_Response.GetResponseStream();
        }

        public void Update(AjusteeConnectionSettings settings, string path, string value)
        {
            // Creates request.
            m_Request = CreateUpdatRequest(settings, path);

            // Gets response.
            m_Response = m_Request.GetResponse();

            // Validate status code, throw exception if it is not success.
            ValidateResponseStatus((int?)(m_Response as HttpWebResponse)?.StatusCode ?? 0);
        }

#if ASYNC
        public async System.Threading.Tasks.Task<Stream> GetStreamAsync(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default)
        {
            // Creates request.
            m_Request = CreateGetRequest(settings, path, properties);

            // Gets response.
            m_Response = await m_Request.GetResponseAsync();

            // Returns response's stream.
            return m_Response.GetResponseStream();
        }

        public async System.Threading.Tasks.Task UpdateAsync(AjusteeConnectionSettings settings, string path, string value, System.Threading.CancellationToken cancellationToken = default)
        {
            // Creates request.
            m_Request = CreateUpdatRequest(settings, path);

            // Gets response.
            m_Response = await m_Request.GetResponseAsync();

            // Validate status code, throw exception if it is not success.
            ValidateResponseStatus((int?)(m_Response as HttpWebResponse)?.StatusCode ?? 0);
        }
#endif

        public void Dispose()
        {
            // Request
            m_Request = null;

            // Response
            if (m_Response is IDisposable _disposable)
                _disposable.Dispose();
            m_Response = null;
        }

        #endregion
    }
}
