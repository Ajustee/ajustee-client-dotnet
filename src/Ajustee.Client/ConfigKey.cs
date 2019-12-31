
namespace Ajustee
{
    public class ConfigKey
    {
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
