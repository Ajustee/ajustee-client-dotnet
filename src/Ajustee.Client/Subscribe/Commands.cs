using System;
using System.Collections.Generic;
using System.Text;

using static Ajustee.Helper;

namespace Ajustee
{
    internal abstract class WsCommand
    {
        #region Private fields region

        private string m_Action;
        private object m_Data;

        #endregion

        #region Public constructors region

        public WsCommand(string action)
        {
            m_Action = action;
        }

        #endregion

        #region Public methods region

        public ArraySegment<byte> GetBinary()
        {
            var _dataText = m_Data as string;
            if (_dataText == null)
                _dataText = "null";
            else
                _dataText = JsonSerializer.Serialize(m_Data);

            return new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{{\"action\":\"{m_Action}\",data:{_dataText}}}"));
        }

        #endregion

        #region Protected methods region

        protected void SetData(object data)
        {
            m_Data = data;
        }

        #endregion
    }

    internal sealed class WsSubscribeCommand : WsCommand
    {
        #region Public constructors region

        public WsSubscribeCommand(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties)
            : base("subscribe")
        {
            SetData($"{{\"{AppIdName}\":\"{settings.ApplicationId}\",\"{KeyPathName}\":\"{path}\",\"{KeyPropsName}:\"{(properties == null ? "null": JsonSerializer.Serialize(properties))}}}");
        }

        #endregion
    }
}
