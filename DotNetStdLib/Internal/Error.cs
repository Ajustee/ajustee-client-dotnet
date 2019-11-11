using System;

namespace Ajustee
{
    internal static class Error
    {
        #region Internal methods region

        internal static Exception InvalidHeaderName(string headerName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_INVALID_HEADER_NAME"), headerName));
        }

        internal static Exception ReservedHeaderName(string headerName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_RESERVER_HEADER_NAME"), headerName));
        }

        internal static Exception ConfigValueCannotBeNull()
        {
            return new AjusteeException(Resources.GetString("ERR_CONFIGVALUE_CANNOT_BE_NULL"));
        }

        internal static Exception HttpServerError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_INTERNAL"));
        }

        internal static Exception HttpForbiddenError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_FORBIDDEN"));
        }

        internal static Exception HttpBadRequestError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_BADREQUEST"));
        }

        internal static Exception HttpNotFoundError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_NOTFOUND"));
        }

        #endregion
    }
}
