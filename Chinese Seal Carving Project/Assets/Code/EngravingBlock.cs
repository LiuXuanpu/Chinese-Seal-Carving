using UnityEngine;
using UnityEngine.XR;    // 提供 InputDevices, CommonUsages
using EzySlice;         // EzySlice 核心

public class EngravingBlock : MonoBehaviour
{
    public Material crossSectionMaterial; // 切割面材质
    public Transform chiselTip;           // 刻刀尖物体
    public float etchCooldown = 0.15f;
    public float debrisForce = 0.3f;

    private float lastEtchTime = -1f;
    private InputDevice rightHandDevice;

    void Start()
    {
        // 获取右手柄设备引用
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        // 确保设备有效
        if (!rightHandDevice.isValid)
        {
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return; // 还没追踪到就等下一帧
        }

        // 读取扳机按下状态（不需要 PICO 专用 API）
        rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);

        if (triggerPressed && chiselTip != null && Time.time - lastEtchTime >= etchCooldown)
        {
            PerformEtch();
            lastEtchTime = Time.time;
        }
    }

    void PerformEtch()
    {
        Vector3 planePos = chiselTip.position;
        Vector3 planeNormal = chiselTip.forward;

        SlicedHull hull = gameObject.Slice(planePos, planeNormal, crossSectionMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(gameObject, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(gameObject, crossSectionMaterial);

            if (upperHull != null && lowerHull != null)
            {
                // --- 下半部分（新主体）---
                lowerHull.transform.SetPositionAndRotation(transform.position, transform.rotation);

                MeshCollider lowerCollider = lowerHull.AddComponent<MeshCollider>();
                lowerCollider.convex = true;
                lowerCollider.sharedMesh = lowerHull.GetComponent<MeshFilter>().mesh;

                EngravingBlock newBlock = lowerHull.AddComponent<EngravingBlock>();
                newBlock.crossSectionMaterial = crossSectionMaterial;
                newBlock.chiselTip = chiselTip;
                newBlock.etchCooldown = etchCooldown;

                // --- 上半部分（碎片）---
                Rigidbody rb = upperHull.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = 0.01f;
                rb.AddForce(planeNormal * debrisForce, ForceMode.Impulse);

                MeshCollider upperCollider = upperHull.AddComponent<MeshCollider>();
                upperCollider.convex = true;

                Destroy(upperHull, 3f);
                upperHull.transform.localScale = Vector3.one * 0.8f;

                // 销毁旧方块
                Destroy(gameObject);
            }
            else
            {
                if (upperHull != null) Destroy(upperHull);
                if (lowerHull != null) Destroy(lowerHull);
            }
        }
    }
}