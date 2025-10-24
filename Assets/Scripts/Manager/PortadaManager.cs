using UnityEngine;
using UnityEngine.SceneManagement;

public class PortadaManager : MonoBehaviour
{
    // Nombre de la escena del juego (asignar desde el inspector)
    public string nombreEscenaJuego = "Juego";
    
    // Método que se llamará desde el botón en la portada
    public void IniciarJuego()
    {
        SceneManager.LoadScene(1);
    }
    
    // Método para cerrar el juego
    public void SalirDelJuego()
    {
        #if UNITY_EDITOR
        // Si estamos en el Editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Si estamos en una build ejecutable
        Application.Quit();
        #endif
    }
}