﻿
namespace Ajustee
{
    public class ConfigKey
    {
        #region Public constructors region

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigKey"/> class.
        /// </summary>
        public ConfigKey()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigKey"/> class.
        /// </summary>
        public ConfigKey(string path, ConfigKeyType dataType, string value)
            : base()
        {
            Path = path;
            DataType = dataType;
            Value = value;
        }

        #endregion

        #region Public propeties region

        /// <summary>
        /// Gets or sets path of the key.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets data type of key.
        /// </summary>
        public ConfigKeyType DataType { get; set; }

        /// <summary>
        /// Gets or sets value of the key.
        /// </summary>
#if SJSON
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonConfigValueConverter))]
#endif
        public string Value { get; set; }

        #endregion
    }
}
