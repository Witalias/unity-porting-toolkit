namespace UPT.Services
{
    public interface ILeaderboardService
    {
        /// <summary>
        /// Get leaderboard data such as the name and number of entries.
        /// </summary>
        /// <param name="leaderboardId">The ID of the leaderboard to get data about. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        void GetLeaderboardInfo(string leaderboardId, GetLeaderboardInfoCallback callback);

        /// <summary>
        /// Request global rankings for a specific leaderboard.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> You can specify <see cref="LeaderboardMetadata.StringValues"/> in <paramref name="metadata"/> to additionally request the corresponding stats for each user in the resulting rating.
        /// <br/><b>Others:</b> <paramref name="metadata"/> is not used, specify NULL.
        /// </remarks>
        /// <param name="leaderboardId">The ID of the leaderboard to get the entries from. You can usually find it on the developer's portal.</param>
        /// <param name="count">The number of entries. Please note that different platforms may have their own limits on the maximum number of entries.</param>
        /// <param name="callback"></param>
        /// <param name="metadata">Metadata that depends on the platform. In general cases, NULL can be specified.</param>
        ///// <summary>
        ///// Запросить глобальные рейтинги указанной таблицы лидеров.
        ///// </summary>
        ///// <remarks>
        ///// <i>Платформенно-специфичные примечания</i>
        ///// <br/><b>EPIC ONLINE SERVICES:</b> Вы можете указать<see cref = "LeaderboardMetadata.StringValues" /> в < paramref name= "metadata" />, чтобы дополнительно запросить соответствующую статистику для каждого пользователя в результирующем рейтинге.
        ///// <br/><b>Другие:</b> <paramref name = "metadata" /> не используется, укажите значение NULL.
        ///// </remarks>
        ///// <param name = "leaderboardId" > Идентификатор таблицы лидеров, из которой нужно получить записи.Как правило, его можно найти на портале разработчика.</param>
        ///// <param name = "count" > Количество записей.Обратите внимание, что на разных платформах могут быть свои ограничения на максимальное количество записей.</param>
        ///// <param name = "callback" ></ param >
        ///// < param name= "metadata" > Метаданные, зависящие от платформы.В общем случае достаточно указать значение NULL.</param>
        void GetGlobalEntries(string leaderboardId, int count, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata);

        /// <summary>
        /// Request friend rankings for a specific leaderboard.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> You can specify <see cref="LeaderboardMetadata.StringValues"/> in <paramref name="metadata"/> to additionally request the corresponding stats for each user in the resulting rating.
        /// <br/><b>Others:</b> <paramref name="metadata"/> is not used, specify NULL.
        /// </remarks>
        /// <param name="leaderboardId">The ID of the leaderboard to get the entries from. You can usually find it on the developer's portal.</param>
        /// <param name="callback"></param>
        /// <param name="metadata">Metadata that depends on the platform. In general cases, NULL can be specified.</param>
        void GetFriendsEntries(string leaderboardId, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata);

        /// <summary>
        /// Request rankings relative a user's entry for a specific leaderboard.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>EOS:</b> You can specify <see cref="LeaderboardMetadata.StringValues"/> in <paramref name="metadata"/> to additionally request the corresponding stats for each user in the resulting rating.
        /// <br/><b>Others:</b> <paramref name="metadata"/> is not used, specify NULL.
        /// </remarks>
        /// <param name="leaderboardId">The ID of the leaderboard to get the entries from. You can usually find it on the developer's portal.</param>
        /// <param name="range">The number of entries to be retrieved before and after the user.
        /// For example, if the user's current position is #5, then setting <paramref name="range"/> to 2 will return 5 entries: ranks from #3 to #7.</param>
        /// <param name="callback"></param>
        /// <param name="metadata">Metadata that depends on the platform. In general cases, NULL can be specified.</param>
        void GetEntriesAroundUser(string leaderboardId, int range, GetLeaderboardEntriesCallback callback, LeaderboardMetadata metadata);

        /// <summary>
        /// Upload a score to a specific leaderboard. When comparing two scores, the best one will be selected.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>STEAM / VK PLAY:</b> You can specify <see cref="LeaderboardMetadata.IntValues"/> in <paramref name="metadata"/> to add additional details to the entry.
        /// <br/><b>EOS: </b> This feature is not supported. Instead, upload additional statistics via <see cref="IAchievementService.SetProgress(string, int, AchievementGeneralCallback)"/> and <see cref="IAchievementService.AddProgress(string, int, AchievementGeneralCallback)"/>.
        /// <br/><b>Others:</b> <paramref name="metadata"/> is not used, specify NULL.
        /// </remarks>
        /// <param name="leaderboardId">The ID of the leaderboard to get the entries from. You can usually find it on the developer's portal.</param>
        /// <param name="score">The score that will be uploaded to the entry.</param>
        /// <param name="callback"></param>
        /// <param name="metadata">Metadata that depends on the platform. In general cases, NULL can be specified.</param>
        void UploadScore(string leaderboardId, int score, LeaderboardGeneralCallback callback, LeaderboardMetadata metadata);
    }

    public class LeaderboardEntry
    {
        public string Username { get; }
        public int Rank { get; }
        public int Score { get; }
        public LeaderboardMetadata Metadata { get; set; }

        public LeaderboardEntry(string username = null, int rank = 0, int score = 0, LeaderboardMetadata metadata = null)
        {
            Username = username;
            Rank = rank;
            Score = score;
            Metadata = metadata;
        }
    }

    public class LeaderboardMetadata
    {
        public int[] IntValues;
        public string[] StringValues;
    }

    public class UptGetLeaderboardEntriesResult : UptResult
    {
        /// <summary>
        /// Leaderboard entries received from the platform.
        /// </summary>
        /// <remarks>
        /// Platform-specific notes
        /// <br/><b>STEAM / VK PLAY:</b> <see cref="LeaderboardMetadata.IntValues"/> contains additional details that were specified when uploading the score in <see cref="ILeaderboardService.UploadScore(string, int, LeaderboardGeneralCallback, LeaderboardMetadata)"/>
        /// <br/><b>EOS:</b> <see cref="LeaderboardMetadata.IntValues"/> contains the values of the stats corresponding to the array <see cref="LeaderboardMetadata.StringValues"/> sent as metadata when requesting entries.
        /// <br/><b>Others:</b> <see cref="LeaderboardEntry.Metadata"/> is always NULL.
        /// </remarks>
        public LeaderboardEntry[] Entries;

        public UptGetLeaderboardEntriesResult(ErrorCode error, string innerMessage = null, LeaderboardEntry[] entries = null) : base(error, innerMessage)
        {
            Entries = entries;
        }
    }

    public class UptGetLeaderboardInfoResult : UptResult
    {
        public string LeaderboardId { get; }
        public string LeaderboardName { get; }
        public int EntryCount { get; }

        public UptGetLeaderboardInfoResult(ErrorCode error, string innerMessage = null, string leaderboardId = null,
            string leaderboardName = null, int entryCount = 0) : base(error, innerMessage)
        {
            LeaderboardId = leaderboardId;
            LeaderboardName = leaderboardName;
            EntryCount = entryCount;
        }
    }

    public delegate void LeaderboardGeneralCallback(UptResult result);
    public delegate void GetLeaderboardEntriesCallback(UptGetLeaderboardEntriesResult result);
    public delegate void GetLeaderboardInfoCallback(UptGetLeaderboardInfoResult result);
}
