using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UPT.Services;

namespace UPT.Core.Samples
{
    public class AchievementsManager : MonoBehaviour
    {
        [Header("Unlock Achievement")]
        [SerializeField] private TMP_InputField m_unlockAchievementIdInputField;
        [SerializeField] private Button m_unlockButton;

        [Header("Clear Achievement")]
        [SerializeField] private TMP_InputField m_clearAchievementIdInputField;
        [SerializeField] private Button m_clearButton;

        [Header("Is Achievement Unlocked")]
        [SerializeField] private TMP_InputField m_checkAchievementIdInputField;
        [SerializeField] private Button m_checkButton;

        [Header("Get Achievement Info")]
        [SerializeField] private TMP_InputField m_getInfoAchievementIdInputField;
        [SerializeField] private Button m_getInfoButton;

        [Header("Get Achievement Icon")]
        [SerializeField] private Image m_getIconImage;
        [SerializeField] private TMP_InputField m_getIconAchievementIdInputField;
        [SerializeField] private Button m_getIconButton;

        [Header("Indicate Achievement Progress")]
        [SerializeField] private TMP_InputField m_indicateAchievementIdInputField;
        [SerializeField] private TMP_InputField m_indicateAmountInputField;
        [SerializeField] private Button m_indicateButton;

        [Header("Add Progress")]
        [SerializeField] private TMP_InputField m_addProgressStatIdInputField;
        [SerializeField] private TMP_InputField m_addProgressAmountInputField;
        [SerializeField] private Button m_addProgressButton;

        [Header("Set Progress")]
        [SerializeField] private TMP_InputField m_setProgressStatIdInputField;
        [SerializeField] private TMP_InputField m_setProgressAmountInputField;
        [SerializeField] private Button m_setProgressButton;

        [Header("Get Progress")]
        [SerializeField] private TMP_InputField m_getProgressStatIdInputField;
        [SerializeField] private Button m_getProgressButton;

        [Header("Clear All Achievements And Stats")]
        [SerializeField] private Button m_clearAllButton;

        private IAchievementService m_achievementService;

        private void Awake()
        {
            m_achievementService = ServiceContainer.Get<IAchievementService>();
        }

        private void OnEnable()
        {
            if (m_achievementService is IMockService)
                Debug.LogWarning("Achievement service is mock! All functions will return a successfull result without any metadata");

            if (m_unlockButton != null)
                m_unlockButton.onClick.AddListener(OnUnlockButtonClick);

            if (m_clearButton != null)
                m_clearButton.onClick.AddListener(OnClearButtonClick);

            if (m_checkButton != null)
                m_checkButton.onClick.AddListener(OnCheckButtonClick);

            if (m_getInfoButton != null)
                m_getInfoButton.onClick.AddListener(OnGetInfoButtonClick);

            if (m_getIconButton != null)
                m_getIconButton.onClick.AddListener(OnGetIconButtonClick);

            if (m_indicateButton != null)
                m_indicateButton.onClick.AddListener(OnIndicateButtonClick);

            if (m_addProgressButton != null)
                m_addProgressButton.onClick.AddListener(OnAddProgressButtonClick);

            if (m_setProgressButton != null)
                m_setProgressButton.onClick.AddListener(OnSetProgressButtonClick);

            if (m_getProgressButton != null)
                m_getProgressButton.onClick.AddListener(OnGetProgressButtonClick);

            if (m_clearAllButton != null)
                m_clearAllButton.onClick.AddListener(OnClearAllButtonClick);
        }

        private void OnDisable()
        {
            if (m_unlockButton != null)
                m_unlockButton.onClick.RemoveListener(OnUnlockButtonClick);

            if (m_clearButton != null)
                m_clearButton.onClick.RemoveListener(OnClearButtonClick);

            if (m_checkButton != null)
                m_checkButton.onClick.RemoveListener(OnCheckButtonClick);

            if (m_getInfoButton != null)
                m_getInfoButton.onClick.RemoveListener(OnGetInfoButtonClick);

            if (m_getIconButton != null)
                m_getIconButton.onClick.RemoveListener(OnGetIconButtonClick);

            if (m_indicateButton != null)
                m_indicateButton.onClick.RemoveListener(OnIndicateButtonClick);

            if (m_addProgressButton != null)
                m_addProgressButton.onClick.RemoveListener(OnAddProgressButtonClick);

            if (m_setProgressButton != null)
                m_setProgressButton.onClick.RemoveListener(OnSetProgressButtonClick);

            if (m_getProgressButton != null)
                m_getProgressButton.onClick.RemoveListener(OnGetProgressButtonClick);

            if (m_clearAllButton != null)
                m_clearAllButton.onClick.RemoveListener(OnClearAllButtonClick);
        }

