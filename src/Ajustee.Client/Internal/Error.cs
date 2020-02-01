using System;

namespace Ajustee
{
    internal static class Error
    {
        #region Internal methods region

        internal static Exception InvalidPropertyName(string propertyName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_INVALID_PROPERTY_NAME"), propertyName));
        }

        internal static Exception ReservedPropertyName(string propertyName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_RESERVER_PROPERTY_NAME"), propertyName));
        }

        internal static Exception ConfigValueCannotBeNull()
        {
            return new AjusteeException(Resources.GetString("ERR_CONFIGVALUE_CANNOT_BE_NULL"));
        }

        internal static Exception HttpServerError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_INTERNAL"));
        }

        internal static Exception HttpUnauthorizedError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_UNAUTHORIZED"));
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

        internal static Exception HttpPaymentRequiredError()
        {
            return new AjusteeException(Resources.GetString("ERR_HTTP_PAYMENT_REQUIRED"));
        }

        #endregion
    }
}
