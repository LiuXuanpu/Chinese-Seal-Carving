using UnityEngine;

public class StampProjector : MonoBehaviour
{
    public RenderTexture stampRT;

    public void Stamp(GameObject paper)
    {
        StampReceiver receiver =
            paper.GetComponent<StampReceiver>();

        if (receiver != null)
        {
            receiver.ApplyStamp(stampRT);
        }
    }
}