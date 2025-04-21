using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class SceneTransitions : MonoBehaviour
{
    private Image? fadeImage;
    private Animator? fadeAnimator;

    private void Start()
    {
        GameObject? fadeObject = GameObject.FindGameObjectWithTag("Fade");
        if (fadeObject) {
            fadeImage = fadeObject.GetComponent<Image>();
            fadeAnimator = fadeObject.GetComponent<Animator>();
        }
    }

    public void LoadMainMenu()
    {
        // Load the main menu scene
        StartCoroutine(FadeOutAndLoadScene("MainMenu"));
    }

    public void LoadPlayerSelection()
    {
        // Load the game scene
        StartCoroutine(FadeOutAndLoadScene("PlayerSelection"));
    }

    public void LoadGame()
    {
        // Load the game scene
        StartCoroutine(FadeOutAndLoadScene("Game"));
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

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage != null && fadeAnimator != null) {
            fadeAnimator.SetBool("Fade", true);
            yield return new WaitUntil(() => fadeImage.color.a == 1);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
