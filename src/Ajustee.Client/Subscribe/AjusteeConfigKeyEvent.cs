
namespace Ajustee
{
    public delegate void AjusteeConfigKeyEventHandler(object ssender, AjusteeConfigKeyEventArgs args);

    public class AjusteeConfigKeyEventArgs
    {
        #region Internal constructors region

        internal AjusteeConfigKeyEventArgs(ConfigKey configKey)
            : base()
        {
            ConfigKey = configKey;
        }

        #endregion

        #region Public properties region

        /// <summary>
        /// Gets configuration key.
        /// </summary>
        public ConfigKey ConfigKey { get; }

        #endregion
    }
}
