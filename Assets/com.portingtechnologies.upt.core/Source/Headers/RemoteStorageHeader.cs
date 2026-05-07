using System;

namespace UPT.Services
{
    public interface IRemoteStorageService
    {
        void Write(string key, byte[] data, RemoteWriteCallback callback);
        void Read(string key, RemoteReadCallback callback);
        void Delete(string key, RemoteDeleteCallback callback);
        void Exists(string key, RemoteExistsCallback callback);
        void GetInfo(string key, RemoteGetInfoCallback callback);
        void GetAllDataInfos(RemoteGetAllDataInfosCallback callback);
    }

    public class RemoteFileInfo
    {
        public string Key { get; }
        public int SizeInBytes { get; }
        public DateTime LastModifiedTime { get; }

        public RemoteFileInfo(string key, int sizeInBytes, DateTime lastModifiedTime)
        {
            Key = key;
            SizeInBytes = sizeInBytes;
            LastModifiedTime = lastModifiedTime;
        }
    }

    public class UptRemoteReadResult : UptResult
    {
        public byte[] Data { get; }

        public UptRemoteReadResult(ErrorCode error, string innerMessage = null, byte[] data = null) : base(error, innerMessage)
        {
            Data = data;
        }
    }

    public class UptRemoteExistsResult : UptResult
    {
        public bool Exists { get; }

        public UptRemoteExistsResult(ErrorCode error, string innerMessage = null, bool exists = false) : base(error, innerMessage)
        {
            Exists = exists;
        }
    }

    public class UptRemoteGetInfoResult : UptResult
    {
        public RemoteFileInfo Info { get; }

        public UptRemoteGetInfoResult(ErrorCode error, string innerMessage = null, RemoteFileInfo fileInfo = null) : base(error, innerMessage)
        {
            Info = fileInfo;
        }
    }

    public class UptRemoteGetAllDataInfos : UptResult
    {
        public RemoteFileInfo[] Infos { get; }

        public UptRemoteGetAllDataInfos(ErrorCode error, string innerMessage = null, RemoteFileInfo[] infos = null) : base(error, innerMessage)
        {
            Infos = infos;
        }
    }

    public delegate void RemoteWriteCallback(UptResult result);
    public delegate void RemoteReadCallback(UptRemoteReadResult result);
    public delegate void RemoteDeleteCallback(UptResult result);
    public delegate void RemoteExistsCallback(UptRemoteExistsResult result);
    public delegate void RemoteGetInfoCallback(UptRemoteGetInfoResult result);
    public delegate void RemoteGetAllDataInfosCallback(UptRemoteGetAllDataInfos result);
}
