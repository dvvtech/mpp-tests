namespace MppTests.Api.BLL.Exceptions
{
    public class ResourceNotFoundException : ColorAnalysisException
    {
        public ResourceNotFoundException(string resourceName, Exception? innerException = null)
            : base($"Resource '{resourceName}' not found", innerException)
        {
        }
    }
}
