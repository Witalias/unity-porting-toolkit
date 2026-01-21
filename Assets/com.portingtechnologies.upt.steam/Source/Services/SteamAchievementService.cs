#if !DISABLESTEAMWORKS

using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UPT.Core;
using UPT.Services;

namespace UPT.Steam
{
    public class SteamAchievementService : IAchievementService, IUpdatableService
    {
        private const float STORE_STATS_DELAY = 30.0f;
        private const float GET_ACHIEVEMENT_ICON_TIMEOUT = 4.0f;

        private readonly DelayedAction m_storeStatsAction;
        private readonly DelayedAction m_getAchievementIconTimeoutAction;
        private readonly Dictionary<int, Texture2D> m_iconCache = new();

        private Callback<UserAchievementIconFetched_t> m_UserAchievementIconFetched;

        private AchievementGetIconCallback m_achievementGetIconCallbackCache;

        public SteamAchievementService()
        {
            m_storeStatsAction = new(STORE_STATS_DELAY, () =>
            {
                if (!SteamUserStats.StoreStats())
                    m_storeStatsAction.Run(true);
            });

            m_getAchievementIconTimeoutAction = new(GET_ACHIEVEMENT_ICON_TIMEOUT, () =>
            {
                m_achievementGetIconCallbackCache?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, "SteamUserStats.GetAchievementIcon failed"));
            });

            m_UserAchievementIconFetched = Callback<UserAchievementIconFetched_t>.Create(OnUserAchievementIconFetched);
        }

        public void AddProgress(string statId, int progress, AchievementGeneralCallback callback = null)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.GetStat(statId, out int currentProgress))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError));
                return;
            }

            SetProgress(statId, currentProgress, callback);
        }

        public void SetProgress(string statId, int progress, AchievementGeneralCallback callback = null)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.SetStat(statId, progress))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError));
                return;
            }

            m_storeStatsAction.Run(true);
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void GetProgress(string statId, AchievementGetProgressCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptStatGetProgressResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.GetStat(statId, out int progress))
            {
                callback?.Invoke(new UptStatGetProgressResult(ErrorCode.UntypedError, "SteamUserStats.GetStat failed"));
                return;
            }

            callback?.Invoke(new UptStatGetProgressResult(ErrorCode.Success, progress));
        }

        public void UnlockAchievement(string achievementId, AchievementGeneralCallback callback = null)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.SetAchievement(achievementId))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.SetAchievement failed"));
                return;
            }

            m_storeStatsAction.Run(true);
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void ClearAchievement(string achievementId, AchievementGeneralCallback callback = null)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }
            
            if (!SteamUserStats.ClearAchievement(achievementId))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.ClearAchievement failed"));
                return;
            }

            m_storeStatsAction.Run(true);
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void ClearAllStatsAndAchievements(AchievementGeneralCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.ResetAllStats(true))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.ResetAllStats failed"));
                return;
            }

            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.GetAchievement(achievementId, out var unlocked))
            {
                callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.UntypedError));
                return;
            }

            callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.Success, unlocked));
        }

        public void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.SdkNotInitialized));
                return;
            }

            var name = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "name");
            var desc = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "desc");
            var hidden = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "hidden");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(desc) || string.IsNullOrEmpty(hidden))
            {
                callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.UntypedError, "SteamUserStats.GetAchievementDisplayAttribute returned empty string"));
                return;
            }

            SteamUserStats.GetAchievementProgressLimits(achievementId, out _, out int maxProgress);
            var isHidden = hidden == "1";

            callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.Success, null, achievementId, name, desc, null, isHidden, maxProgress));
        }

        public void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.SdkNotInitialized));
                return;
            }

            var iconHandle = SteamUserStats.GetAchievementIcon(achievementId);
            if (iconHandle == 0)
            {
                m_achievementGetIconCallbackCache = callback;
                m_getAchievementIconTimeoutAction?.Run();
                return;
            }

            if (!GetAchievementIconByHandle(iconHandle, out var icon, out var message))
            {
                callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, message));
                return;
            }

            callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.Success, null, icon));
        }

        public void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            if (!SteamUserStats.GetAchievementProgressLimits(achievementId, out _, out int maxProgress))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.GetAchievementProgressLimits failed"));
                return;
            }

            if (!SteamUserStats.IndicateAchievementProgress(achievementId, (uint)progress, (uint)maxProgress))
            {
                callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.IndicateAchievementProgress failed"));
                return;
            }

            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void Update()
        {
            m_storeStatsAction.Update();
            m_getAchievementIconTimeoutAction.Update();
        }

        private bool GetAchievementIconByHandle(int handle, out Texture2D outTexture, out string outMessage)
        {
            outTexture = null;
            outMessage = null;

            if (m_iconCache.TryGetValue(handle, out var cachedTexture))
            {
                outTexture = cachedTexture;
                return true;
            }

            if (!SteamUtils.GetImageSize(handle, out var width, out var height))
            {
                outMessage = "SteamUtils.GetImageSize failed";
                return false;
            }

            var imageData = new byte[width * height * 4];

            if (!SteamUtils.GetImageRGBA(handle, imageData, (int)(width * height * 4)))
            {
                outMessage = "SteamUtils.GetImageRGBA failed";
                return false;
            }

            byte[] flippedData = new byte[imageData.Length];
            int bytesPerRow = (int)width * 4;

            // Flips the image vertically (corrects Steam -> Unity orientation)
            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * bytesPerRow;
                int targetRow = ((int)height - 1 - y) * bytesPerRow;

                Buffer.BlockCopy(imageData, sourceRow, flippedData, targetRow, bytesPerRow);
            }

            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(flippedData);
            texture.Apply();

            m_iconCache[handle] = texture;
            outTexture = texture;

            return true;
        }

        private void OnUserAchievementIconFetched(UserAchievementIconFetched_t data)
        {
            m_getAchievementIconTimeoutAction?.Abort();

            if (data.m_nIconHandle == 0)
            {
                m_achievementGetIconCallbackCache?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, "SteamUserStats.GetAchievementIcon returned incorrect icon handle"));
                return;
            }

            if (!GetAchievementIconByHandle(data.m_nIconHandle, out var icon, out var message))
            {
                m_achievementGetIconCallbackCache?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, message));
                return;
            }

            m_achievementGetIconCallbackCache?.Invoke(new UptAchievementGetIconResult(ErrorCode.Success, null, icon));
        }
    }
}

#endif