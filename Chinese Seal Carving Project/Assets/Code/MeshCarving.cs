using UnityEngine;
using UnityEngine.XR;

public class MeshCarving : MonoBehaviour
{
    [Header("手动雕刻")]
    public Transform chiselTip;          // 刻刀刀尖
    public float carveRadius = 0.005f;
    public float carveDepth = 0.001f;
    public float carveInterval = 0.05f;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] currentVerts;
    private float lastCarveTime;
    private InputDevice rightHand;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        if (!mesh.isReadable)
        {
            Debug.LogError("网格不可读写！请开启 Read/Write Enabled。");
            enabled = false;
            return;
        }
        originalVertices = mesh.vertices;
        currentVerts = (Vector3[])originalVertices.Clone();
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        // 手动雕刻（用刻刀）
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
        if (trigger && chiselTip && Time.time - lastCarveTime > carveInterval)
        {
            if (IsChiselTouching())
            {
                Vector3 localPoint = transform.InverseTransformPoint(chiselTip.position);
                Vector3 pushDir = transform.InverseTransformDirection(chiselTip.forward).normalized;
                for (int i = 0; i < currentVerts.Length; i++)
                {
                    float dist = Vector3.Distance(currentVerts[i], localPoint);
                    if (dist < carveRadius)
                    {
                        float falloff = 1f - (dist / carveRadius);
                        currentVerts[i] += pushDir * carveDepth * falloff;
                    }
                }
                mesh.vertices = currentVerts;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                GetComponent<MeshCollider>().sharedMesh = mesh;
                lastCarveTime = Time.time;
            }
        }
    }

    bool IsChiselTouching()
    {
        Collider[] hits = Physics.OverlapSphere(chiselTip.position, carveRadius * 1.2f);
        foreach (var hit in hits)
            if (hit.gameObject == gameObject) return true;
        return false;
    }

    // 生成红色印章纹理（被 StampPaper 调用）
    public Texture2D CreateStampTexture(int resolution = 512)
    {
        if (currentVerts == null || originalVertices == null) return null;

        Bounds bounds = mesh.bounds;
        Vector3 axis = Vector3.up;      // 印面朝上
        float maxProj = float.MinValue;
        foreach (Vector3 v in currentVerts)
        {
            float proj = Vector3.Dot(v - bounds.center, axis);
            if (proj > maxProj) maxProj = proj;
        }

        // 容差根据模型大小自动计算，至少 0.001 米
        float threshold = Mathf.Max(bounds.extents.magnitude * 0.01f, 0.001f);

        Vector3 t1 = Vector3.Cross(axis, Vector3.forward).normalized;
        if (t1.sqrMagnitude < 0.1f) t1 = Vector3.Cross(axis, Vector3.right).normalized;
        Vector3 t2 = Vector3.Cross(axis, t1).normalized;

        float minU = float.MaxValue, maxU = float.MinValue;
        float minV = float.MaxValue, maxV = float.MinValue;
        foreach (Vector3 v in currentVerts)
        {
            float proj = Vector3.Dot(v - bounds.center, axis);
            if (Mathf.Abs(proj - maxProj) < threshold)
            {
                float u = Vector3.Dot(v - bounds.center, t1);
                float vv = Vector3.Dot(v - bounds.center, t2);
                if (u < minU) minU = u;
                if (u > maxU) maxU = u;
                if (vv < minV) minV = vv;
                if (vv > maxV) maxV = vv;
            }
        }

        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[resolution * resolution];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        int count = 0;
        for (int i = 0; i < currentVerts.Length; i++)
        {
            Vector3 v = currentVerts[i];
            float proj = Vector3.Dot(v - bounds.center, axis);
            if (Mathf.Abs(proj - maxProj) < threshold)
            {
                float u = Mathf.InverseLerp(minU, maxU, Vector3.Dot(v - bounds.center, t1));
                float vv = Mathf.InverseLerp(minV, maxV, Vector3.Dot(v - bounds.center, t2));
                int x = Mathf.Clamp(Mathf.RoundToInt(u * resolution), 0, resolution - 1);
                int y = Mathf.Clamp(Mathf.RoundToInt(vv * resolution), 0, resolution - 1);

                Vector3 diff = originalVertices[i] - v;
                float depth = diff.magnitude;
                if (depth > 0.0001f)
                {
                    float intensity = Mathf.Clamp01(depth / 0.005f);   // 0.5mm深度为满红
                    pixels[y * resolution + x] = new Color(1, 0, 0, intensity);
                    count++;
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        Debug.Log($"印章纹理生成：{count} 个雕刻顶点");
        return tex;
    }
}