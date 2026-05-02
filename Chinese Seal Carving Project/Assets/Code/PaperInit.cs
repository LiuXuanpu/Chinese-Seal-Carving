using UnityEngine;

public class PaperInit : MonoBehaviour
{
    [SerializeField] private RenderTexture paperRT;   // 在Inspector中拖入你的PaperRT
    [SerializeField] private Color clearColor = Color.white;

    void Start()
    {
        if (paperRT == null)
        {
            Debug.LogError("PaperRT 没有指定！请在Inspector中拖入 Render Texture。");
            return;
        }

        // 清空为白色
        RenderTexture.active = paperRT;
        GL.Clear(true, true, clearColor);
        RenderTexture.active = null;
    }
}