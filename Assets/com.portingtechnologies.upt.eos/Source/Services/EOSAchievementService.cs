#if !EOS_DISABLE

using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Stats;
using PlayEveryWare.EpicOnlineServices;
using System;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSAchievementService : IAchievementService
    {
        private readonly EOSServiceContext m_context;
        private bool m_achievementDefinitionsCached;
        private bool m_playerAchievementsCached;
        private bool m_statsCached;

        private AchievementsInterface AchievementsInterface => EOSManager.Instance.GetEOSAchievementInterface();
        private StatsInterface StatsInterface => EOSManager.Instance.GetEOSStatsInterface();

        public EOSAchievementService(EOSServiceContext context)
        {
            m_context = context;
        }

        public void AchievementUnlocked(string achievementId, AchievementUnlockedCallback callback)
        {
            QueryPlayerAchievements((success, errorMessage) =>
            {
                if (!success || !CopyPlayerAchievement(achievementId, out var playerAchievement, out errorMessage))
                {
                    callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var isUnlocked = playerAchievement.Progress >= 1.0;
                callback?.Invoke(new UptAchievementUnlockedResult(ErrorCode.Success, isUnlocked));
            });
        }

        public void AddProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            GetProgress(statId, result =>
            {
                if (!result.IsSuccess)
                {
                    callback?.Invoke(result);
                    return;
                }

                var currentProgress = result.Progress;
                SetProgress(statId, currentProgress + progress, callback);
            });
        }

        public void ClearAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.UnsupportedFeature, "You can do this in Epic Developer Portal"));
        }

        public void ClearAllStatsAndAchievements(AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.UnsupportedFeature, "You can do this in Epic Developer Portal"));
        }

        public void GetAchievementIcon(string achievementId, AchievementGetIconCallback callback)
        {
            /*
            QueryAchievementDefinitions((success, errorMessage) =>
            {
                if (!success || !CopyAchievementDefinition(achievementId, out var definition, out errorMessage))
                {
                    callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                Core.UptLogger.Info($"Achievement icon ID: {definition.UnlockedIconId}");
                var request = UnityWebRequestTexture.GetTexture(definition.UnlockedIconId);
                var operation = request.SendWebRequest();

                operation.completed += op =>
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.UntypedError, $"Couldn't load the icon by URL: {definition.UnlockedIconId}"));
                        return;
                    }
                    var texture = DownloadHandlerTexture.GetContent(request);
                    request.Dispose();
                    callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.Success, null, texture));
                };
            });
            */
            callback?.Invoke(new UptAchievementGetIconResult(ErrorCode.UnsupportedFeature));
        }

        public void GetAchievementInfo(string achievementId, AchievementGetInfoCallback callback)
        {
            QueryAchievementDefinitions((success, errorMessage) =>
            {
                if (!success || !CopyAchievementDefinition(achievementId, out var definition, out errorMessage))
                {
                    callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var name = definition.DisplayName;
                var description = definition.Description;
                var isHidden = definition.IsHidden;
                var maxProgress = 0;

                if (definition.StatThresholds != null)
                {
                    foreach (var threshold in definition.StatThresholds)
                        maxProgress += threshold.Threshold;
                }

                callback?.Invoke(new UptAchievementGetInfoResult(ErrorCode.Success, null, achievementId, name, description, isHidden, maxProgress));
            });
        }

        public void GetProgress(string statId, AchievementGetProgressCallback callback)
        {
            QueryStats((success, errorMessage) =>
            {
                if (!success || !CopyStat(statId, out var stat, out errorMessage))
                {
                    callback?.Invoke(new UptStatGetProgressResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var progress = stat.Value;
                callback?.Invoke(new UptStatGetProgressResult(ErrorCode.Success, progress));
            });
        }

        public void IndicateAchievementProgress(string achievementId, int progress, AchievementGeneralCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.UnsupportedFeature));
        }

        public void SetProgress(string statId, int progress, AchievementGeneralCallback callback)
        {
            var statsData = new IngestData[] { new() { StatName = statId, IngestAmount = progress } };
            var options = new IngestStatOptions()
            {
                LocalUserId = m_context.ProductUserId,
                TargetUserId = m_context.ProductUserId,
                Stats = statsData,
            };

            StatsInterface.IngestStat(ref options, null, Callback);

            void Callback(ref IngestStatCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_statsCached = false;
                    m_playerAchievementsCached = false;
                    callback?.Invoke(new UptResult(ErrorCode.Success));
                }
                else
                {
                    callback?.Invoke(new UptResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)));
                }
            }
        }

        public void UnlockAchievement(string achievementId, AchievementGeneralCallback callback)
        {
            var achievementIds = new Utf8String[] { achievementId };
            var options = new UnlockAchievementsOptions()
            {
                UserId = m_context.ProductUserId,
                AchievementIds = achievementIds
            };

            AchievementsInterface.UnlockAchievements(ref options, null, Callback);

            void Callback(ref OnUnlockAchievementsCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_playerAchievementsCached = false;
                    callback?.Invoke(new UptResult(ErrorCode.Success));
                }
                else
                {
                    callback?.Invoke(new UptResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)));
                }
            }
        }

        private void QueryAchievementDefinitions(Action<bool, string> callback)
        {
            if (m_achievementDefinitionsCached)
            {
                callback?.Invoke(true, null);
                return;
            }

            var options = new QueryDefinitionsOptions()
            {
                LocalUserId = m_context.ProductUserId
            };

            AchievementsInterface.QueryDefinitions(ref options, null, Callback);

            void Callback(ref OnQueryDefinitionsCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_achievementDefinitionsCached = true;
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
                }
            }
        }

        private void QueryPlayerAchievements(Action<bool, string> callback)
        {
            if (m_playerAchievementsCached)
            {
                callback?.Invoke(true, null);
                return;
            }

            var options = new QueryPlayerAchievementsOptions()
            {
                LocalUserId = m_context.ProductUserId,
                TargetUserId = m_context.ProductUserId
            };

            AchievementsInterface.QueryPlayerAchievements(ref options, null, Callback);

            void Callback(ref OnQueryPlayerAchievementsCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_playerAchievementsCached = true;
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
                }
            }
        }

        private void QueryStats(Action<bool, string> callback)
        {
            if (m_statsCached)
            {
                callback?.Invoke(true, null);
                return;
            }

            var options = new QueryStatsOptions()
            {
                LocalUserId = m_context.ProductUserId,
                TargetUserId = m_context.ProductUserId
            };

            StatsInterface.QueryStats(ref options, null, Callback);

            void Callback(ref OnQueryStatsCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_statsCached = true;
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
                }
            }
        }

        private bool CopyAchievementDefinition(string achievementId, out Definition outDefinition, out string errorMessage)
        {
            outDefinition = new Definition();
            errorMessage = null;

            var options = new CopyAchievementDefinitionByAchievementIdOptions
            {
                AchievementId = achievementId
            };

            var result = AchievementsInterface.CopyAchievementDefinitionByAchievementId(ref options, out var definition);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!definition.HasValue)
            {
                errorMessage = $"No definition found for achievement ID '{achievementId}'";
                return false;
            }

            outDefinition = definition.Value;
            return true;
        }

        private bool CopyPlayerAchievement(string achievementId, out PlayerAchievement outPlayerAchievement, out string errorMessage)
        {
            outPlayerAchievement = new PlayerAchievement();
            errorMessage = null;

            var options = new CopyPlayerAchievementByAchievementIdOptions
            {
                AchievementId = achievementId,
                LocalUserId = m_context.ProductUserId,
                TargetUserId = m_context.ProductUserId
            };

            var result = AchievementsInterface.CopyPlayerAchievementByAchievementId(ref options, out var playerAchievement);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!playerAchievement.HasValue)
            {
                errorMessage = $"No player achievement found for achievement ID '{achievementId}'";
                return false;
            }

            outPlayerAchievement = playerAchievement.Value;
            return true;
        }

        private bool CopyStat(string statId, out Stat outStat, out string errorMessage)
        {
            outStat = new Stat();
            errorMessage = null;

            var options = new CopyStatByNameOptions
            {
                Name = statId,
                TargetUserId = m_context.ProductUserId
            };

            var result = StatsInterface.CopyStatByName(ref options, out var stat);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!stat.HasValue)
            {
                errorMessage = $"No player achievement found for achievement ID '{statId}'";
                return false;
            }

            outStat = stat.Value;
            return true;
        }
    }
}

#endif