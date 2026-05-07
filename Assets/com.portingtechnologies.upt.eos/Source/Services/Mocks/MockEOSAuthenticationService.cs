#if !EOS_DISABLE

using UPT.Core;

namespace UPT.Services.Mocks
{
    public class MockEOSAuthenticationService : IEOSAuthenticationService, IMockService
    {
        public string OriginalServiceName => nameof(IEOSAuthenticationService);

        public void Auth_DeletePersistentAuth(EOSAuthenticationGeneralCallback callback)
        {
            callback?.Invoke(new UptEOSResult(ErrorCode.Success));
        }

        public bool Auth_IsLoggedIn()
        {
            return false;
        }

        public void Auth_LoginDeveloper(EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public void Auth_LoginExchangeCode(EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public void Auth_LoginWithAccountPortal(EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public void Auth_Logout(EOSLogoutCallback callback)
        {
            callback?.Invoke(new UptEOSLogoutResult(ErrorCode.Success));
        }

        public void Connect_CreateUser(EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public bool Connect_IsLoggedIn()
        {
            return false;
        }

        public void Connect_LoginEpicUser(EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public void Connect_LoginSteamUser(string sessionTicket, EOSLoginCallback callback)
        {
            callback?.Invoke(new UptEOSLoginResult(ErrorCode.Success));
        }

        public void Connect_Logout(EOSLogoutCallback callback)
        {
            callback?.Invoke(new UptEOSLogoutResult(ErrorCode.Success));
        }
    }
}

#endif