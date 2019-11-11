using System;
using System.Globalization;

namespace Ajustee
{
    public static class Extensions
    {
        #region Public methods region

        public static T Value<T>(this ConfigKey key)
        {
            if (key == null) return default;

            object _value = key.Value;

            switch (key.DataType)
            {
                // Convert to integer.
                case ConfigKeyType.Integer:
                    {
                        if (key.Value is string _formattedValue)
                            _value = int.Parse(_formattedValue);
                    }
                    break;

                // Convert to boolean.
                case ConfigKeyType.Boolean:
                    {
                        if (key.Value is string _formattedValue)
                            _value = bool.Parse(_formattedValue);
                    }
                    break;

                // Convert to date and date time.
                case ConfigKeyType.DateTime:
                case ConfigKeyType.Date:
                    {
                        if (key.Value is string _formattedValue)
                            _value = DateTime.Parse(_formattedValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    }
                    break;
            }

            return (T)_value;
        }

        #endregion
    }
}
