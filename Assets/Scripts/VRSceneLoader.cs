using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRSceneLoader : MonoBehaviour
{
    public static VRSceneLoader Instance;

    [Header("XR Player")]
    public Transform xrOrigin; // ton XR Origin

    [Header("Settings")]
    public string currentSceneName;

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
    void Start()
{
    currentSceneName = SceneManager.GetActiveScene().name;
}

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
        Debug.Log("Loading scene: " + sceneName);
        Debug.Log("Current scene: " + currentSceneName);
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Charger la nouvelle scène
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!loadOp.isDone)
            yield return null;

        // Définir la scène active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // Chercher un spawn point dans la scène
        GameObject spawn = GameObject.FindWithTag("SpawnPoint");

        if (spawn != null && xrOrigin != null)
        {
            xrOrigin.position = spawn.transform.position;
            xrOrigin.rotation = spawn.transform.rotation;
        }
        else
        {
            Debug.LogWarning("SpawnPoint ou XR Origin manquant !");
        }

        // Décharger l'ancienne scène
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            yield return SceneManager.UnloadSceneAsync(currentSceneName);
        }

        currentSceneName = sceneName;
    }
}