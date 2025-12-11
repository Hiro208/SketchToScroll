using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using System.Collections;

public class readResult : MonoBehaviour
{
    public string imageFolderPath;
    public Renderer targetRenderer;
    public float refreshInterval = 1.0f;

    private string lastImagePath = "";

    void Start()
    {
        StartCoroutine(CheckForNewImage());
    }

    IEnumerator CheckForNewImage()
    {
        while (true)
        {
            if (Directory.Exists(imageFolderPath))
            {
                var imageFiles = Directory.GetFiles(imageFolderPath)
                    .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
                    .OrderBy(f => f) // 按文件名排序
                    .ToList();

                if (imageFiles.Count > 0)
                {
                    string latestPath = imageFiles.Last();

                    // 确保不是重复的图
                    if (latestPath != lastImagePath)
                    {
                        if (IsFileReady(latestPath))
                        {
                            lastImagePath = latestPath;
                            yield return StartCoroutine(LoadTextureFromPath(latestPath));
                        }
                    }
                }
            }
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    bool IsFileReady(string path)
    {
        try
        {
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
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

    IEnumerator LoadTextureFromPath(string path)
    {
        string fileUrl = "file://" + path.Replace("\\", "/");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null && targetRenderer != null)
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

