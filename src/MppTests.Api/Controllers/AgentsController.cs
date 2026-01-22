
using Microsoft.AspNetCore.Mvc;
using MppTests.Api.BLL.Abstract;

namespace MppTests.Api.Controllers
{
    [Route("agents")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IAiClient _aiClient;
        private readonly ILogger<AgentsController> _logger;

        public AgentsController(
            IAiClient aiClient,
            ILogger<AgentsController> logger)        
        {
            _aiClient = aiClient;
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<string> Test()
        {
            _logger.LogInformation("call test");
            var res = await _aiClient.GetTextResponseAsync("напиши четырехстишье про природу", "ты профессиональный писатель");
            return res;
        }

        [HttpGet("test2")]
        public string Test2()
        {
            _logger.LogInformation("call test2");            
            return "1477";
        }
    }
}
