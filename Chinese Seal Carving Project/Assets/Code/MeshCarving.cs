using UnityEngine;
using UnityEngine.XR;

public class MeshCarving : MonoBehaviour
{
    [Header("刻刀设置")]
    public Transform chiselTip;   // 拖入抓取方块上的 Tip 子物体
    public float carveRadius = 0.02f;   // 凹坑半径（米）——先用大值测试
    public float carveDepth = 0.005f;   // 单次雕刻深度（米）
    public float carveInterval = 0.05f; // 最小间隔（秒）

    private Mesh mesh;
    private Vector3[] currentVerts;
    private float lastCarveTime;
    private InputDevice rightHand;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        if (!mesh.isReadable)
        {
            Debug.LogError("Mesh 不可读写！请在模型导入设置中开启 Read/Write Enabled。");
            enabled = false;
            return;
        }
        currentVerts = mesh.vertices;
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);

        if (trigger && chiselTip != null && Time.time - lastCarveTime > carveInterval)
        {
            // 判断刀尖是否接触当前物体（简单距离检测）
            if (IsChiselTouchingModel())
            {
                Carve(chiselTip.position, chiselTip.forward);
                lastCarveTime = Time.time;
            }
        }
    }

    bool IsChiselTouchingModel()
    {
        // 使用 Physics.OverlapSphere 检测，注意排除刻刀自身
        Collider[] hits = Physics.OverlapSphere(chiselTip.position, carveRadius * 1.2f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) return true;
        }
        return false;
    }

    void Carve(Vector3 worldPoint, Vector3 direction)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector3 localPushDir = transform.InverseTransformDirection(-direction).normalized; // 向内

        for (int i = 0; i < currentVerts.Length; i++)
        {
            float dist = Vector3.Distance(currentVerts[i], localPoint);
            if (dist < carveRadius)
            {
                float falloff = 1f - (dist / carveRadius);
                currentVerts[i] += localPushDir * carveDepth * falloff;
            }
        }

        mesh.vertices = currentVerts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        // 更新碰撞体
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void OnDrawGizmosSelected()
    {
        if (chiselTip != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(chiselTip.position, carveRadius);
        }
    }
}