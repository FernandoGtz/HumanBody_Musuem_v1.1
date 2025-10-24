using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ZoneDetector : MonoBehaviour
{
    [Header("Configuración de Colliders")]
    public Collider colliderCentro;
    public Collider colliderSala1;
    public Collider colliderSala2;
    public Collider colliderSala3;
    public Collider colliderSala4;

    [Header("Configuración UI")]
    public TextMeshProUGUI zoneText;

    private List<Collider> activeColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        // Agregar a la lista si es un collider de zona
        if (IsZoneCollider(other) && !activeColliders.Contains(other))
        {
            activeColliders.Add(other);
        }
        UpdateZoneDisplay();
    }

    private void OnTriggerExit(Collider other)
    {
        // Remover de la lista
        if (activeColliders.Contains(other))
        {
            activeColliders.Remove(other);
        }
        UpdateZoneDisplay();
    }

    private void UpdateZoneDisplay()
    {
        // PRIORIDAD 1: Si está en el centro, siempre mostrar "Centro"
        if (activeColliders.Contains(colliderCentro))
        {
            zoneText.text = "Centro";
            return;
        }

        // PRIORIDAD 2: Mostrar la última sala en la que entró (o la que queda)
        if (activeColliders.Contains(colliderSala1))
        {
            zoneText.text = "Sistema Óseo";
        }
        else if (activeColliders.Contains(colliderSala2))
        {
            zoneText.text = "Sistema Muscular";
        }
        else if (activeColliders.Contains(colliderSala3))
        {
            zoneText.text = "Sistema Circulatorio";
        }
        else if (activeColliders.Contains(colliderSala4))
        {
            zoneText.text = "Sistema Nervioso";
        }
        else
        {
            zoneText.text = "";
        }
    }

    private bool IsZoneCollider(Collider col)
    {
        return col == colliderCentro || 
               col == colliderSala1 || 
               col == colliderSala2 || 
               col == colliderSala3 || 
               col == colliderSala4;
    }
}