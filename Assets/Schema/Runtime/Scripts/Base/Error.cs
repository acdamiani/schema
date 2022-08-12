namespace Schema
{
    /// <summary>
    ///     Used to create an error for a GraphObject
    /// </summary>
    public struct Error
    {
        /// <summary>
        ///     Severity of the error
        /// </summary>
        public enum Severity
        {
            Info,
            Warning,
            Error
        }

        /// <summary>
        ///     Error message
        /// </summary>
        public string message;

        /// <summary>
        ///     Severity of the error
        /// </summary>
        public Severity severity;

        public Error(string message, Severity severity)
        {
            this.message = message;
            this.severity = severity;
        }
    }
}