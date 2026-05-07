using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    public class LeaderboardsManager : MonoBehaviour
    {
        [Header("Get Global Entries")]
        [SerializeField] private TMP_InputField m_getGlobalEntriesLeaderboardId;
        [SerializeField] private TMP_InputField m_getGlobalEntriesCount;
        [SerializeField] private DynamicInputList m_getGlobalEntriesMetaIntValues;
        [SerializeField] private DynamicInputList m_getGlobalEntriesMetaStringValues;
        [SerializeField] private Button m_getGlobalEntriesButton;

        [Header("Get Entries Around User")]
        [SerializeField] private TMP_InputField m_getEntriesAroundUserLeaderboardId;
        [SerializeField] private TMP_InputField m_getEntriesAroundUserRange;
        [SerializeField] private DynamicInputList m_getEntriesAroundUserMetaIntValues;
        [SerializeField] private DynamicInputList m_getEntriesAroundUserMetaStringValues;
        [SerializeField] private Button m_getEntriesAroundUserButton;

        [Header("Get Friends Entries")]
        [SerializeField] private TMP_InputField m_getFriendsEntriesLeaderboardId;
        [SerializeField] private DynamicInputList m_getFriendsEntriesMetaIntValues;
        [SerializeField] private DynamicInputList m_getFriendsEntriesMetaStringValues;
        [SerializeField] private Button m_getFriendsEntriesButton;

        [Header("Upload Score")]
        [SerializeField] private TMP_InputField m_uploadScoreLeaderboardId;
        [SerializeField] private TMP_InputField m_uploadScoreValue;
        [SerializeField] private DynamicInputList m_uploadScoreMetaIntValues;
        [SerializeField] private DynamicInputList m_uploadScoreMetaStringValues;
        [SerializeField] private Button m_uploadScoreButton;

        [Header("Get Leaderboard Info")]
        [SerializeField] private TMP_InputField m_getInfoLeaderboardId;
        [SerializeField] private Button m_getInfoButton;

        private Services.ILeaderboardService m_leaderboardService;

        private void Awake()
        {
            m_leaderboardService = ServiceContainer.Get<Services.ILeaderboardService>();
        }

        private void OnEnable()
        {
            if (m_leaderboardService is IMockService)
                Debug.LogWarning("Leaderboard service is mock! All functions will return a successfull result without any metadata");

            if (m_getGlobalEntriesButton)
                m_getGlobalEntriesButton.onClick.AddListener(OnGetEntriesButtonClick);

            if (m_getEntriesAroundUserButton)
                m_getEntriesAroundUserButton.onClick.AddListener(OnGetEntriesAroundUserButtonClick);

            if (m_getFriendsEntriesButton)
                m_getFriendsEntriesButton.onClick.AddListener(OnGetFriendsEntriesButtonClick);

            if (m_uploadScoreButton)
                m_uploadScoreButton.onClick.AddListener(OnUploadScoresButtonClick);

            if (m_getInfoButton)
                m_getInfoButton.onClick.AddListener(OnGetLeaderboardInfoButtonClick);
        }

        private void OnDisable()
        {
            if (m_getGlobalEntriesButton)
                m_getGlobalEntriesButton.onClick.RemoveListener(OnGetEntriesButtonClick);

            if (m_getEntriesAroundUserButton)
                m_getEntriesAroundUserButton.onClick.RemoveListener(OnGetEntriesAroundUserButtonClick);

            if (m_getFriendsEntriesButton)
                m_getFriendsEntriesButton.onClick.RemoveListener(OnGetFriendsEntriesButtonClick);

            if (m_uploadScoreButton)
                m_uploadScoreButton.onClick.RemoveListener(OnUploadScoresButtonClick);

            if (m_getInfoButton)
                m_getInfoButton.onClick.RemoveListener(OnGetLeaderboardInfoButtonClick);
        }

        private void OnGetEntriesButtonClick()
        {
            if (!m_getGlobalEntriesLeaderboardId || !m_getGlobalEntriesCount || !m_getGlobalEntriesMetaIntValues || !m_getGlobalEntriesMetaStringValues)
                return;

            var leaderboardId = m_getGlobalEntriesLeaderboardId.text;
            var countStr = m_getGlobalEntriesCount.text;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID field is empty. Please enter the correct leaderboard ID");
                return;
            }

            if (string.IsNullOrEmpty(countStr))
            {
                Debug.LogWarning("Count field is empty. Please enter the correct count value");
                return;
            }

            if (!int.TryParse(countStr, out var count))
            {
                Debug.LogWarning("Count value is incorrect. Please enter the correct count value");
                return;
            }

            var stringValues = m_getGlobalEntriesMetaStringValues.GetStringValues();
            var intValues = m_getGlobalEntriesMetaIntValues.GetIntValues();

            if (stringValues == null)
            {
                Debug.LogWarning("At least one value is missing from the String Values. Please make sure that all fields are filled in");
                return;
            }

            if (intValues == null)
            {
                Debug.LogWarning("At least one value in Int Values is missing or incorrect. Please make sure that all fields are filled in and the correct values are specified");
                return;
            }

            Services.LeaderboardMetadata metadata = null;
            if (stringValues.Length > 0 || intValues.Length > 0)
                metadata = new() { IntValues = intValues, StringValues = stringValues };

            GlobalManager.Instance.SetSpinnerActive(true);
            m_leaderboardService.GetGlobalEntries(leaderboardId, count, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Get global entries with the leaderboard ID '{leaderboardId}' success!");
                    LogEntries(result.Entries);
                }
                else
                {
                    Debug.LogError($"Get global entries with the leaderboard ID '{leaderboardId}' failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            }, metadata);
        }

        private void OnGetEntriesAroundUserButtonClick()
        {
            if (!m_getEntriesAroundUserLeaderboardId || !m_getEntriesAroundUserRange || !m_getEntriesAroundUserMetaIntValues || !m_getEntriesAroundUserMetaStringValues)
                return;

            var leaderboardId = m_getEntriesAroundUserLeaderboardId.text;
            var rangeStr = m_getEntriesAroundUserRange.text;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID field is empty. Please enter the correct leaderboard ID");
                return;
            }

            if (string.IsNullOrEmpty(rangeStr))
            {
                Debug.LogWarning("Count field is empty. Please enter the correct count value");
                return;
            }

            if (!int.TryParse(rangeStr, out var range))
            {
                Debug.LogWarning("Count value is incorrect. Please enter the correct count value");
                return;
            }

            var stringValues = m_getEntriesAroundUserMetaStringValues.GetStringValues();
            var intValues = m_getEntriesAroundUserMetaIntValues.GetIntValues();

            if (stringValues == null)
            {
                Debug.LogWarning("At least one value is missing from the String Values. Please make sure that all fields are filled in");
                return;
            }

            if (intValues == null)
            {
                Debug.LogWarning("At least one value in Int Values is missing or incorrect. Please make sure that all fields are filled in and the correct values are specified");
                return;
            }

            Services.LeaderboardMetadata metadata = null;
            if (stringValues.Length > 0 || intValues.Length > 0)
                metadata = new() { IntValues = intValues, StringValues = stringValues };

            GlobalManager.Instance.SetSpinnerActive(true);
            m_leaderboardService.GetEntriesAroundUser(leaderboardId, range, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Get entries around user with the leaderboard ID '{leaderboardId}' success!");
                    LogEntries(result.Entries);
                }
                else
                {
                    Debug.LogError($"Get entries around user with the leaderboard ID '{leaderboardId}' failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            }, metadata);
        }

        private void OnGetFriendsEntriesButtonClick()
        {
            if (!m_getFriendsEntriesLeaderboardId || !m_getFriendsEntriesMetaIntValues || !m_getFriendsEntriesMetaStringValues)
                return;

            var leaderboardId = m_getFriendsEntriesLeaderboardId.text;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID field is empty. Please enter the correct leaderboard ID");
                return;
            }

            var stringValues = m_getFriendsEntriesMetaStringValues.GetStringValues();
            var intValues = m_getFriendsEntriesMetaIntValues.GetIntValues();

            if (stringValues == null)
            {
                Debug.LogWarning("At least one value is missing from the String Values. Please make sure that all fields are filled in");
                return;
            }

            if (intValues == null)
            {
                Debug.LogWarning("At least one value in Int Values is missing or incorrect. Please make sure that all fields are filled in and the correct values are specified");
                return;
            }

            Services.LeaderboardMetadata metadata = null;
            if (stringValues.Length > 0 || intValues.Length > 0)
                metadata = new() { IntValues = intValues, StringValues = stringValues };

            GlobalManager.Instance.SetSpinnerActive(true);
            m_leaderboardService.GetFriendsEntries(leaderboardId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Get friends entries with the leaderboard ID '{leaderboardId}' success!");
                    LogEntries(result.Entries);
                }
                else
                {
                    Debug.LogError($"Get friends entries with the leaderboard ID '{leaderboardId}' failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            }, metadata);
        }

        private void OnUploadScoresButtonClick()
        {
            if (!m_uploadScoreLeaderboardId || !m_uploadScoreValue || !m_uploadScoreMetaIntValues || !m_uploadScoreMetaStringValues)
                return;

            var leaderboardId = m_uploadScoreLeaderboardId.text;
            var scoreStr = m_uploadScoreValue.text;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID field is empty. Please enter the correct leaderboard ID");
                return;
            }

            if (string.IsNullOrEmpty(scoreStr))
            {
                Debug.LogWarning("Count field is empty. Please enter the correct count value");
                return;
            }

            if (!int.TryParse(scoreStr, out var score))
            {
                Debug.LogWarning("Count value is incorrect. Please enter the correct count value");
                return;
            }

            var stringValues = m_uploadScoreMetaStringValues.GetStringValues();
            var intValues = m_uploadScoreMetaIntValues.GetIntValues();

            if (stringValues == null)
            {
                Debug.LogWarning("At least one value is missing from the String Values. Please make sure that all fields are filled in");
                return;
            }

            if (intValues == null)
            {
                Debug.LogWarning("At least one value in Int Values is missing or incorrect. Please make sure that all fields are filled in and the correct values are specified");
                return;
            }

            Services.LeaderboardMetadata metadata = null;
            if (stringValues.Length > 0 || intValues.Length > 0)
                metadata = new() { IntValues = intValues, StringValues = stringValues };

            GlobalManager.Instance.SetSpinnerActive(true);
            m_leaderboardService.UploadScore(leaderboardId, score, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Upload score for the leaderboard ID '{leaderboardId}' success!");
                else
                    Debug.LogError($"Upload score for the leaderboard ID '{leaderboardId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            }, metadata);
        }

        private void OnGetLeaderboardInfoButtonClick()
        {
            if (!m_getInfoLeaderboardId)
                return;

            var leaderboardId = m_getInfoLeaderboardId.text;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID field is empty. Please enter the correct leaderboard ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_leaderboardService.GetLeaderboardInfo(leaderboardId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Leaderboard '{leaderboardId}' info:\n\tLeaderboard ID: {result.LeaderboardId}" +
                        $"\n\tLeaderboard name: {result.LeaderboardName}" +
                        $"\n\tEntry count: {result.EntryCount}");
                }
                else
                {
                    Debug.LogError($"Get leaderboard '{leaderboardId}' info failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            });
        }

        private void LogEntries(Services.LeaderboardEntry[] entries)
        {
            var log = new StringBuilder();
            foreach (var entry in entries)
            {
                log.Append($"\t{entry.Rank}. {entry.Username} : {entry.Score}. ");
                if (entry.Metadata != null)
                {
                    var meta = entry.Metadata;

                    if (meta.StringValues != null && meta.StringValues.Length > 0)
                        log.Append($"String Values: [{string.Join(", ", meta.StringValues)}]. ");

                    if (meta.IntValues != null && meta.IntValues.Length > 0)
                        log.Append($"Int Values: [{string.Join(", ", meta.IntValues)}]. ");
                }
                log.Append('\n');
            }
            Debug.Log(log.ToString());
        }
    }
}
