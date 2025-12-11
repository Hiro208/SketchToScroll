using UnityEngine;

public class BoardPaintManager : MonoBehaviour
{
    public Renderer boardRenderer;
    public int textureSize = 512;

    [HideInInspector]
    public Texture2D drawTexture;

    void Awake()
    {
        drawTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] whitePixels = new Color[textureSize * textureSize];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }
        drawTexture.SetPixels(whitePixels);
        drawTexture.Apply();

        boardRenderer.material = new Material(boardRenderer.material);
        boardRenderer.material.mainTexture = drawTexture;
    }
}
