using UnityEngine;

public class Collectable : MonoBehaviour
{
    [HideInInspector]
    public int id = -1;

    [HideInInspector]
    public bool collected = false;

    /// <summary>
    /// Llamar cuando el player confirme la recolección (ej: desde el raycast).
    /// Este método ejecuta el efecto de partículas si se le pasa, marca como colectado y devuelve true si la operación fue exitosa.
    /// </summary>
    public bool Collect(GameObject particlePrefab = null, float particleLifetime = 2f)
    {
        if (collected) return false;
        collected = true;

        if (particlePrefab != null)
        {
            GameObject part = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            Destroy(part, particleLifetime);
        }

        // Notificar al manager
        CollectableManager.Instance?.MarkCollected(id);

        // Destruir el objeto (o desactivar según tu preferencia)
        Destroy(gameObject);

        return true;
    }
}
