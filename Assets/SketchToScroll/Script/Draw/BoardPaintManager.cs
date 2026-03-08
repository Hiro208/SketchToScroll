using UnityEngine;

/// <summary>
/// Manages the runtime drawing texture used as the painting board.
/// Responsible for creating a clean texture instance and assigning it
/// to the provided renderer's material so other scripts can paint on it.
/// </summary>
public class BoardPaintManager : MonoBehaviour
{
    [Header("Board Settings")]
    [Tooltip("Renderer whose material will receive the runtime drawing texture.")]
    public Renderer boardRenderer;

    [Tooltip("Resolution of the generated drawing texture (square).")]
    public int textureSize = 512;

    [HideInInspector]
    public Texture2D drawTexture;

    private void Awake()
    {
        if (boardRenderer == null)
        {
            boardRenderer = GetComponent<Renderer>();
        }

        if (boardRenderer == null)
        {
            Debug.LogError($"{nameof(BoardPaintManager)} on '{name}' requires a Renderer reference.", this);
            enabled = false;
            return;
        }

        var size = Mathf.Max(1, textureSize);
        drawTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        var clearColor = Color.white;
        var pixels = new Color[size * size];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clearColor;
        }

        drawTexture.SetPixels(pixels);
        drawTexture.Apply();

        var materialInstance = new Material(boardRenderer.material)
        {
            mainTexture = drawTexture
        };

        boardRenderer.material = materialInstance;
    }
}
