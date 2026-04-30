using UnityEngine;
using UnityEngine.XR; // Unity 原生 XR 输入

public class SimpleGrab : MonoBehaviour
{
    [Header("抓取设置")]
    [Tooltip("多近才能抓住（米）")]
    public float grabDistance = 0.3f;
    [Tooltip("抓取后相对于手柄的位置偏移")]
    public Vector3 attachOffset = new Vector3(0f, -0.1f, 0.1f); // 可调
    public Vector3 attachRotationOffset = new Vector3(45f, 0f, 0f); // 欧拉角，可调

    private bool isGrabbed = false;
    private Rigidbody rb;
    private InputDevice rightHand;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        // 获取右手柄位置和旋转
        rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 handPos);
        rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion handRot);
        rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed);

        float dist = Vector3.Distance(handPos, transform.position);

        // 抓取：靠近且按下侧键，且当前未抓住
        if (gripPressed && !isGrabbed && dist < grabDistance)
        {
            isGrabbed = true;
            rb.isKinematic = true; // 抓取时免受物理影响
        }
        // 松开：释放
        else if (!gripPressed && isGrabbed)
        {
            isGrabbed = false;
            rb.isKinematic = false;
        }

        // 抓住状态下，将刻刀吸附到手柄位置（带偏移）
        if (isGrabbed)
        {
            // 计算目标位置：手柄位置 + 手柄旋转下的偏移
            Vector3 targetPos = handPos + handRot * attachOffset;
            Quaternion targetRot = handRot * Quaternion.Euler(attachRotationOffset);

            transform.SetPositionAndRotation(targetPos, targetRot);
        }
    }

    // 在 Scene 视图中画出抓取范围（方便调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabDistance);
    }
}