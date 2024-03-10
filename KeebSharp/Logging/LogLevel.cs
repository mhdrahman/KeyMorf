namespace KeebSharp.Logging
{
    /// <summary>
    /// Enum indicating level of logging.
    /// </summary>
    internal enum LogLevel
    {
        /// <summary>
        /// Only log errors.
        /// </summary>
        Error,

        /// <summary>
        /// Log warnings and errors.
        /// </summary>
        Warn,

        /// <summary>
        /// Log info, warnings and errors.
        /// </summary>
        Info,

        /// <summary>
        /// Log info, warnings, info and debug information.
        /// </summary>
        Debug, 
    }
}
