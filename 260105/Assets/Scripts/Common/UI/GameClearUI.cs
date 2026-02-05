using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameClearUIData : BaseUIData
{
    public int TotalScore;
    public int TotalWaves;
    public float PlayTime;
    public Action OnLobby;
    public Action OnQuit;
}

public class GameClearUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _quitButton;

    private Action _onLobby;
    private Action _onQuit;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var data = uiData as GameClearUIData;
        if (_scoreText) _scoreText.text = $"Total Score: {data.TotalScore}";
        if (_waveText) _waveText.text = $"Clear Wave: {data.TotalWaves}";
        if (_timeText)
        {
            int min = (int)(data.PlayTime / 60);
            int sec = (int)(data.PlayTime % 60);
            _timeText.text = $"Play Time: {min:00}:{sec:00}";
        }

        _onLobby = data.OnLobby;
        _onQuit = data.OnQuit;
    }

    public void OnClickLobbyButton()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onLobby?.Invoke();
        CloseUI();
    }

    public void OnClickQuitButton()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onQuit?.Invoke();
    }
}