        private void OnUnlockButtonClick()
        {
            if (m_achievementService == null || m_unlockAchievementIdInputField == null)
                return;

            var achievementId = m_unlockAchievementIdInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.UnlockAchievement(achievementId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Unlock achievement '{achievementId}' success!");
                else
                    Debug.LogError($"Unlock achievement '{achievementId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });

            ServiceContainer.Get<IAchievementService>().UnlockAchievement(achievementId, result =>
            {
                if (result.IsSuccess)
                    Debug.Log("Success!!!");
                else
                    Debug.Log("Error: " + result.ErrorCode);
            });
        }

        private void OnClearButtonClick()
        {
            if (m_achievementService == null || m_clearAchievementIdInputField == null)
                return;

            var achievementId = m_clearAchievementIdInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.ClearAchievement(achievementId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Clear achievement '{achievementId}' success!");
                else
                    Debug.LogError($"Clear achievement '{achievementId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnCheckButtonClick()
        {
            if (m_achievementService == null || m_checkAchievementIdInputField == null)
                return;

            var achievementId = m_checkAchievementIdInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.AchievementUnlocked(achievementId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Unlock status '{achievementId}': {result.IsUnlocked}");
                else
                    Debug.LogError($"Check achievement '{achievementId}' status failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnGetInfoButtonClick()
        {
            if (m_achievementService == null || m_getInfoAchievementIdInputField == null)
                return;

            var achievementId = m_getInfoAchievementIdInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.GetAchievementInfo(achievementId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Achievement '{achievementId}' info:\n\tAchievement ID: {result.AchievementId}" +
                        $"\n\tDisplay name: {result.DisplayName}\n\tDescription: {result.Description}\n\tFlavor text: {result.FlavorText}" +
                        $"\n\tIs hidden: {result.IsHidden}\n\tMax progress: {result.MaxProgress}");
                }
                else
                {
                    Debug.LogError($"Get achievement '{achievementId}' info failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            });
        }

        private void OnGetIconButtonClick()
        {
            if (m_achievementService == null || m_getIconAchievementIdInputField == null)
                return;

            var achievementId = m_getIconAchievementIdInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.GetAchievementIcon(achievementId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    Debug.Log($"Get achievement '{achievementId}' icon success!");
                    var achievementTexture = result.Icon;
                    if (m_getIconImage != null && achievementTexture != null)
                    {
                        var sprite = Sprite.Create(achievementTexture, new Rect(0, 0, achievementTexture.width, achievementTexture.height), new Vector2(0.5f, 0.5f));
                        m_getIconImage.gameObject.SetActive(true);
                        m_getIconImage.sprite = sprite;
                    }
                }
                else
                {
                    Debug.LogError($"Get achievement '{achievementId}' icon failed: {result.ErrorCode}. {result.InnerMessage}");
                    if (m_getIconImage != null)
                        m_getIconImage.gameObject.SetActive(false);
                }
            });
        }

        private void OnIndicateButtonClick()
        {
            if (m_achievementService == null || m_indicateAchievementIdInputField == null || m_indicateAmountInputField == null)
                return;

            var achievementId = m_indicateAchievementIdInputField.text;
            var progressStr = m_indicateAmountInputField.text;

            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("Achievement ID field is empty. Please enter the correct achievement ID");
                return;
            }

            if (string.IsNullOrEmpty(progressStr))
            {
                Debug.LogWarning("Progress field is empty. Please enter the correct progress value");
                return;
            }

            if (!int.TryParse(progressStr, out var progress))
            {
                Debug.LogWarning("Progress value is incorrect. Please enter the correct progress value");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.IndicateAchievementProgress(achievementId, progress, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Indicate achievement '{achievementId}' progress {progress} success!");
                else
                    Debug.LogError($"Indicate achievement '{achievementId}' progress {progress} failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnAddProgressButtonClick()
        {
            if (m_achievementService == null || m_addProgressStatIdInputField == null || m_addProgressAmountInputField == null)
                return;

            var statId = m_addProgressStatIdInputField.text;
            var progressStr = m_addProgressAmountInputField.text;

            if (string.IsNullOrEmpty(statId))
            {
                Debug.LogWarning("Stat ID field is empty. Please enter the correct stat ID");
                return;
            }

            if (string.IsNullOrEmpty(progressStr))
            {
                Debug.LogWarning("Progress field is empty. Please enter the correct progress value");
                return;
            }

            if (!int.TryParse(progressStr, out var progress))
            {
                Debug.LogWarning("Progress value is incorrect. Please enter the correct progress value");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.AddProgress(statId, progress, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Add progress {progress} to stat '{statId}' success!");
                else
                    Debug.LogError($"Add progress {progress} to stat '{statId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnSetProgressButtonClick()
        {
            if (m_achievementService == null || m_setProgressStatIdInputField == null || m_setProgressAmountInputField == null)
                return;

            var statId = m_setProgressStatIdInputField.text;
            var progressStr = m_setProgressAmountInputField.text;

            if (string.IsNullOrEmpty(statId))
            {
                Debug.LogWarning("Stat ID field is empty. Please enter the correct stat ID");
                return;
            }

            if (string.IsNullOrEmpty(progressStr))
            {
                Debug.LogWarning("Progress field is empty. Please enter the correct progress value");
                return;
            }

            if (!int.TryParse(progressStr, out var progress))
            {
                Debug.LogWarning("Progress value is incorrect. Please enter the correct progress value");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.SetProgress(statId, progress, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Set progress {progress} to stat '{statId}' success!");
                else
                    Debug.LogError($"Set progress {progress} to stat '{statId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnGetProgressButtonClick()
        {
            if (m_achievementService == null || m_getProgressStatIdInputField == null)
                return;

            var statId = m_getProgressStatIdInputField.text;

            if (string.IsNullOrEmpty(statId))
            {
                Debug.LogWarning("Stat ID field is empty. Please enter the correct stat ID");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.GetProgress(statId, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Current progress '{statId}': {result.Progress}");
                else
                    Debug.LogError($"Get progress for stat '{statId}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnClearAllButtonClick()
        {
            if (m_achievementService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_achievementService.ClearAllStatsAndAchievements(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Clear all stats and achievements success!");
                else
                    Debug.LogError($"Clear all stats and achievements failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }
    }
}
