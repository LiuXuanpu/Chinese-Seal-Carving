using UnityEngine;

public class StampReceiver : MonoBehaviour
{
    public Renderer paperRenderer;

    private Texture2D paperTexture;

    void Start()
    {
        paperTexture = new Texture2D(
            1024,
            1024,
            TextureFormat.RGBA32,
            false
        );

        Color[] colors = new Color[1024 * 1024];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        paperTexture.SetPixels(colors);
        paperTexture.Apply();

        // URP必须用_BaseMap
        paperRenderer.material.SetTexture("_BaseMap", paperTexture);
    }

    public void ApplyStamp(RenderTexture stampRT)
    {
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture.active = stampRT;

        Texture2D stampTex = new Texture2D(
            stampRT.width,
            stampRT.height,
            TextureFormat.RGBA32,
            false
        );

        stampTex.ReadPixels(
            new Rect(0, 0, stampRT.width, stampRT.height),
            0,
            0
        );

        stampTex.Apply();

        // 直接覆盖纸张
        paperTexture.SetPixels(stampTex.GetPixels());

        paperTexture.Apply();

        // URP刷新
        paperRenderer.material.SetTexture(
            "_BaseMap",
            paperTexture
        );

        RenderTexture.active = currentRT;

        Debug.Log("盖章成功");
    }
}