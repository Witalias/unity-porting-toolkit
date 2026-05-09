using UnityEngine;

namespace UPT.Services
{
    public interface IAchievementService
    {
        /// <summary>
        /// Unlock the achievement on the platform immediately.
        /// </summary>
        /// <param name="achievementId">The ID of the achievement that needs to be unlocked. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void UnlockAchievement(string achievementId, AchievementGeneralCallback callback);

        /// <summary>
        /// Resets all achievement progress. You shouldn't call this in production.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> This feature is not supported. You can delete the achievement data on the developer's portal.
        /// </remarks>
        /// <param name="achievementId">The ID of the achievement that needs to be unlocked. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void ClearAchievement(string achievementId, AchievementGeneralCallback callback);

        /// <summary>
        /// Resets all achievement progress. You shouldn't call this in production.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> This feature is not supported. You can delete the achievement data on the developer's portal.
        /// </remarks>
        /// <param name="achievementId">The ID of the achievement that needs to be unlocked. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void ClearAllStatsAndAchievements(AchievementGeneralCallback callback);

        /// <summary>
        /// Add progress to statistics. Statistics are usually linked to achievements.
        /// </summary>
        /// <param name="statId">The ID of the statistics to add progress to. You can usually find it on the developer's portal.
        /// <list>Platform-specific notes
        /// <br/><b>VK Play:</b> There are no statistics on the platform as a separate entity. Instead, specify the achievement ID in this parameter.</list></param>
        /// <param name="progress">Added value.</param>
        /// <param name="callback"></param>
        void AddProgress(string statId, int progress, AchievementGeneralCallback callback);

        /// <summary>
        /// Set the statistics progress. Statistics are usually linked to achievements.
        /// </summary>
        /// <param name="statId">The ID of the statistics to set the progress for. You can usually find it on the developer's portal.
        /// <list>Platform-specific notes
        /// <br/><b>VK Play:</b> There are no statistics on the platform as a separate entity. Instead, specify the achievement ID in this parameter.</list></param>
        /// <param name="progress">The value being set.</param>
        /// <param name="callback"></param>
        void SetProgress(string statId, int progress, AchievementGeneralCallback callback);

        /// <summary>
        /// Get the current progress based on certain statistics.
        /// </summary>
        /// <param name="statId">The ID of the statistics to get the value for. You can usually find it on the developer's portal.
        /// <list>Platform-specific notes
        /// <br/><b>VK Play:</b> There are no statistics on the platform as a separate entity. Instead, specify the achievement ID in this parameter.</list></param>
        /// <param name="callback"></param>
        void GetProgress(string statId, AchievementGetProgressCallback callback);

        /// <summary>
        /// Get the achievement status from the platform.
        /// </summary>
        /// <param name="achievementId">The ID of the achievement to find out the status of. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback);

        /// <summary>
        /// Get achievement data from the platform, such as the displayed name, description, flag hidden, maximum progress.
        /// </summary>
        /// <param name="achievementId">The ID of the achievement to get the data for. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback);

        /// <summary>
        /// Get the achievement icon as a texture.
        /// </summary>
        /// <param name="achievementId">The ID of the achievement whose icon you want to receive. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback);

        /// <summary>
        /// Shows the user a pop-up notification with the current progress of an achievement.
        /// </summary>
        /// <remarks>
        /// <b>This feature is only supported on Steam.</b>
        /// </remarks>
        /// <param name="achievementId">The ID of the achievement to show the pop-up notification for.</param>
        /// <param name="progress">The progress value to be displayed in the notification.</param>
        /// <param name="callback"></param>
        void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback);
    }

    public class UptAchievementUnlockedResult : UptResult
    {
        /// <summary>
        /// Is the achievement unlocked.
        /// </summary>
        public bool IsUnlocked { get; }

        public UptAchievementUnlockedResult(ErrorCode error, bool isUnlocked = false) : this(error, null, isUnlocked) { }

        public UptAchievementUnlockedResult(ErrorCode error, string innerMessage, bool isUnlocked = false) : base(error, innerMessage)
        {
            IsUnlocked = isUnlocked;
        }
    }

    public class UptStatGetProgressResult : UptResult
    {
        /// <summary>
        /// Current progress on statistics.
        /// </summary>
        public int Progress { get; }

        public UptStatGetProgressResult(ErrorCode error, int progress = 0) : this(error, null, progress) { }

        public UptStatGetProgressResult(ErrorCode error, string innerMessage, int progress = 0) : base(error, innerMessage)
        {
            Progress = progress;
        }
    }

    public class UptAchievementGetInfoResult : UptResult
    {
        /// <summary>
        /// The ID of the achievement.
        /// </summary>
        public string AchievementId { get; }

        /// <summary>
        /// The display name of the achievement.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The description of the achievement.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Whether the information about the achievement is hidden before it is unlocked.
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// The maximum progress required to unlock the achievement.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> The maximum progress is the sum of the requirements of all the statistics linked to the achievement.
        /// </remarks>
        public int MaxProgress { get; }

        public UptAchievementGetInfoResult(ErrorCode error, string innerMessage = null, string achievementId = null, string displayName = null, 
            string desc = null, bool isHidden = false, int maxProgress = 0) 
            : base(error, innerMessage)
        {
            AchievementId = achievementId;
            DisplayName = displayName;
            Description = desc;
            IsHidden = isHidden;
            MaxProgress = maxProgress;
        }
    }

    public class UptAchievementGetIconResult : UptResult
    {
        /// <summary>
        /// The icon of the requested achievement.
        /// </summary>
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
