using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

namespace Ajustee
{
    internal class Subscriber : IDisposable
    {
        #region Private fields region

        private const string m_WebSocketSchema = "wss";
        private const string m_AppIdName = "x-app-key";
        private const string m_KeyPathName = "x-key-path";
        private const string m_KeyPropsName = "x-key-props";
        private static readonly IJsonSerializer m_JsonSerializer;

        private AjusteeConnectionSettings m_Settings;
        private ClientWebSocket m_WebSocket;

        #endregion

        #region Private methods region

        private static ClientWebSocket CreateWebSocket(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
        {
            // Validate properties.
            Helper.ValidateProperties(properties);
            Helper.ValidateProperties(settings.DefaultProperties);

            var _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader(m_AppIdName, settings.ApplicationId);
            _socket.Options.SetRequestHeader(m_KeyPathName, path);
            if (properties != null)
                _socket.Options.SetRequestHeader(m_KeyPropsName, m_JsonSerializer.Serialize(properties));
            return _socket;
        }

        private static Uri GetConnectionUrl(AjusteeConnectionSettings settings)
        {
#if DEBUG
            return new Uri("wss://viz8masph1.execute-api.us-west-2.amazonaws.com/demo");
#else
            var _uriBuilder = new UriBuilder(settings.ApiUrl);
            _uriBuilder.Scheme = m_WebSocketSchema;// Sets websocket secure schema
            return _uriBuilder.Uri;
#endif
        }

        #endregion

        #region Public constructors region

        public Subscriber(AjusteeConnectionSettings settings)
            : base()
        {
            m_Settings = settings;
        }

        static Subscriber()
        {
            m_JsonSerializer = JsonSerializerFactory.Create();
        }

        #endregion

        #region Public methods region

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            var _initalConnected = false;
            lock (this)
            {
                if (m_WebSocket == null)
                {
                    m_WebSocket = CreateWebSocket(m_Settings, path, properties);
                    m_WebSocket.ConnectAsync(GetConnectionUrl(m_Settings), CancellationToken.None).Wait();
                    _initalConnected = true;
                }
            }

            if (!_initalConnected)
            {
                //m_WebSocket.SendAsync();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (m_WebSocket != null)
                {
                    m_WebSocket.Dispose();
                    m_WebSocket = null;
                }
            }
        }

        #endregion
    }
}
