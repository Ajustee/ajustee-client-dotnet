using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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

        private static WebRequest CreateRequest(AjusteeConnectionSettings settings, string path, IDictionary<string, string> headers)
        {
            // Creates get http request with api url and specified configuration path.
            var _request = WebRequest.Create(string.Format(RequestHelper.ConfigurationPathUrlTemplate, settings.ApiUrl, path ?? settings.DefaultPath));

            // Sets method name.
            _request.Method = "GET";

            // Adds header to specify customer's application id.
            _request.Headers.Add(RequestHelper.AppicationHeaderName, settings.ApplicationId);

            // Adds the specified headers to the request message.
            foreach (var _headerEntry in RequestHelper.ValidateAndGetHeaders(headers))
                _request.Headers.Add(_headerEntry.Key, _headerEntry.Value);

            return _request;
        }

        #endregion

        #region Public methods region

        public Stream GetStream(AjusteeConnectionSettings settings, string path, IDictionary<string, string> headers)
        {
            // Creates request.
            m_Request = CreateRequest(settings, path, headers);

            // Gets response.
            m_Response = m_Request.GetResponse();

            // Returns response's stream.
            return m_Response.GetResponseStream();
        }

#if ASYNC
        public async System.Threading.Tasks.Task<Stream> GetStreamAsync(AjusteeConnectionSettings settings, string path, IDictionary<string, string> headers, System.Threading.CancellationToken cancellationToken = default)
        {
            // Creates request.
            m_Request = CreateRequest(settings, path, headers);

            // Gets response.
            m_Response = await m_Request.GetResponseAsync();

            // Returns response's stream.
            return m_Response.GetResponseStream();
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
