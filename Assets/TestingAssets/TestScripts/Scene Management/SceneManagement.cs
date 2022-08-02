using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public Image loadingScreen;
    public float waitScreenTime;
    private List<AsyncOperation> _operations;
    [SerializeField] private Camera cam;
    public bool loading { get; private set; }

    private void Awake()
    {
        GameData.sceneManagement = this;
        _operations = new List<AsyncOperation>();
        SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
        Color col = loadingScreen.color;
        col.a = 0;
        loadingScreen.color = col;
    }
    public void LoadScene(string newScene, string curScene)
    {
        cam.gameObject.SetActive(true);
        loading = true;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1,2.75f).OnComplete(() =>
        {
            Load(newScene, curScene);
        });
        Time.timeScale = 1;
    }
    private void Load(string newScene, string curScene)
    {
        _operations.Add(SceneManager.UnloadSceneAsync(curScene));
        _operations.Add(SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive));
        StartCoroutine(LoadingProgress());
    }

    public IEnumerator LoadingProgress()
    {
        for (int i = 0; i < _operations.Count; i++)
        {
            while (!_operations[i].isDone)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(waitScreenTime);
        loadingScreen.DOFade(0, 2.75f).OnComplete(Unload);
    }
    private void Unload()
    {
        loadingScreen.gameObject.SetActive(false);
        loading = false;
        cam.gameObject.SetActive(false);
    }
}
