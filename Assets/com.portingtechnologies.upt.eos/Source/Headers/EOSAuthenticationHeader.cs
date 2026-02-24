using UnityEngine;

namespace UPT.EOS
{
    public interface IEOSAuthenticationService
    {
        void Auth_Login();
        void Auth_Logout();
        void Connect_Login();
        void Connect_Logout();
        void Connect_CreateUser();
    }
}
