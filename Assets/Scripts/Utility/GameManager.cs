using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public CanvasGroup canvasInicio;
    public float tiempoEspera = 3f;
    
    private void Start()
    {
        StartCoroutine(SecuenciaInicioJuego());
    }
    
    private IEnumerator SecuenciaInicioJuego()
    {
        // Pausar TODO el juego
        Time.timeScale = 0f;
        
        // Mostrar canvas
        if (canvasInicio != null)
        {
            canvasInicio.alpha = 1f;
            canvasInicio.gameObject.SetActive(true);
        }
        
        // Esperar 3 segundos (usando tiempo real, no afectado por la pausa)
        yield return new WaitForSecondsRealtime(tiempoEspera);
        
        // Efecto fade out
        yield return StartCoroutine(FadeOutCanvas());
        
        // Reanudar TODO el juego
        Time.timeScale = 1f;
    }
    
    private IEnumerator FadeOutCanvas()
    {
        if (canvasInicio == null) yield break;
        
        float duracionFade = 1f;
        float tiempoInicio = Time.unscaledTime;
        
        while (Time.unscaledTime - tiempoInicio < duracionFade)
        {
            float progreso = (Time.unscaledTime - tiempoInicio) / duracionFade;
            canvasInicio.alpha = Mathf.Lerp(1f, 0f, progreso);
            yield return null;
        }
        
        canvasInicio.alpha = 0f;
        canvasInicio.gameObject.SetActive(false);
    }
    
    // Método para volver a la portada (escena 0)
    public void VolverAPortada()
    {
        // Asegurarse de que el tiempo vuelva a la normalidad
        Time.timeScale = 1f;
        
        // Cargar la escena de portada (escena 0)
        SceneManager.LoadScene(0);
    }
}