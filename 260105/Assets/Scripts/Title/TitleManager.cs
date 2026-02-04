
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{

    [SerializeField] private Animation _logoAnim;
    [SerializeField] private TextMeshProUGUI _logoText;

    [SerializeField] private GameObject _title;
    [SerializeField] private Slider _loadingSlider;
    [SerializeField] private TextMeshProUGUI _loadingProgressText;

    private AsyncOperation _asyncOperation;

    private void Awake()
    {
        _logoAnim.gameObject.SetActive(true);
        _title.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LoadGameCo());
    }


    private IEnumerator LoadGameCo()
    {
        Logger.Log($"{GetType()}::LoadGameCo Start");

        _logoAnim.Play();
        yield return new WaitForSeconds(_logoAnim.clip.length);

        _logoAnim.gameObject.SetActive(false);
        _title.SetActive(true);

        // NetworkManager에 위임
        NetworkManager.Instance.Connect();

        _asyncOperation = SceneLoader.Instance.LoadSceneAsync(SceneType.Lobby);
        if (_asyncOperation == null)
        {
            Logger.LogError("Failed to start loading Lobby scene.");
            yield break;
        }

        _asyncOperation.allowSceneActivation = false;
        float currentBarValue = 0f;

        while (!_asyncOperation.isDone)
        {
            float targetValue = _asyncOperation.progress;

            if (_asyncOperation.progress >= 0.9f && !NetworkManager.Instance.IsConnected)
                targetValue = 0.9f;
            else if (_asyncOperation.progress >= 0.9f && NetworkManager.Instance.IsConnected)
                targetValue = 1.0f;

            currentBarValue = Mathf.MoveTowards(currentBarValue, targetValue, Time.deltaTime);
            _loadingSlider.value = currentBarValue;
            _loadingProgressText.text = $"{(currentBarValue * 100)}%";

            if (_asyncOperation.progress >= 0.9f && NetworkManager.Instance.IsConnected)
                break;

            yield return null;
        }

        _loadingSlider.value = 1f;
        _loadingProgressText.text = "100%";
        yield return new WaitForSeconds(0.5f);

        _asyncOperation.allowSceneActivation = true;
    }
}
