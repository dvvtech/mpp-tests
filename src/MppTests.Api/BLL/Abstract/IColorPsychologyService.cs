using MppTests.Api.Models;
using MppTests.Models;

namespace MppTests.Api.BLL.Abstract
{
    public interface IColorPsychologyService
    {
        Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(ColorDataRequest request, CancellationToken cancellationToken = default);
    }
}
