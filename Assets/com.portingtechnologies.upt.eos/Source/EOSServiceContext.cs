#if !EOS_DISABLE

using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;

namespace UPT.EOS
{
    public sealed class EOSServiceContext
    {
        public const string BACKEND_ERROR_MESSAGE = "The EOS backend returned an unsuccessful result: ";
        public const int READ_WRITE_CHUNK_LENGTH = 8 * 1024; // 8 KB per frame

        public EOSManager.EOSSingleton.EpicLauncherArgs EpicLauncherArgs { get; }
        public EpicAccountId EpicAccountId { get; set; }
        public ProductUserId ProductUserId { get; set; }

#if UNITY_EDITOR
        public string DeveloperHost { get; set; }
        public string DeveloperName { get; set; }
#endif

        public EOSServiceContext(EOSManager.EOSSingleton.EpicLauncherArgs epicLauncherArgs)
        {
            EpicLauncherArgs = epicLauncherArgs;
        }

        public static string GetBackendErrorMsg(Result result) => BACKEND_ERROR_MESSAGE + result.ToString();
    }
}

#endif