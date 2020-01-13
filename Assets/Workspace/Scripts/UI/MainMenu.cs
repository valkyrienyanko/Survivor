using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool FirstTime = true;

    public void Awake()
    {
        if (FirstTime)
        {
            FirstTime = false;
            SetResolution();
            SetQuality();
            SetFullscreen();
        }
    }

    private void SetResolution()
    {
        if (!PlayerPrefs.HasKey("options.resolution"))
            return;
        Resolution[] resolutions = Screen.resolutions;
        int index = PlayerPrefs.GetInt("options.resolution") - 1;
        Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
    }

    private void SetQuality()
    {
        if (!PlayerPrefs.HasKey("options.quality"))
            return;
        int index = PlayerPrefs.GetInt("options.quality");
        QualitySettings.SetQualityLevel(index);
    }

    private void SetFullscreen()
    {
        if (!PlayerPrefs.HasKey("options.fullscreen"))
            return;
        Screen.fullScreen = PlayerPrefsX.GetBool("options.fullscreen");
    }

    public void StartGame()
    {
        LoadSceneAsync("Loading");
        Destroy(MenuMusicManager.MenuMusic);
    }

    public void LoadRoadmap()
    {
        LoadSceneAsync("Roadmap");
    }

    public void LoadOptions()
    {
        LoadSceneAsync("Options");
    }

    public void LoadCredits()
    {
        LoadSceneAsync("Credits");
    }

    public void QuitGame()
    {
#if UNITYEDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadSceneAsync(string scene)
    {
        StartCoroutine(LoadAsynchronously(scene));
    }

    private IEnumerator LoadAsynchronously(string scene)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}