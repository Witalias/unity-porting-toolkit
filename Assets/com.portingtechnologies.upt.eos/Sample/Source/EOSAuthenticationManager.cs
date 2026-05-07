using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UPT.Core;
using UPT.Core.Samples;

namespace UPT.EOS.Samples
{
    public class EOSAuthenticationManager : MonoBehaviour
    {
        [Header("Auth Login Developer")]
        [SerializeField] private Button m_authLoginDeveloperButton;

        [Header("Auth Login Epic User")]
        [SerializeField] private Button m_authLoginExchangeCodeButton;

        [Header("Auth Login Account Portal")]
        [SerializeField] private Button m_authLoginAccountPortalButton;

        [Header("Auth Logout")]
        [SerializeField] private Button m_authLogoutButton;

        [Header("Is Auth Logged In")]
        [SerializeField] private Button m_isAuthLoggedInButton;

        [Header("Auth Delete Persistent Auth")]
        [SerializeField] private Button m_authDeletePersistentAuth;

        [Header("Connect Login Epic User")]
        [SerializeField] private Button m_connectLoginEpicUserButton;

        [Header("Connect Login Steam User")]
        [SerializeField] private TMP_InputField m_connectLoginSteamUserInputField;
        [SerializeField] private Button m_connectLoginSteamUserButton;

        [Header("Connect Create User")]
        [SerializeField] private Button m_connectCreateUserButton;

        [Header("Connect Logout")]
        [SerializeField] private Button m_connectLogoutButton;

        [Header("Is Connect Logged In")]
        [SerializeField] private Button m_isConnectLoggedInButton;

        private Services.IEOSAuthenticationService m_authenticationService;

        private void Awake()
        {
            m_authenticationService = ServiceContainer.Get<Services.IEOSAuthenticationService>();
        }

        private void OnEnable()
        {
            if (m_authenticationService is IMockService)
                Debug.LogWarning("EOS authentication service is mock! All functions will return a successfull result without any metadata");

            if (m_authLoginDeveloperButton)
                m_authLoginDeveloperButton.onClick.AddListener(OnAuthLoginDeveloperButton);

            if (m_authLoginExchangeCodeButton)
                m_authLoginExchangeCodeButton.onClick.AddListener(OnAuthLoginExchangeCodeButton);

            if (m_authLoginAccountPortalButton)
                m_authLoginAccountPortalButton.onClick.AddListener(OnAuthLoginAccountPortalButton);

            if (m_authLogoutButton)
                m_authLogoutButton.onClick.AddListener(OnAuthLogoutButton);

            if (m_isAuthLoggedInButton)
                m_isAuthLoggedInButton.onClick.AddListener(OnIsAuthLoggedInButton);

            if (m_connectLoginEpicUserButton)
                m_connectLoginEpicUserButton.onClick.AddListener(OnConnectLoginEpicUserButton);

            if (m_connectLoginSteamUserButton)
                m_connectLoginSteamUserButton.onClick.AddListener(OnConnectLoginSteamUserButton);

            if (m_connectCreateUserButton)
                m_connectCreateUserButton.onClick.AddListener(OnConnectCreateUserButton);

            if (m_connectLogoutButton)
                m_connectLogoutButton.onClick.AddListener(OnConnectLogoutButton);

            if (m_isConnectLoggedInButton)
                m_isConnectLoggedInButton.onClick.AddListener(OnIsConnectLoggedInButton);

            if (m_authDeletePersistentAuth)
                m_authDeletePersistentAuth.onClick.AddListener(OnAuthDeletePersistentAuthButton);
        }

