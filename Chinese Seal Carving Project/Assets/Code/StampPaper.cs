using UnityEngine;
using UnityEngine.XR;

public class StampPaper : MonoBehaviour
{
    [Header("盖章纸 RenderTexture")]
    public RenderTexture paperRT;

    [Header("印章物体（带 MeshCarving）")]
    public Transform stampTransform;

    [Header("接触距离（米）")]
    public float stampDistance = 0.03f;   // 印章底部距离纸面多近算接触

    private Texture2D paperBuffer;
    private InputDevice rightHand;
    private bool lastTrigger;

    void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        paperBuffer = new Texture2D(paperRT.width, paperRT.height, TextureFormat.RGBA32, false);
        ClearPaper(Color.white);
    }

    void Update()
    {
        if (!stampTransform) return;

        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);

        // 按下扳机且印章足够靠近纸面
        if (trigger && !lastTrigger && IsStampClose())
        {
            Stamp();
        }
        lastTrigger = trigger;
    }

    bool IsStampClose()
    {
        // 简单用 Y 轴距离：纸水平朝上(Y轴正方向)，印章底面Y坐标 = 印章中心Y - 半高
        float paperY = transform.position.y;
        float stampBottomY = stampTransform.position.y - (stampTransform.lossyScale.y * 0.5f);
        return Mathf.Abs(stampBottomY - paperY) < stampDistance;
    }

    void Stamp()
    {
        MeshCarving carving = stampTransform.GetComponent<MeshCarving>();
        if (!carving)
        {
            Debug.LogError("印章上未找到 MeshCarving 组件！");
            return;
        }

        Texture2D stampTex = carving.CreateStampTexture();
        if (stampTex == null)
        {
            Debug.LogError("印章纹理生成失败！");
            return;
        }

        // 将红色印泥叠加到纸上
        for (int y = 0; y < paperBuffer.height; y++)
        {
            for (int x = 0; x < paperBuffer.width; x++)
            {
                Color paperPixel = paperBuffer.GetPixel(x, y);
                Color stampPixel = stampTex.GetPixel(x, y);
                if (stampPixel.a > 0.01f)
                {
                    paperPixel = Color.Lerp(paperPixel, stampPixel, stampPixel.a);
                }
                paperBuffer.SetPixel(x, y, paperPixel);
            }
        }
        paperBuffer.Apply();
        Graphics.Blit(paperBuffer, paperRT);
        Debug.Log("盖章完成！");
    }

    void ClearPaper(Color bgColor)
    {
        Color[] pixels = paperBuffer.GetPixels();
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bgColor;
        paperBuffer.SetPixels(pixels);
        paperBuffer.Apply();
        Graphics.Blit(paperBuffer, paperRT);
    }
}