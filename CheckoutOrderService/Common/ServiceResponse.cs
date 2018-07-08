using System.Collections.Generic;

namespace CheckoutOrderService.Common
{
    /// <summary>
    /// A basic response object for our service layer
    /// </summary>
    public class ServiceResponse
    {
        public ServiceResponse(ServiceError error = ServiceError.None, params string[] messages)
        {
            ServiceError = error;
            ErrorMessages = messages;
        }

        public ServiceError ServiceError { get; }
        public bool IsSuccessful { get => ServiceError == ServiceError.None; }
        public IEnumerable<string> ErrorMessages { get; }
    }

    /// <summary>
    /// A generic response payload for our service layer
    /// </summary>
    public class ServiceResponse<T> : ServiceResponse
    {
        public ServiceResponse(ServiceError error = ServiceError.None, params string[] messages) : base(error, messages)
        {
        }

        public ServiceResponse(T data)
        {
            Data = data;
        }

        public T Data { get; }
    }
}
