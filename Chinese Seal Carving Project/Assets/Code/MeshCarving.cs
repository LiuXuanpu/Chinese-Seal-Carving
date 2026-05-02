using UnityEngine;
using UnityEngine.XR;

public class MeshCarving : MonoBehaviour
{
    public Transform chiselTip;          // 刻刀刀尖
    public float carveRadius = 0.005f;
    public float carveDepth = 0.001f;
    public float carveInterval = 0.05f;

    private Mesh mesh;
    private Vector3[] currentVerts;
    private float lastCarveTime;
    private InputDevice rightHand;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        if (!mesh.isReadable) { enabled = false; return; }
        currentVerts = mesh.vertices;
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
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
}