using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("TestCharacter");
    }

    public void OpenSettings()
    {
        Debug.Log("Ouverture des paramètres...");
    }

    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}