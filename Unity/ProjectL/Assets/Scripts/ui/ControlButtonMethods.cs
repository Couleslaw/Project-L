using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OnNewGamePressed()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("PlayerSelection");
    }

    public void OnExitGamePressed()
    {
        // Exit the game
        Application.Quit();
        Debug.Log("Game exited");
    }

    public void OnUserGuidePressed()
    {
        // Open https://couleslaw.github.io/Project-L/UserDocs/ in the default web browser
        Application.OpenURL("https://couleslaw.github.io/Project-L/UserDocs/");
    }
}
