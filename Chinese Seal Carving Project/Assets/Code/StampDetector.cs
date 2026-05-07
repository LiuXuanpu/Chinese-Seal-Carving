using UnityEngine;

public class StampDetector : MonoBehaviour
{
    public StampProjector projector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Paper"))
        {
            Debug.Log("碰到Paper了");

            projector.Stamp(other.gameObject);
        }
    }
}