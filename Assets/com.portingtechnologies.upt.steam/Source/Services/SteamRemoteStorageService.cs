#if !DISABLESTEAMWORKS

using Steamworks;
using System;
using UPT.Services;

namespace UPT.Steam
{
    public class SteamRemoteStorageService : IRemoteStorageService
    {
        private CallResult<RemoteStorageFileWriteAsyncComplete_t> m_RemoteStorageFileWriteAsyncComplete;
        private CallResult<RemoteStorageFileReadAsyncComplete_t> m_RemoteStorageFileReadAsyncComplete;

        private RemoteWriteCallback m_writeCallbackCache;
        private RemoteReadCallback m_readCallbackCache;

        public SteamRemoteStorageService()
        {
            m_RemoteStorageFileWriteAsyncComplete = CallResult<RemoteStorageFileWriteAsyncComplete_t>.Create(OnRemoteStorageFileWriteAsyncComplete);
            m_RemoteStorageFileReadAsyncComplete = CallResult<RemoteStorageFileReadAsyncComplete_t>.Create(OnRemoteStorageFileReadAsyncComplete);
        }

        public void Delete(string key, RemoteDeleteCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            SteamRemoteStorage.FileDelete(key);
            callback?.Invoke(new UptResult(ErrorCode.Success));
        }

        public void Exists(string key, RemoteExistsCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptRemoteExistsResult(ErrorCode.SdkNotInitialized));
                return;
            }

            var exists = SteamRemoteStorage.FileExists(key);
            callback?.Invoke(new UptRemoteExistsResult(ErrorCode.Success, null, exists));
        }

        public void GetAllDataInfos(RemoteGetAllDataInfosCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptRemoteGetAllDataInfos(ErrorCode.SdkNotInitialized));
                return;
            }

            var count = SteamRemoteStorage.GetFileCount();
            var fileList = new RemoteFileInfo[count];
            for (var i = 0; i < count; i++)
            {
                var filename = SteamRemoteStorage.GetFileNameAndSize(i, out var size);
                var timestep = SteamRemoteStorage.GetFileTimestamp(filename);
                var lastModifiedTime = DateTimeOffset.FromUnixTimeSeconds(timestep).UtcDateTime;
                fileList[i] = new RemoteFileInfo(filename, size, lastModifiedTime);
            }

            callback?.Invoke(new UptRemoteGetAllDataInfos(ErrorCode.Success, null, fileList));
        }

        public void GetInfo(string key, RemoteGetInfoCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.SdkNotInitialized));
                return;
            }

            var size = SteamRemoteStorage.GetFileSize(key);
            var timestep = SteamRemoteStorage.GetFileTimestamp(key);
            var lastModifiedTime = DateTimeOffset.FromUnixTimeSeconds(timestep).UtcDateTime;
            var fileInfo = new RemoteFileInfo(key, size, lastModifiedTime);
            callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.Success, null, fileInfo));
        }

        public void Read(string key, RemoteReadCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptRemoteReadResult(ErrorCode.SdkNotInitialized));
                return;
            }

            var size = SteamRemoteStorage.GetFileSize(key);
            if (size == 0)
            {
                callback?.Invoke(new UptRemoteReadResult(ErrorCode.NotFound));
                return;
            }

            m_readCallbackCache = callback;
            var apiCall = SteamRemoteStorage.FileReadAsync(key, 0, (uint)size);
            m_RemoteStorageFileReadAsyncComplete.Set(apiCall);
        }

        public void Write(string key, byte[] data, RemoteWriteCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                callback?.Invoke(new UptResult(ErrorCode.SdkNotInitialized));
                return;
            }

            m_writeCallbackCache = callback;
            var apiCall = SteamRemoteStorage.FileWriteAsync(key, data, (uint)data.Length);
            m_RemoteStorageFileWriteAsyncComplete.Set(apiCall);
        }

        private void OnRemoteStorageFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t data, bool failure)
        {
            if (!failure && data.m_eResult == EResult.k_EResultOK)
                m_writeCallbackCache?.Invoke(new UptResult(ErrorCode.Success, null));
            else
                m_writeCallbackCache?.Invoke(new UptResult(ErrorCode.UntypedError, $"RemoteStorageFileWriteAsyncComplete_t has returned a callback with the error code {data.m_eResult}"));
            m_writeCallbackCache = null;
        }

        private void OnRemoteStorageFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t data, bool failure)
        {
            if (!failure && data.m_eResult == EResult.k_EResultOK)
            {
                var fileData = new byte[data.m_cubRead];
                if (SteamRemoteStorage.FileReadAsyncComplete(data.m_hFileReadAsync, fileData, data.m_cubRead))
                    m_readCallbackCache?.Invoke(new UptRemoteReadResult(ErrorCode.Success, null, fileData));
                else
                    m_readCallbackCache?.Invoke(new UptRemoteReadResult(ErrorCode.UntypedError, "SteamRemoteStorage.FileReadAsyncComplete has returned false"));
            }
            else
            {
                m_readCallbackCache?.Invoke(new UptRemoteReadResult(ErrorCode.UntypedError, $"RemoteStorageFileReadAsyncComplete_t has returned a callback with the error code {data.m_eResult}"));
            }
            m_readCallbackCache = null;
        }
    }
}

#endif