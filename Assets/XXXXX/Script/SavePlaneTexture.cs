using UnityEngine;
using System.IO;

public class SavePlaneTexture : MonoBehaviour
{
    [Header("目标平面（带材质）")]
    public Renderer targetRenderer;

    [Header("保存文件夹路径（绝对路径）")]
    public string saveDirectory = "D:/RealtimePaint";

    [Header("保存的文件名（带后缀）")]
    public string fileName = "saved_texture.png";

    [Header("每帧是否保存（实时覆盖）")]
    public bool saveEveryFrame = true;

    private Texture2D savedTexture;

    void Start()
    {
        // 确保文件夹存在
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    void Update()
    {
        if (saveEveryFrame)
        {
            SaveTexture();
        }
    }

    void SaveTexture()
    {
        if (targetRenderer == null || targetRenderer.material.mainTexture == null)
        {
            Debug.LogWarning("❌ 未分配 targetRenderer 或其材质贴图为空！");
            return;
        }

        // 从材质中获取当前贴图（必须是 Texture2D 或 RenderTexture）
        Texture mainTex = targetRenderer.material.mainTexture;

        if (mainTex is Texture2D tex2D)
        {
            // 直接保存 Texture2D
            SaveTextureToFile(tex2D);
        }
        else if (mainTex is RenderTexture renderTex)
        {
            // 从 RenderTexture 读取为 Texture2D 再保存
            RenderTexture currentRT = RenderTexture.active;
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
        else
        {
            Debug.LogWarning("❌ 不支持的贴图类型：" + mainTex.GetType());
        }
    }

    void SaveTextureToFile(Texture2D tex)
    {
        byte[] pngData = tex.EncodeToPNG();
        string fullPath = Path.Combine(saveDirectory, fileName);
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("✅ 保存贴图到: " + fullPath);
    }
}

