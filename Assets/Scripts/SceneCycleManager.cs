using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSequenceManager : MonoBehaviour
{
    [Header("Scene names")]
    public string scene1Name = "Scene1";
    public string scene2Name = "Scene2";
    public string scene3Name = "Scene3";

    [Header("Delays")]
    public float timeInScene1 = 10f;
    public float timeInScene2 = 10f;

    private void Start()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // Load Scene 1 immediately
        yield return LoadSceneAdditive(scene1Name);

        yield return new WaitForSeconds(timeInScene1);

        // Load Scene 2
        yield return LoadSceneAdditive(scene2Name);
        yield return UnloadSceneIfLoaded(scene1Name);

        yield return new WaitForSeconds(timeInScene2);

        // Load Scene 3
        yield return LoadSceneAdditive(scene3Name);
        yield return UnloadSceneIfLoaded(scene2Name);
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
            yield break;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);
    }

    private IEnumerator UnloadSceneIfLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(scene);
        }
    }
}