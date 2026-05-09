using System;

namespace UPT.Services
{
    public interface IRemoteStorageService
    {
        /// <summary>
        /// Write data to the remote storage by the specific key.
        /// </summary>
        /// <param name="key">The unique key (identifier) of the data. <br/>Please note that different platforms may have their own limits on the maximum length of the key name.</param>
        /// <param name="data">The data to be written.</param>
        /// <param name="callback"></param>
        void Write(string key, byte[] data, RemoteWriteCallback callback);

        /// <summary>
        /// Read data from remote storage by the specific key. Returns NotFound error if no data exists for the specified key.
        /// </summary>
        /// <param name="key">The unique key (identifier) of the data.</param>
        /// <param name="callback">A callback with a structure containing read data.</param>
        void Read(string key, RemoteReadCallback callback);

        /// <summary>
        /// Delete data from remote storage by the specific key. Returns success even if no data exists for the specified key.
        /// </summary>
        /// <param name="key">The unique key (identifier) of the data.</param>
        /// <param name="callback"></param>
        void Delete(string key, RemoteDeleteCallback callback);

        /// <summary>
        /// Checks whether data exists by the specified key.
        /// </summary>
        /// <param name="key">The unique key (identifier) of the data.</param>
        /// <param name="callback">A callback with a structure containing data existence status.</param>
        void Exists(string key, RemoteExistsCallback callback);

        /// <summary>
        /// Retrieves information about the data by the specified key, such as the size in bytes and the last modified time.
        /// Returns NotFound error if no data exists for the specified key.
        /// </summary>
        /// <param name="key">The unique key (identifier) of the data.</param>
        /// <param name="callback">A callback with a structure containing information about the data.</param>
        void GetInfo(string key, RemoteGetInfoCallback callback);

        /// <summary>
        /// Gets all the information about all the data in the remote storage.
        /// </summary>
        /// <param name="callback">A callback with a structure containing an array of metadata about each file.</param>
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
