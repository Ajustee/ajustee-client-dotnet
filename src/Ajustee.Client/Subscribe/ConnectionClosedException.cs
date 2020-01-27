using System;

namespace Ajustee
{
    internal class ConnectionClosedException : Exception
    {
        #region Public constructors region

        public ConnectionClosedException(bool reconnect, int errorCode)
            : base()
        {
            Reconnect = reconnect;
            ErrorCode = errorCode;
        }

        public ConnectionClosedException(bool reconnect, int errorCode, Exception innerException)
            : base(innerException.Message, innerException)
        {
            Reconnect = reconnect;
            ErrorCode = errorCode;
        }

        #endregion

        #region Public properties region

        public bool Reconnect { get; }

        public int ErrorCode { get; }

        #endregion
    }
}
