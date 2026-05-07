#if !EOS_DISABLE

using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UPT.Core;
using UPT.EOS;

namespace UPT.Services
{
    public class EOSAuthenticationService : IEOSAuthenticationService, IUpdatableService
    {
        private const int MAX_CONNECT_LOGIN_ATTEMPTS = 3;
        private const float CONNECT_RELOGIN_DELAY = 3.0f;

        private readonly EOSServiceContext m_context;
        private ContinuanceToken m_lastContinuanceToken;
        private DelayedAction m_connectLoginAction;
        private int m_connectLoginAttempt;
        private ulong m_notifyAuthExpirationNID;

        private AuthInterface AuthInterface => EOSManager.Instance.GetEOSAuthInterface();
        private ConnectInterface ConnectInterface => EOSManager.Instance.GetEOSConnectInterface();

        public EOSAuthenticationService(EOSServiceContext context)
        {
            m_context = context;
        }

        public void Auth_LoginExchangeCode(EOSLoginCallback callback)
        {
            if (Auth_IsLoggedIn())
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AlreadyLoggedIn));
                return;
            }
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            Auth_LoginInternal(LoginCredentialType.ExchangeCode, null, m_context.EpicLauncherArgs.authPassword, callback);
#else
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.UnsupportedFeature, $"To log in using Exchange Code, you need to launch the build from Epic Games Launcher. To log in to the editor, use {nameof(Auth_LoginDeveloper)} and Dev Auth Tool"));
#endif
        }

        public void Auth_LoginDeveloper(EOSLoginCallback callback)
        {
            if (Auth_IsLoggedIn())
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AlreadyLoggedIn));
                return;
            }
#if UNITY_EDITOR
            if (m_context.DeveloperHost == null || m_context.DeveloperHost == null)
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AnotherActionRequired, $"Can't read developer credentials. " +
                    $"Probably you need to create the credentials by following Tools > Porting Toolkit > EOS > Create developer credentials"));
                return;
            }
            Auth_LoginInternal(LoginCredentialType.Developer, m_context.DeveloperHost, m_context.DeveloperName, callback);
#elif DEVELOPMENT_BUILD
            Auth_LoginInternal(LoginCredentialType.Developer, m_context.EpicLauncherArgs.authLogin, m_context.EpicLauncherArgs.authPassword, callback);
#else
            callback?.Invoke(new UptEOSAuthLoginResult(ErrorCode.UnsupportedFeature, "You can't log in as a developer in the release build"));
