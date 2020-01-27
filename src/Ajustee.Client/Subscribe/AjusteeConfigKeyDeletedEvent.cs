
namespace Ajustee
{
    public delegate void AjusteeConfigKeyDeletedEventHandler(object sender, AjusteeConfigKeyDeletedEventArgs args);

    public class AjusteeConfigKeyDeletedEventArgs
    {
        internal AjusteeConfigKeyDeletedEventArgs(string path)
            : base()
        {
            Path = path;
        }

        /// <summary>
        /// Gets key path of the deleted configuration.
        /// </summary>
        public string Path { get; }
    }
}
