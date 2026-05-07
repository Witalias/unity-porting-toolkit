#if !EOS_DISABLE

using Epic.OnlineServices.UI;
using PlayEveryWare.EpicOnlineServices;
using UPT.Core;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSPlatformService : IPlatformService
    {
        private readonly EOSServiceContext m_context;

        private ulong m_displaySettingsUpdateNID;
        private bool m_isExclusiveInput;

        public event OverlayStateChanged OnOverlayStateChanged;

        private UIInterface UIInterface => EOSManager.Instance.GetEOSUIInterface();

        public EOSPlatformService(EOSServiceContext context)
        {
            m_context = context;
        }

        public string GetLocaleCode()
        {
            return MapLocale(m_context.EpicLauncherArgs.epicLocale);

        }

        public string GetUsername()
        {
            return m_context.EpicLauncherArgs.epicUsername;
        }

        public string GetUserID()
        {
            return m_context.EpicLauncherArgs.epicUserID;
        }

        public bool IsOverlayVisible()
        {
            var options = new GetFriendsExclusiveInputOptions()
            {
                LocalUserId = m_context.EpicAccountId
            };
            return UIInterface.GetFriendsExclusiveInput(ref options);
        }

        public void SubscribeDisplaySettingsUpdate()
        {
            var options = new AddNotifyDisplaySettingsUpdatedOptions();
            m_displaySettingsUpdateNID = UIInterface.AddNotifyDisplaySettingsUpdated(ref options, null, OnDisplaySettingsUpdated);
        }

        private void OnDisplaySettingsUpdated(ref OnDisplaySettingsUpdatedCallbackInfo data)
        {
            if (data.IsExclusiveInput != m_isExclusiveInput)
            {
                m_isExclusiveInput = data.IsExclusiveInput;
                OnOverlayStateChanged?.Invoke(data.IsExclusiveInput);
            }
        }

        private string MapLocale(string epicLocale)
        {
            return epicLocale switch
            {
                "ar" => Constants.Locale.Arabic,
                "bg" => Constants.Locale.Bulgarian,
                "cs" => Constants.Locale.Czech,
                "da" => Constants.Locale.Danish,
                "nl" => Constants.Locale.Dutch,
                "en" => Constants.Locale.English,
                "fil" => Constants.Locale.Filipino,
                "fi" => Constants.Locale.Finnish,
                "fr" => Constants.Locale.French,
                "de" => Constants.Locale.German,
                "hi" => Constants.Locale.Hindi,
                "hu" => Constants.Locale.Hungarian,
                "id" => Constants.Locale.Indonesian,
                "it" => Constants.Locale.Italian,
                "ja" => Constants.Locale.Japanese,
                "ko" => Constants.Locale.Korean,
                "ms" => Constants.Locale.Malay,
                "no" => Constants.Locale.Norwegian,
                "pl" => Constants.Locale.Polish,
                "pt-BR" => Constants.Locale.BrazilianPortuguese,
                "pt" => Constants.Locale.Portuguese,
                "ro" => Constants.Locale.Romanian,
                "ru" => Constants.Locale.Russian,
                "es-ES" => Constants.Locale.Spanish,
                "es" => Constants.Locale.LatinSpanish,
                "sv" => Constants.Locale.Swedish,
                "th" => Constants.Locale.Thai,
                "tr" => Constants.Locale.Turkish,
                "uk" => Constants.Locale.Ukrainian,
                "vi" => Constants.Locale.Vietnamese,
                "zh-CN" => Constants.Locale.SimplifedChinise,
                "zh-Hant" => Constants.Locale.TraditionalChinise,
                _ => Constants.Locale.English
            };
        }
    }
}

#endif