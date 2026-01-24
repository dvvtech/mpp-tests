using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MppTests.Api.BLL.Abstract;
using MppTests.Api.Models;
using MppTests.Models;

namespace MppTests.Api.Controllers
{
    [Route("v1/color-analysis")]
    [ApiController]
    public class ColorAnalysisController : ControllerBase
    {
        private readonly IColorPsychologyService _psychologyService;

        public ColorAnalysisController(IColorPsychologyService psychologyService)
        {
            _psychologyService = psychologyService;
        }

        [HttpPost("analyze-lusher")]
        public async Task<ActionResult<PsychologicalAnalysisResponse>> AnalyzeByLusherMethod(
        [FromBody] ColorDataRequest request)
        {
            var analysis = await _psychologyService.AnalyzeColorPreferencesAsync(request);
            return Ok(analysis);
        }
    }
}
