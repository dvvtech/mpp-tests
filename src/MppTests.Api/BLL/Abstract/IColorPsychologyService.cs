using MppTests.Api.Models;
using MppTests.Models;

namespace MppTests.Api.BLL.Abstract
{
    public interface IColorPsychologyService
    {
        Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(ApiRequest request, CancellationToken cancellationToken = default);
    }
}
