using UPT.Core;

namespace UPT.Services.Mocks
{
    [MockService(typeof(IRemoteStorageService))]
    public class MockRemoteStorageService : IRemoteStorageService, IMockService
    {
        public string OriginalServiceName => nameof(IRemoteStorageService);

        public void Delete(string key, RemoteDeleteCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void Exists(string key, RemoteExistsCallback callback)
        {
            callback?.Invoke(new UptRemoteExistsResult(ErrorCode.Success));
        }

        public void GetAllDataInfos(RemoteGetAllDataInfosCallback callback)
        {
            callback?.Invoke(new UptRemoteGetAllDataInfos(ErrorCode.Success));
        }

        public void GetInfo(string key, RemoteGetInfoCallback callback)
        {
            callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.Success));
        }

        public void Read(string key, RemoteReadCallback callback)
        {
            callback?.Invoke(new UptRemoteReadResult(ErrorCode.Success));
        }

        public void Write(string key, byte[] data, RemoteWriteCallback callback)
        {
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }
    }
}
