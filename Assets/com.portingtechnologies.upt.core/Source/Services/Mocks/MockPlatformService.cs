using UPT.Services;

namespace UPT.Core
{
    [MockService(typeof(IPlatformService))]
    public class MockPlatformService : IPlatformService, IMockService
    {
        public string OriginalServiceName => nameof(IPlatformService);

        public event OverlayStateChanged OnOverlayStateChanged;

        public string GetLocaleCode()
        {
            return null;
        }

        public string GetUserID()
        {
            return null;
        }

        public string GetUsername()
        {
            return null;
        }

        public bool IsOverlayVisible()
        {
            return false;
        }
    }
}
