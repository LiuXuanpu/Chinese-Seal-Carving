using UnityEngine;

public class PaintingCanvas : MonoBehaviour
{
    public Transform penTip;
    public float penRadius = 0.002f;
    public RenderTexture paperRT;
    public Color brushColor = Color.black;
    public float drawInterval = 0.02f;

    private Texture2D bufferTex;
    private Vector2? lastUV;
    private float lastDrawTime;
    

    void Start()
    {
        bufferTex = new Texture2D(paperRT.width, paperRT.height, TextureFormat.RGBA32, false);
        ClearBuffer(Color.white);
    }

    void Update()
    {
        var rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool trigger);

        if (!trigger || penTip == null)
        {
            lastUV = null;
            return;
        }

        if (Time.time - lastDrawTime < drawInterval) return;

        Ray ray = new Ray(penTip.position, penTip.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.1f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Vector2 uv = hit.textureCoord;
                DrawPoint(uv);
                lastUV = uv;
                lastDrawTime = Time.time;
            }
        }
        else
        {
            lastUV = null;
        }
    }

    void DrawPoint(Vector2 uv)
    {
        int cx = Mathf.RoundToInt(uv.x * bufferTex.width);
        int cy = Mathf.RoundToInt(uv.y * bufferTex.height);
        int r = Mathf.RoundToInt(penRadius * bufferTex.width);

        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < bufferTex.width && py >= 0 && py < bufferTex.height)
                        bufferTex.SetPixel(px, py, brushColor);
                }

        if (lastUV.HasValue)
        {
            Vector2 prev = lastUV.Value;
            float dist = Vector2.Distance(uv, prev);
            int steps = Mathf.CeilToInt(dist * bufferTex.width);
            for (int i = 1; i <= steps; i++)
            {
                Vector2 lerp = Vector2.Lerp(prev, uv, (float)i / steps);
                int ix = Mathf.RoundToInt(lerp.x * bufferTex.width);
                int iy = Mathf.RoundToInt(lerp.y * bufferTex.height);
                for (int y = -r; y <= r; y++)
                    for (int x = -r; x <= r; x++)
                        if (x * x + y * y <= r * r)
                        {
                            int px = ix + x, py = iy + y;
                            if (px >= 0 && px < bufferTex.width && py >= 0 && py < bufferTex.height)
                                bufferTex.SetPixel(px, py, brushColor);
                        }
            }
        }

        bufferTex.Apply();
        Graphics.Blit(bufferTex, paperRT);
    }

    void ClearBuffer(Color c)
    {
        var pixels = bufferTex.GetPixels();
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
        bufferTex.SetPixels(pixels);
        bufferTex.Apply();
        Graphics.Blit(bufferTex, paperRT);
    }

    public Texture2D GetCanvasTexture() => bufferTex;
}