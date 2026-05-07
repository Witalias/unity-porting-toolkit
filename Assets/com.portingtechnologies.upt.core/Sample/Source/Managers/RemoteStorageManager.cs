using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    public class RemoteStorageManager : MonoBehaviour
    {
        [Header("Write")]
        [SerializeField] private TMP_InputField m_writeKey;
        [SerializeField] private TMP_InputField m_writeString;
        [SerializeField] private Button m_writeButton;

        [Header("Read")]
        [SerializeField] private TMP_InputField m_readKey;
        [SerializeField] private Button m_readButton;

        [Header("Delete")]
        [SerializeField] private TMP_InputField m_deleteKey;
        [SerializeField] private Button m_deleteButton;

        [Header("Exists")]
        [SerializeField] private TMP_InputField m_existsKey;
        [SerializeField] private Button m_existsButton;

        [Header("Get Info")]
        [SerializeField] private TMP_InputField m_getInfoKey;
        [SerializeField] private Button m_getInfoButton;

        [Header("Get All File Infos")]
        [SerializeField] private Button m_getAllFileInfosButton;

        private Services.IRemoteStorageService m_remoteStorageService;

        private void Awake()
        {
            m_remoteStorageService = ServiceContainer.Get<Services.IRemoteStorageService>();
        }

        private void OnEnable()
        {
            if (m_remoteStorageService is IMockService)
                Debug.LogWarning("Remote storage service is mock! All functions will return a successfull result without any metadata");

            if (m_writeButton)
                m_writeButton.onClick.AddListener(OnWriteButtonClick);

            if (m_readButton)
                m_readButton.onClick.AddListener(OnReadButtonClick);

            if (m_deleteButton)
                m_deleteButton.onClick.AddListener(OnDeleteButtonClick);

            if (m_existsButton)
                m_existsButton.onClick.AddListener(OnExistsButtonClick);

            if (m_getInfoButton)
                m_getInfoButton.onClick.AddListener(OnGetFileInfoButtonClick);

            if (m_getAllFileInfosButton)
                m_getAllFileInfosButton.onClick.AddListener(OnGetAllFileInfosButtonClick);
        }

        private void OnDisable()
        {
            if (m_writeButton)
                m_writeButton.onClick.RemoveListener(OnWriteButtonClick);

            if (m_readButton)
                m_readButton.onClick.RemoveListener(OnReadButtonClick);

            if (m_deleteButton)
                m_deleteButton.onClick.RemoveListener(OnDeleteButtonClick);

            if (m_existsButton)
                m_existsButton.onClick.RemoveListener(OnDeleteButtonClick);

            if (m_getInfoButton)
                m_getInfoButton.onClick.RemoveListener(OnGetFileInfoButtonClick);

            if (m_getAllFileInfosButton)
                m_getAllFileInfosButton.onClick.RemoveListener(OnGetAllFileInfosButtonClick);
        }

        private void OnWriteButtonClick()
        {
            if (!m_writeKey || !m_writeString)
                return;

            var key = m_writeKey.text;
            var dataString = m_writeString.text;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key field is empty. Please enter the file key");
                return;
            }

            if (string.IsNullOrEmpty(dataString))
            {
                Debug.LogWarning("Data field is empty. Please enter the string data");
                return;
            }

            var dataBytes = Encoding.UTF8.GetBytes(dataString);

            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.Write(key, dataBytes, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Write file '{key}' success!");
                else
                    Debug.LogError($"Write file '{key}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnReadButtonClick()
        {
            if (!m_readKey)
                return;

            var key = m_readKey.text;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key field is empty. Please enter the file key");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.Read(key, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    var dataBytes = result.Data;
                    if (dataBytes == null)
                        return;

                    var dataString = Encoding.UTF8.GetString(dataBytes);
                    Debug.Log($"Read file '{key}' success! Content: {dataString}");
                }
                else
                {
                    Debug.LogError($"Read file '{key}' failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            });
        }

        private void OnDeleteButtonClick()
        {
            if (!m_deleteKey)
                return;

            var key = m_deleteKey.text;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key field is empty. Please enter the file key");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.Delete(key, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"Delete file '{key}' success!");
                else
                    Debug.LogError($"Delete file '{key}' failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnExistsButtonClick()
        {
            if (!m_existsKey)
                return;

            var key = m_existsKey.text;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key field is empty. Please enter the file key");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.Exists(key, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                    Debug.Log($"File '{key}' exists: {result.Exists}");
                else
                    Debug.LogError($"Checking file '{key}' existance failed: {result.ErrorCode}. {result.InnerMessage}");
            });
        }

        private void OnGetFileInfoButtonClick()
        {
            if (!m_getInfoButton)
                return;

            var key = m_getInfoKey.text;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key field is empty. Please enter the file key");
                return;
            }

            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.GetInfo(key, result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    var info = result.Info;
                    if (info == null)
                        return;

                    Debug.Log($"File '{key}' info:\n\tName: {info.Key}\n\tSize in bytes: {info.SizeInBytes}\n\tLast modified time: {info.LastModifiedTime}");
                }
                else
                {
                    Debug.LogError($"Get file info failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            });
        }

        private void OnGetAllFileInfosButtonClick()
        {
            GlobalManager.Instance.SetSpinnerActive(true);
            m_remoteStorageService.GetAllDataInfos(result =>
            {
                GlobalManager.Instance.SetSpinnerActive(false);
                if (result.IsSuccess)
                {
                    var fileList = result.Infos;
                    if (fileList == null)
                        return;

                    var log = new StringBuilder("Get all file infos success! ");

                    if (fileList.Length == 0)
                    {
                        log.Append("Remote storage is empty");
                    }
                    else
                    {
                        log.Append("Files list:");
                        foreach (var file in fileList)
                            log.Append($"\n\tName: {file.Key}. Size in bytes: {file.SizeInBytes}. Last modified time: {file.LastModifiedTime}");
                    }

                    Debug.Log(log);
                }
                else
                {
                    Debug.LogError($"Get all file infos failed: {result.ErrorCode}. {result.InnerMessage}");
                }
            });
        }
    }
}
