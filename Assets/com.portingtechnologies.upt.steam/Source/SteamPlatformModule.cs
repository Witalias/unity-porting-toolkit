using System;
using System.Collections.Generic;
using UPT.Core;
using UPT.Services;

#if !DISABLESTEAMWORKS
using UnityEngine;
#endif

namespace UPT.Steam
{
    [PlatformModule("upt.portingtechnologies.upt.steam", "DISABLESTEAMWORKS", true)]
    public class SteamPlatformModule : PlatformModule
    {
        public override string DisplayName => "Steamworks";
        public override string Version => "1.0.0";

        private readonly List<Type> m_providedServiceTypes = new()
        {
            typeof(IPlatformService),
            typeof(IAchievementService),
            typeof(ILeaderboardService),
            typeof(IRemoteStorageService),
        };
        public override IReadOnlyCollection<Type> ProvidedServiceTypes => m_providedServiceTypes;

        protected override void RegisterServiceFactories()
        {
#if !DISABLESTEAMWORKS
            RegisterServiceFactory<IPlatformService>(() => new SteamPlatformService());
            RegisterServiceFactory<IAchievementService>(() => new SteamAchievementService());
            RegisterServiceFactory<ILeaderboardService>(() => new SteamLeaderboardService());
            RegisterServiceFactory<IRemoteStorageService>(() => new SteamRemoteStorageService());
#endif
        }

        public override bool Initialize()
        {
#if !DISABLESTEAMWORKS
            var steamManager = new GameObject("[Steam Manager]");
            steamManager.AddComponent<SteamManager>();
#endif
            return base.Initialize();
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
