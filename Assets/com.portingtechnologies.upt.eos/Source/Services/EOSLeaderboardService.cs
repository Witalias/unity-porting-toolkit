#if !EOS_DISABLE

using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;
using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using UnityEngine;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSLeaderboardService : ILeaderboardService
    {
        private readonly EOSServiceContext m_context;

        private bool m_leaderboardDefinitionsQueried;

        private LeaderboardsInterface LeaderboardsInterface => EOSManager.Instance.GetEOSLeaderboardsInterface();

        public EOSLeaderboardService(EOSServiceContext context)
        {
            m_context = context;
        }

        public void GetEntriesAroundUser(string leaderboardId, int range, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            QueryLeaderboardRanks(leaderboardId, (success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var recordCount = (int)GetLeaderboardRecordCount();
                if (recordCount == 0)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, new LeaderboardEntry[0]));
                    return;
                }

                if (!CopyLeaderboardRank(m_context.ProductUserId, out var selfRecord, out errorMessage))
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var selfEntryIndex = (int)selfRecord.Rank - 1;
                var countToFetch = range * 2 + 1;
                var startIndex = 0;

                if (countToFetch >= recordCount)
                {
                    countToFetch = recordCount;
                }
                else
                {
                    startIndex = Mathf.Max(0, selfEntryIndex - range);
                    startIndex = Mathf.Min(startIndex, recordCount - countToFetch);
                }

                var entries = new LeaderboardEntry[countToFetch];
                var userIds = new ProductUserId[countToFetch];

                for (var i = startIndex; i < startIndex + countToFetch; i++)
                {
                    LeaderboardRecord record;
                    if (i == selfEntryIndex)
                    {
                        record = selfRecord;
                    }
                    else if (!CopyLeaderboardRank((uint)i, out record, out errorMessage))
                    {
                        callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                        return;
                    }

                    var username = record.UserDisplayName;
                    var rank = (int)record.Rank;
                    var score = record.Score;
                    entries[i - startIndex] = new LeaderboardEntry(username, rank, score);
                    userIds[i - startIndex] = record.UserId;
                }

                RetrieveUserStatsAdditional(entries, userIds, metadata, (success, errorMessage) =>
                {
                    if (success)
                        callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, entries));
                    else
                        callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                });
            });
        }

        public void GetFriendsEntries(string leaderboardId, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            throw new System.NotImplementedException();
        }

        public void GetGlobalEntries(string leaderboardId, int count, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata)
        {
            if (count <= 0)
            {
                callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, new LeaderboardEntry[0]));
                return;
            }

            QueryLeaderboardRanks(leaderboardId, (success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var recordCount = GetLeaderboardRecordCount();
                if (recordCount == 0)
                {
                    callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, new LeaderboardEntry[0]));
                    return;
                }

                var requestedCount = Mathf.Min((int)recordCount, count);
                var entries = new LeaderboardEntry[requestedCount];
                var userIds = new ProductUserId[recordCount];
                for (uint i = 0; i < requestedCount; i++)
                {
                    if (!CopyLeaderboardRank(i, out var record, out errorMessage))
                    {
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                            return;
                        }
                        continue;
                    }
                    var username = record.UserDisplayName;
                    var rank = record.Rank;
                    var score = record.Score;
                    entries[i] = new LeaderboardEntry(username, (int)rank, score);
                    userIds[i] = record.UserId;
                }

                RetrieveUserStatsAdditional(entries, userIds, metadata, (success, errorMessage) =>
                {
                    if (success)
                        callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.Success, null, entries));
                    else
                        callback?.Invoke(new UptGetLeaderboardEntriesResult(ErrorCode.UntypedError, errorMessage));
                });
            });
        }

        public void GetLeaderboardInfo(string leaderboardId, GetLeaderboardInfoCallback callback)
        {
            QueryLeaderboardDefinitions((success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                if (!CopyLeaderboardDefinition(leaderboardId, out var definition, out errorMessage))
                {
                    callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                QueryLeaderboardRanks(leaderboardId, (success, errorMessage) =>
                {
                    if (!success)
                    {
                        callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.UntypedError, errorMessage));
                        return;
                    }

                    var name = definition.StatName;
                    var entryCount = (int)GetLeaderboardRecordCount();

                    callback?.Invoke(new UptGetLeaderboardInfoResult(ErrorCode.Success, null, leaderboardId, name, entryCount));
                });
            });
        }

        public void UploadScore(string leaderboardId, int score, LeaderboardGeneralCallback callback, LeaderboardMetadata metadata)
        {
            callback?.Invoke(new UptResult(ErrorCode.UnsupportedFeature, "Upload statistics via AchievementService"));
        }

        private void RetrieveUserStatsAdditional(LeaderboardEntry[] entries, ProductUserId[] userIds, LeaderboardMetadata metadata, Action<bool, string> callback)
        {
            if (metadata == null || metadata.StringValues == null || metadata.StringValues.Length == 0) // stat names
            {
                callback?.Invoke(true, null);
                return;
            }

            var statNames = metadata.StringValues;
            QueryLeaderboardUserScores(userIds, statNames, (success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, errorMessage);
                    return;
                }

                for (uint i = 0; i < userIds.Length; i++)
                {
                    var entryMetadata = new LeaderboardMetadata() { IntValues = new int[statNames.Length] };
                    for (uint j = 0; j < statNames.Length; j++)
                    {
                        if (!CopyLeaderboardUserScore(i, statNames[j], out var leaderboardUserScore, out errorMessage))
                        {
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                callback?.Invoke(false, errorMessage);
                                return;
                            }
                            continue;
                        }
                        entryMetadata.IntValues[j] = leaderboardUserScore.Score;
                    }
                    entries[i].Metadata = entryMetadata;
                }

                callback?.Invoke(true, null);
            });
        }

        private void QueryLeaderboardRanks(string leaderboardId, Action<bool, string> callback)
        {
            var options = new QueryLeaderboardRanksOptions()
            {
                LeaderboardId = leaderboardId,
                LocalUserId = m_context.ProductUserId
            };

            LeaderboardsInterface.QueryLeaderboardRanks(ref options, null, Callback);

            void Callback(ref OnQueryLeaderboardRanksCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                    callback?.Invoke(true, null);
                else
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
            }
        }

        private void QueryLeaderboardDefinitions(Action<bool, string> callback)
        {
            if (m_leaderboardDefinitionsQueried)
            {
                callback?.Invoke(true, null);
                return;
            }

            var options = new QueryLeaderboardDefinitionsOptions()
            {
                LocalUserId = m_context.ProductUserId
            };

            LeaderboardsInterface.QueryLeaderboardDefinitions(ref options, null, Callback);

            void Callback(ref OnQueryLeaderboardDefinitionsCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_leaderboardDefinitionsQueried = true;
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
                }
            }
        }

        private void QueryLeaderboardUserScores(ProductUserId[] userIds, IReadOnlyList<string> statNames, Action<bool, string> callback)
        {
            var statInfo = new UserScoresQueryStatInfo[statNames.Count];
            for (var i = 0; i < statInfo.Length; i++)
            {
                statInfo[i] = new UserScoresQueryStatInfo()
                {
                    StatName = statNames[i],
                    Aggregation = LeaderboardAggregation.Max
                };
            }

            var options = new QueryLeaderboardUserScoresOptions()
            {
                LocalUserId = m_context.ProductUserId,
                UserIds = userIds,
                StatInfo = statInfo
            };

            LeaderboardsInterface.QueryLeaderboardUserScores(ref options, null, Callback);

            void Callback(ref OnQueryLeaderboardUserScoresCompleteCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                    callback?.Invoke(true, null);
                else
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
            }
        }

        private bool CopyLeaderboardDefinition(string leaderboardId, out Definition outDefinition, out string errorMessage)
        {
            outDefinition = new Definition();
            errorMessage = null;

            var options = new CopyLeaderboardDefinitionByLeaderboardIdOptions()
            {
                LeaderboardId = leaderboardId
            };

            var result = LeaderboardsInterface.CopyLeaderboardDefinitionByLeaderboardId(ref options, out var definition);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!definition.HasValue)
            {
                errorMessage = $"No leaderboard definition found '{leaderboardId}'";
                return false;
            }

            outDefinition = definition.Value;
            return true;
        }

        private bool CopyLeaderboardRank(uint index, out LeaderboardRecord outLeaderboardRecord, out string errorMessage)
        {
            outLeaderboardRecord = new LeaderboardRecord();
            errorMessage = null;

            var options = new CopyLeaderboardRecordByIndexOptions()
            {
                LeaderboardRecordIndex = index
            };

            var result = LeaderboardsInterface.CopyLeaderboardRecordByIndex(ref options, out var leaderboardRecord);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!leaderboardRecord.HasValue)
                return false;

            outLeaderboardRecord = leaderboardRecord.Value;
            return true;
        }

        private bool CopyLeaderboardRank(ProductUserId userId, out LeaderboardRecord outLeaderboardRecord, out string errorMessage)
        {
            outLeaderboardRecord = new LeaderboardRecord();
            errorMessage = null;

            var options = new CopyLeaderboardRecordByUserIdOptions()
            {
                UserId = userId
            };

            var result = LeaderboardsInterface.CopyLeaderboardRecordByUserId(ref options, out var leaderboardRecord);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!leaderboardRecord.HasValue)
                return false;

            outLeaderboardRecord = leaderboardRecord.Value;
            return true;
        }

        private bool CopyLeaderboardUserScore(uint index, string statName, out LeaderboardUserScore outLeaderboardUserScore, out string errorMessage)
        {
            outLeaderboardUserScore = new LeaderboardUserScore();
            errorMessage = null;

            var options = new CopyLeaderboardUserScoreByIndexOptions()
            {
                LeaderboardUserScoreIndex = index,
                StatName = statName
            };

            var result = LeaderboardsInterface.CopyLeaderboardUserScoreByIndex(ref options, out var leaderboardUserScore);
            if (result != Result.Success)
            {
                errorMessage = EOSServiceContext.GetBackendErrorMsg(result);
                return false;
            }

            if (!leaderboardUserScore.HasValue)
                return false;

            outLeaderboardUserScore = leaderboardUserScore.Value;
            return true;
        }

        private uint GetLeaderboardRecordCount()
        {
            var options = new GetLeaderboardRecordCountOptions();
            return LeaderboardsInterface.GetLeaderboardRecordCount(ref options);
        }
    }
}

#endif