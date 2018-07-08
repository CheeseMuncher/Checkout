namespace CheckoutOrderService.Common
{
    /// <summary>
    /// Service Level Error Types
    /// Currently a proxy for HTTP status codes
    /// Could be extended to provide more specific error types
    /// </summary>
    public enum ServiceError
    {
        None = 0,
        BadRequest = 1,
        NotFound = 2,
        InternalServerError = 3
    }
}
