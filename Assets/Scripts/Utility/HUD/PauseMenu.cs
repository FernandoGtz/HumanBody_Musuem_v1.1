using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Referencias de Canvas")]
    public GameObject pauseMenuCanvas; // Canvas del menú de pausa
    public GameObject playerHUDCanvas; // Canvas de interfaz del jugador (HUD)

    [Header("Estado del juego")]
    public static bool isPaused = false;

    void Start()
    {
        // Aseguramos que el juego empiece sin pausa
        pauseMenuCanvas.SetActive(false);
        playerHUDCanvas.SetActive(true);
        ResumeGame();
    }

    void Update()
    {
        // Presionamos ESC para pausar / continuar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        // Activar el canvas del menú de pausa
        pauseMenuCanvas.SetActive(true);
        // Desactivar el HUD
        playerHUDCanvas.SetActive(false);
        // Detener el tiempo en la escena
        Time.timeScale = 0f;
        isPaused = true;
        // Mostrar el cursor para interactuar con botones
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        // Desactivar el canvas del menú de pausa
        pauseMenuCanvas.SetActive(false);
        // Activar el HUD
        playerHUDCanvas.SetActive(true);
        // Reanudar el tiempo en la escena
        Time.timeScale = 1f;
        isPaused = false;
        // Ocultar el cursor y bloquearlo al centro
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
