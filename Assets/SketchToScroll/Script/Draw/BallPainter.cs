using UnityEngine;

/// <summary>
/// Uses a sphere-like object as a brush to paint onto a board managed by <see cref="BoardPaintManager"/>.
/// When the ball stays inside the paint zone, it raycasts towards the board and draws along its path.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BallPainter : MonoBehaviour
{
    [Header("Board")]
    [Tooltip("Board manager that provides the drawable texture and renderer.")]
    public BoardPaintManager boardManager;

    [Header("Painting Zone")]
    [Tooltip("Trigger collider that enables painting when the ball is inside.")]
    public Collider paintZoneTrigger;

    [Header("Brush Settings")]
    public Color paintColor = Color.red;

    [Min(1)]
    public int brushSize = 8;

    private bool canPaint;
    private Vector2? lastUV;

    private void Update()
    {
        if (!canPaint)
        {
            lastUV = null;
            return;
        }

        if (boardManager == null || boardManager.boardRenderer == null || boardManager.drawTexture == null)
        {
            Debug.LogWarning($"{nameof(BallPainter)} on '{name}' is enabled but missing board references.", this);
            return;
        }

        var boardTransform = boardManager.boardRenderer.transform;
        var paintDirection = boardTransform.TransformDirection(Vector3.down);
        var ray = new Ray(transform.position, paintDirection);

        if (!Physics.Raycast(ray, out var hit, 2.0f))
        {
            return;
        }

        if (hit.collider.gameObject != boardManager.boardRenderer.gameObject)
        {
            return;
        }

        var currentUV = hit.textureCoord;

        if (lastUV.HasValue)
        {
            DrawInterpolated(lastUV.Value, currentUV);
        }
        else
        {
            DrawAtUV(currentUV);
        }

        lastUV = currentUV;
    }

    private void DrawInterpolated(Vector2 fromUV, Vector2 toUV)
    {
        var texture = boardManager.drawTexture;
        if (texture == null)
        {
            return;
        }

        var distance = Vector2.Distance(fromUV, toUV);
        var steps = Mathf.CeilToInt(distance * texture.width * 2);

        for (var i = 0; i <= steps; i++)
        {
            var t = i / (float)steps;
            var interpolatedUV = Vector2.Lerp(fromUV, toUV, t);
            DrawAtUV(interpolatedUV);
        }
    }

    private void DrawAtUV(Vector2 uv)
    {
        var drawTexture = boardManager.drawTexture;
        if (drawTexture == null)
        {
            return;
        }

        var x = (int)(uv.x * drawTexture.width);
        var y = (int)(uv.y * drawTexture.height);

        for (var i = -brushSize; i <= brushSize; i++)
        {
            for (var j = -brushSize; j <= brushSize; j++)
            {
                var dist = Mathf.Sqrt(i * i + j * j);
                if (dist > brushSize)
                {
                    continue;
                }

                var px = Mathf.Clamp(x + i, 0, drawTexture.width - 1);
                var py = Mathf.Clamp(y + j, 0, drawTexture.height - 1);

                var alpha = 1f - (dist / brushSize);
                var existingColor = drawTexture.GetPixel(px, py);
                var blendedColor = Color.Lerp(existingColor, paintColor, alpha);
                drawTexture.SetPixel(px, py, blendedColor);
            }
        }

        drawTexture.Apply();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == paintZoneTrigger)
        {
            canPaint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == paintZoneTrigger)
        {
            canPaint = false;
        }
    }
}

/*using UnityEngine;

public class BallPainter : MonoBehaviour
{
    
    public GameObject board;
    public Collider paintZoneTrigger;
    public Color paintColor = Color.red;
    public int brushSize = 8;

    private Texture2D drawTexture;
    private Renderer boardRenderer;

    private bool canPaint = false;
    private Vector2? lastUV = null;

    void Start()
    {
        boardRenderer = board.GetComponent<Renderer>();

        // 为该实例创建一个独立材质，避免所有球共用材质
        boardRenderer.material = new Material(boardRenderer.material);

        drawTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        Color[] whitePixels = new Color[drawTexture.width * drawTexture.height];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }
        drawTexture.SetPixels(whitePixels);
        drawTexture.Apply();

        boardRenderer.material.mainTexture = drawTexture;
    }

    void Update()
    {
        if (canPaint)
        {
            //沿着画板的局部 Y 轴负方向
            Vector3 paintDir = board.transform.TransformDirection(Vector3.down);
            Ray ray = new Ray(transform.position, paintDir);

            // 如果射线击中画布
            if (Physics.Raycast(ray, out RaycastHit hit, 2.0f))
            {

                if (hit.collider.gameObject == board)
                {
                    Vector2 currentUV = hit.textureCoord;// 获取击中位置的UV坐标

                    if (lastUV != null)
                    {
                        // 如果有上一个UV点，进行插值绘制，防止出现断裂
                        DrawInterpolated(lastUV.Value, currentUV);
                    }
                    else
                    {
                        DrawAtUV(currentUV);
                    }

                    lastUV = currentUV;
                }
            }
        }
        else
        {
            lastUV = null;
        }
    }

    // 插值
    void DrawInterpolated(Vector2 fromUV, Vector2 toUV)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(fromUV, toUV) * drawTexture.width * 2);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 interpolatedUV = Vector2.Lerp(fromUV, toUV, t);
            DrawAtUV(interpolatedUV);
        }
    }

    // 圆形画笔
    void DrawAtUV(Vector2 uv)
    {
        int x = (int)(uv.x * drawTexture.width);
        int y = (int)(uv.y * drawTexture.height);

        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                float dist = Mathf.Sqrt(i * i + j * j);
                if (dist <= brushSize)
                {
                    int px = Mathf.Clamp(x + i, 0, drawTexture.width - 1);
                    int py = Mathf.Clamp(y + j, 0, drawTexture.height - 1);

                    float alpha = 1f - (dist / brushSize);
                    Color existingColor = drawTexture.GetPixel(px, py);
                    Color blendedColor = Color.Lerp(existingColor, paintColor, alpha);
                    drawTexture.SetPixel(px, py, blendedColor);
                }
            }
        }

        drawTexture.Apply();
    }

    // 进入绘制区域
    void OnTriggerEnter(Collider other)
    {
        if (other == paintZoneTrigger)
        {
            canPaint = true;
        }
    }
    // 离开绘制区域
    void OnTriggerExit(Collider other)
    {
        if (other == paintZoneTrigger)
        {
            canPaint = false;
        }
    }
}*/
