using System;
using System.Collections.Generic;
using UPT.Core;
using UPT.Services;

#if !EOS_DISABLE
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endif

namespace UPT.EOS
{
    [PlatformModule("com.portingtechnologies.upt.eos", "EOS_DISABLE", true)]
    public class EOSPlatformModule : PlatformModule
    {
        public override string DisplayName => "Epic Online Services";
        public override string Version => "1.0.0";

        public const string DEV_CREDENTIALS_PATH = "Assets/Editor/EOS/DevCredentials.txt";

        private readonly Type[] m_providedServiceTypes = new[]
        {
            typeof(IPlatformService),
            typeof(IEOSAuthenticationService),
            typeof(IAchievementService),
            typeof(ILeaderboardService),
            typeof(IRemoteStorageService),
        };
        public override IReadOnlyCollection<Type> ProvidedServiceTypes => m_providedServiceTypes;

#if !EOS_DISABLE
        private EOSServiceContext m_serviceContext;
        private EOSPlatformService m_platformService;
#endif

        public override bool Initialize()
        {
#if !EOS_DISABLE

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            var platformInterface = EOSManager.Instance.GetEOSPlatformInterface();
            if (platformInterface.CheckForLauncherAndRestart() == Result.Success)
            {
                Application.Quit();
                return false;
            }
#endif

            var epicLauncherArgs = EOSManager.EOSSingleton.GetCommandLineArgsFromEpicLauncher();
            m_serviceContext = new EOSServiceContext(epicLauncherArgs);

#if UNITY_EDITOR
            if (AssetDatabase.AssetPathExists(DEV_CREDENTIALS_PATH))
            {
                var devCredentials = AssetDatabase.LoadAssetAtPath<TextAsset>(DEV_CREDENTIALS_PATH).text.Split(',');
                if (devCredentials.Length == 2)
                {
                    m_serviceContext.DeveloperHost = devCredentials[0];
                    m_serviceContext.DeveloperName = devCredentials[1];
                }
                else
                {
                    UptLogger.Warning($"Developer credentials file {DEV_CREDENTIALS_PATH} is invalid. " +
                        $"Try to re-create the credentials by following Tools > Porting Toolkit > EOS > Create developer credentials", this);
                }
            }
#endif

#endif

            return base.Initialize();
        }

        public override void PostInitialize()
        {
#if !EOS_DISABLE
            var eosManager = new GameObject("[EOS Manager]");
            eosManager.AddComponent<EOSManager>();
            m_platformService.SubscribeDisplaySettingsUpdate();
#endif
            base.PostInitialize();
        }

        protected override void RegisterServiceFactories()
        {
#if !EOS_DISABLE
            m_platformService = new EOSPlatformService(m_serviceContext);

            RegisterServiceFactory<IPlatformService>(() => m_platformService);
            RegisterServiceFactory<IEOSAuthenticationService>(() => new EOSAuthenticationService(m_serviceContext));
            RegisterServiceFactory<IAchievementService>(() => new EOSAchievementService(m_serviceContext));
            RegisterServiceFactory<ILeaderboardService>(() => new EOSLeaderboardService(m_serviceContext));
            RegisterServiceFactory<IRemoteStorageService>(() => new EOSRemoteStorageService(m_serviceContext));
#endif
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
