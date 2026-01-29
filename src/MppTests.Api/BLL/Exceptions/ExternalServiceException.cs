namespace MppTests.Api.BLL.Exceptions
{
    public class ExternalServiceException : ColorAnalysisException
    {
        public string ServiceName { get; }

        public ExternalServiceException(
            string serviceName,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ServiceName = serviceName;
        }
    }
}
