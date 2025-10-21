using UnityEngine;

public class FlotacionSuave : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [SerializeField] private float alturaFlotacion = 0.05f;
    [SerializeField] private float velocidadFlotacion = 1f;
    [SerializeField] private float rotacionSuave = 15f;
    
    [Header("Opciones")]
    [SerializeField] private bool flotarEnY = true;
    [SerializeField] private bool rotarSuavemente = true;
    
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private float tiempo;

    void Start()
    {
        // Guardar la posición y rotación inicial del objeto
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        tiempo = Random.Range(0f, 100f);
    }

    void Update()
    {
        tiempo += Time.deltaTime;
        
        // Movimiento de flotación vertical
        if (flotarEnY)
        {
            float movimientoY = Mathf.Sin(tiempo * velocidadFlotacion) * alturaFlotacion;
            transform.position = new Vector3(
                posicionInicial.x,
                posicionInicial.y + movimientoY,
                posicionInicial.z
            );
        }
        
        // Rotación suave opcional (respetando la rotación inicial)
        if (rotarSuavemente)
        {
            float rotacionY = Mathf.Sin(tiempo * velocidadFlotacion * 0.5f) * rotacionSuave;
            transform.rotation = rotacionInicial * Quaternion.Euler(0, rotacionY, 0);
        }
        else
        {
            // Mantener siempre la rotación inicial si no hay rotación suave
            transform.rotation = rotacionInicial;
        }
    }
    
    // Método para redefinir la rotación inicial si es necesario
    public void SetRotacionInicial(Quaternion nuevaRotacion)
    {
        rotacionInicial = nuevaRotacion;
    }
    
    public void SetAlturaFlotacion(float nuevaAltura)
    {
        alturaFlotacion = nuevaAltura;
    }
    
    public void SetVelocidadFlotacion(float nuevaVelocidad)
    {
        velocidadFlotacion = nuevaVelocidad;
    }
    
    public void SetRotacionSuave(float nuevaRotacion)
    {
        rotacionSuave = nuevaRotacion;
    }
}