using System;
using System.Collections.Generic;

namespace Ajustee
{
    internal static class RequestHelper
    {
        #region Public properties region

        public const string ConfigurationPathUrlTemplate = "{0}?path={1}";
        public const string AppicationHeaderName = "x-api-key";

        #endregion

        #region Public methods region

        public static IEnumerable<KeyValuePair<string, string>> ValidateAndGetHeaders(IDictionary<string, string> parameteres)
        {
            if (parameteres != null)
            {
                foreach (var _header in parameteres)
                {
                    if (string.IsNullOrEmpty(_header.Key))
                        throw Error.InvalidHeaderName(_header.Key);

                    if (string.Equals(AppicationHeaderName, _header.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedHeaderName(_header.Key);

                    yield return _header;
                }
            }
        }

        #endregion
    }
}
