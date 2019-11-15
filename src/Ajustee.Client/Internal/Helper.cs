using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Ajustee
{
    internal static class Helper
    {
        #region Public fields region

        public const string ConfigurationPathUrlTemplate = "{0}?path={1}";
        public const string AppicationHeaderName = "x-api-key";

        #endregion

        #region Public methods region

        private static string FormatPropertyValue(object value)
        {
            if (value is string _str) return _str;
            if (value is null) return string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);

        }

        private static string ValidateGetPropertyName(object name)
        {
            if (name is string _name) return _name;
            throw new InvalidCastException("Property value should be string");
        }

        public static void ValidateProperties(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties != null)
            {
                foreach (var _property in properties)
                {
                    if (string.IsNullOrEmpty(_property.Key))
                        throw Error.InvalidPropertyName(_property.Key);

                    if (string.Equals(AppicationHeaderName, _property.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedPropertyName(_property.Key);
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetMergedProperties(params IEnumerable<KeyValuePair<string, string>>[] properties)
        {
            return properties.Where(ps => ps != null).SelectMany(ps => ps).GroupBy(ps => ps.Key, ps => ps.Value).Select(g => new KeyValuePair<string, string>(g.Key, g.First()));
        }

        #endregion
    }
}
