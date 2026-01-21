using UPT.Core;

namespace UPT.Services.Mocks
{
    [MockService(typeof(IAchievementService))]
    public class MockAchievementService : IAchievementService, IMockService
    {
        public string OriginalServiceName => nameof(IAchievementService);

        public void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback)
        {
            callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.Success));
        }

        public void AddProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void ClearAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void ClearAllStatsAndAchievements(AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback)
        {
            callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.Success));
        }

        public void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback)
        {
            callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.Success));
        }

        public void GetProgress(string statId, AchievementGetProgressCallback callback)
        {
            callback?.Invoke(new UptStatGetProgressResult(ErrorCode.Success));
        }

        public void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void SetProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void UnlockAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }
    }
}
