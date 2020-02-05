using System;

namespace Ajustee
{
    internal static class Error
    {
        #region Internal methods region

        internal static Exception InvalidKeyPath(string keyPath)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_INVALID_KEYPATH"), keyPath), AjusteeErrorCode.Invalid);
        }

        internal static Exception InvalidPropertyName(string propertyName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_INVALID_PROPERTY_NAME"), propertyName), AjusteeErrorCode.Invalid);
        }

        internal static Exception ReservedPropertyName(string propertyName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_RESERVER_PROPERTY_NAME"), propertyName), AjusteeErrorCode.Invalid);
        }

        internal static Exception ConfigValueCannotBeNull()
        {
            return new AjusteeException(Resources.GetString("ERR_CONFIGVALUE_CANNOT_BE_NULL"), AjusteeErrorCode.Invalid);
        }

        internal static Exception Unknown(Exception innerException)
        {
            return new AjusteeException(Resources.GetString("ERR_UNKNOWN"), AjusteeErrorCode.Unknown, innerException);
        }

        internal static Exception InvalidApplication(string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId))
                return new AjusteeException(Resources.GetString("ERR_MISSING_APPLICATION"), AjusteeErrorCode.Invalid);
            else
                return new AjusteeException(string.Format(Resources.GetString("ERR_INVALID_APPLICATION"), applicationId), AjusteeErrorCode.Forbidden);
        }

        internal static Exception Forbidden()
        {
            return new AjusteeException(Resources.GetString("ERR_FORBIDDEN"), AjusteeErrorCode.Forbidden);
        }

        internal static Exception InvalidRequest()
        {
            return new AjusteeException(Resources.GetString("ERR_INVALID_REQUEST"), AjusteeErrorCode.Invalid);
        }

        internal static Exception NotFound(string resourceName)
        {
            return new AjusteeException(string.Format(Resources.GetString("ERR_NOTFOUND"), resourceName), AjusteeErrorCode.NotFound);
        }

        internal static Exception ReachedLimit()
        {
            return new AjusteeException(Resources.GetString("ERR_REACHED_LIMIT"), AjusteeErrorCode.ReachedLimit);
        }

        internal static Exception InvalidApiUrl(Uri apiUrl)
        {
            if (apiUrl == null)
                return new AjusteeException(Resources.GetString("ERR_MISSING_APIURL"), AjusteeErrorCode.Invalid);
            else
                return new AjusteeException(string.Format(Resources.GetString("ERR_SERVICE_UNAVAILABLE"), apiUrl), AjusteeErrorCode.Invalid);
        }

        #endregion
    }
}
