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

        public static IEnumerable<KeyValuePair<string, string>> ValidateAndGetProperties(IDictionary<string, string> properties)
        {
            if (properties != null)
            {
                foreach (var _property in properties)
                {
                    if (string.IsNullOrEmpty(_property.Key))
                        throw Error.InvalidPropertyName(_property.Key);

                    if (string.Equals(AppicationHeaderName, _property.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedPropertyName(_property.Key);

                    yield return _property;
                }
            }
        }

        #endregion
    }
}
