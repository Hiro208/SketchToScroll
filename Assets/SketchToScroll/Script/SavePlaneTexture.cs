using System.IO;
using UnityEngine;

/// <summary>
/// Saves the main texture from a target renderer's material to disk as a PNG.
/// Can either save every frame or be triggered manually via <see cref="SaveOnce"/>.
/// </summary>
[DisallowMultipleComponent]
public class SavePlaneTexture : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Renderer whose material texture will be saved.")]
    public Renderer targetRenderer;

    [Header("Output")]
    [Tooltip("Absolute folder path where the texture PNG will be written.")]
    public string saveDirectory = "D:/RealtimePaint";

    [Tooltip("File name for the saved texture, including extension.")]
    public string fileName = "saved_texture.png";

    [Header("Behaviour")]
    [Tooltip("If true, saves the texture every frame (overwriting the same file).")]
    public bool saveEveryFrame = false;

    private Texture2D savedTexture;

    private void Start()
    {
        EnsureDirectoryExists();
    }

    private void Update()
    {
        if (saveEveryFrame)
        {
            SaveTexture();
        }
    }

    /// <summary>
    /// Public entry point to save the current texture once, without enabling per-frame saving.
    /// </summary>
    public void SaveOnce()
    {
        SaveTexture();
    }

    private void EnsureDirectoryExists()
    {
        if (string.IsNullOrWhiteSpace(saveDirectory))
        {
            Debug.LogWarning("❌ Save directory is null or empty. Skipping directory creation.");
            return;
        }

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    private void SaveTexture()
    {
        if (targetRenderer == null || targetRenderer.material == null || targetRenderer.material.mainTexture == null)
        {
            Debug.LogWarning("❌ 未分配 targetRenderer、材质为空，或其材质贴图为空！", this);
            return;
        }

        EnsureDirectoryExists();

        var mainTex = targetRenderer.material.mainTexture;

        if (mainTex is Texture2D tex2D)
        {
            SaveTextureToFile(tex2D);
        }
        else if (mainTex is RenderTexture renderTex)
        {
            SaveRenderTextureToFile(renderTex);
        }
        else
        {
            Debug.LogWarning("❌ 不支持的贴图类型：" + mainTex.GetType(), this);
        }
    }

    private void SaveRenderTextureToFile(RenderTexture renderTex)
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTex;

        if (savedTexture == null || savedTexture.width != renderTex.width || savedTexture.height != renderTex.height)
        {
            savedTexture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);
        }

        savedTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        savedTexture.Apply();

        RenderTexture.active = currentRT;

        SaveTextureToFile(savedTexture);
    }

    private void SaveTextureToFile(Texture2D tex)
    {
        var pngData = tex.EncodeToPNG();
        if (pngData == null || pngData.Length == 0)
        {
            Debug.LogWarning("❌ Failed to encode texture to PNG data.", this);
            return;
        }

        var fullPath = Path.Combine(saveDirectory, fileName);
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("✅ 保存贴图到: " + fullPath);
    }
}

