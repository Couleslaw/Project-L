using UnityEditor.Build; 
using UnityEditor.Build.Reporting;
using UnityEngine;

// This class will execute code before the build process
public class ClearPlayerPrefsPreBuild : IPreprocessBuildWithReport
{
    // This property defines the order in which pre-build steps are executed.
    // Lower numbers execute earlier. You usually want this to be a low number.
    public int callbackOrder { get { return 0; } }

    // This method is called by Unity before the build starts.
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("--- Pre-Build Step: Clearing PlayerPrefs ---");

        // Clear all PlayerPrefs data.
        PlayerPrefs.DeleteAll();

        // Save changes to disk immediately.
        PlayerPrefs.Save();

        Debug.Log("--- PlayerPrefs cleared successfully before build. ---");
    }
}