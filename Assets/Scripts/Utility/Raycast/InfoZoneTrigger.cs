using UnityEngine;

public class InfoZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (RaycastInteractorModels.Instance != null)
        {
            RaycastInteractorModels.Instance.PlayerEnteredZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (RaycastInteractorModels.Instance != null)
        {
            RaycastInteractorModels.Instance.PlayerExitedZone();
        }
    }
}