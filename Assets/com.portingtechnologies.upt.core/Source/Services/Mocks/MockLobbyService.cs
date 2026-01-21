using UPT.Core;

namespace UPT.Services.Mocks
{
    [MockService(typeof(ILobbyService))]
    public class MockLobbyService : ILobbyService, IMockService
    {
        public string OriginalServiceName => nameof(ILobbyService);
    }
}
