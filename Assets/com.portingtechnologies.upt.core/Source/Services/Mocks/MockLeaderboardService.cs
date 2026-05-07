using UPT.Core;

namespace UPT.Services.Mocks
{
    [MockService(typeof(ILeaderboardService))]
    public class MockLeaderboardService : ILeaderboardService, IMockService
    {
        public string OriginalServiceName => nameof(ILeaderboardService);

        public void GetEntriesAroundUser(string leaderboardId, int range, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success));
        }

        public void GetFriendsEntries(string leaderboardId, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success));
        }

        public void GetGlobalEntries(string leaderboardId, int count, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success));
        }

        public void GetLeaderboardInfo(string leaderboardId, GetLeaderboardInfoCallback callback)
        {
            callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.Success));
        }

        public void UploadScore(string leaderboardId, int score, LeaderboardGeneralCallback callback, LeaderboardMetadata metadata)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }
    }
}
