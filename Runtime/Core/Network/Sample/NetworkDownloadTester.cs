using UnityEngine;
using UnityEngine.UI;
using Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;

public class NetworkDownloadTester : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage imageDisplay;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text statusText;
    [SerializeField] private Button loadImageButton;
    [SerializeField] private Button downloadPdfButton;
    [SerializeField] private Button downloadDocButton;
    [SerializeField] private Button downloadZipButton;
    [SerializeField] private Button downloadAllButton;

    [Header("Test URLs")]
    [SerializeField] private string imageUrl = "https://picsum.photos/512/512";
    [SerializeField]
    private List<FileDownloadInfo> fileUrls = new List<FileDownloadInfo>
    {
        new FileDownloadInfo { Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", FileName = "sample_pdf.pdf" },
        new FileDownloadInfo { Url = "https://file-examples.com/wp-content/uploads/2017/02/file-sample_100kB.doc", FileName = "sample_doc.doc" },
        new FileDownloadInfo { Url = "https://github.com/mathiasbynens/small/raw/master/sample.zip", FileName = "sample_zip.zip" }
    };

    [Header("Save Path")]
    [SerializeField] private string saveFolderName = "DownloadedFiles";

    [System.Serializable]
    public struct FileDownloadInfo
    {
        public string Url;
        public string FileName;
    }

    private void Start()
    {
        ValidateUIComponents();
        SetupButtonListeners();
        CreateSaveDirectory();
    }

    private void ValidateUIComponents()
    {
        if (imageDisplay == null)
            Debug.LogWarning("[NetworkDownloadTester] RawImage reference is missing!");
        if (progressBar == null)
            Debug.LogWarning("[NetworkDownloadTester] Slider reference is missing!");
        else
        {
            progressBar.value = 0;
            progressBar.gameObject.SetActive(false);
        }
        if (statusText == null)
            Debug.LogWarning("[NetworkDownloadTester] Text reference is missing!");
        else
            statusText.text = "Ready to test downloads";
    }

    private void SetupButtonListeners()
    {
        if (loadImageButton != null)
            loadImageButton.onClick.AddListener(() => LoadImageTexture().Forget());
        if (downloadPdfButton != null)
            downloadPdfButton.onClick.AddListener(() => DownloadSpecificFile(0).Forget());
        if (downloadDocButton != null)
            downloadDocButton.onClick.AddListener(() => DownloadSpecificFile(1).Forget());
        if (downloadZipButton != null)
            downloadZipButton.onClick.AddListener(() => DownloadSpecificFile(2).Forget());
        if (downloadAllButton != null)
            downloadAllButton.onClick.AddListener(() => TestFileDownload().Forget());
    }

    private void CreateSaveDirectory()
    {
        try
        {
            string saveFolder = Path.Combine(Application.persistentDataPath, saveFolderName ?? "Downloads");
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);
            Debug.Log($"[NetworkDownloadTester] Files will be saved to: {saveFolder}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkDownloadTester] Failed to create save directory: {ex.Message}");
        }
    }

    private async UniTaskVoid LoadImageTexture()
    {
        Debug.Log($"[LoadImageTexture] Starting texture download from: {imageUrl}");
        try
        {
            progressBar.value = 0;
            UpdateStatus("Loading image...");
            ShowProgressBar(true);

            var result = await ClientHttpService.GetTextureAsync(
                imageUrl,
                null,
                HttpRequestOptions.Default,
                null,
                UpdateProgress);

            if (result.IsSuccess)
            {
                var texture = result.Data;
                Debug.Log($"[LoadImageTexture] Texture loaded: {texture.width}x{texture.height}");
                if (imageDisplay != null)
                {
                    imageDisplay.gameObject.SetActive(true);
                    imageDisplay.texture = texture;
                    imageDisplay.color = Color.white;
                    UpdateStatus($"Image loaded: {texture.width}x{texture.height}");
                }
                else
                {
                    Debug.LogWarning("[LoadImageTexture] Cannot display texture: RawImage reference is null");
                    UpdateStatus("Cannot display image: UI element missing");
                }
            }
            else
            {
                Debug.LogError($"[LoadImageTexture] Failed to load texture: {result.ErrorMessage}");
                UpdateStatus($"Failed to load image: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            UpdateStatus($"Error loading image: {ex.Message}");
        }
        finally
        {
            ShowProgressBar(false);
        }
    }

    private async UniTaskVoid DownloadSpecificFile(int index)
    {
        if (index < 0 || index >= fileUrls.Count)
        {
            Debug.LogError($"[DownloadSpecificFile] Invalid file index: {index}");
            UpdateStatus($"Invalid file index: {index}");
            return;
        }

        var fileInfo = fileUrls[index];
        Debug.Log($"[DownloadSpecificFile] Starting download of {fileInfo.FileName} from: {fileInfo.Url}");
        await DownloadAndSaveFile(fileInfo.Url, fileInfo.FileName);
    }

    private async UniTaskVoid TestFileDownload()
    {
        Debug.Log("[TestFileDownload] Started");

        for (int i = 0; i < fileUrls.Count; i++)
        {
            var fileInfo = fileUrls[i];
            if (string.IsNullOrEmpty(fileInfo.Url) || string.IsNullOrEmpty(fileInfo.FileName))
            {
                Debug.LogError($"[TestFileDownload] File URL or name {i + 1} is empty!");
                UpdateStatus($"File URL or name {i + 1} is empty!");
                continue;
            }

            Debug.Log($"[TestFileDownload] Starting download from: {fileInfo.Url}");
            await DownloadAndSaveFile(fileInfo.Url, fileInfo.FileName);
            Debug.Log($"[TestFileDownload] Download task {i + 1} completed");
        }
    }

    private async UniTask DownloadAndSaveFile(string url, string fileName)
    {
        Debug.Log($"[DownloadAndSaveFile] Started for {fileName} from {url}");
        try
        {
            progressBar.value = 0;
            UpdateStatus($"Downloading {fileName}...");
            ShowProgressBar(true);

            var result = await ClientHttpService.GetStreamingAsync(
                url,
                chunk =>
                {
                    Debug.Log($"[DownloadAndSaveFile] Received chunk of size: {chunk.Length} bytes");
                },
                (received, total) =>
                {
                    Debug.Log($"percent: {((float) received /(float) total) * 100f} %");
                    UpdateProgress((float) received / total);
                },
                () => Debug.Log($"[DownloadAndSaveFile] Download completed for {fileName}"),
                HttpRequestOptions.Default,
                null);

            if (result.IsSuccess && result.Data != null)
            {
                string saveFolderPath = Path.Combine(Application.persistentDataPath,
                    string.IsNullOrEmpty(saveFolderName) ? "Downloads" : saveFolderName);

                if (!Directory.Exists(saveFolderPath))
                    Directory.CreateDirectory(saveFolderPath);

                string filePath = Path.Combine(saveFolderPath, fileName);
                File.WriteAllBytes(filePath, result.Data);
                Debug.Log($"[DownloadAndSaveFile] File saved: {filePath}");
                UpdateStatus($"{fileName} downloaded and saved to: {filePath}");
            }
            else
            {
                Debug.LogError($"[DownloadAndSaveFile] Failed to download {fileName}: {result.ErrorMessage}");
                UpdateStatus($"Failed to download {fileName}: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DownloadAndSaveFile] Exception during download: {ex.Message}");
            UpdateStatus($"Error during file download: {ex.Message}");
        }
        finally
        {
            ShowProgressBar(false);
        }
    }

    private void UpdateProgress(float progress)
    {
        try
        {
            if (progressBar != null)
                progressBar.value = progress;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkDownloadTester] Error updating progress: {ex.Message}");
        }
    }

    private void UpdateStatus(string message)
    {
        try
        {
            if (statusText != null)
                statusText.text = message;
            Debug.Log($"[NetworkDownloadTester] {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkDownloadTester] Error updating status: {ex.Message}");
        }
    }

    private void ShowProgressBar(bool show)
    {
        try
        {
            if (progressBar != null)
                progressBar.gameObject.SetActive(show);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkDownloadTester] Error showing progress bar: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        if (loadImageButton != null)
            loadImageButton.onClick.RemoveAllListeners();
        if (downloadPdfButton != null)
            downloadPdfButton.onClick.RemoveAllListeners();
        if (downloadDocButton != null)
            downloadDocButton.onClick.RemoveAllListeners();
        if (downloadZipButton != null)
            downloadZipButton.onClick.RemoveAllListeners();
        if (downloadAllButton != null)
            downloadAllButton.onClick.RemoveAllListeners();

        if (imageDisplay != null && imageDisplay.texture != null)
        {
            Destroy(imageDisplay.texture);
            imageDisplay.texture = null;
        }
    }
}