using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

/// <summary>
/// Manages transitioning between different scenes.
/// </summary>
public class SceneTransitions : MonoBehaviour
{
    #region Fields

    private Image? fadeImage;
    private Animator? fadeAnimator;

    #endregion

    #region Methods

    /// <summary>
    /// Loads the main menu scene with a fade-out effect. Also disables the logger if it exists.
    /// </summary>
    public void LoadMainMenu()
    {
        StartCoroutine(FadeOutAndLoadScene("MainMenu"));
        DisableLogger();
    }

    /// <summary>
    /// Loads the player selection scene with a fade-out effect.
    /// </summary>
    public void LoadPlayerSelection()
    {
        StartCoroutine(FadeOutAndLoadScene("PlayerSelection"));
    }

    /// <summary>
    /// Loads the game scene with a fade-out effect.
    /// </summary>
    public void LoadGame()
    {
        StartCoroutine(FadeOutAndLoadScene("Game"));
    }

    public void LoadFinalResults()
    {
        StartCoroutine(FadeOutAndLoadScene("FinalResults"));
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game exited");
    }

    /// <summary>
    /// Opens https://couleslaw.github.io/Project-L/UserDocs/ in the default web browser.
    /// </summary>
    public void OpenUserGuide()
    {
        Application.OpenURL("https://couleslaw.github.io/Project-L/UserDocs/");
    }

    private void Start()
    {
        // try to find the fade object in the scene for fade effects
        GameObject? fadeObject = GameObject.FindGameObjectWithTag("Fade");
        if (fadeObject) {
            fadeImage = fadeObject.GetComponent<Image>();
            fadeAnimator = fadeObject.GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Disables the logger if it exists.
    /// </summary>
    private void DisableLogger()
    {
        if (EasyUI.Logger.Instance != null) {
            EasyUI.Logger.Instance.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Fades out the screen, if the current scene contains the Fade prefab. Loads the specified scene after.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage != null && fadeAnimator != null) {
            fadeAnimator.SetBool("Fade", true);
            yield return new WaitUntil(() => fadeImage.color.a == 1);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    #endregion
}
