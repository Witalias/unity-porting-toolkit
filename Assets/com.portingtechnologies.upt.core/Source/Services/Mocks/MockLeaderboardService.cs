using UPT.Core;

namespace UPT.Services.Mocks
{
    [MockService(typeof(ILeaderboardService))]
    public class MockLeaderboardService : ILeaderboardService, IMockService
    {
        public string OriginalServiceName => nameof(ILeaderboardService);
    }
}
