using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Polls a folder for the latest generated image and applies it to a target renderer.
/// Note: class name is kept as 'readResult' to avoid breaking existing Unity scene references.
/// </summary>
[DisallowMultipleComponent]
public class readResult : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Folder that will be scanned for the latest generated image.")]
    public string imageFolderPath;

    [Header("Target")]
    [Tooltip("Renderer whose material will receive the loaded image as a texture.")]
    public Renderer targetRenderer;

    [Header("Polling")]
    [Tooltip("Interval in seconds between folder scans.")]
    public float refreshInterval = 1.0f;

    private string lastImagePath = string.Empty;

    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg" };

    private void Start()
    {
        StartCoroutine(CheckForNewImage());
    }

    private IEnumerator CheckForNewImage()
    {
        var wait = new WaitForSeconds(refreshInterval);

        while (true)
        {
            if (!string.IsNullOrWhiteSpace(imageFolderPath) && Directory.Exists(imageFolderPath))
            {
                var imageFiles = Directory
                    .GetFiles(imageFolderPath)
                    .Where(IsSupportedImageFile)
                    .OrderBy(f => f)
                    .ToList();

                if (imageFiles.Count > 0)
                {
                    var latestPath = imageFiles[^1];

                    if (latestPath != lastImagePath && IsFileReady(latestPath))
                    {
                        lastImagePath = latestPath;
                        yield return LoadTextureFromPath(latestPath);
                    }
                }
            }

            yield return wait;
        }
    }

    private static bool IsSupportedImageFile(string path)
    {
        var extension = Path.GetExtension(path)?.ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    private static bool IsFileReady(string path)
    {
        try
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return stream.Length > 0;
            }
        }
        catch (IOException)
        {
            // 文件还在写入中
            return false;
        }
    }

    private IEnumerator LoadTextureFromPath(string path)
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning("❌ Target renderer is not assigned. Skipping image load.", this);
            yield break;
        }

        var fileUrl = "file://" + path.Replace("\\", "/");

        using (var request = UnityWebRequestTexture.GetTexture(fileUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    targetRenderer.material.mainTexture = texture;
                    Debug.Log("✅ 已加载新图片: " + path);
                }
            }
            else
            {
                Debug.LogError("❌ 图片加载失败: " + request.error);
            }
        }
    }
}

