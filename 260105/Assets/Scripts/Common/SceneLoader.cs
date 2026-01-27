
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{ 
    Title,
    Lobby,
    Room,
    InGame,

}

public class SceneLoader : SingletonBehaviour<SceneLoader>
{
    protected override void Init()
    {
        
        base.isDestroyOnLoad = true;

        base.Init();
    }
    public void LoadScene(SceneType sceneType)
    {
        string sceneName = sceneType.ToString();
        Logger.Log($"Loading scene: {sceneName}");

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }
    public void ReloadCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Logger.Log($"Reloading current scene: {sceneName}");
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }
    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        Logger.Log($"{sceneType} Scene Load Async Start");
        Time.timeScale = 1.0f;
        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }
}