#endif
        }

        public void Auth_LoginWithAccountPortal(EOSLoginCallback callback)
        {
            if (Auth_IsLoggedIn())
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AlreadyLoggedIn));
                return;
            }

            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.PersistentAuth, null, null, data =>
            {
                if (data.ResultCode == Result.Success)
                    OnAuthSuccess(data.LocalUserId);
                else
                    Auth_DeletePersistentAuthInternal(DeletePersistentAuthCallback);

                void DeletePersistentAuthCallback(ref DeletePersistentAuthCallbackInfo data)
                {
                    EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.AccountPortal, null, null, data =>
                    {
                        if (data.ResultCode == Result.Success)
                            OnAuthSuccess(data.LocalUserId);
                        else
                            callback?.Invoke(new UptEOSLoginResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                    });
                }
            });

            void OnAuthSuccess(EpicAccountId eaid)
            {
                m_context.EpicAccountId = eaid;
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success, null, eaid.ToString()));
            }
        }

        public void Auth_Logout(EOSLogoutCallback callback)
        {
            var options = new Epic.OnlineServices.Auth.LogoutOptions
            {
                LocalUserId = m_context.EpicAccountId
            };

            AuthInterface.Logout(ref options, null, Callback);

            void Callback(ref Epic.OnlineServices.Auth.LogoutCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_context.EpicAccountId = null;
                    callback?.Invoke(new UptEOSLogoutResult(ErrorCode.Success, null, data.LocalUserId.ToString()));
                }
                else
                {
                    callback?.Invoke(new UptEOSLogoutResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                }
            }
        }

        public bool Auth_IsLoggedIn()
        {
            return m_context.EpicAccountId != null && AuthInterface.GetLoginStatus(m_context.EpicAccountId) == LoginStatus.LoggedIn;
        }

        public void Auth_DeletePersistentAuth(EOSAuthenticationGeneralCallback callback)
        {
            Auth_DeletePersistentAuthInternal(Callback);

            void Callback(ref DeletePersistentAuthCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                    callback?.Invoke(new UptEOSResult(ErrorCode.Success));
                else
                    callback?.Invoke(new UptEOSResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
            }
        }

        public void Connect_CreateUser(EOSLoginCallback callback)
        {
            if (m_lastContinuanceToken == null)
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AnotherActionRequired, $"Continuance token is null! You should call this function after another failed request providing a continuation token, for example, {nameof(Connect_LoginEpicUser)}") { EOSErrorCode = Result.UnexpectedError});
                return;
            }

            EOSManager.Instance.CreateConnectUserWithContinuanceToken(m_lastContinuanceToken, data =>
            {
                if (data.ResultCode == Result.Success)
                {
                    m_context.ProductUserId = data.LocalUserId;
                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success, null, data.LocalUserId.ToString()));
                }
                else
                {
                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                }
            });
        }

        public void Connect_LoginEpicUser(EOSLoginCallback callback)
        {
            if (Connect_IsLoggedIn())
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AlreadyLoggedIn));
                return;
            }
            m_connectLoginAction = new(CONNECT_RELOGIN_DELAY, () =>
            {
                EOSManager.Instance.StartConnectLoginWithEpicAccount(m_context.EpicAccountId, data =>
                {
                    Connect_LoginCallback(data, callback);
                });
            });
            m_connectLoginAction.Run(true);
        }

        public void Connect_LoginSteamUser(string sessionTicket, EOSLoginCallback callback)
        {
            if (Connect_IsLoggedIn())
            {
                callback?.Invoke(new UptEOSLoginResult(ErrorCode.AlreadyLoggedIn));
                return;
            }
            m_connectLoginAction = new(CONNECT_RELOGIN_DELAY, () => Connect_LoginInternal(ExternalCredentialType.SteamSessionTicket, sessionTicket, callback));
            m_connectLoginAction.Run(true);
        }

        public void Connect_Logout(EOSLogoutCallback callback)
        {
            var options = new Epic.OnlineServices.Connect.LogoutOptions
            {
                LocalUserId = m_context.ProductUserId
            };

            void Callback(ref Epic.OnlineServices.Connect.LogoutCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_context.ProductUserId = null;
                    m_connectLoginAction = null;
                    callback?.Invoke(new UptEOSLogoutResult(ErrorCode.Success, null, data.LocalUserId.ToString()));
                }
                else
                {
                    callback?.Invoke(new UptEOSLogoutResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                }
            }

            ConnectInterface.Logout(ref options, null, Callback);
        }

        public bool Connect_IsLoggedIn()
        {
            return m_context.ProductUserId != null && ConnectInterface.GetLoginStatus(m_context.ProductUserId) == LoginStatus.LoggedIn;
        }

        public void Update()
        {
            m_connectLoginAction?.Update();
        }

        private void Auth_LoginInternal(LoginCredentialType loginType, string id, string token, EOSLoginCallback callback)
        {
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(loginType, id, token, data =>
            {
                if (data.ResultCode == Result.Success)
                {
                    m_context.EpicAccountId = data.LocalUserId;
                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success, null, data.LocalUserId.ToString()));
                }
                else if (loginType == LoginCredentialType.PersistentAuth)
                {
                    Auth_LoginInternal(LoginCredentialType.AccountPortal, null, null, callback);
                }
                else
                {
                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                }
            });
        }

        private void Auth_DeletePersistentAuthInternal(OnDeletePersistentAuthCallback callback)
        {
            var deletePersistentAuthOptions = new DeletePersistentAuthOptions();
            AuthInterface.DeletePersistentAuth(ref deletePersistentAuthOptions, null, callback);
        }

        private void Connect_LoginInternal(ExternalCredentialType credentialType, string token, EOSLoginCallback callback)
        {
            var credentials = new Epic.OnlineServices.Connect.Credentials
            {
                Token = token,
                Type = credentialType
            };

            var options = new Epic.OnlineServices.Connect.LoginOptions
            {
                Credentials = credentials
            };

            ConnectInterface.Login(ref options, null, Callback);

            void Callback(ref Epic.OnlineServices.Connect.LoginCallbackInfo data)
            {
                Connect_LoginCallback(data, callback);
            }
        }

        private void Connect_LoginCallback(Epic.OnlineServices.Connect.LoginCallbackInfo data, EOSLoginCallback callback)
        {
            if (data.ResultCode == Result.InvalidAuth && ++m_connectLoginAttempt < MAX_CONNECT_LOGIN_ATTEMPTS)
            {
                // Sometimes it's enough to wait for the SDK to update the ID token of an authenticated epic user, and then try again.
                m_connectLoginAction?.Run();
            }
            else
            {
                m_connectLoginAttempt = 0;

                if (data.ResultCode == Result.Success)
                {
                    m_context.ProductUserId = data.LocalUserId;

                    if (m_notifyAuthExpirationNID != 0)
                        Connect_UnsubscribeNotifyAuthExpiration(m_notifyAuthExpirationNID);

                    m_notifyAuthExpirationNID = Connect_SubscribeNotifyAuthExpiration(Connect_OnAuthExpire);

                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success, null, data.LocalUserId.ToString()));
                }
                else
                {
                    if (data.ResultCode == Result.InvalidUser)
                        m_lastContinuanceToken = data.ContinuanceToken;

                    callback?.Invoke(new UptEOSLoginResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)) { EOSErrorCode = data.ResultCode });
                }
            }
        }

        private void Connect_OnAuthExpire(ref AuthExpirationCallbackInfo data)
        {
            m_connectLoginAction?.Run(true);
        }

        private ulong Connect_SubscribeNotifyAuthExpiration(OnAuthExpirationCallback callback)
        {
            var options = new AddNotifyAuthExpirationOptions();
            return ConnectInterface.AddNotifyAuthExpiration(ref options, null, callback);
        }

        private void Connect_UnsubscribeNotifyAuthExpiration(ulong notificationId)
        {
            ConnectInterface.RemoveNotifyAuthExpiration(notificationId);
        }
    }
}

#endif