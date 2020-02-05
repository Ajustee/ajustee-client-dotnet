
namespace Ajustee
{
    internal struct RequestUpdateContent
    {
        /// <summary>
        /// lower-case required by server implementation.
        /// </summary>
        public string value { get; set; }
        public RequestUpdateContent(string value) => this.value = value;
    }
}
