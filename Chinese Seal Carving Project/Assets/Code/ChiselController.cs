using UnityEngine;
using UnityEngine.XR; // Unity XR 原生输入

public class ChiselController : MonoBehaviour
{
    [Tooltip("0 = LeftHand, 1 = RightHand")]
    public int handIndex = 1;  // 默认右手

    private InputDevice targetDevice;

    void Start()
    {
        // 根据 handIndex 获取对应手柄设备
        XRNode node = (handIndex == 0) ? XRNode.LeftHand : XRNode.RightHand;
        targetDevice = InputDevices.GetDeviceAtXRNode(node);
    }

    void Update()
    {
        // 如果设备无效，尝试重新获取（避免 Start 时未追踪到）
        if (!targetDevice.isValid)
        {
            XRNode node = (handIndex == 0) ? XRNode.LeftHand : XRNode.RightHand;
            targetDevice = InputDevices.GetDeviceAtXRNode(node);
        }

        // 读取位置和旋转
        if (targetDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
        {
            transform.position = pos;
        }
        if (targetDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
        {
            transform.rotation = rot;
        }
    }
}