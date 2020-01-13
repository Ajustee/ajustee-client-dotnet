using System;
using System.Collections.Generic;
using System.Text;

using static Ajustee.Helper;

namespace Ajustee
{
    internal abstract class WsCommand
    {
        #region Public constructors region

        public WsCommand(string action)
        {
            Action = action;
        }

        #endregion

        #region Public methods region

        public ArraySegment<byte> GetBinary()
        {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this)));
        }

        #endregion

        #region Public properties region

        public string Action { get; }
        public object Data { get; private set; }

        #endregion

        #region Protected methods region

        protected void SetData(object data)
        {
            Data = data;
        }

        #endregion
    }

    internal sealed class WsSubscribeCommand : WsCommand
    {
        public struct SubscribeData
        {
            public string Path { get; set; }
            public IDictionary<string, string> Props { get; set; }
        }

        #region Public constructors region

        public WsSubscribeCommand(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
            : base("subscribe")
        {
            SetData(new SubscribeData { Path = path, Props = properties });
        }

        #endregion
    }
}
