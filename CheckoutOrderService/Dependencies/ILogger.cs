namespace CheckoutOrderService.Dependencies
{
    /// <summary>
    /// An abstraction for a logging facility
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the supplied message as an error
        /// </summary>
        void LogError(string message);

        // TODO add other log levels
    }
}
