using System;
using System.Collections.Generic;
using UPT.Core;
using UPT.Services;

namespace UPT.Steam
{
    [PlatformModule("upt.portingtechnologies.upt.steam", "DISABLESTEAMWORKS", true)]
    public class SteamPlatformModule : PlatformModule
    {
        public override string DisplayName => "Steamworks";
        public override string Version => "1.0.0";

        private readonly List<Type> m_providedServiceTypes = new()
        {
            typeof(IAchievementService),
            typeof(ILeaderboardService),
            typeof(ILobbyService),
        };
        public override IReadOnlyCollection<Type> ProvidedServiceTypes => m_providedServiceTypes;

        protected override void RegisterServiceFactories()
        {
#if !DISABLESTEAMWORKS
            RegisterServiceFactory<IAchievementService>(() => new SteamAchievementService());
#endif
        }

        public override bool IsAvailable()
        {
#if !DISABLESTEAMWORKS
            return true;
#else
            return false;
#endif
        }
    }
}
