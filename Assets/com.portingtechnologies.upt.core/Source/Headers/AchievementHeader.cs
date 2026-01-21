using UnityEngine;

namespace UPT.Services
{
    public interface IAchievementService
    {
        void UnlockAchievement(string achievementId, AchievementGeneralCallback callback);
        void ClearAchievement(string achievementId, AchievementGeneralCallback callback);
        void ClearAllStatsAndAchievements(AchievementGeneralCallback callback);
        void AddProgress(string statId, int progress, AchievementGeneralCallback callback);
        void SetProgress(string statId, int progress, AchievementGeneralCallback callback);
        void GetProgress(string statId, AchievementGetProgressCallback callback);
        void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback);
        void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback);
        void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback);
        void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback);
    }

    public class UptAchievementUnlockedResult : UptResult
    {
        public bool IsUnlocked { get; }

        public UptAchievementUnlockedResult(ErrorCode error, bool isUnlocked = false) : base(error)
        {
            IsUnlocked = isUnlocked;
        }
    }

    public class UptStatGetProgressResult : UptResult
    {
        public int Progress { get; }

        public UptStatGetProgressResult(ErrorCode error, int progress = 0) : this(error, null, progress) { }

        public UptStatGetProgressResult(ErrorCode error, string innerMessage, int progress = 0) : base(error, innerMessage)
        {
            Progress = progress;
        }
    }

    public class UptAchievementGetInfoResult : UptResult
    {
        public string AchievementId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string FlavorText { get; }
        public bool IsHidden { get; }
        public int MaxProgress { get; }

        public UptAchievementGetInfoResult(ErrorCode error, string innerMessage = null, string achievementId = null, string displayName = null, 
            string desc = null, string flavorText = null, bool isHidden = false, int maxProgress = 0) 
            : base(error, innerMessage)
        {
            AchievementId = achievementId;
            DisplayName = displayName;
            Description = desc;
            FlavorText = flavorText;
            IsHidden = isHidden;
            MaxProgress = maxProgress;
        }
    }

    public class UptAchievementGetIconResult : UptResult
    {
        public Texture2D Icon { get; }

        public UptAchievementGetIconResult(ErrorCode error, string innerMessage = null, Texture2D icon = null) : base(error, innerMessage)
        {
            Icon = icon;
        }
    }

    public delegate void AchievementGeneralCallback(UptResult result);
    public delegate void AchievementUnlockedCallback(UptAchievementUnlockedResult result);
    public delegate void AchievementGetProgressCallback(UptStatGetProgressResult result);
    public delegate void AchievementGetInfoCallback(UptAchievementGetInfoResult result);
    public delegate void AchievementGetIconCallback(UptAchievementGetIconResult result);
}
