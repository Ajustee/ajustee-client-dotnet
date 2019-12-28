using System;

namespace Ajustee
{
    internal class ConnectionClosedException : Exception
    {
        #region Public constructors region

        public ConnectionClosedException(bool reconnect)
            : base()
        {
            Reconnect = reconnect;
        }

        public ConnectionClosedException(bool reconnect, int errorCode)
            : this(reconnect)
        {
            ErrorCode = errorCode;
        }

        #endregion

        #region Public properties region

        public bool Reconnect { get; }

        public int ErrorCode { get; }

        #endregion
    }
}
