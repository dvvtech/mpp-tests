namespace MppTests.Api.BLL.Exceptions
{
    public class LlmInvalidResponseException : ColorAnalysisException
    {
        public string? RawResponse { get; }

        public LlmInvalidResponseException(string? rawResponse, Exception? innerException = null)
            : base("LLM returned invalid JSON response", innerException)
        {
            RawResponse = rawResponse;
        }
    }
}
