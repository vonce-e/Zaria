using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    public GameObject loadingScreen;
    public Slider progressBar;
    [SerializeField] private float minimumLoadingTime = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// This method will change the scene of the player
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void ChangeScene(string sceneName)
    {
        loadingScreen.SetActive(true);
        progressBar.value = 0;
        StartCoroutine(ChangeToSceneAsync(sceneName));
    }

    IEnumerator ChangeToSceneAsync(string sceneName)
    {
        float loadingStartTime = Time.unscaledTime;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            progressBar.value = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            yield return null;
        }

        progressBar.value = 1f;

        float timeAlreadyShown =
        Time.unscaledTime - loadingStartTime;

        float remainingTime = minimumLoadingTime - timeAlreadyShown;

        if (remainingTime > 0f)
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }
        
        loadingScreen.SetActive(false);
    }
}
