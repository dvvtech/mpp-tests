using MppTests.Models;

namespace MppTests.Client
{
    public class MppTestsClient
    {
        private readonly HttpClient _httpClient;

        public MppTestsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task GenerateAnswer(UserColor colorDataRequest)
        {
            //v1/agents
        }
    }
}
