
using System.Collections.Generic;

namespace Ajustee
{
    public delegate void AjusteeConfigKeyEventHandler(object ssender, AjusteeConfigKeyEventArgs args);

    public class AjusteeConfigKeyEventArgs
    {
        #region Internal constructors region

        internal AjusteeConfigKeyEventArgs(IEnumerable<ConfigKey> configKeys)
            : base()
        {
            ConfigKeys = configKeys;
        }

        #endregion

        #region Public properties region

        /// <summary>
        /// Gets configuration keys.
        /// </summary>
        public IEnumerable<ConfigKey> ConfigKeys { get; }

        #endregion
    }
}
