using UnityEngine;
using UnityEngine.XR;

public class TransferButton : MonoBehaviour
{
    [Header("拖入 ReferenceGuide 物体")]
    public Renderer guideRenderer;

    [Header("画布 RenderTexture")]
    public RenderTexture paperRT;

    [Header("触发距离")]
    public float pressDistance = 0.2f;

    private InputDevice rightHand;
    private bool triggerPressed;

    void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!guideRenderer) Debug.LogError("❌ 请把 ReferenceGuide 拖入 Guide Renderer 槽位！");
        if (!paperRT) Debug.LogError("❌ 没有拖入 Paper RT！");
    }

    void Update()
    {
        if (!guideRenderer || !paperRT) return;

        // ----- 编辑器测试（可保留，方便调试）-----
        if (Input.GetKeyDown(KeyCode.T))
        {
            ApplyPaperTexture();
            return;
        }

        // ----- PICO 手柄触发 -----
        rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos);
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
        float dist = Vector3.Distance(pos, transform.position);

        if (trigger && !triggerPressed && dist < pressDistance)
        {
            ApplyPaperTexture();
        }
        triggerPressed = trigger;
    }

    void ApplyPaperTexture()
    {
        if (!guideRenderer || !paperRT) return;

        // 创建全新材质并赋纹理，彻底避免引用混乱
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        Material mat = new Material(shader);
        mat.mainTexture = paperRT;
        mat.SetTexture("_BaseMap", paperRT);   // URP 主纹理属性
        guideRenderer.material = mat;

        Debug.Log("✅ 字迹已显示在印章参考平面上！");
    }
}