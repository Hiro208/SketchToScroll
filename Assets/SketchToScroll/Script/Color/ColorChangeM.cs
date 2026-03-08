using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeM : MonoBehaviour
{
    [Header("被改变颜色的 Renderer")]
    public Renderer targetRenderer;

    [Tooltip("要修改的材质槽索引（第几个材质）")]
    public int materialIndex = 0;

    [Tooltip("Shader中要修改的颜色属性名，例如 _Color")]
    public string colorProperty = "_BaseColor";

    private Color originalColor;
    private bool hasOriginalColor = false;

    private void Start()
    {
        if (targetRenderer != null && materialIndex < targetRenderer.materials.Length)
        {
            originalColor = targetRenderer.materials[materialIndex].GetColor(colorProperty);
            hasOriginalColor = true;
        }
    }

    public void ApplyColor(Color newColor)
    {
        if (targetRenderer != null && materialIndex < targetRenderer.materials.Length)
        {
            targetRenderer.materials[materialIndex].SetColor(colorProperty, newColor);
        }
    }
}
