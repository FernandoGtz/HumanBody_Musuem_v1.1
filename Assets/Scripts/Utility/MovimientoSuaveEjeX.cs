using UnityEngine;

public class MovimientoSuaveEjeX : MonoBehaviour
{
    [Header("Configuración del movimiento")]
    [Tooltip("Distancia máxima en X (local) que recorrerá el objeto desde su posición inicial")]
    public float distancia = 1f;

    [Tooltip("Velocidad del movimiento de ida y vuelta")]
    public float velocidad = 1f;

    // Posición inicial del objeto
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        // Usamos Mathf.Sin para generar un movimiento oscilante suave
        float offset = Mathf.Sin(Time.time * velocidad) * distancia;

        // Solo modificamos el eje X local
        transform.localPosition = new Vector3(posicionInicial.x + offset, posicionInicial.y, posicionInicial.z);
    }

}
