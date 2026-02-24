#if !EOS_DISABLE

using System;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSAchievementService : IAchievementService
    {
        public void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback)
        {
            throw new NotImplementedException();
        }

        public void AddProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }

        public void ClearAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }

        public void ClearAllStatsAndAchievements(AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }

        public void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback)
        {
            throw new NotImplementedException();
        }

        public void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback)
        {
            throw new NotImplementedException();
        }

        public void GetProgress(string statId, AchievementGetProgressCallback callback)
        {
            throw new NotImplementedException();
        }

        public void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }

        public void SetProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }

        public void UnlockAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            throw new NotImplementedException();
        }
    }
}

#endif