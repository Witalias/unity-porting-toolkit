#if !EOS_DISABLE

using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;
using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using UnityEngine;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSRemoteStorageService : IRemoteStorageService
    {
        private readonly EOSServiceContext m_context;
        private readonly HashSet<string> m_queriedFiles = new();

        private PlayerDataStorageInterface PlayerDataStorageInterface => EOSManager.Instance.GetPlayerDataStorageInterface();

        public EOSRemoteStorageService(EOSServiceContext context)
        {
            m_context = context;
        }

        public void Delete(string key, RemoteDeleteCallback callback)
        {
            var options = new DeleteFileOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Filename = key
            };

            PlayerDataStorageInterface.DeleteFile(ref options, null, Callback);

            void Callback(ref DeleteFileCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_queriedFiles.Remove(key);
                    callback?.Invoke(new UptResult(ErrorCode.Success));
                }
                else
                {
                    callback?.Invoke(new UptResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)));
                }
            }
        }

        public void Exists(string key, RemoteExistsCallback callback)
        {
            QueryFile(key, (success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptRemoteExistsResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var fileExists = CopyFileMetadata(key, out _);
                callback?.Invoke(new UptRemoteExistsResult(ErrorCode.Success, null, fileExists));
            });
        }

        public void GetInfo(string key, RemoteGetInfoCallback callback)
        {
            QueryFile(key, (success, errorMessage) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                if (!CopyFileMetadata(key, out var metadata))
                {
                    callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.NotFound, string.Empty));
                    return;
                }

                var size = metadata.UnencryptedDataSizeBytes;
                var lastModifiedTime = metadata.LastModifiedTime.Value.UtcDateTime;
                var fileInfo = new RemoteFileInfo(key, (int)size, lastModifiedTime);
                callback?.Invoke(new UptRemoteGetInfoResult(ErrorCode.Success, null, fileInfo));
            });
        }

        public void GetAllDataInfos(RemoteGetAllDataInfosCallback callback)
        {
            QueryFileList((success, errorMessage, count) =>
            {
                if (!success)
                {
                    callback?.Invoke(new UptRemoteGetAllDataInfos(ErrorCode.UntypedError, errorMessage));
                    return;
                }

                var fileList = new RemoteFileInfo[count];
                for (uint i = 0; i < count; i++)
                {
                    if (!CopyFileMetadata(i, out var metadata))
                        continue;

                    var key = metadata.Filename;
                    var size = metadata.UnencryptedDataSizeBytes;
                    var lastModifiedTime = metadata.LastModifiedTime.Value.UtcDateTime;
                    var fileInfo = new RemoteFileInfo(key, (int)size, lastModifiedTime);
                    fileList[i] = fileInfo;
                }

                callback?.Invoke(new UptRemoteGetAllDataInfos(ErrorCode.Success, null, fileList));
            });
        }

        public void Read(string key, RemoteReadCallback callback)
        {
            var fileBytes = new List<byte>();

            var readOptions = new ReadFileOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Filename = key,
                ReadChunkLengthBytes = EOSServiceContext.READ_WRITE_CHUNK_LENGTH,
                ReadFileDataCallback = ReadDataCallback,
            };

            PlayerDataStorageInterface.ReadFile(ref readOptions, null, ReadCompleteCallback);

            ReadResult ReadDataCallback(ref ReadFileDataCallbackInfo data)
            {
                fileBytes.AddRange(data.DataChunk);
                return ReadResult.ContinueReading;
            }

            void ReadCompleteCallback(ref ReadFileCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                    callback?.Invoke(new UptRemoteReadResult(ErrorCode.Success, null, fileBytes.ToArray()));
                else
                    callback?.Invoke(new UptRemoteReadResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)));
            }
        }

        public void Write(string key, byte[] data, RemoteWriteCallback callback)
        {
            var chunksWritten = 0;

            var writeOptions = new WriteFileOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Filename = key,
                ChunkLengthBytes = EOSServiceContext.READ_WRITE_CHUNK_LENGTH,
                WriteFileDataCallback = WriteDataCallback,
            };

            PlayerDataStorageInterface.WriteFile(ref writeOptions, null, WriteCompleteCallback);

            WriteResult WriteDataCallback(ref WriteFileDataCallbackInfo writeFileData, out ArraySegment<byte> outDataBuffer)
            {
                var lengthBytes = (int)writeFileData.DataBufferLengthBytes;
                var offset = chunksWritten * lengthBytes;

                if (offset >= data.Length) // writing has completed
                {
                    outDataBuffer = ArraySegment<byte>.Empty;
                    return WriteResult.CompleteRequest;
                }

                var nextLength = Mathf.Min(lengthBytes, data.Length - offset);
                outDataBuffer = new ArraySegment<byte>(data, offset, nextLength);
                chunksWritten++;

                return WriteResult.ContinueWriting;
            }

            void WriteCompleteCallback(ref WriteFileCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_queriedFiles.Remove(key);
                    callback?.Invoke(new UptResult(ErrorCode.Success));
                }
                else
                {
                    callback?.Invoke(new UptResult(ErrorCode.UntypedError, EOSServiceContext.GetBackendErrorMsg(data.ResultCode)));
                }
            }
        }

        private void QueryFile(string key, Action<bool, string> callback)
        {
            if (m_queriedFiles.Contains(key))
            {
                callback?.Invoke(true, null);
                return;
            }

            var options = new QueryFileOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Filename = key
            };

            PlayerDataStorageInterface.QueryFile(ref options, null, Callback);

            void Callback(ref QueryFileCallbackInfo data)
            {
                if (data.ResultCode == Result.Success)
                {
                    m_queriedFiles.Add(key);
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode));
                }
            }
        }

        private void QueryFileList(Action<bool, string, uint> callback)
        {
            var options = new QueryFileListOptions()
            {
                LocalUserId = m_context.ProductUserId
            };

            PlayerDataStorageInterface.QueryFileList(ref options, null, Callback);

            void Callback(ref QueryFileListCallbackInfo data)
            {
                if (data.ResultCode != Result.Success)
                {
                    callback?.Invoke(false, EOSServiceContext.GetBackendErrorMsg(data.ResultCode), 0);
                    return;
                }

                m_queriedFiles.Clear();

                for (uint i = 0; i < data.FileCount; i++)
                {
                    if (CopyFileMetadata(i, out var metadata))
                        m_queriedFiles.Add(metadata.Filename);
                }

                callback?.Invoke(true, null, data.FileCount);
            }
        }

        private bool CopyFileMetadata(string key, out FileMetadata outFileMetadata)
        {
            outFileMetadata = new FileMetadata();

            var options = new CopyFileMetadataByFilenameOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Filename = key
            };

            var result = PlayerDataStorageInterface.CopyFileMetadataByFilename(ref options, out var fileMetadata);
            if (result != Result.Success || !fileMetadata.HasValue)
                return false;

            outFileMetadata = fileMetadata.Value;
            return true;
        }

        private bool CopyFileMetadata(uint index, out FileMetadata outFileMetadata)
        {
            outFileMetadata = new FileMetadata();

            var options = new CopyFileMetadataAtIndexOptions()
            {
                LocalUserId = m_context.ProductUserId,
                Index = index
            };

            var result = PlayerDataStorageInterface.CopyFileMetadataAtIndex(ref options, out var fileMetadata);
            if (result != Result.Success || !fileMetadata.HasValue)
                return false;

            outFileMetadata = fileMetadata.Value;
            return true;
        }
    }
}

#endif