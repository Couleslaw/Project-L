using UnityEngine;

public class SceneTransitions : MonoBehaviour
{
    public void LoadMainMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void LoadPlayerSelection()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("PlayerSelection");
    }

    public void LoadGame()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        // Exit the game
        Application.Quit();
        Debug.Log("Game exited");
    }

    public void OpenUserGuide()
    {
        // Open https://couleslaw.github.io/Project-L/UserDocs/ in the default web browser
        Application.OpenURL("https://couleslaw.github.io/Project-L/UserDocs/");
    }
}
