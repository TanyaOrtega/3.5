using UnityEngine;

public class PauseOnEscape : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
  
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        if (isPaused && Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Juego PAUSADO");
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Juego REANUDADO");
    }
}
