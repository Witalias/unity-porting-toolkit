#if !DISABLESTEAMWORKS

using Steamworks;
using System;
using System.Collections.Generic;
using UPT.Services;

namespace UPT.Steam
{
    public class SteamLeaderboardService : ILeaderboardService
    {
        private readonly Dictionary<string, SteamLeaderboard_t> leaderboardHandlers = new();

        private CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded;
        private CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded;
        private CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult;

        private GetLeaderboardEntriesCallback m_getLeaderboardEntriesCallbackCache;

        public SteamLeaderboardService()
        {
            m_LeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
            m_LeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create();
            m_LeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create();
        }

        public void GetLeaderboardInfo(string leaderboardId, GetLeaderboardInfoCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.SdkNotInitialized));
                return;
            }

            GetLeaderboardHandler(leaderboardId, (success, errorMessage, leaderboardHandler) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }
                var name = SteamUserStats.GetLeaderboardName(leaderboardHandler);
                var entryCount = SteamUserStats.GetLeaderboardEntryCount(leaderboardHandler);
                callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.Success, null, leaderboardId, name, entryCount));
            });
        }

        public void GetGlobalEntries(string leaderboardId, int count, GetLeaderboardEntriesCallback callback, LeaderboardMetadata meta)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.SdkNotInitialized));
                return;
            }

            GetLeaderboardHandler(leaderboardId, (success, errorMessage, leaderboardHandler) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }
                m_getLeaderboardEntriesCallbackCache = callback;
                var apiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboardHandler, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, count);
                m_LeaderboardScoresDownloaded.Set(apiCall);
            });
        }

        public void GetFriendsEntries(string leaderboardId, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.SdkNotInitialized));
                return;
            }

            GetLeaderboardHandler(leaderboardId, (success, errorMessage, leaderboardHandler) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }
                m_getLeaderboardEntriesCallbackCache = callback;
                var apiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboardHandler, ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, 0, 0);
                m_LeaderboardScoresDownloaded.Set(apiCall);
            });
        }

        public void GetEntriesAroundUser(string leaderboardId, int range, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.SdkNotInitialized));
                return;
            }

            GetLeaderboardHandler(leaderboardId, (success, errorMessage, leaderboardHandler) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }
                m_getLeaderboardEntriesCallbackCache = callback;
                var apiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboardHandler, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -range, range);
                m_LeaderboardScoresDownloaded.Set(apiCall);
            });
        }

        public void UploadScore(string leaderboardId, int score, LeaderboardGeneralCallback callback, LeaderboardMetadata meta)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            GetLeaderboardHandler(leaderboardId, (success, errorMessage, leaderboardHandler) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var scoreDetailsCount = meta?.IntValues?.Length ?? 0;
                var scoreDetails = meta?.IntValues;

                var apiCall = SteamUserStats.UploadLeaderboardScore(leaderboardHandler, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, scoreDetails, scoreDetailsCount);
                m_LeaderboardScoreUploaded.Set(apiCall, (data, failure) =>
                {
                    var success = !failure && data.m_bSuccess == 1;
                    if (success)
                        callback?.Invoke(new UptResult(ErrorCode.Success));
                    else
                        callback?.Invoke(new UptResult(ErrorCode.UntypedError, "SteamUserStats.UploadLeaderboardScore failed"));
                });
            });
        }

        private void GetLeaderboardHandler(string leaderboardId, Action<bool, string, SteamLeaderboard_t> callback)
        {
            if (leaderboardHandlers.ContainsKey(leaderboardId))
            {
                callback?.Invoke(true, null, leaderboardHandlers[leaderboardId]);
            }
            else
            {
                var apiCall = SteamUserStats.FindLeaderboard(leaderboardId);
                m_LeaderboardFindResult.Set(apiCall, (data, failure) =>
                {
                    var success = !failure && data.m_bLeaderboardFound != 0;
                    var leaderboardHandler = data.m_hSteamLeaderboard;

                    if (success)
                        callback?.Invoke(true, null, leaderboardHandler);
                    else
                        callback?.Invoke(false, $"Leaderboard '{leaderboardId}' no found", leaderboardHandler);
                });
            }
        }

        private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t data, bool failure)
        {
            if (failure)
            {
                m_getLeaderboardEntriesCallbackCache?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, "SteamUserStats.DownloadLeaderboardEntries failed"));
                return;
            }
            var entriesCount = data.m_cEntryCount;
            var entriesHandler = data.m_hSteamLeaderboardEntries;
            var entries = new LeaderboardEntry[entriesCount];
            for (var i = 0; i < entriesCount; i++)
            {
                var details = new int[Constants.k_cLeaderboardDetailsMax];
                SteamUserStats.GetDownloadedLeaderboardEntry(entriesHandler, i, out var entry, details, Constants.k_cLeaderboardDetailsMax);
                var username = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                var rank = entry.m_nGlobalRank;
                var score = entry.m_nScore;
                var metadata = new LeaderboardMetadata() { IntValues = details };
                entries[i] = new LeaderboardEntry(username, rank, score, metadata);
            }
            m_getLeaderboardEntriesCallbackCache?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, entries));
        }
    }
}

#endif