using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public string nextSceneName;

    private AsyncOperation _preloadOperation;

    public void PreloadNextScene()
    {
        StartCoroutine(Preload());
    }

    public void LoadNextScene()
    {
        StartCoroutine(ActivateScene());
    }

    private IEnumerator Preload()
    {
        _preloadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        _preloadOperation.allowSceneActivation = false;
        yield return _preloadOperation;
    }

    private IEnumerator ActivateScene()
    {
        string currentSceneName = gameObject.scene.name;

        if (_preloadOperation == null)
        {
            yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        }
        else
        {
            while (_preloadOperation.progress < 0.9f)
                yield return null;

            _preloadOperation.allowSceneActivation = true;

            while (!_preloadOperation.isDone)
                yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

        yield return SceneManager.UnloadSceneAsync(currentSceneName);
    }
}