using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    [Header("Spawnables (length = 4)")]
    public GameObject[] prefabs; // 4 prefabs
    public BoxCollider[] spawnBoxes; // 4 box colliders (uno por prefab)
    [Tooltip("Zona donde NO puede aparecer el objeto.")]
    public CapsuleCollider forbiddenZone;

    [Header("Spawn Settings")]
    public int maxAttemptsPerItem = 50;
    public bool spawnOnStart = true;

    [Header("Spawn Height")]
    [Tooltip("Altura fija a la que aparecerán los objetos.")]
    public float spawnHeight = 1f;


    void Start()
    {
        if (spawnOnStart) SpawnAll();
    }

    [ContextMenu("Spawn All Collectables")]
    public void SpawnAll()
    {
        if (prefabs == null || spawnBoxes == null)
        {
            Debug.LogWarning("CollectableSpawner: prefabs o spawnBoxes no asignados.");
            return;
        }

        int n = Mathf.Min(prefabs.Length, spawnBoxes.Length);

        for (int i = 0; i < n; i++)
        {
            SpawnOne(i);
        }
    }

    void SpawnOne(int index)
    {
        if (prefabs[index] == null || spawnBoxes[index] == null)
        {
            Debug.LogWarning($"CollectableSpawner: prefab o spawnBox faltante en índice {index}");
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        bool found = false;
        int attempts = 0;

        Bounds b = spawnBoxes[index].bounds;

        while (!found && attempts < maxAttemptsPerItem)
        {
            attempts++;

            float x = Random.Range(b.min.x, b.max.x);
            float y = spawnHeight;
            float z = Random.Range(b.min.z, b.max.z);
            Vector3 candidate = new Vector3(x, y, z);

            // Si hay forbiddenZone y el candidato está dentro -> rechazar
            if (forbiddenZone != null)
            {
                // Usamos ClosestPoint para evaluar si el punto está dentro del capsule (si ClosestPoint == point => inside)
                Vector3 closest = forbiddenZone.ClosestPoint(candidate);
                if (Vector3.Distance(closest, candidate) < 0.001f)
                {
                    continue; // está dentro del capsule, inténtalo otra vez
                }
            }

            // además comprobamos que el punto quede dentro estrictamente del box (defensivo)
            if (candidate.x < b.min.x || candidate.x > b.max.x ||
                candidate.y < b.min.y || candidate.y > b.max.y ||
                candidate.z < b.min.z || candidate.z > b.max.z)
            {
                continue;
            }

            spawnPos = candidate;
            found = true;
        }

        if (!found)
        {
            // si no encontró punto válido, colocarlo en el centro del box (fallback)
            spawnPos = spawnBoxes[index].bounds.center;
            Debug.LogWarning($"CollectableSpawner: No se encontró posición fuera del forbiddenZone para índice {index}. Usando centro del box.");
        }

        GameObject go = Instantiate(prefabs[index], spawnPos, Quaternion.identity);
        // asignar id en componente Collectable (lo añadimos si no existe)
        Collectable c = go.GetComponent<Collectable>();
        if (c == null) c = go.AddComponent<Collectable>();
        c.id = index;
        c.collected = false;
    }
}