        private void OnDisable()
        {
            if (m_authLoginDeveloperButton)
                m_authLoginDeveloperButton.onClick.RemoveListener(OnAuthLoginDeveloperButton);

            if (m_authLoginExchangeCodeButton)
                m_authLoginExchangeCodeButton.onClick.RemoveListener(OnAuthLoginExchangeCodeButton);

            if (m_authLoginAccountPortalButton)
                m_authLoginAccountPortalButton.onClick.RemoveListener(OnAuthLoginAccountPortalButton);

            if (m_authLogoutButton)
                m_authLogoutButton.onClick.RemoveListener(OnAuthLogoutButton);

            if (m_isAuthLoggedInButton)
                m_isAuthLoggedInButton.onClick.RemoveListener(OnIsAuthLoggedInButton);

            if (m_connectLoginEpicUserButton)
                m_connectLoginEpicUserButton.onClick.RemoveListener(OnConnectLoginEpicUserButton);

            if (m_connectLoginSteamUserButton)
                m_connectLoginSteamUserButton.onClick.RemoveListener(OnConnectLoginSteamUserButton);

            if (m_connectCreateUserButton)
                m_connectCreateUserButton.onClick.RemoveListener(OnConnectCreateUserButton);

            if (m_connectLogoutButton)
                m_connectLogoutButton.onClick.RemoveListener(OnConnectLogoutButton);

            if (m_isConnectLoggedInButton)
                m_isConnectLoggedInButton.onClick.RemoveListener(OnIsConnectLoggedInButton);

            if (m_authDeletePersistentAuth)
                m_authDeletePersistentAuth.onClick.RemoveListener(OnAuthDeletePersistentAuthButton);
        }

        private void OnAuthLoginDeveloperButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Auth_LoginDeveloper(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Auth login developer success! Epic Account ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Auth login developer failed");
            });
        }

        private void OnAuthLoginExchangeCodeButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Auth_LoginExchangeCode(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Auth login with exchange code success! Epic Account ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Auth login with exchange code failed");
            });
        }

        private void OnAuthLoginAccountPortalButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Auth_LoginWithAccountPortal(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Auth login with account portal success! Epic Account ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Auth login with account portal failed");
            });
        }

        private void OnAuthLogoutButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Auth_Logout(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Auth logout success! Epic Account ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Auth logout failed");
            });
        }

        private void OnIsAuthLoggedInButton()
        {
            if (m_authenticationService == null)
                return;

            Debug.Log($"Is auth logged in: {m_authenticationService.Auth_IsLoggedIn()}");
        }

        private void OnAuthDeletePersistentAuthButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Auth_DeletePersistentAuth(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Delete persistent auth success!");
                else
                    LogErrorMessage(result, "Delete persistent auth failed");
            });
        }

        private void OnConnectLoginEpicUserButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Connect_LoginEpicUser(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Connect login epic user success! Product User ID: {result.LocalUserId}");
                else
                    LogEOSConnectLoginErrorMessage(result, "Connect login epic user failed");
            });
        }

        private void OnConnectLoginSteamUserButton()
        {
            if (m_authenticationService == null || m_connectLoginSteamUserInputField == null)
                return;

            var steamSessionTicket = m_connectLoginSteamUserInputField.text;
            if (string.IsNullOrEmpty(steamSessionTicket))
            {
                Debug.LogWarning("Steam session ticket field is empty. Please enter the correct Steam session ticket");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Connect_LoginSteamUser(steamSessionTicket, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Connect login steam user success! Product User ID: {result.LocalUserId}");
                else
                    LogEOSConnectLoginErrorMessage(result, "Connect login steam user failed");
            });
        }

        private void OnConnectCreateUserButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Connect_CreateUser(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Connect create user success! You are currently logged into EOS Game Services. Product User ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Connect create user user failed");
            });
        }

        private void OnConnectLogoutButton()
        {
            if (m_authenticationService == null)
                return;

            GlobalManager.Instance.SetSpinnerActive(true);
            m_authenticationService.Connect_Logout(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Connect logout success! Product User ID: {result.LocalUserId}");
                else
                    LogErrorMessage(result, "Connect logout user failed");
            });
        }

        private void OnIsConnectLoggedInButton()
        {
            if (m_authenticationService == null)
                return;

            Debug.Log($"Is connect logged in: {m_authenticationService.Connect_IsLoggedIn()}");
        }

        private void LogErrorMessage(Services.UptResult result, string message)
        {
            var log = $"{message}: {result.ErrorCode}. {result.InnerMessage}";
            Debug.LogError(log);
        }

        private void LogEOSConnectLoginErrorMessage(Services.UptResult result, string message)
        {
#if !EOS_DISABLE
            if (result is Services.UptEOSLoginResult eosLoginResult && eosLoginResult.EOSErrorCode == Epic.OnlineServices.Result.InvalidUser)
                Debug.Log($"{message}. EOS error: {eosLoginResult.EOSErrorCode}. Now you may want to create a connect user using the Connect Create User option.");
            else
#endif
            LogErrorMessage(result, message);
        }
    }
}
