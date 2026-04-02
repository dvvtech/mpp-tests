namespace MppTests.Api.BLL.Abstract
{
    public interface IAnalyticsTrackingService
    {
        Task TrackVisitAsync(
            string operationType,
            string clientIp,
            string? userAgent,
            CancellationToken cancellationToken = default);
    }
}
