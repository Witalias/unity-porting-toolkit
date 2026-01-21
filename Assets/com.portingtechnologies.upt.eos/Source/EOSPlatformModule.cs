using System;
using System.Collections.Generic;
using UPT.Core;
using UPT.Services;

namespace UPT.EOS
{
    [PlatformModule("com.portingtechnologies.upt.eos", "EOS_DISABLE", true)]
    public class EOSPlatformModule : PlatformModule
    {
        public override string DisplayName => "Epic Online Services";
        public override string Version => "1.0.0";

        private readonly Type[] m_providedServiceTypes = new[]
        {
            typeof(IAchievementService),
            typeof(ILeaderboardService),
            typeof(ILobbyService),
        };
        public override IReadOnlyCollection<Type> ProvidedServiceTypes => m_providedServiceTypes;

        protected override void RegisterServiceFactories()
        {
            
        }

        public override bool IsAvailable()
        {
#if !EOS_DISABLE
            return true;
#else
            return false;
#endif
        }
    }
}
