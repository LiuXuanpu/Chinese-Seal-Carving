using UnityEngine;

public class StampProcessor : MonoBehaviour
{
    [Header("深度相机与纹理")]
    public Camera depthCamera;
    public RenderTexture depthRenderTexture;

    [Header("纸张与变形设置")]
    public Transform stampPaperTransform;
    [Range(0.001f, 0.05f)]
    public float stampDepthScale = 0.01f; // 凸起/凹陷的强度

    [Header("颜色印记设置")]
    public Material stampPaperMaterial;   // 纸张使用的材质
    public string maskPropertyName = "_StampMask"; // 材质中接收遮罩的属性名
    public Color stampColor = Color.red;  // 印记颜色，可以在材质里调整

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Texture2D stampMask;          // 用于给材质遮罩的纹理

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        // 初始化一张跟深度RT分辨率一致的遮罩纹理
        stampMask = new Texture2D(depthRenderTexture.width, depthRenderTexture.height, TextureFormat.R8, false);
        stampMask.filterMode = FilterMode.Bilinear;

        // 将遮罩纹理赋给材质，初始为全黑（无印记）
        stampPaperMaterial.SetTexture(maskPropertyName, stampMask);
        ClearMask();
    }

    /// <summary>
    /// 在XR交互事件中调用这个方法，例如Grab Interactable的OnSelectExited或自定扳机事件
    /// </summary>
    public void ProcessStamp()
    {
        // 1. 渲染深度图
        Debug.Log("盖章函数被触发了！");
        depthCamera.Render();

        // 2. 从RenderTexture中读取深度数据
        RenderTexture.active = depthRenderTexture;
        Texture2D depthTexture = new Texture2D(depthRenderTexture.width, depthRenderTexture.height, TextureFormat.RHalf, false);
        depthTexture.ReadPixels(new Rect(0, 0, depthRenderTexture.width, depthRenderTexture.height), 0, 0);
        depthTexture.Apply();
        RenderTexture.active = null;

        // 3. 获取纸张网格的世界空间顶点并基于深度进行变形
        Vector3[] vertices = mesh.vertices;
        Color[] maskPixels = stampMask.GetPixels(); // 获取当前遮罩数据

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = stampPaperTransform.TransformPoint(vertices[i]);
            Vector3 screenPoint = depthCamera.WorldToScreenPoint(worldVertex);

            // 判断顶点是否在深度相机的视野内
            if (screenPoint.z > 0 &&
                screenPoint.x >= 0 && screenPoint.x < depthRenderTexture.width &&
                screenPoint.y >= 0 && screenPoint.y < depthRenderTexture.height)
            {
                int px = (int)screenPoint.x;
                int py = (int)screenPoint.y;

                // 采样深度值 (R通道)
                float depth = depthTexture.GetPixel(px, py).r;

                // 深度大于0，说明印章覆盖了该区域，进行变形
                if (depth > 0.0f)
                {
                    Vector3 stampWorldPos = depthCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
                    Vector3 stampLocalPos = stampPaperTransform.InverseTransformPoint(stampWorldPos);

                    // 沿纸张本地Y轴（法线方向）应用深度偏移
                    vertices[i].y = Mathf.Max(vertices[i].y, stampLocalPos.y * stampDepthScale);

                    // 同步更新遮罩像素（设置为1，代表有印记）
                    int maskIndex = py * depthRenderTexture.width + px;
                    if (maskIndex >= 0 && maskIndex < maskPixels.Length)
                        maskPixels[maskIndex] = Color.white;
                }
            }
        }

        // 4. 将变形写回网格
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 可选：更新MeshCollider以支持碰撞
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }

        // 5. 更新遮罩纹理并传给材质
        stampMask.SetPixels(maskPixels);
        stampMask.Apply();
        stampPaperMaterial.SetTexture(maskPropertyName, stampMask);

        // 6. 清理
        Destroy(depthTexture);
    }

    // 清空印记（用于重置纸张，如有需要）
    public void ClearMask()
    {
        Color[] black = new Color[stampMask.width * stampMask.height];
        for (int i = 0; i < black.Length; i++) black[i] = Color.black;
        stampMask.SetPixels(black);
        stampMask.Apply();
    }
}