using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;   

public class GameOverUIData : BaseUIData
{
    public int Score;
    public int Wave;
    public Action OnRestart;
    public Action OnLobby;
    public Action OnQuit;
}

public class GameOverUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _quitButton;

    private Action _onRestart;
    private Action _onLobby;
    private Action _onQuit;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var data = uiData as GameOverUIData;
        if (_scoreText) _scoreText.text = $"Score: {data.Score}";
        if (_waveText) _waveText.text = $"Wave: {data.Wave}";

        _onRestart = data.OnRestart;
        _onLobby = data.OnLobby;
        _onQuit = data.OnQuit;
        if (_restartButton != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _restartButton.gameObject.SetActive(true); 
            }
            else
            {
                _restartButton.gameObject.SetActive(false); 
            }
        }
    }

    public void OnClickRestartButton()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onRestart?.Invoke();
        CloseUI();
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
