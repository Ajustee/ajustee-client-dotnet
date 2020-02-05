using System;

namespace Ajustee
{
    /// <summary>
    /// Represents errors that occur during ajustee depended operation.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif 
    public class AjusteeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AjusteeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AjusteeException(string message, AjusteeErrorCode errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
            HelpLink = Helper.HelpUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AjusteeException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AjusteeException(string message, AjusteeErrorCode errorCode, Exception innerException)
            : base(message, innerException)
        {
            HelpLink = Helper.HelpUrl;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public AjusteeErrorCode ErrorCode { get; }
    }

    public enum AjusteeErrorCode
    {
        /// <summary>
        /// No errors
        /// </summary>
        Success,
        /// <summary>
        /// Reached limit of plan to execute operation to API.
        /// </summary>
        ReachedLimit,
        /// <summary>
        /// No necessary permissions for a resource.
        /// </summary>
        Forbidden,
        /// <summary>
        /// Not found required resource.
        /// </summary>
        NotFound,
        /// <summary>
        /// Invalid request data.
        /// </summary>
        Invalid,
        /// <summary>
        /// Other unknown errors
        /// </summary>
        Unknown
    }
}
