namespace UPT.Services
{
    public interface IEOSAuthenticationService
    {
        /// <summary>
        /// Log in the user using a short-lived exchange code transmitted to the application from Epic Games Launcher.
        /// This is usually the standard way to log in an Epic Games user.
        /// </summary>
        void Auth_LoginExchangeCode(EOSLoginCallback callback);

        /// <summary>
        /// <list>Log in the user in developer mode.
        /// Common cases are the use in the editor or development builds to provide automatic login.</list>
        /// <list>To work in the editor, you will need to create a file with developer credentials by going to <c>Tools > Porting Toolkit > Epic Online Services > Create Developer Credentials</c>.</list>
        /// <list>To work outside the editor, you will need to run the executable file with the command line arguments: <code>-AUTH_LOGIN=your_host</code><code>-AUTH_PASSWORD=your_username</code></list>
        /// <list>Please refer to the EOS Dev Auth Tool on the corresponding EOS documentation page for more information.</list>
        /// </summary>
        void Auth_LoginDeveloper(EOSLoginCallback callback);

        /// <summary>
        /// Use it for a user login outside of Epic Games Launcher.
        /// At the first login, the user will be shown an overlay for manually entering credentials.
        /// The SDK caches the data on the device, and as long as it is valid, any subsequent login attempts will be automatic.
        /// </summary>
        void Auth_LoginWithAccountPortal(EOSLoginCallback callback);

        /// <summary>
        /// Log out the user from Epic Account Services.
        /// </summary>
        void Auth_Logout(EOSLogoutCallback callback);

        /// <summary>
        /// Check if the user is logged into Epic Account Services.
        /// </summary>
        bool Auth_IsLoggedIn();

        /// <summary>
        /// Revoke the player's long-lived refresh token on the authorization server. This also deletes the long-lived refresh token from the keychain of the local player.
        /// If the player logged in using <see cref="Auth_LoginWithAccountPortal"/> and then wished to opt out of the automatic login, you should call <see cref="Auth_Logout"/> and <see cref="Auth_DeletePersistentAuth"/>.
        /// </summary>
        void Auth_DeletePersistentAuth(EOSAuthenticationGeneralCallback callback);

        /// <summary>
        /// <list>Log in an Epic Games user to EOS Game Services.
        /// Before doing this, you must log the user into Epic Account Services by calling <see cref="Auth_LoginExchangeCode(EOSLoginCallback)"/>, <see cref="Auth_LoginWithAccountPortal(EOSLoginCallback)"/> or <see cref="Auth_LoginDeveloper(EOSLoginCallback)"/>.</list>
        /// <list>The callback may return an InvalidAuth error, which indicates that the user does not exist.
        /// A common practice in this case is to call <see cref="Connect_CreateUser(EOSLoginCallback)"/> to create a new user.</list>
        /// </summary>
        void Connect_LoginEpicUser(EOSLoginCallback callback);

        /// <summary>
        /// <list>Log in the user to EOS Game Services using a Steam Session Ticket received from his Steam account. Please use the Steamworks documentation for detailed information about the Steam Session Ticket.</list>
        /// <list>The callback may return an InvalidAuth error, which indicates that the user does not exist.
        /// A common practice in this case is to call <see cref="Connect_CreateUser(EOSLoginCallback)"/> to create a new user.</list>
        /// </summary>
        void Connect_LoginSteamUser(string sessionTicket, EOSLoginCallback callback);

        /// <summary>
        /// Log out the user from EOS Game Services.
        /// </summary>
        void Connect_Logout(EOSLogoutCallback callback);

        /// <summary>
        /// Create a new user in the EOS Game Services ecosystem.
        /// You usually use this when the login function in EOS Game Services fails with the InvalidAuth error returned.
        /// </summary>
        /// <param name="callback"></param>
        void Connect_CreateUser(EOSLoginCallback callback);

        /// <summary>
        /// Check if the user is logged into EOS Game Services.
        /// </summary>
        bool Connect_IsLoggedIn();
    }

    public class UptEOSLoginResult : UptEOSResult
    {
        /// <summary>
        /// The ID of the logged-in user.
        /// </summary>
        public string LocalUserId { get; }

        public UptEOSLoginResult(ErrorCode error, string innerMessage = null, string userId = null) : base(error, innerMessage)
        {
            LocalUserId = userId;
        }
    }

    public class UptEOSLogoutResult : UptEOSResult
    {
        /// <summary>
        /// The ID of the user who was logged out.
        /// </summary>
        public string LocalUserId { get; }

        public UptEOSLogoutResult(ErrorCode error, string innerMessage = null, string userId = null) : base(error, innerMessage)
        {
            LocalUserId = userId;
        }
    }

    public delegate void EOSAuthenticationGeneralCallback(UptEOSResult result);
    public delegate void EOSLoginCallback(UptEOSLoginResult result);
    public delegate void EOSLogoutCallback(UptEOSLogoutResult result);
}
