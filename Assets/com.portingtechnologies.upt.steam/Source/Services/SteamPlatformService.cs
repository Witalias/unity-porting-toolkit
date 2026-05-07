#if !DISABLESTEAMWORKS

using Steamworks;
using UPT.Services;

namespace UPT.Steam
{
    public class SteamPlatformService : IPlatformService
    {
        public event OverlayStateChanged OnOverlayStateChanged;

        private readonly Callback<GameOverlayActivated_t> m_GameOverlayActivated;

        public SteamPlatformService()
        {
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }

        public string GetLocaleCode()
        {
            return MapLocale(SteamApps.GetCurrentGameLanguage());
        }

        public string GetUserID()
        {
            return SteamUser.GetSteamID().ToString();
        }

        public string GetUsername()
        {
            return SteamFriends.GetPersonaName();
        }

        public bool IsOverlayVisible()
        {
            return SteamUtils.IsOverlayEnabled();
        }

        private void OnGameOverlayActivated(GameOverlayActivated_t data)
        {
            OnOverlayStateChanged?.Invoke(data.m_bActive != 0);
        }

        private string MapLocale(string steamLocale)
        {
            return steamLocale switch
            {
                "arabic" => Core.Constants.Locale.Arabic,
                "bulgarian" => Core.Constants.Locale.Bulgarian,
                "schinese" => Core.Constants.Locale.SimplifedChinise,
                "tchinese" => Core.Constants.Locale.TraditionalChinise,
                "czech" => Core.Constants.Locale.Czech,
                "danish" => Core.Constants.Locale.Danish,
                "dutch" => Core.Constants.Locale.Dutch,
                "english" => Core.Constants.Locale.English,
                "finnish" => Core.Constants.Locale.Finnish,
                "french" => Core.Constants.Locale.French,
                "german" => Core.Constants.Locale.German,
                "greek" => Core.Constants.Locale.Greek,
                "hungarian" => Core.Constants.Locale.Hungarian,
                "indonesian" => Core.Constants.Locale.Indonesian,
                "italian" => Core.Constants.Locale.Italian,
                "japanese" => Core.Constants.Locale.Japanese,
                "koreana" => Core.Constants.Locale.Korean,
                "norwegian" => Core.Constants.Locale.Norwegian,
                "polish" => Core.Constants.Locale.Polish,
                "portuguese" => Core.Constants.Locale.Portuguese,
                "brazilian" => Core.Constants.Locale.BrazilianPortuguese,
                "romanian" => Core.Constants.Locale.Romanian,
                "russian" => Core.Constants.Locale.Russian,
                "spanish" => Core.Constants.Locale.Spanish,
                "latam" => Core.Constants.Locale.LatinSpanish,
                "swedish" => Core.Constants.Locale.Swedish,
                "thai" => Core.Constants.Locale.Thai,
                "turkish" => Core.Constants.Locale.Turkish,
                "ukrainian" => Core.Constants.Locale.Ukrainian,
                "vietnamese" => Core.Constants.Locale.Vietnamese,
                _ => Core.Constants.Locale.English,
            };
        }
    }
}

#endif