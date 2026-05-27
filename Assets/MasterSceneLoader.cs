using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MasterSceneLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadFirstScene());
    }

    private IEnumerator LoadFirstScene()
    {
        yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
    }
}