using System.Collections.Generic;

namespace Ajustee
{
    public delegate void AjusteeConfigKeyChangedEventHandler(object sender, AjusteeConfigKeyChangedEventArgs args);

    public class AjusteeConfigKeyChangedEventArgs
    {
        internal AjusteeConfigKeyChangedEventArgs(IEnumerable<ConfigKey> configKeys)
            : base()
        {
            ConfigKeys = configKeys;
        }

        /// <summary>
        /// Gets changed configuration keys.
        /// </summary>
        public IEnumerable<ConfigKey> ConfigKeys { get; }
    }
}
