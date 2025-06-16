using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePause();
        }
    }

    public void TogglePause() {
        bool isPaused = Time.timeScale == 0f;
        Time.timeScale = isPaused ? 1f : 0f;
        pauseMenu.SetActive(!isPaused);
    }
}
