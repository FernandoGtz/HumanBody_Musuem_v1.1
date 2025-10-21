using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;                // Referencia al jugador real
    public RectTransform minimapRect;       // Rect del minimapa (imagen)
    public Vector2 worldMin;                // Mínimo de coordenadas mundo
    public Vector2 worldMax;                // Máximo de coordenadas mundo

    private RectTransform iconRect;

    void Start()
    {
        iconRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        UpdateIconPosition();
    }

    void UpdateIconPosition()
    {
        // Posición actual del jugador en mundo
        Vector3 playerPos = player.position;

        // Normalizamos posición (de 0 a 1)
        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, playerPos.x);
        float normalizedY = Mathf.InverseLerp(worldMin.y, worldMax.y, playerPos.z); 
        // Ojo: usamos .z si el mundo es plano en XZ.

        // Convertimos a posición dentro del minimapa
        float minimapX = (normalizedX * minimapRect.rect.width) - (minimapRect.rect.width * 0.5f);
        float minimapY = (normalizedY * minimapRect.rect.height) - (minimapRect.rect.height * 0.5f);

        iconRect.anchoredPosition = new Vector2(minimapX, minimapY);
        iconRect.localEulerAngles = new Vector3(0, 0, -player.eulerAngles.y);
    }
}
