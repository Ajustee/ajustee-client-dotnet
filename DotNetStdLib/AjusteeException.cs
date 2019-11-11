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
        #region Public constructors region

        /// <summary>
        /// Initializes a new instance of the <see cref="AjusteeException"/> class.
        /// </summary>
        public AjusteeException()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AjusteeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AjusteeException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AjusteeException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AjusteeException(string message, Exception innerException)
            : base(message, innerException)
        { }

        #endregion
    }
}
