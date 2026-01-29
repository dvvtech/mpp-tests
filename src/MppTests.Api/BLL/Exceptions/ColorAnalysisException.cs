namespace MppTests.Api.BLL.Exceptions
{
    public abstract class ColorAnalysisException : Exception
    {
        protected ColorAnalysisException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
