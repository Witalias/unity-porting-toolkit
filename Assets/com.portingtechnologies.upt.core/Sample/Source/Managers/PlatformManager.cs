using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    public class PlatformManager : MonoBehaviour
    {
        [Header("Get Locale Code")]
        [SerializeField] private Button m_getLocaleCodeButton;

        [Header("Get Username")]
        [SerializeField] private Button m_getUsernameButton;

        [Header("Get User ID")]
        [SerializeField] private Button m_getUserIdButton;

        [Header("Is Overlay Visible")]
        [SerializeField] private Button m_isOverlayVisibleButton;

        private Services.IPlatformService m_platformService;

        private void Awake()
        {
            m_platformService = ServiceContainer.Get<Services.IPlatformService>();
            m_platformService.OnOverlayStateChanged += OnOverlayStateChanged;
        }

        private void OnEnable()
        {
            if (m_platformService is IMockService)
                Debug.LogWarning("Platform service is mock! All functions will return a successfull result without any metadata");

            if (m_getLocaleCodeButton)
                m_getLocaleCodeButton.onClick.AddListener(OnGetLocaleCodeButton);

            if (m_getUsernameButton)
                m_getUsernameButton.onClick.AddListener(OnGetUsernameButton);

            if (m_getUserIdButton)
                m_getUserIdButton.onClick.AddListener(OnGetUserIdButton);

            if (m_isOverlayVisibleButton)
                m_isOverlayVisibleButton.onClick.AddListener(OnIsOverlayVisibleButton);
        }

        private void OnDisable()
        {
            if (m_getLocaleCodeButton)
                m_getLocaleCodeButton.onClick.RemoveListener(OnGetLocaleCodeButton);

            if (m_getUsernameButton)
                m_getUsernameButton.onClick.RemoveListener(OnGetUsernameButton);

            if (m_getUserIdButton)
                m_getUserIdButton.onClick.RemoveListener(OnGetUserIdButton);

            if (m_isOverlayVisibleButton)
                m_isOverlayVisibleButton.onClick.AddListener(OnIsOverlayVisibleButton);
        }

        private void OnOverlayStateChanged(bool isVisible)
        {
            Debug.Log($"Overlay state changed. Is visible: {isVisible}");
        }

        private void OnGetLocaleCodeButton()
        {
            var localeCode = m_platformService.GetLocaleCode();
            Debug.Log($"Locale code: {localeCode}");

            var leaderboardService = ServiceContainer.Get<Services.ILeaderboardService>();

            leaderboardService.GetEntriesAroundUser("my_leaderboard_id", 2, result =>
            {
                if (result.IsSuccess)
                {
                    foreach (var entry in result.Entries)
                    {
                        Debug.Log($"Username - {entry.Username}, rank - {entry.Rank}, score - {entry.Score}");
                    }
                }
            }, null);
        }

        private void OnGetUsernameButton()
        {
            var username = m_platformService.GetUsername();
            if (string.IsNullOrEmpty(username))
                Debug.Log("Username is NULL. The game may not have been launched via Epic Games Launcher");
            Debug.Log($"Username: {username}");
        }

        private void OnGetUserIdButton()
        {
            var userId = m_platformService.GetUserID();
            Debug.Log($"User ID: {userId}");
        }

        private void OnIsOverlayVisibleButton()
        {
            var isVisible = m_platformService.IsOverlayVisible();
            Debug.Log($"Is overlay visible: {isVisible}");
        }
    }
}
