using System;
using System.Runtime.Serialization;

namespace KeyMorf
{
    /// <summary>
    /// Application specific exception.
    /// </summary>
    [Serializable]
    public class KeyMorfException : Exception
    {
        /// <summary>
        /// Initialise an instance of <see cref="KeyMorfException"/> with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Message detailing the cause of the exception.</param>
        public KeyMorfException(string? message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        protected KeyMorfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
